using FishNet.Example.Scened;
using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrenadeThrower : NetworkBehaviour
{
    private PlayerControlls playerControlls;
    private PredictedPlayerController playerController;

    private bool justThrew = false;

    [System.NonSerialized] public float resetTime;
    [System.NonSerialized] public float throwSpeed;

    [System.NonSerialized] public NetworkObject GrenadePrefab;
    [System.NonSerialized] public NetworkObject GrenadePrefabVisual;
    [System.NonSerialized] public Transform Muzzle;
    [System.NonSerialized] public Transform Cam;

    public void DoStart()
    {
        playerControlls = new PlayerControlls();
        playerControlls.Enable();

        playerController = GetComponent<PredictedPlayerController>();
        playerController._grenadeThrower = this;

        Muzzle = transform.Find("Cam/FPItems/FPGun/Muzzle");
        Cam = transform.Find("Cam");

        GrenadeType();
    }

    protected virtual void GrenadeType()
    {
        //overide in specific grenade script
    }

    private void Update()
    {
        if (playerControlls == null || playerController == null) return;
        if (playerControlls.OnFoot.Grenade.WasPressedThisFrame() && playerController._activated)
        {
            ThrowGrenade();
        }
    }

    private void ThrowGrenade()
    {
        if (!justThrew)
        {
            justThrew = true;

            playerController._throwGrenade = true;

            StartCoroutine(ResetGrenade());
        }
    }

    IEnumerator ResetGrenade()
    {
        yield return new WaitForSeconds(resetTime);
        justThrew = false;
    }
}
