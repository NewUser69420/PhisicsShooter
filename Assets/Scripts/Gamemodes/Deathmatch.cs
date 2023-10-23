using FishNet;
using FishNet.Object;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Deathmatch : NetworkBehaviour
{
    [SerializeField] private List<NetworkObject> Team1 = new();
    [SerializeField] private List<NetworkObject> Team2 = new();
   
    [SerializeField] private Color Team1Colour;
    [SerializeField] private Color Team2Colour;


    private bool addedTeam1;
    private bool switchSides;

    public Dictionary<string, Vector3> spawnPos = new();

    private void OnEnable()
    {
        if(!InstanceFinder.IsServer) return;

        Invoke(nameof(MoveThis), 2f);
        Invoke(nameof(MakeTeams), 2.5f);
        Invoke(nameof(SpawnPlayers), 3f);
    }

    private void MoveThis()
    {
        if (base.IsServer) return;

        Debug.Log("test0");
        foreach (var pair in base.SceneManager.SceneConnections)
        {
            Debug.Log("test1");
            if (pair.Key.name != "Lobbies") UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(gameObject, pair.Key);
        }
    }

    private void MakeTeams()
    {   
        foreach (var pobj in gameObject.scene.GetRootGameObjects())
        {
            if (pobj.CompareTag("Player"))
            {
                if (!addedTeam1) { Team1.Add(pobj.GetComponent<NetworkObject>()); addedTeam1 = true; }
                else { Team2.Add(pobj.GetComponent<NetworkObject>()); addedTeam1 = false; }
            }
        }

        foreach (var pobj in Team1)
        {
            DoTeamColours(pobj.OwnerId, Team1Colour);
        }
        foreach (var pobj in Team2)
        {
            DoTeamColours(pobj.OwnerId, Team2Colour);
        }
    }

    [ObserversRpc]
    private void DoTeamColours(int _id, Color _color)
    {
        List<UnityEngine.SceneManagement.Scene> loadedScenes = new();
        for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
        {
            loadedScenes.Add(UnityEngine.SceneManagement.SceneManager.GetSceneAt(i));
            Debug.Log(UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).name);
        }
        
        foreach (UnityEngine.SceneManagement.Scene scene in loadedScenes)
        {
            if (scene.name != "Lobbies")
            {
                foreach (var pobj in scene.GetRootGameObjects())
                {
                    if (pobj.CompareTag("Player") && pobj.GetComponent<NetworkObject>().Owner == LocalConnection)
                    {
                        foreach (Transform item in pobj.transform.Find("UI/ScoreBoard/Holder"))
                        {
                            if (item.GetComponent<ScoreBoardItemTracker>().id == _id) item.GetComponent<RawImage>().color = _color;
                        }
                    }
                }
            }
        }
    }

    private void SpawnPlayers()
    {
        int playersSpawned = 0;
        for (int i = 0; i < Team1.Count; i++)
        {
            switch (switchSides)
            {
                case false:
                    foreach (var pair in spawnPos)
                    {
                        if (pair.Key == "SideA")
                        {
                            Team1[i].transform.position = pair.Value;
                            Team1[i].GetComponent<ServerHealthManager>().spawnPosition = pair.Value;
                        }
                    }
                    break;
                case true:
                    foreach (var pair in spawnPos)
                    {
                        if (pair.Key == "SideB")
                        {
                            Team1[i].transform.position = pair.Value;
                            Team1[i].GetComponent<ServerHealthManager>().spawnPosition = pair.Value;
                        }
                    }
                    break;
            }

            for (int ii = 0; ii < playersSpawned; ii++)
            {
                Team1[i].transform.position += i * new Vector3(2, 0, 0);
                Team1[i].GetComponent<ServerHealthManager>().spawnPosition += i * new Vector3(2, 0, 0);
            }

            playersSpawned++;
        }
        for (int i = 0; i < Team2.Count; i++)
        {
            switch (!switchSides)
            {
                case false:
                    foreach (var pair in spawnPos)
                    {
                        if (pair.Key == "SideA")
                        {
                            Team2[i].transform.position = pair.Value;
                            Team2[i].GetComponent<ServerHealthManager>().spawnPosition = pair.Value;
                        }
                    }
                    break;
                case true:
                    foreach (var pair in spawnPos)
                    {
                        if (pair.Key == "SideB")
                        {
                            Team2[i].transform.position = pair.Value;
                            Team2[i].GetComponent<ServerHealthManager>().spawnPosition = pair.Value;
                        }
                    }
                    break;
            }

            for (int ii = 0; ii < playersSpawned; ii++)
            {
                Team2[i].transform.position += i * new Vector3(2, 0, 0);
                Team2[i].GetComponent<ServerHealthManager>().spawnPosition += i * new Vector3(2, 0, 0);
            }

            playersSpawned++;
        }
    }
}
