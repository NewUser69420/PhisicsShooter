using FishNet.Object;
using Unity.VisualScripting;
using UnityEngine;
using static PlayerState;

public class WallRunning : NetworkBehaviour
{
    [Header("Wallrunning")]
    public LayerMask whatIsWall;
    public LayerMask whatIsGround;
    public float wallRunForce;
    public float upwardForce;
    public float wallRunTimer;
    public float wallRunTimerMax;
    public bool canWallRun = true;

    [Header("Input")]
    [System.NonSerialized] public float horizontalInput;
    [System.NonSerialized] public float verticalInput;

    [Header("Detection")]
    public float wallCheckDistance;
    public float minJumpHeight;
    [System.NonSerialized] public RaycastHit leftWallhit;
    [System.NonSerialized] public RaycastHit rightWallhit;
    public bool wallLeft;
    public bool wallRight;
    //private bool isWallrunning;
    private string newWallName;
    private string oldWallName;

    [Header("References")]
    private PredictedPlayerController pm;
    private PlayerState playerState;
    private PlayerControlls playerControlls;

    public override void OnStartNetwork()
    {
        pm = GetComponent<PredictedPlayerController>();
        
        playerState = GetComponent<PlayerState>();
        
        playerControlls = new PlayerControlls();
        playerControlls.Enable();
    }

    private void Update()
    {
        if(wallRunTimer > 0 && playerState.aState == ActionState.WallRunning)
        {
            wallRunTimer -= Time.deltaTime;
        }

        if(wallRunTimer <= 0)
        {
            StopWallRun();
            wallRunTimer = wallRunTimerMax;
            canWallRun = false;
        }

        if(playerState.aState != ActionState.WallRunning)
        {
            wallRunTimer = wallRunTimerMax;
        }

        if(newWallName != oldWallName)
        {
            ResetAbleToRunOnWall();
        }

        CheckForWall();
        StateMachine();
    }

    private void ResetAbleToRunOnWall()
    {
        wallRunTimer = wallRunTimerMax;
        pm._didWallJump = false;
        canWallRun = true;
    }

    private void CheckForWall()
    {
        wallRight = Physics.Raycast(transform.position, transform.right, out rightWallhit, wallCheckDistance, whatIsWall);
        wallLeft = Physics.Raycast(transform.position, -transform.right, out leftWallhit, wallCheckDistance, whatIsWall);

        if (wallRight)
        {
            if (newWallName == null)
            {
                newWallName = rightWallhit.collider.gameObject.name;
            }
            else
            {
                oldWallName = newWallName;
                newWallName = rightWallhit.collider.gameObject.name;
            }
        }
        else if (wallLeft)
        {
            if (newWallName == null)
            {
                newWallName = leftWallhit.collider.gameObject.name;
            }
            else
            {
                oldWallName = newWallName;
                newWallName = leftWallhit.collider.gameObject.name;
            }
        }

        if (wallRight || wallLeft)
        {
            pm._canDashOnWallJump = true;
        }
    }

    private bool AboveGround()
    {
        return !Physics.Raycast(transform.position, Vector3.down, minJumpHeight, whatIsGround);
    }

    private void StateMachine()
    {
        // Getting Inputs
        horizontalInput = playerControlls.OnFoot.Movement.ReadValue<Vector2>().x;
        verticalInput = playerControlls.OnFoot.Movement.ReadValue<Vector2>().y;

        // State 1 - Wallrunning
        if ((wallLeft || wallRight) && verticalInput > 0 && AboveGround() && canWallRun)
        {
            if (playerState.aState != ActionState.WallRunning)
                StartWallRun();
        }

        // State 3 - None
        else
        {
            if (playerState.aState == ActionState.WallRunning)
                StopWallRun();
        }
    }

    private void StartWallRun()
    {
        if (playerState.aState != ActionState.WallRunning)
        {
            //FindObjectOfType<AudioManger>().Play("Wallrun Slide");
        }
        playerState.aState = ActionState.WallRunning;
        //isWallrunning = true;
    }

    private void StopWallRun()
    {
        if (playerState.aState == ActionState.WallRunning)
        {
            //FindObjectOfType<AudioManger>().Stop("Wallrun Slide");
        }
        playerState.aState = ActionState.Passive;
        //isWallrunning = false;
    }
}
