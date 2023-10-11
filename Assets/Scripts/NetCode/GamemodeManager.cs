using FishNet.Connection;
using FishNet.Managing.Scened;
using FishNet.Object;
using FishNet.Transporting;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamemodeManager : NetworkBehaviour
{
    public override void OnStartNetwork()
    {
        Invoke(nameof(TurnOfLoadingScreen), 5f);
        Invoke(nameof(InitializePlayer), 3f);
        Invoke(nameof(EnablePlayerMovement), 5.5f);

        base.ServerManager.OnRemoteConnectionState += OnClientConnectState;
    }

    private void TurnOfLoadingScreen()
    {
        foreach(var obj in gameObject.scene.GetRootGameObjects())
        {
            if (obj.name == "Canvas") { obj.transform.Find("LoadingScreen").gameObject.SetActive(false); }
        }
    }

    private void InitializePlayer()
    {
        //initialize player (move this as needed)
        foreach (var obj in gameObject.scene.GetRootGameObjects())
        {
            if (obj.CompareTag("Player"))
            {
                obj.GetComponent<InitializePlayer>().InitializeThePlayerOnClient(obj.GetComponent<NetworkObject>().Owner);
                obj.GetComponent<CameraWorker>().Initialize(obj.GetComponent<NetworkObject>().Owner);
            }
        }
    }

    private void EnablePlayerMovement()
    {
        foreach(var obj in gameObject.scene.GetRootGameObjects())
        {
            if (obj.CompareTag("Player")) { obj.GetComponent<PredictedPlayerController>()._activated = true; }
        }
    }

    private void OnClientConnectState(NetworkConnection conn, RemoteConnectionStateArgs args)
    {
        if(args.ConnectionState == RemoteConnectionState.Stopped)
        {
            int playerCount = 0;
            foreach(var obj in  gameObject.scene.GetRootGameObjects())
            {
                if (obj.CompareTag("Player")) playerCount++;
            }
            Debug.Log(playerCount);
            if (playerCount == 0) UnloadScene();
        }
    }

    private void UnloadScene()
    {
        Debug.Log("Unloading GameScene");
        SceneUnloadData sud = new SceneUnloadData(gameObject.scene);
        base.NetworkManager.SceneManager.UnloadConnectionScenes(sud);
    }
}
