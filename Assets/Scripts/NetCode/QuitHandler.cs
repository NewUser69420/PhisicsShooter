using FishNet;
using FishNet.Connection;
using FishNet.Managing.Scened;
using FishNet.Transporting;
using System.Collections;
using UnityEngine;
using UnityEngine.ProBuilder.Shapes;

public class QuitHandler : MonoBehaviour
{
    private void OnEnable()
    {
        InstanceFinder.ClientManager.OnClientConnectionState += OnClientConnectionChange;
        InstanceFinder.ServerManager.OnRemoteConnectionState += OnServerConnectionChange;
    }
    private void OnDisable()
    {
        InstanceFinder.ClientManager.OnClientConnectionState -= OnClientConnectionChange;
        InstanceFinder.ServerManager.OnRemoteConnectionState -= OnServerConnectionChange;
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
            StartCoroutine(Wait1(conn));
        }
    }

    IEnumerator Wait1(NetworkConnection _conn)
    {
        yield return new WaitForSeconds(0.5f);
        
        foreach (var obj in _conn.Objects)
        {
            if (obj == null) break;
            Debug.Log($"Removing player obj: {obj.name}");
            InstanceFinder.ServerManager.Despawn(obj);
            Destroy(obj);
        }

        Debug.Log("test0");
        for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
        {
            Debug.Log("test1");
            int playerCount = 0;
            foreach (var obj in UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).GetRootGameObjects())
            {
                if (obj.CompareTag("Player")) playerCount++;
            }
            Debug.Log($"test2: {playerCount}");
            if (playerCount == 0)
            {
                SceneUnloadData sud = new SceneUnloadData(UnityEngine.SceneManagement.SceneManager.GetSceneAt(i));
                InstanceFinder.SceneManager.UnloadConnectionScenes(sud);
            }
        }
    }
}
