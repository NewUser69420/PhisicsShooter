using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserShooter : NetworkBehaviour
{
    private PlayerControlls playerControlls;
    private PredictedPlayerController playerController;

    [System.NonSerialized] public Transform Cam;
    [System.NonSerialized] public Transform Eye;
    [System.NonSerialized] public Transform Muzzle;
    public NetworkObject LaserPrefab;
    [System.NonSerialized] public NetworkObject LaserPrefabVisual;

    [System.NonSerialized] public float resetTime;
    [System.NonSerialized] public float laserSpeed;

    private bool justShot;

    public void DoStart()
    {
        playerControlls = new PlayerControlls();
        playerControlls.Enable();

        playerController = GetComponent<PredictedPlayerController>();
        playerController._laserShooter = this;

        Cam = transform.Find("Cam");
        Eye = transform.Find("Eye");
        Muzzle = transform.Find("Cam/FPItems/FPGun/Muzzle");

        LaserType();
    }

    protected virtual void LaserType()
    {
        //overwritten in laser specific script
    }

    private void Update()
    {
        if(playerControlls == null || playerController == null) return;
        if(playerControlls.OnFoot.Fire.WasPressedThisFrame() && playerController._activated)
        {
            ShootLaser();
        }
    }

    private void ShootLaser()
    {
        if(!justShot)
        {
            justShot = true;

            playerController._shootLaser = true;

            StartCoroutine(ResetLaser());
        }
    }

    IEnumerator ResetLaser()
    {
        yield return new WaitForSeconds(resetTime);
        justShot = false;
    }
}
