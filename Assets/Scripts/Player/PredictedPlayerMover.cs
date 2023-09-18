using FishNet;
using FishNet.Component.Animating;
using FishNet.Object;
using FishNet.Object.Prediction;
using FishNet.Transporting;
using UnityEngine;
using UnityEngine.UIElements;

public class PredictedPlayerMover : NetworkBehaviour
{
    public struct MoveData : IReplicateData
    {
        public bool Jump;
        public float Horizontal;
        public float Vertical;
        public Quaternion LookData;
        public bool Grounded;
        public bool WasGrounded;

        public MoveData(float horizontal, float vertical, bool jump, Quaternion lookData, bool grounded, bool wasGrounded)
        {
            Jump = jump;
            Horizontal = horizontal;
            Vertical = vertical;
            LookData = lookData;
            Grounded = grounded;
            WasGrounded = wasGrounded;
            _tick = 0;
        }

        private uint _tick;
        public void Dispose() { }
        public uint GetTick() => _tick;
        public void SetTick(uint value) => _tick = value;
    }

    private struct ReconcileData : IReconcileData
    {
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 Velocity;
        public Vector3 AngularVelocity;

        public ReconcileData(Vector3 position, Quaternion rotation, Vector3 velocity, Vector3 angularVelocity)
        {
            Position = position;
            Rotation = rotation;
            Velocity = velocity;
            AngularVelocity = angularVelocity;
            _tick = 0;
        }

        private uint _tick;
        public void Dispose() { }
        public uint GetTick() => _tick;
        public void SetTick(uint value) => _tick = value;
    }

    [SerializeField]
    private float _speed;
    [SerializeField]
    private float _jumpForce;
    private float _nextJumpTime;
    [SerializeField]
    private float jumpSpeedMod;
    [SerializeField]
    private float groundedOffset;
    [SerializeField]
    private LayerMask layerMask;
    private float groundedTimer;
    [SerializeField]
    private float groundedTimerMax;
    public bool serverGrounded;
    private bool serverWasGrounded;
    private float serverGroundedTimer;
    
    private bool _jump;
    private bool _subscribed = false;
    public bool activated = false;

    private Rigidbody _rb;
    public GameObject _cam;
    private Animator _animator;
    private NetworkAnimator _netAnimator;

    PlayerControlls playerControlls;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _animator = GetComponentInChildren<Animator>();
        _netAnimator = GetComponentInChildren<NetworkAnimator>();
        InstanceFinder.TimeManager.OnTick += TimeManager_OnTick;
        InstanceFinder.TimeManager.OnPostTick += TimeManager_OnPostTick;

        playerControlls = new PlayerControlls();
        playerControlls.Enable();
    }

    private void OnDestroy()
    {
        if (InstanceFinder.TimeManager != null)
        {
            InstanceFinder.TimeManager.OnTick -= TimeManager_OnTick;
            InstanceFinder.TimeManager.OnPostTick -= TimeManager_OnPostTick;
        }
    }

    public override void OnStartClient()
    {
        base.PredictionManager.OnPreReplicateReplay += PredictionManager_OnPreReplicateReplay;
    }
    public override void OnStopClient()
    {
        base.PredictionManager.OnPreReplicateReplay -= PredictionManager_OnPreReplicateReplay;
    }

    private void Update()
    {
        if (base.IsServer)
        {
            if (serverGrounded) serverGroundedTimer = groundedTimerMax;
            
            if (serverGroundedTimer > 0)
            {
                serverGroundedTimer -= Time.time;
                serverWasGrounded = true;
            }
            else
            {
                serverWasGrounded = false;
            }
        }

        if (!base.IsOwner) return;

        if(playerControlls.OnFoot.ResetMovement.WasPressedThisFrame())
        {
            _rb.velocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
        }
        
        if(playerControlls.OnFoot.Jump.WasPressedThisFrame() && Time.time > _nextJumpTime)
        {
            _nextJumpTime = Time.time + jumpSpeedMod;
            _jump = true;
        }

        if(base.IsClient) HandleAnimations();
    }

    private void PredictionManager_OnPreReplicateReplay(uint arg1, PhysicsScene arg2, PhysicsScene2D arg3)
    {
        if(!base.IsServer)
        {
            AddGravity(serverGrounded, serverWasGrounded);
        }
    }

    private void TimeManager_OnTick()
    {
        if (base.IsOwner)
        {
            Reconciliation(default, false);
            BuildMoveData(out MoveData md);
            Move(md, false);
        }

        if(base.IsServer)
        {
            Move(default, true);
        }
        
        AddGravity(serverGrounded, serverWasGrounded);
    }

    private void TimeManager_OnPostTick()
    {
        if (base.IsServer)
        {
            ReconcileData rd = new ReconcileData(transform.position, transform.rotation, _rb.velocity, _rb.angularVelocity);
            Reconciliation(rd, true);
        }
    }

    private void BuildMoveData(out MoveData md)
    {
        md = default;

        float vertical = playerControlls.OnFoot.Movement.ReadValue<Vector2>().y;
        float horizontal = playerControlls.OnFoot.Movement.ReadValue<Vector2>().x;
        Quaternion lookData;
        bool grounded = serverGrounded;
        bool wasGrounded;
        if (_cam != null)
        {
            lookData = _cam.transform.rotation;
        }
        else
        {
            lookData = Quaternion.identity;
        }

        if (grounded) groundedTimer = groundedTimerMax;
        if (groundedTimer > 0)
        {
            groundedTimer -= Time.time;
            wasGrounded = true;
        }
        else
        {
            wasGrounded = false;
        }

        if (horizontal == 0f && vertical == 0f && lookData == null && !_jump) return;

        md = new MoveData(vertical, horizontal, _jump, lookData, grounded, wasGrounded);
        _jump = false;
    }

    private void AddGravity(bool _grounded, bool _wasGrounded)
    {
        if (_grounded || _wasGrounded)
        {
            _rb.AddForce(Physics.gravity * 2);
        }
        else
        {
            _rb.AddForce(Physics.gravity * 50);
        }
        Debug.Log($"{_grounded} + {_wasGrounded}");
    }

    [Replicate]
    private void Move(MoveData md, bool asServer, Channel channel = Channel.Unreliable, bool replaying = false)
    {
        if (!activated) return;

        Vector3 direction = (Vector3)(md.Vertical * transform.right + md.Horizontal * transform.forward); 
        _rb.AddForce(direction * _speed);

        if (md.Jump && (md.Grounded || md.WasGrounded))
        {
            _rb.AddForce(new Vector3(0f, _jumpForce, 0f), ForceMode.Impulse);
        }

        transform.rotation = md.LookData;
    }

    [Reconcile]
    private void Reconciliation(ReconcileData rd, bool asServer, Channel channel = Channel.Unreliable)
    {
        transform.position = rd.Position;
        transform.rotation = rd.Rotation;
        _rb.velocity = rd.Velocity;
        _rb.angularVelocity = rd.AngularVelocity;
    }

    private void HandleAnimations()
    {
        Vector2 inputRaw = playerControlls.OnFoot.Movement.ReadValue<Vector2>();
        if (_rb.velocity.magnitude > 0.3f)
        {
            _animator.SetBool("isWalking", true);
            _animator.SetFloat("xMovement", inputRaw.x);
            _animator.SetFloat("yMovement", inputRaw.y);
        }
        else
        {
            _animator.SetBool("isWalking", false);
        }

        if (_jump) _netAnimator.SetTrigger("jump");
        if (Physics.Raycast(transform.position, transform.up * -1, 1f, layerMask)) _netAnimator.SetTrigger("hitGround");
    }
}