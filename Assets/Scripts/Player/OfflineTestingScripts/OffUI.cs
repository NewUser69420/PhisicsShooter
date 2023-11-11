using static PlayerState;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class OffUI : MonoBehaviour
{
    [SerializeField] private Slider dashSlider;
    [SerializeField] private Slider grappleSlider;
    [SerializeField] private TMP_Text speed;

    private OfflinePlayerMovement playerController;
    private OffGrappling grappling;
    private PlayerState playerState;

    private PlayerControlls playerControlls;

    [System.NonSerialized] public float dashTimer;

    private void Start()
    {
        playerControlls = new PlayerControlls();
        playerControlls.Enable();

        playerController = GetComponentInParent<OfflinePlayerMovement>();
        grappling = GetComponentInParent<OffGrappling>();
        playerState = GetComponentInParent<PlayerState>();
        dashTimer = playerController._dashReset;
    }

    private void Update()
    {
        //calculate
        if (dashTimer < playerController._dashReset)
        {
            dashTimer += Time.deltaTime;
        }
        else
        {
            dashTimer = playerController._dashReset;
        }

        if (playerState.aState == ActionState.Dashing && playerState.gState == GroundedState.InAir)
        {
            dashTimer = 0;
        }

        //update ui
        speed.text = Mathf.Round(playerController._rb.velocity.magnitude).ToString();
        dashSlider.value = Mathf.InverseLerp(0, playerController._dashReset, dashTimer);
        grappleSlider.value = Mathf.InverseLerp(0, grappling.grapplingValueMax, grappling.grapplingValue);
    }
}
