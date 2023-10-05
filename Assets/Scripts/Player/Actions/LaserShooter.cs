using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserShooter : NetworkBehaviour
{
    public Transform Cam;
    public Transform Eye;
    public Transform Muzzle;

    [System.NonSerialized] public NetworkObject LaserPrefab;
    [System.NonSerialized] public NetworkObject LaserPrefabVisual;

    [System.NonSerialized] public float resetTime;
    [System.NonSerialized] public float laserSpeed;

    private PlayerControlls playerControlls;
    private PredictedPlayerController playerController;

    private bool justShot;
    
    public override void OnStartNetwork()
    {
        playerControlls = new PlayerControlls();
        playerControlls.Enable();

        playerController = GetComponent<PredictedPlayerController>();
        
        LaserType();
    }

    protected virtual void LaserType()
    {
        //overwritten in laser specific script
    }

    private void Update()
    {
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

            //FindObjectOfType<AduioManager>().Play("Laser1");

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
