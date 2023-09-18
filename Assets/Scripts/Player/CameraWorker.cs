using FishNet.Connection;
using FishNet.Example.Scened;
using FishNet.Object;
using System;
using Unity.VisualScripting;
using UnityEngine;

public class CameraWorker : NetworkBehaviour
{
    private PlayerControlls playerControlls;
    private GameObject player;
    
    private Vector2 inputRaw;
    private float xRotation;
    private float yRotation;

    public float sensitivity;

    public override void OnSpawnServer(NetworkConnection conn)
    {
        playerControlls = new PlayerControlls();
        playerControlls.Enable();

        foreach (var obj in conn.Objects)
        {
            if (obj.gameObject.tag == "Player")
            {
                player = obj.gameObject;
                SetPlayerClientRpc(conn, this.gameObject, player);
            }
        }
    }

    [TargetRpc]
    private void SetPlayerClientRpc(NetworkConnection _conn, GameObject _obj, GameObject _player)
    {
        var _urmom = _obj.GetComponent<CameraWorker>();
        _urmom.playerControlls = new PlayerControlls();
        _urmom.playerControlls.Enable();
        _urmom.transform.Find("Cam").gameObject.SetActive(true);
        _urmom.transform.Find("CMCam").gameObject.SetActive(true);
        _urmom.player = _player;
    }

    private void Update()
    {
        CollectLookingData();
        DoLooking();
        SetPos();
    }

    private void SetPos()
    {
        Vector3 playerTransfrom = player.GetComponent<Transform>().position;
        transform.position = new Vector3(playerTransfrom.x, playerTransfrom.y + 1.81f, playerTransfrom.z);
    }

    private void CollectLookingData()
    {
        inputRaw = playerControlls.OnFoot.Look.ReadValue<Vector2>();
    }

    private void DoLooking()
    {
        if (!player.GetComponent<PredictedPlayerMover>().activated) return;

        float mouseX = inputRaw.x;
        float mouseY = inputRaw.y;

        xRotation -= (mouseY * sensitivity);
        xRotation = Mathf.Clamp(xRotation, -70, 70);
        yRotation -= (mouseX * sensitivity);

        transform.Find("CMCam").transform.rotation = Quaternion.Euler(xRotation, yRotation * -1, 0);
        transform.rotation = Quaternion.Euler(0, yRotation * -1, 0);
    }
}
