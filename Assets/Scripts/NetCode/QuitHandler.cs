using FishNet;
using FishNet.Connection;
using FishNet.Managing.Scened;
using FishNet.Transporting;
using UnityEngine;

public class QuitHandler : MonoBehaviour
{
    private void OnEnable()
    {
        InstanceFinder.ClientManager.OnClientConnectionState += OnClientConnectionChange;
        InstanceFinder.ServerManager.OnRemoteConnectionState += OnServerConnectionChange;
        InstanceFinder.SceneManager.OnUnloadEnd += OnSceneUnload;
    }
    private void OnDisable()
    {
        InstanceFinder.ClientManager.OnClientConnectionState -= OnClientConnectionChange;
        InstanceFinder.ServerManager.OnRemoteConnectionState -= OnServerConnectionChange;
        InstanceFinder.SceneManager.OnUnloadEnd -= OnSceneUnload;
    }

    private void OnClientConnectionChange(ClientConnectionStateArgs args)
    {
        if(args.ConnectionState == LocalConnectionState.Stopping)
        {
            Debug.Log("Going back to main menu");
            MainMenu mm = FindObjectOfType<MainMenu>(true);
            mm.gameObject.SetActive(true);
            mm.transform.Find("Background").gameObject.SetActive(true);
            mm.transform.Find("ServerSelectionScreen").gameObject.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
        }
    }

    private void OnServerConnectionChange(NetworkConnection conn, RemoteConnectionStateArgs args)
    {
        if (args.ConnectionState == RemoteConnectionState.Stopped && InstanceFinder.IsServer)
        {
            foreach(var obj in conn.Objects)
            {
                if (obj == null) return;
                Debug.Log($"Removing player obj: {obj.name}");
                InstanceFinder.ServerManager.Despawn(obj);
                Destroy(obj);
            }
        }
    }

    private void OnSceneUnload(SceneUnloadEndEventArgs args)
    {

    }
}
