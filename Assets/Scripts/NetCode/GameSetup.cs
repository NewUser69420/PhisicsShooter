using FishNet;
using FishNet.Connection;
using FishNet.Managing.Scened;
using FishNet.Object;
using FishNet.Transporting;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class KeyValuePair
{
    public string key;
    public Vector3 val;
}

public class GameSetup : MonoBehaviour
{
    public string gamemode;
    public string lobbyName;

    [SerializeField] private GameObject DeathMatch;

    [SerializeField] private List<KeyValuePair> SpawnPositions = new();

    private Dictionary<string, Vector3> spawnPos = new();

    private void Awake()
    {
        Invoke(nameof(AddGamemode), 2f);
        Invoke(nameof(InitializePlayer), 3f);
        Invoke(nameof(TurnOfLoadingScreen), 5f);
        Invoke(nameof(EnablePlayerMovement), 5.5f);

        foreach (var thing in SpawnPositions)
        {
            spawnPos[thing.key] = thing.val;
        }
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
        if (!InstanceFinder.IsServer) return;
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

    private void AddGamemode()
    {
        UnityEngine.SceneManagement.SceneManager.SetActiveScene(gameObject.scene);
        if (!InstanceFinder.IsServer) return;

        GameObject GM = null;
        switch (gamemode)
        {
            case "1v1Deathmatch":
                GM = Instantiate(DeathMatch);
                GM.GetComponent<Deathmatch>().spawnPos = spawnPos;
                break;
            default:
                Debug.Log("No gamemode found");
                break;
        }
        GM.GetComponent<GamemodeBase>().lobbyName = lobbyName;
        InstanceFinder.ServerManager.Spawn(GM);
    }
}
