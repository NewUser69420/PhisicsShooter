using FishNet;
using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public NetworkObject KillerPrefab;
    public NetworkObject PhysicsBallPrefab;

    private void Awake()
    {
        if(InstanceFinder.IsServer)
        {
            NetworkObject Killer = Instantiate(KillerPrefab);
            NetworkObject PhysicsBall = Instantiate(PhysicsBallPrefab);

            InstanceFinder.ServerManager.Spawn(Killer);
            InstanceFinder.ServerManager.Spawn(PhysicsBall);
        }
    }
}
