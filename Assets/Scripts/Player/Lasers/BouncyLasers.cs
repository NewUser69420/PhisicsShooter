using FishNet.Object;
using UnityEngine;

public class BouncyLasers : LaserShooter
{
    protected override void LaserType()
    {
        LaserPrefab = Resources.Load("LaserBouncy", typeof(NetworkObject)) as NetworkObject;
        LaserPrefabVisual = Resources.Load("LaserBouncyVisual", typeof(NetworkObject)) as NetworkObject;

        resetTime = 0.05f;
        laserSpeed = 100;
    }

    private void Awake()
    {
        DoStart();
    }
}
