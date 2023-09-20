using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefaultLasers : LaserShooter
{
    protected override void LaserType(){
        LaserPrefab = Resources.Load("LaserDefault", typeof(NetworkObject)) as NetworkObject;

        resetTime = 0.3f;
        laserSpeed = 100;
    }
}
