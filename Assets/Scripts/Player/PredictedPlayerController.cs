using FishNet;
using FishNet.Component.Animating;
using FishNet.Example.Scened;
using FishNet.Object;
using FishNet.Object.Prediction;
using FishNet.Transporting;
using UnityEngine;

public class PredictedPlayerController : NetworkBehaviour
{
    [SerializeField] private float _speed;
    [SerializeField] private float _jumpForce;
    [SerializeField] private float _jumpReset;
    [SerializeField] private float _dashForce;
    [SerializeField] private float _dashReset;
    [SerializeField] private float _groundedTimerMax;
    [SerializeField] private LayerMask _whatIsGround;


    [System.NonSerialized] public bool _grounded;
    [System.NonSerialized] public bool _activated;

    private PlayerControlls _playerControlls;
    private Rigidbody _rb;
    private Animator _animator;
    private NetworkAnimator _netAnimator;
    private Vector3 _dashDirection;
    private float _nextJumpTime;
    private float _nextDashTime;
    private bool _jump;
    private bool _dash;
    private bool _allowDash = true;
    private bool _wasGrounded;
    private float _groundedTimer;
    
    

    public struct MoveData : IReplicateData
    {
        public bool Jump;
        public bool Dash;
        public bool Grounded;
        public bool WasGrounded;
        public float MoveHorizontal;
        public float MoveVertical;
        public Vector3 DashDirection;

        public MoveData(bool jump, bool dash, bool grounded, bool wasGrounded, float moveHorizontal, float moveVertical, Vector3 dashDirection)
        {
            Jump = jump;
            Dash = dash;
            Grounded = grounded;
            WasGrounded = wasGrounded;
            MoveHorizontal = moveHorizontal;
            MoveVertical = moveVertical;
            DashDirection = dashDirection;

            _tick = 0;
        }

        private uint _tick;
        public void Dispose() { }
        public uint GetTick() => _tick;
        public void SetTick(uint value) => _tick = value;
    }

    public struct ReconcileData : IReconcileData
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

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _animator = GetComponentInChildren<Animator>();
        _netAnimator = GetComponentInChildren<NetworkAnimator>();

        _playerControlls = new PlayerControlls();
        _playerControlls.Enable();

        InstanceFinder.TimeManager.OnTick += TimeManager_OnTick;
        InstanceFinder.TimeManager.OnPostTick += TimeManager_OnPostTick;
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

    private void PredictionManager_OnPreReplicateReplay(uint arg1, PhysicsScene arg2, PhysicsScene2D arg3)
    {
        if (!base.IsServer)
            AddGravity();
    }

    private void Update()
    {
        if (base.IsOwner)
        {
            //prepare Jump
            if (_playerControlls.OnFoot.Jump.WasPressedThisFrame() && Time.time > _nextJumpTime)
            {
                _nextJumpTime = Time.time + _jumpReset;
                _jump = true;
            }

            //prepare dash
            if(_playerControlls.OnFoot.Dash.WasPressedThisFrame() && Time.time > _nextDashTime && _allowDash)
            {
                _nextDashTime = Time.time + _dashReset;
                _dash = true;
            }
            _dashDirection = transform.Find("Cam").forward;

            //grounded timer
            if (_groundedTimer > 0)
            {
                _groundedTimer -= Time.time;
                _wasGrounded = true;
            }
            else
            {
                _wasGrounded = false;
            }

            if (_grounded) _groundedTimer = _groundedTimerMax;

            HandleAnimations();
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
        if (base.IsServer)
        {
            Move(default, true);
        }
        AddGravity();
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

        float moveHorizontal = _playerControlls.OnFoot.Movement.ReadValue<Vector2>().x;
        float moveVertical = _playerControlls.OnFoot.Movement.ReadValue<Vector2>().y;
        Vector3 dashDirection = _dashDirection;

        md = new MoveData(_jump, _dash, _grounded, _wasGrounded, moveHorizontal, moveVertical, dashDirection);
        _jump = false;
        _dash = false;
    }

    private void AddGravity()
    {
        if(_grounded || _wasGrounded)
        {
            _rb.AddForce(Physics.gravity * 2f);
        }
        else
        {
            _rb.AddForce(Physics.gravity * 50f);
        }
    }

    [Replicate]

    private void Move(MoveData md, bool asServer, Channel channel = Channel.Unreliable, bool replaying = false)
    {
        if (!_activated) return;
        
        //move
        Vector3 direction = (Vector3)(md.MoveHorizontal * transform.right + md.MoveVertical * transform.forward);
        _rb.AddForce(direction * _speed);

        //jump
        if(md.Jump && (md.Grounded || md.WasGrounded))
        {
            _rb.AddForce(transform.up * _jumpForce, ForceMode.Impulse);
        }

        //dash
        if(md.Dash)
        {
            _rb.AddForce(md.DashDirection * _dashForce, ForceMode.Impulse);
            _allowDash = false;
        }
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
        Vector2 inputRaw = _playerControlls.OnFoot.Movement.ReadValue<Vector2>();
        if (_rb.velocity.magnitude > 0.5f)
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
        if (Physics.Raycast(transform.position, transform.up * -1, 0.6f, _whatIsGround)) _netAnimator.SetTrigger("hitGround");
    }

    private void OnCollisionStay(Collision collision)
    {
        if(collision.gameObject.layer == 7 || collision.gameObject.layer == 8 ||  collision.gameObject.layer == 9 || collision.gameObject.layer == 10)
        {
            _allowDash = true;
        }
    }
}