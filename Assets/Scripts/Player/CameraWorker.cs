using FishNet.Connection;
using FishNet.Managing.Scened;
using FishNet.Object;
using UnityEngine;

public class CameraWorker : NetworkBehaviour
{
    [System.NonSerialized] public bool isTestPlayer;
    
    private float xRotation;
    private float yRotation;
    [SerializeField] private float sensitivity;
    private PlayerControlls playerControlls;
    [SerializeField] private Transform Player;
    [System.NonSerialized] public bool initialized = false;
    [System.NonSerialized] public bool active;

    [TargetRpc]
    public void Initialize(NetworkConnection _conn)
    {
        if (Owner.IsLocalClient)
        {
            transform.Find("Cam").GetComponent<Camera>().enabled = true;
            transform.Find("Cam").GetComponent<AudioListener>().enabled = true;
            transform.Find("Cam/FPItems").gameObject.SetActive(true);

            playerControlls = new PlayerControlls();
            playerControlls.Enable();

            if(!isTestPlayer)
            {
                //GameObject.Find("Lobbies").GetComponent<Canvas>().enabled = false;

                initialized = true;
                active = true;
            }
        }
    }

    private void Update()
    {
        if (!initialized || !active) return;
        
        //look
        float mouseX = playerControlls.OnFoot.Look.ReadValue<Vector2>().x;
        float mouseY = playerControlls.OnFoot.Look.ReadValue<Vector2>().y;

        xRotation -= (mouseY * sensitivity * 0.5f);
        xRotation = Mathf.Clamp(xRotation, -70, 70);
        yRotation -= (mouseX * sensitivity * 0.5f);

        transform.Find("Cam").transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        Player.rotation = Quaternion.Euler(0f, yRotation * -1, 0f);

        SyncWithServerRpc(this.gameObject, yRotation, xRotation);
    }

    [ServerRpc]
    private void SyncWithServerRpc(GameObject obj, float _yRotation, float _xRotation)
    {
        obj.transform.rotation = Quaternion.Euler (0f, _yRotation * -1, 0f);
        obj.transform.Find("Cam").transform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);

        SyncWithClientsRpc(obj, _xRotation);
    }

    [ObserversRpc]
    private void SyncWithClientsRpc(GameObject obj, float __xRotation)
    {
        obj.transform.Find("Cam").transform.localRotation = Quaternion.Euler(__xRotation, 0f, 0f);
    }
}
