using FishNet.Example.Scened;
using System;
using UnityEngine;

public class CameraWorker : MonoBehaviour
{
    private PlayerControlls playerControlls;
    private TempPredictedPlayerController player;
    
    private Vector2 inputRaw;
    private float xRotation;
    private float yRotation;

    private void Start()
    {
        playerControlls = new PlayerControlls();
        playerControlls.Enable();

        player = GameObject.Find("Player(Clone)").GetComponent<TempPredictedPlayerController>();
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
        transform.position = new Vector3(playerTransfrom.x, playerTransfrom.y + 1.81f, player.GetComponent<Transform>().position.z);
    }

    private void CollectLookingData()
    {
        inputRaw = playerControlls.OnFoot.Look.ReadValue<Vector2>();
    }

    private void DoLooking()
    {
        if (!player.activated) return;

        float mouseX = inputRaw.x;
        float mouseY = inputRaw.y;

        xRotation -= -1 * (mouseY * player.sensitivity);
        xRotation = Mathf.Clamp(xRotation, -70, 70);
        yRotation -= (mouseX * player.sensitivity);

        transform.Find("CMCam").transform.rotation = Quaternion.Euler(xRotation * -1, yRotation * -1, 0);
    }
}
