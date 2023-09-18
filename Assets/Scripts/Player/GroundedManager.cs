using UnityEngine;

public class GroundedManager : MonoBehaviour
{
    private PredictedPlayerMover playerScript;

    private void Awake()
    {
        playerScript = GetComponent<PredictedPlayerMover>();
    }

    private void OnCollisionStay(Collision collision)
    {
        playerScript.serverGrounded = true;
    }

    private void OnCollisionExit(Collision collision)
    {
        playerScript.serverGrounded = false;
    }
}
