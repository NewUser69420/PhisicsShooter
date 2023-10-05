using static PlayerState;
using UnityEngine;

public class IK_Hand : MonoBehaviour
{
    [SerializeField] private Transform GrabPoint;
    [SerializeField] private Transform HandPositioner;
    [SerializeField] private LayerMask whatIsWall;
    [SerializeField] private Transform Player;

    private PlayerState playerState;

    private Vector3 velocity;
    private Vector3 velocityy;

    private void Start()
    {
        playerState = GetComponentInParent<PlayerState>();
    }

    void Update()
    {
        switch (gameObject.name)
        {
            case "Right Arm IK":
                RaycastHit hit;
                if (Physics.Raycast(Player.position, Player.right, out hit, 0.7f, whatIsWall))
                {
                    //if (playerState.aState == ActionState.WallRunning)
                    //{
                    HandPositioner.position = Vector3.SmoothDamp(HandPositioner.position, hit.point, ref velocity, 25 * Time.deltaTime);
                    //}
                }
                else
                {
                    HandPositioner.position = Vector3.SmoothDamp(HandPositioner.position, GrabPoint.position, ref velocityy, 15 * Time.deltaTime);
                }
                break;
            case "Left Arm IK":
                RaycastHit hitt;
                if (Physics.Raycast(Player.position, -Player.right, out hitt, 0.7f, whatIsWall))
                {
                    //if (playerState.aState == ActionState.WallRunning)
                    //{
                    HandPositioner.position = Vector3.SmoothDamp(HandPositioner.position, hitt.point, ref velocity, 25 * Time.deltaTime);
                    //}
                }
                else
                {
                    HandPositioner.position = Vector3.SmoothDamp(HandPositioner.position, GrabPoint.position, ref velocityy, 15 * Time.deltaTime);
                }
                break;
        }
    }
}
