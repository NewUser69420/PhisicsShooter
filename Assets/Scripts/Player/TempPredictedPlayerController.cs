using FishNet;
using FishNet.Example.Scened;
using FishNet.Object;
using FishNet.Object.Prediction;
using FishNet.Transporting;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

public class TempPredictedPlayerController : NetworkBehaviour
{
    public struct MoveData : IReplicateData
    {
        public bool Jump;
        public float Horizontal;
        public float Vertical;

        public MoveData(float horizontal, float vertical, bool jump)
        {
            Jump = jump;
            Horizontal = horizontal;
            Vertical = vertical;
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
    
    private bool _jump;
    private bool _subscribed = false;
    public bool activated = false;

    private Rigidbody _rb;

    PlayerControlls playerControlls;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
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
        if (!base.IsOwner) return;

        if(playerControlls.OnFoot.ResetMovement.WasPressedThisFrame())
        {
            _rb.velocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
        }
        
        if(playerControlls.OnFoot.Jump.WasPressedThisFrame() && Time.time > _nextJumpTime)
        {
            _nextJumpTime = Time.time + 1f;
            _jump = true;
        }
    }

    private void PredictionManager_OnPreReplicateReplay(uint arg1, PhysicsScene arg2, PhysicsScene2D arg3)
    {
        if(!base.IsServer)
        {
            AddGravity();
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

        float horizontal = playerControlls.OnFoot.Movement.ReadValue<Vector2>().y;
        float vertical = playerControlls.OnFoot.Movement.ReadValue<Vector2>().x;

        if (horizontal == 0f && vertical == 0f && !_jump) return;

        md = new MoveData(vertical, horizontal, _jump);
        _jump = false;
    }

    private void AddGravity()
    {
        _rb.AddForce(Physics.gravity * 2f);
    }

    [Replicate]

    private void Move(MoveData md, bool asServer, Channel channel = Channel.Unreliable, bool replaying = false)
    {
        if(!activated) return;
        
        Vector3 direction = new Vector3(md.Horizontal, 0f, md.Vertical) * _speed;
        _rb.AddForce(direction);

        if(md.Jump)
        {
            _rb.AddForce(new Vector3(0f, _jumpForce, 0f), ForceMode.Impulse);
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
}