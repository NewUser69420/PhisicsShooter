using UnityEngine;

public class GroundedManager : MonoBehaviour
{
    private PredictedPlayerMover playerScript;
    public LayerMask whatIsGround;

    private void Awake()
    {
        playerScript = GetComponent<PredictedPlayerMover>();
    }

    private void OnCollisionStay(Collision collision)
    {
        if(collision.gameObject.layer == whatIsGround) playerScript.serverGrounded = true;
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.layer == whatIsGround) playerScript.serverGrounded = false;
    }
}
