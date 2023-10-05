using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefaultLasers : LaserShooter
{
    protected override void LaserType(){
        LaserPrefab = Resources.Load("LaserDefault", typeof(NetworkObject)) as NetworkObject;
        LaserPrefabVisual = Resources.Load("LaserDefaultVisual", typeof(NetworkObject)) as NetworkObject;

        resetTime = 0.05f;
        laserSpeed = 100;
    }
}
