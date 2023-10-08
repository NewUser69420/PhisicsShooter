using FishNet;
using FishNet.Demo.AdditiveScenes;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Transporting;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SetupTestPlayer : MonoBehaviour
{
    private GameObject Player;
    [SerializeField] private Vector3 Spawnposition;


    private void Awake()
    {
        InstanceFinder.ServerManager.OnServerConnectionState += OnServerChange;
        InstanceFinder.ClientManager.OnAuthenticated += OnClientConnect;
        InstanceFinder.ServerManager.StartConnection();

        transform.Find("SpawnPos").position = Spawnposition;
    }

    private void OnServerChange(ServerConnectionStateArgs args)
    {
        if(args.ConnectionState == LocalConnectionState.Started)
        {
            InstanceFinder.ClientManager.StartConnection();
        }
    }

    private void OnClientConnect()
    {
        Invoke(nameof(SetupClient), 0.1f);
    }

    private void SetupClient()
    {
        Player = GameObject.FindGameObjectWithTag("Player");
        Player.GetComponent<InitializePlayer>().isTestPlayer = true;
        Player.GetComponent<CameraWorker>().isTestPlayer = true;
        Player.GetComponent<PredictedPlayerController>()._isTestPlayer = true;
        Player.GetComponent<PredictedPlayerController>()._activated = true;
        Player.GetComponent<CameraWorker>().Initialize(Player.GetComponent<NetworkObject>().Owner);
        Invoke(nameof(InitializeClient), 0.5f);
    }

    private void InitializeClient()
    {
        Player.GetComponent<CameraWorker>().initialized = true;

        Cursor.lockState = CursorLockMode.Locked;
    }
}
