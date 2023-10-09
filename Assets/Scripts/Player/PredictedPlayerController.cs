using FishNet;
using FishNet.Component.Animating;
using FishNet.Connection;
using FishNet.Managing.Scened;
using FishNet.Object;
using FishNet.Object.Prediction;
using FishNet.Transporting;
using System.Collections;
using UnityEngine;
using static PlayerState;

public class PredictedPlayerController : NetworkBehaviour
{
    [SerializeField] private float _speed;
    [SerializeField] private float _jumpForce;
    [SerializeField] private float _grappleSpeed;
    [SerializeField] private float _jumpReset;
    [SerializeField] private float _dashForce;
    [SerializeField] private float _speedLimitDef;
    [SerializeField] private LayerMask _whatIsGround;
    [SerializeField] private float _wallJumpForce;
    [SerializeField] private float _wallJumpUp;
    [SerializeField] private float _wallJumpForward;
    
    public bool _activated;
    
    [System.NonSerialized] public bool _didWallJump;
    [System.NonSerialized] public bool _isTestPlayer;
    [System.NonSerialized] public bool _canDashOnWallJump;
    [System.NonSerialized] public bool _shootLaser;
    [System.NonSerialized] public Rigidbody _rb;

    private UnityEngine.SceneManagement.Scene currentScene;
    private PlayerControlls _playerControlls;
    private PlayerState _playerState;
    private Animator _animator;
    private NetworkAnimator _netAnimator;
    private WallRunning _wallRunner;
    private LaserShooter _laserShooter;
    private MainMenu _mainMenu;
    private PlayerAudioManager _playerAudioManager;
    private RaycastHit _hit;
    private Vector3 _dashDirection;
    private Vector3 _velocityToSet;
    private Vector3 _wallNormal;
    private Vector3 _wallForward;
    private Vector3 oldVel = Vector3.zero;
    private Vector3 newVel = Vector3.zero;
    private GroundedState newGState;
    private GroundedState oldGState;
    private float _speedLimit;
    private float _nextJumpTime;
    private float _nextDashTime;
    private float _velTimer;
    private float _velTimerMax = 0.3f;
    private bool _allowDash = true;
    private bool _wallLeft;
    private bool _speedLimitDisabled;
    private bool _impactDelay = true;

    public float _dashReset;


    public struct MoveData : IReplicateData
    {
        public ActionState aState;
        public GroundedState gState;
        public float MoveHorizontal;
        public float MoveVertical;
        public Vector3 DashDirection;
        public Vector3 GrappleVelocity;
        public Vector3 WallNormal;
        public Vector3 WallForward;
        public bool WallLeft;
        public bool WallJump;

        public MoveData(ActionState astate, GroundedState gstate, float moveHorizontal, float moveVertical, Vector3 dashDirection, Vector3 grappleVelocity, Vector3 wallNormal, Vector3 wallForward, bool wallLeft, bool wallJump)
        {
            aState = astate;
            gState = gstate;
            MoveHorizontal = moveHorizontal;
            MoveVertical = moveVertical;
            DashDirection = dashDirection;
            GrappleVelocity = grappleVelocity;
            WallNormal = wallNormal;
            WallForward = wallForward;
            WallLeft = wallLeft;
            WallJump = wallJump;


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
        _playerState = GetComponent<PlayerState>();
        _wallRunner = GetComponent<WallRunning>();
        _laserShooter = GetComponent<LaserShooter>();
        _playerAudioManager = GetComponent<PlayerAudioManager>();
        if (GameObject.Find("MainMenuUI") != null) _mainMenu = GameObject.Find("MainMenuUI").GetComponent<MainMenu>();

        _playerControlls = new PlayerControlls();
        _playerControlls.Enable();


        InstanceFinder.TimeManager.OnTick += TimeManager_OnTick;
        InstanceFinder.TimeManager.OnPostTick += TimeManager_OnPostTick;
    }

    public override void OnStartNetwork()
    {
        base.SceneManager.OnLoadEnd += _OnSceneLoad;
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

    private void _OnSceneLoad(SceneLoadEndEventArgs args)
    {
        foreach (var scene in args.LoadedScenes)
        {
            if (scene.name != "Lobbies") currentScene = scene;
        }
    }

    private void Update()
    {
        newVel = _rb.velocity;
        newGState = _playerState.gState;
        
        //prepare wallrun
        _wallNormal = _wallRunner.wallRight ? _wallRunner.rightWallhit.normal : _wallRunner.leftWallhit.normal;
        _wallForward = Vector3.Cross(_wallNormal, transform.up);
        if (!(_wallRunner.wallLeft && _wallRunner.verticalInput > 0) && !(_wallRunner.wallRight && _wallRunner.horizontalInput < 0)) _wallLeft = true;
        else _wallLeft = false;

        //prepare Jump
        if (_playerControlls.OnFoot.Jump.WasPressedThisFrame() && Time.time > _nextJumpTime && (_playerState.aState != ActionState.Dashing || _playerState.aState != ActionState.Grappling || _playerState.aState != ActionState.WallRunning))
        {
            _nextJumpTime = Time.time + _jumpReset;
            _playerState.aState = ActionState.Jumping;
        }

        //prepare walljump
        if (_playerControlls.OnFoot.Jump.WasPressedThisFrame() && _playerState.gState != GroundedState.Grounded)
        {
            _didWallJump = true;
        }

        //prepare dash
        if (_playerControlls.OnFoot.Dash.WasPressedThisFrame() && Time.time > _nextDashTime && _allowDash)
        {
            _nextDashTime = Time.time + _dashReset;
            _playerState.aState = ActionState.Dashing;
        }
        _dashDirection = transform.Find("Cam").forward;

        //do base anim
        HandleAnimations();
        
        if (base.IsOwner)
        {
            SyncDataServerRpc(base.ClientManager.Connection, _playerState.aState, _playerState.gState);

            if(_activated)
            {
                if(!_isTestPlayer) _mainMenu.gameObject.SetActive(false);
            }
            else
            {
                _mainMenu.gameObject.SetActive(true);
            }
        }

        //velocityCheck
        if(_velTimer > 0)
        {
            _velTimer -= Time.deltaTime;
        }
        else
        {
            _velTimer = _velTimerMax;
            oldVel = _rb.velocity;
            _impactDelay = true;
        }

        //SpeedControll();

        if(_playerState.aState != ActionState.Passive) { StartCoroutine(ResetSpeed()); }

        oldGState = newGState;
    }

    IEnumerator ResetSpeed()
    {
        _speedLimitDisabled = true;
        yield return new WaitForSeconds(1f);
        _speedLimitDisabled = false;
    }

    [ServerRpc]
    private void SyncDataServerRpc(NetworkConnection conn, ActionState aState, GroundedState Gstate)
    {
        foreach(NetworkObject obj in  conn.Objects)
        {
            if(obj.gameObject.tag == "Player")
            {
                GetComponent<PlayerState>().aState = aState;
                GetComponent<PlayerState>().gState = Gstate;
            }
        }
    }

    private void TimeManager_OnTick()
    {
        if (base.IsOwner)
        {
            Reconciliation(default, false);
            BuildMoveData(out MoveData md);
            Move(md, false);

            TrySpawnBullet();
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
        Vector3 grappleVelocity = _velocityToSet;
        Vector3 wallNormal = _wallNormal;
        Vector3 wallForward = _wallForward;

        md = new MoveData(_playerState.aState, _playerState.gState, moveHorizontal, moveVertical, dashDirection, grappleVelocity, wallNormal, wallForward, _wallLeft, _didWallJump);
        if(_playerState.aState == ActionState.Jumping || _playerState.aState == ActionState.Dashing) _playerState.aState = ActionState.Passive;
        _didWallJump = false;
    }

    private void TrySpawnBullet()
    {
        if(_shootLaser)
        {
            _shootLaser = false;

            if (_laserShooter.LaserPrefab == null || _laserShooter.LaserPrefabVisual == null)
            {
                Debug.Log("Prefab is null");
                return;
            }
            if(!_isTestPlayer) UnityEngine.SceneManagement.SceneManager.SetActiveScene(currentScene);
            
            NetworkObject Bullet = Instantiate(_laserShooter.LaserPrefab, _laserShooter.Eye.position + (_laserShooter.Cam.forward * 1f), Quaternion.Euler(_laserShooter.Cam.transform.eulerAngles.x + 90, _laserShooter.Cam.transform.eulerAngles.y, _laserShooter.Cam.transform.eulerAngles.z));
            NetworkObject BulletVisual = Instantiate(_laserShooter.LaserPrefabVisual, _laserShooter.Muzzle.position + (_laserShooter.Cam.forward * 1f), Quaternion.Euler(_laserShooter.Cam.transform.eulerAngles.x + 90, _laserShooter.Cam.transform.eulerAngles.y, _laserShooter.Cam.transform.eulerAngles.z));

            Laser_Bullet predictedBullet = Bullet.GetComponent<Laser_Bullet>();
            Laser_BulletVisual predictedBulletVisual = BulletVisual.GetComponent<Laser_BulletVisual>();
            predictedBullet.SetStartingForce(_laserShooter.Cam.forward * _laserShooter.laserSpeed);
            predictedBulletVisual.SetStartingForce(_laserShooter.Cam.forward * _laserShooter.laserSpeed);

            base.Spawn(Bullet, base.Owner);
            base.Spawn(BulletVisual, base.Owner);

            predictedBullet.PlayerConn = NetworkObject.LocalConnection;
            SetPlayerBulletRpc(Bullet, BulletVisual, NetworkObject.LocalConnection);
        }
    }

    [ServerRpc]
    private void SetPlayerBulletRpc(NetworkObject obj1, NetworkObject obj2, NetworkConnection playerConn)
    {
        foreach(var scene in playerConn.Scenes)
        {
            if (scene.name != "Lobbies") { UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(obj1.gameObject, scene); UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(obj2.gameObject, scene); }
        }
        
        if(obj1 != null) obj1.GetComponent<Laser_Bullet>().PlayerConn = playerConn;
    }

    private void AddGravity()
    {
        if (_playerState.gState == GroundedState.Grounded)
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
        if(md.aState == ActionState.Jumping && md.gState == GroundedState.Grounded)
        {
            _rb.AddForce(transform.up * _jumpForce, ForceMode.Impulse);
            if(md.aState == ActionState.Jumping)
            {
                _playerState.aState = ActionState.Passive;
            }
            _playerAudioManager.Play("JetpackBurst");
            if (base.IsServer) SyncSoundRpc(gameObject, "JetpackBurst");
        }

        //dash
        if(md.aState == ActionState.Dashing && md.gState == GroundedState.InAir)
        {
            _rb.AddForce(md.DashDirection * _dashForce, ForceMode.Impulse);
            _playerAudioManager.Play("DashWhoosh");
            if (base.IsServer) SyncSoundRpc(gameObject, "DashWhoosh");
            _playerAudioManager.Play("JetpackBurst");
            if (base.IsServer) SyncSoundRpc(gameObject, "JetpackBurst");
            _allowDash = false;
        }

        //grapple
        if(md.aState == ActionState.Grappling)
        {
            _rb.velocity = md.GrappleVelocity;

        }

        //wallrun
        if(md.aState == ActionState.WallRunning)
        {
            _rb.useGravity = false;
            _rb.velocity = new Vector3(_rb.velocity.x, 0f, _rb.velocity.z);

            if ((transform.forward - md.WallForward).magnitude > (transform.forward - -md.WallForward).magnitude)
            {
                md.WallForward -= md.WallForward;
            }

            ////forward force
            _rb.AddForce(md.WallForward * _wallRunner.wallRunForce, ForceMode.Force);

            ////upward force
            _rb.AddForce(Vector3.up * _wallRunner.upwardForce, ForceMode.Force);

            ////push to wall force
            if (md.WallLeft) _rb.AddForce(-_wallNormal * 100, ForceMode.Force);

            ////wallrun jump
            if (md.WallJump)
            {
                _rb.AddForce(transform.forward * _wallJumpForward, ForceMode.Impulse);
                _rb.AddForce(transform.up * _wallJumpUp, ForceMode.Impulse);
                _rb.AddForce(_wallNormal * _wallJumpForce , ForceMode.Impulse);
                if (md.aState == ActionState.Jumping)
                {
                    _playerState.aState = ActionState.Passive;
                }
                _playerAudioManager.Play("JetpackBurst");
                if (base.IsServer) SyncSoundRpc(gameObject, "JetpackBurst");
            }
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
        if (base.Owner.IsLocalClient)
        {
            if (_playerState.gState == GroundedState.InAir) _animator.SetBool("inAir", true);
            else _animator.SetBool("inAir", false);
        }
    }

    //handle impact detection
    private void OnCollisionEnter(Collision collision)
    {
        if ((collision.gameObject.layer == 8 || collision.gameObject.layer == 10) && oldVel.y < -5f && _impactDelay)
        {
            GetComponentInChildren<GunPositioner>().AnimateLand();
            _netAnimator.SetTrigger("land");
            _impactDelay = false;
        }
    }

    [ObserversRpc]
    private void SyncSoundRpc(GameObject obj, string sound)
    {
        obj.GetComponent<PlayerAudioManager>().Play(sound);
    }

    private void OnCollisionStay(Collision collision)
    {
        if(collision.gameObject.layer == 7 || collision.gameObject.layer == 8 ||  collision.gameObject.layer == 9 || collision.gameObject.layer == 10)
        {
            _allowDash = true;
        }
    }

    public void jumpToPosition(Vector3 targetPosition, float trajectoryHeight)
    {
        _velocityToSet = CalculateJumpVelocity(transform.position, targetPosition, trajectoryHeight);
        Invoke(nameof(SetVelocity), 0.1f);
    }

    public Vector3 CalculateJumpVelocity(Vector3 startPoint, Vector3 endPoint, float trajectoryHeight)
    {
        float gravity = Physics.gravity.y;
        float displacementY = endPoint.y - startPoint.y;
        Vector3 displacementXZ = new Vector3(endPoint.x - startPoint.x, 0f, endPoint.z - startPoint.z);

        Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * gravity * trajectoryHeight);
        Vector3 velocityXZ = (displacementXZ / (Mathf.Sqrt(-2 * trajectoryHeight / gravity)
            + Mathf.Sqrt(2 * (displacementY - trajectoryHeight) / gravity))) * _grappleSpeed;

        return velocityXZ + velocityY;
    }

    
    private void SetVelocity()
    {
        _playerState.aState = ActionState.Grappling;
    }

    private void SpeedControll()
    {
        if (_speedLimitDisabled) return;
        Vector3 flatVel = _rb.velocity;

        if (flatVel.magnitude > _speedLimit)
        {
            Vector3 limitedVel = flatVel.normalized * _speedLimit;
            _rb.velocity = limitedVel;
        }
    }
}