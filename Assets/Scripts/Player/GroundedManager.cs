using FishNet.Connection;
using FishNet.Object;
using UnityEngine;

public class GroundedManager : NetworkBehaviour
{
    private PredictedPlayerController playerScript;

    private void Awake()
    {
        playerScript = GetComponent<PredictedPlayerController>();
    }

    private void OnCollisionStay(Collision collision)
    {
        if(collision.gameObject.layer == 8 || collision.gameObject.layer == 10)
        {
            playerScript._grounded = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.layer == 8 || collision.gameObject.layer == 10)
        {
            playerScript._grounded = false;
        }
    }
}
