using FishNet.Connection;
using FishNet.Object;
using UnityEngine;
using static PlayerState;

public class GroundedManager : NetworkBehaviour
{
    private PlayerState playerState;
    [SerializeField] private feet footLeft;
    [SerializeField] private feet footRight;

    private float groundedTimer;
    [SerializeField] private float groundedTimerMax;

    private void Awake()
    {
        playerState = GetComponent<PlayerState>();
    }

    private void OnCollisionStay(Collision collision)
    {
        if(collision.gameObject.layer == 8 || collision.gameObject.layer == 10)
        {
            playerState.gState = GroundedState.Grounded;
            footRight.groundedTimer = groundedTimerMax;
            footLeft.groundedTimer = groundedTimerMax;
        }
    }
}
