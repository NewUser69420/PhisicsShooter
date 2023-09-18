using FishNet.Object;
using UnityEngine;

public class CameraWorker : NetworkBehaviour
{
    private float xRotation;
    private float yRotation;
    [SerializeField] private float sensitivity;
    private PlayerControlls playerControlls;
    [SerializeField] private Transform Player;
    private bool initialized = false;

    public override void OnStartNetwork()
    {
        if (base.IsServer) return;
        transform.Find("Cam").gameObject.SetActive(true);

        playerControlls = new PlayerControlls();
        playerControlls.Enable();

        initialized = true;
    }

    private void Update()
    {
        if (!initialized) return;
        
        //look
        float mouseX = playerControlls.OnFoot.Look.ReadValue<Vector2>().x;
        float mouseY = playerControlls.OnFoot.Look.ReadValue<Vector2>().y;

        xRotation -= (mouseY * sensitivity);
        xRotation = Mathf.Clamp(xRotation, -70, 70);
        yRotation -= (mouseX * sensitivity);

        Debug.Log($"{xRotation} + {yRotation}");
        transform.Find("Cam").transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        Player.rotation = Quaternion.Euler(0f, yRotation * -1, 0f);

        SyncWithServerRpc(this.gameObject, yRotation, xRotation);
    }

    [ServerRpc]
    private void SyncWithServerRpc(GameObject obj, float _yRotation, float _xRotation)
    {
        obj.transform.rotation = Quaternion.Euler (0f, _yRotation * -1, 0f);
        obj.transform.Find("Cam").transform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
    }
}
