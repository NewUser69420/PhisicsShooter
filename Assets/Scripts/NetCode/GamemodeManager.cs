using FishNet;
using FishNet.Connection;
using FishNet.Managing.Scened;
using FishNet.Object;
using FishNet.Transporting;
using UnityEngine;

public class GamemodeManager : MonoBehaviour
{
    private void Awake()
    {
        Invoke(nameof(TurnOfLoadingScreen), 5f);
        Invoke(nameof(InitializePlayer), 3f);
        Invoke(nameof(EnablePlayerMovement), 5.5f);
    }

    private void OnEnable()
    {
        InstanceFinder.ServerManager.OnRemoteConnectionState += OnConnectionChange;
    }
    private void OnDisable()
    {
        InstanceFinder.ServerManager.OnRemoteConnectionState -= OnConnectionChange;
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

    private void OnConnectionChange(NetworkConnection conn, RemoteConnectionStateArgs args)
    {
        Debug.Log($"ConnectionChanged");
        if (args.ConnectionState == RemoteConnectionState.Stopped)
        {
            int playerObjs = 0;
            foreach(var obj in gameObject.scene.GetRootGameObjects())
            {
                if(obj.CompareTag("Player")) playerObjs++;
            }
            if(playerObjs == 0) UnloadScene();
        }
    }

    private void UnloadScene()
    {
        Debug.Log("Unloading GameScene");
        SceneUnloadData sud = new SceneUnloadData(gameObject.scene);
        InstanceFinder.NetworkManager.SceneManager.UnloadConnectionScenes(sud);
    }
}
