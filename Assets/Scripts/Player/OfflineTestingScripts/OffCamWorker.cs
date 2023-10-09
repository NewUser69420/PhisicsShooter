using UnityEngine;

public class OffCamWorker : MonoBehaviour
{
    [System.NonSerialized] public bool isTestPlayer;

    private float xRotation;
    private float yRotation;
    [SerializeField] private float sensitivity;
    private PlayerControlls playerControlls;
    [SerializeField] private Transform Player;
    [System.NonSerialized] public bool initialized = false;

    public void Start()
    {   
        playerControlls = new PlayerControlls();
        playerControlls.Enable();
        initialized = true;

        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        if (!initialized) return;

        //look
        float mouseX = playerControlls.OnFoot.Look.ReadValue<Vector2>().x;
        float mouseY = playerControlls.OnFoot.Look.ReadValue<Vector2>().y;

        xRotation -= (mouseY * sensitivity * 0.5f);
        xRotation = Mathf.Clamp(xRotation, -70, 70);
        yRotation -= (mouseX * sensitivity * 0.5f);

        transform.Find("Cam").transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        Player.rotation = Quaternion.Euler(0f, yRotation * -1, 0f);
    }
}
