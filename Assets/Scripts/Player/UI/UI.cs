using FishNet;
using FishNet.Object;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static PlayerState;

public class UI : NetworkBehaviour
{
    private PredictedPlayerController playerController;
    private PlayerControlls playerControlls;
    private PlayerState playerState;
    private Grappling grappling;

    [SerializeField] private TMP_Text speed;
    [SerializeField] private TMP_Text deathCounter;
    [SerializeField] private TMP_Text killCounter;
    [SerializeField] private Slider dashSlider;
    [SerializeField] private Slider grappleSlider;
    [SerializeField] private GameObject ScoreBoard;

    [System.NonSerialized] public float deathCounterValue = 0;
    [System.NonSerialized] public float killCounterValue = 0;

    private float dashTimer;

    private void Awake()
    {
        playerControlls = new PlayerControlls();
        playerControlls.Enable();
        
        playerController = GetComponentInParent<PredictedPlayerController>();
        playerState = GetComponentInParent<PlayerState>();
        grappling = GetComponentInParent<Grappling>();
        dashTimer = playerController._dashReset;
    }

    private void Update()
    {
        if (!base.IsOwner) return;
        if (!playerController._activated) return;

        //set scoreboard active
        if(playerControlls.OnFoot.ScoreBoard.IsPressed())
        {
            ScoreBoard.SetActive(true);
            foreach (Transform child in ScoreBoard.transform.Find("Holder"))
            {
                if(!child.gameObject.activeSelf) child.gameObject.SetActive(true);
            }
        }
        else
        {
            ScoreBoard.SetActive(false);
        }
        
        //calculate
        if(dashTimer < playerController._dashReset)
        {
            dashTimer += Time.deltaTime;
        }
        else
        {
            dashTimer = playerController._dashReset;
        }

        if(playerState.aState == ActionState.Dashing && playerState.gState == GroundedState.InAir)
        {
            dashTimer = 0;
        }

        //update ui
        speed.text = Mathf.Round(playerController._rb.velocity.magnitude).ToString();
        dashSlider.value = Mathf.InverseLerp(0, playerController._dashReset, dashTimer);
        grappleSlider.value = Mathf.InverseLerp(0, grappling.grapplingValueMax, grappling.grapplingValue);
        deathCounter.text = deathCounterValue.ToString();
        killCounter.text = killCounterValue.ToString();
    }
}
