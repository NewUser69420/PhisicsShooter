using FishNet.Managing.Scened;
using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : NetworkBehaviour
{
    private UnityEngine.SceneManagement.Scene currentScene;
    
    public NetworkObject KillerPrefab;
    public NetworkObject PhysicsBallPrefab;
    public List<string> scenesToIgnore = new List<string>();

    private bool hasDone;

    public override void OnStartNetwork()
    {
        base.SceneManager.OnLoadEnd += OnSceneLoaded;
        Invoke(nameof(Spawn), 3f);
        Invoke(nameof(SyncSpawn), 4f);
    }

    private void OnSceneLoaded(SceneLoadEndEventArgs args)
    {
        foreach(var scene in args.LoadedScenes)
        {
            if(!scenesToIgnore.Contains(scene.name) && !hasDone) { currentScene = scene; hasDone = true; }
        }
    }

    private void Spawn()
    {
        if (base.IsServer)
        {   
            UnityEngine.SceneManagement.SceneManager.SetActiveScene(currentScene);
            NetworkObject Killer = Instantiate(KillerPrefab);
            NetworkObject PhysicsBall = Instantiate(PhysicsBallPrefab);

            base.ServerManager.Spawn(Killer);
            base.ServerManager.Spawn(PhysicsBall);
        }
    }

    [ObserversRpc]
    private void SyncSpawn()
    {
        Debug.Log(gameObject.scene.name);
        UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(GameObject.Find("PhisicsBall(Clone)"), gameObject.scene);
        UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(GameObject.Find("Killer(Clone)"), gameObject.scene);
    }
}
