using FishNet.Connection;
using FishNet.Object;
using UnityEngine;
using static PlayerState;

public class GroundedManager : NetworkBehaviour
{
    private PlayerState playerState;

    private float groundedTimer;
    [SerializeField] private float groundedTimerMax;

    private void Awake()
    {
        playerState = GetComponent<PlayerState>();
    }

    private void Update()
    {
        if(groundedTimer > 0)
        {
            groundedTimer -= Time.deltaTime;
        }
        else
        {
            playerState.gState = GroundedState.InAir;
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if(collision.gameObject.layer == 8 || collision.gameObject.layer == 10)
        {
            playerState.gState = GroundedState.Grounded;
            groundedTimer = groundedTimerMax;
        }
    }
}
