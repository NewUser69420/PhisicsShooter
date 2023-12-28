using FishNet.Component.Animating;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PlayerState;

public class OfflinePlayerMovement : MonoBehaviour
{
    [SerializeField] private float _speed;
    [SerializeField] private float _jumpForce;
    [SerializeField] private float _grappleSpeed;
    [SerializeField] private float _jumpReset;
    [SerializeField] private float _dashForce;
    [SerializeField] private LayerMask _whatIsGround;
    [SerializeField] private float _wallJumpForce;
    [SerializeField] private float _wallJumpUp;
    [SerializeField] private float _wallJumpForward;

    [System.NonSerialized] public bool _didWallJump;
    [System.NonSerialized] public bool _canDashOnWallJump;
    [System.NonSerialized] public bool _shootLaser;
    [System.NonSerialized] public Rigidbody _rb;

    private PlayerControlls _playerControlls;
    private PlayerState _playerState;
    private OffWallrunning _wallRunner;
    private LaserShooter _laserShooter;
    private Transform _Cam;
    private RaycastHit _hit;
    private Vector3 _dashDirection;
    private Vector3 _wallNormal;
    private Vector3 _wallForward;
    private Vector3 oldVel = Vector3.zero;
    private Vector3 newVel = Vector3.zero;
    private Vector3 _velocityToSet;
    private GroundedState newGState;
    private GroundedState oldGState;
    private float _nextJumpTime;
    private float _nextDashTime;
    private float _velTimer;
    private float _velTimerMax = 0.3f;
    private bool _allowDash = true;
    private bool _allowWallJump = true;
    private bool _wallLeft;
    private bool _speedLimitDisabled;

    public float _dashReset;

    private void Start()
    {
        _playerControlls = new PlayerControlls();
        _playerControlls.Enable();

        _rb = GetComponent<Rigidbody>();
        _playerState = GetComponent<PlayerState>();
        _wallRunner = GetComponent<OffWallrunning>();

        _Cam = transform.Find("Cam");
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
        if (_playerControlls.OnFoot.Jump.IsPressed())
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

        //velocityCheck
        if (_velTimer > 0)
        {
            _velTimer -= Time.deltaTime;
        }
        else
        {
            _velTimer = _velTimerMax;
            oldVel = _rb.velocity;
        }

        oldGState = newGState;

        Physics.Simulate(Time.fixedDeltaTime);
    }

    private void FixedUpdate()
    {
        DoMove();
        ResetMovement();
    }

    private void ResetMovement()
    {
        if (_playerState.aState == ActionState.Jumping || _playerState.aState == ActionState.Dashing) _playerState.aState = ActionState.Passive;
        _didWallJump = false;
    }

    private void DoMove()
    {
        //move
        Vector3 direction = (Vector3)(_playerControlls.OnFoot.Movement.ReadValue<Vector2>().x * transform.right + _playerControlls.OnFoot.Movement.ReadValue<Vector2>().y * transform.forward);
        _rb.AddForce(direction * _speed);

        //jump
        if (_playerState.aState == ActionState.Jumping && _playerState.gState == GroundedState.Grounded)
        {
            _rb.AddForce(transform.up * _jumpForce, ForceMode.Impulse);
            if (_playerState.aState == ActionState.Jumping)
            {
                _playerState.aState = ActionState.Passive;
            }
        }

        //dash
        if (_playerState.aState == ActionState.Dashing && _playerState.gState == GroundedState.InAir)
        {
            _rb.AddForce(_dashDirection * _dashForce, ForceMode.Impulse);
            _allowDash = false;
            GetComponentInChildren<OffUI>().dashTimer = 0;
        }

        //grapple
        if (_playerState.aState == ActionState.Grappling)
        {
            _rb.velocity = _velocityToSet;

        }

        //wallrun
        if (_playerState.aState == ActionState.WallRunning)
        {
            _rb.useGravity = false;
            _rb.velocity = new Vector3(_rb.velocity.x, 0f, _rb.velocity.z);

            if ((transform.forward - _wallForward).magnitude > (transform.forward - -_wallForward).magnitude)
            {
                _wallForward -= _wallForward;
            }

            //forward force
            _rb.AddForce(_wallForward * _wallRunner.wallRunForce, ForceMode.Force);

            //upward force
            _rb.AddForce(Vector3.up * _wallRunner.upwardForce, ForceMode.Force);

            //push to wall force
            if (_wallLeft) _rb.AddForce(-_wallNormal * 100, ForceMode.Force);

            //wallrun jump
            if (_didWallJump && _allowWallJump)
            {
                _rb.AddForce(_Cam.forward * _wallJumpForward, ForceMode.Impulse);
                _rb.AddForce(transform.up * _wallJumpUp, ForceMode.Impulse);
                _rb.AddForce(_wallNormal * _wallJumpForce, ForceMode.Impulse);
                if (_playerState.aState == ActionState.Jumping)
                {
                    _playerState.aState = ActionState.Passive;
                }
                StartCoroutine(Wait());
            }
        }

        //do gravity
        if (_playerState.gState == GroundedState.Grounded)
        {
            _rb.AddForce(Physics.gravity * 2f);
        }
        else
        {
            _rb.AddForce(Physics.gravity * 85f);
        }
    }

    IEnumerator Wait()
    {
        _allowWallJump = false;
        yield return new WaitForSeconds(0.2f);
        _allowWallJump = true;
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.layer == 7 || collision.gameObject.layer == 8 || collision.gameObject.layer == 9 || collision.gameObject.layer == 10)
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
}
