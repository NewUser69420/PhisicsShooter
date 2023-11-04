using FishNet.Connection;
using FishNet.Demo.AdditiveScenes;
using FishNet.Managing.Client;
using FishNet.Managing.Scened;
using FishNet.Object;
using System.Collections;
using TMPro;
using TMPro.Examples;
using UnityEngine;
using UnityEngine.ProBuilder.Shapes;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static PlayerState;

public class UI : NetworkBehaviour
{
    private PredictedPlayerController playerController;
    private PlayerControlls playerControlls;
    private PlayerState playerState;
    private Grappling grappling;
    private CameraWorker cam;

    [SerializeField] private TMP_Text speed;
    [SerializeField] private TMP_Text deathCounter;
    [SerializeField] private TMP_Text killCounter;
    [SerializeField] private Slider dashSlider;
    [SerializeField] private Slider grappleSlider;
    [SerializeField] private GameObject ScoreBoard;
    [SerializeField] private GameObject ESC;
    [SerializeField] private GameObject Crosshair;

    [System.NonSerialized] public int deathCounterValue = 0;
    [System.NonSerialized] public int killCounterValue = 0;

    private float dashTimer;
    private bool esc;

    private void Awake()
    {
        playerControlls = new PlayerControlls();
        playerControlls.Enable();
        
        playerController = GetComponentInParent<PredictedPlayerController>();
        playerState = GetComponentInParent<PlayerState>();
        grappling = GetComponentInParent<Grappling>();
        cam = GetComponentInParent<CameraWorker>();
        dashTimer = playerController._dashReset;

        transform.Find("Score/T1Value").GetComponent<TMP_Text>().text = "0";
        transform.Find("Score/T2Value").GetComponent<TMP_Text>().text = "0";
    }

    private void Update()
    {
        if (!base.IsOwner) return;

        //toggle ESC
        if (playerControlls.MainMenu.GoBackOne.WasPressedThisFrame())
        {
            esc = !esc;
            switch (esc)
            {
                case true:
                    ESC.SetActive(true);
                    Crosshair.SetActive(false);
                    Cursor.lockState = CursorLockMode.None;
                    playerController._activated = false;
                    cam.active = false;
                    break;
                case false:
                    ESC.SetActive(false);
                    Crosshair.SetActive(true);
                    Cursor.lockState = CursorLockMode.Locked;
                    playerController._activated = true;
                    cam.active = true;
                    break;
            }
        }

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

    public void GoBackToMM()
    {
        //going back
        Debug.Log("Going Back To Main Menu");
        FindObjectOfType<AudioManger>().Play("click2");

        FindObjectOfType<ServerMenu>(true).gameObject.SetActive(true);
        FindObjectOfType<MainMenu>(true).gameObject.SetActive(true);

        GoBackToLobbiesServer(LocalConnection);
    }

    [ServerRpc]
    private void GoBackToLobbiesServer(NetworkConnection conn)
    {
        GameObject Player = null;
        foreach (var obj in conn.Objects) if (obj.CompareTag("Player")) Player = obj.gameObject;

        FindObjectOfType<ServerMenu>().StartPlayerReset(Player.GetComponent<ScoreTracker>().playerName, conn);

        SceneUnloadData sud = new SceneUnloadData(gameObject.scene);
        base.SceneManager.UnloadConnectionScenes(conn, sud);
    }

    public void Quit()
    {
        Debug.Log("Quiting...");
        FindAnyObjectByType<AudioManger>().Play("click3");
        Application.Quit();
    }
}
