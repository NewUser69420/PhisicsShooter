using FishNet;
using FishNet.Transporting;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetupTestPlayer : MonoBehaviour
{
    private void Awake()
    {
        InstanceFinder.ServerManager.OnServerConnectionState += OnServerChange;
        InstanceFinder.ServerManager.StartConnection();
    }

    private void OnServerChange(ServerConnectionStateArgs args)
    {
        if(args.ConnectionState == LocalConnectionState.Started)
        {
            InstanceFinder.ClientManager.StartConnection();
        }
    }
}
