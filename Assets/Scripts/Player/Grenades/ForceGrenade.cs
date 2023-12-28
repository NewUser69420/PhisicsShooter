using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForceGrenade : GrenadeThrower
{
    protected override void GrenadeType()
    {
        GrenadePrefab = Resources.Load("ForceGrenade", typeof(NetworkObject)) as NetworkObject;
        GrenadePrefabVisual = Resources.Load("ForceGrenadeVisual", typeof(NetworkObject)) as NetworkObject;

        resetTime = 3f;
        throwSpeed = 30;
    }

    private void Start()
    {
        DoStart();
    }
}
