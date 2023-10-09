using FishNet;
using FishNet.Managing.Scened;
using FishNet.Object;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    private UnityEngine.SceneManagement.Scene currentScene;
    
    public NetworkObject KillerPrefab;
    public NetworkObject PhysicsBallPrefab;

    private void Awake()
    {
        if (InstanceFinder.ServerManager == null) return;
        InstanceFinder.SceneManager.OnLoadEnd += OnSceneLoaded;
        Invoke(nameof(Spawn), 0.7f);
    }

    private void OnSceneLoaded(SceneLoadEndEventArgs args)
    {
        foreach(var scene in args.LoadedScenes)
        {
            if(scene.name != "Lobbies") currentScene = scene;
        }
    }

    private void Spawn()
    {
        if (InstanceFinder.IsServer)
        {
            UnityEngine.SceneManagement.SceneManager.SetActiveScene(currentScene);
            
            NetworkObject Killer = Instantiate(KillerPrefab);
            NetworkObject PhysicsBall = Instantiate(PhysicsBallPrefab);

            InstanceFinder.ServerManager.Spawn(Killer);
            InstanceFinder.ServerManager.Spawn(PhysicsBall);
        }
    }
}
