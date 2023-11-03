using FishNet;
using FishNet.Demo.AdditiveScenes;
using FishNet.Object;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Deathmatch : GamemodeBase
{
    private bool addedTeam1;
    private bool switchSides;

    public Dictionary<string, Vector3> spawnPos = new();

    [SerializeField] private GameObject playerPrefab;

    private void OnEnable()
    {
        if(!InstanceFinder.IsServer) return;

        Invoke(nameof(MoveThis), 2f);
        Invoke(nameof(MakeTeams), 2.5f);
        Invoke(nameof(SpawnPlayers), 3f);
    }

    public override void OnStartNetwork()
    {
        SetPlayerPrefab();
    }

    protected override void SetPlayerPrefab()
    {
        PlayerPrefab = playerPrefab;
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

    protected override void MakeTeams()
    {
        List<GameObject> needAdd = new();

        foreach (var pobj in gameObject.scene.GetRootGameObjects())
        {
            if(pobj.CompareTag("Player")) needAdd.Add(pobj);
        }

        foreach (var party in parties)
        {
            switch(needAdd.Count) 
            {
                case 3:
                    if (party.Value.Count == 2)
                    {
                        if(!addedTeam1) { foreach (var obj in party.Value) { Team1.Add(obj); needAdd.Remove(obj.gameObject); } addedTeam1 = true; }
                        if (addedTeam1) { foreach (var obj in party.Value) { Team2.Add(obj); needAdd.Remove(obj.gameObject); } addedTeam1 = false; }
                    }
                    break;
                case 4:
                    if (party.Value.Count == 2)
                    {
                        if(!addedTeam1) { foreach (var obj in party.Value) { Team1.Add(obj); needAdd.Remove(obj.gameObject); } addedTeam1 = true; }
                        if (addedTeam1) { foreach (var obj in party.Value) { Team2.Add(obj); needAdd.Remove(obj.gameObject); } addedTeam1 = false; }
                    }
                    break;
                case 5:
                    if(party.Value.Count == 3 || party.Value.Count == 2)
                    {
                        if(!addedTeam1) { foreach (var obj in party.Value) { Team1.Add(obj); needAdd.Remove(obj.gameObject); } addedTeam1 = true; }
                        if (addedTeam1) { foreach (var obj in party.Value) { Team2.Add(obj); needAdd.Remove(obj.gameObject); } addedTeam1 = false; }
                    }
                    break;
                case 6:
                    if(party.Value.Count == 3 || party.Value.Count == 2)
                    {
                        if(!addedTeam1) { foreach (var obj in party.Value) { Team1.Add(obj); needAdd.Remove(obj.gameObject); } addedTeam1 = true; }
                        if (addedTeam1) { foreach (var obj in party.Value) { Team2.Add(obj); needAdd.Remove(obj.gameObject); } addedTeam1 = false; }
                    }
                    break;
                default: break;
            }
        }

        foreach(var pobj in needAdd)
        {
            //fill in the teams
            if (Team1.Count < Team2.Count) Team1.Add(pobj.GetComponent<NetworkObject>());
            else Team2.Add(pobj.GetComponent<NetworkObject>());
        }

        foreach (var pobj in Team1)
        {
            DoTeamColours(pobj.GetComponent<NetworkObject>().OwnerId, Team1Colour, Team2Colour, "team1");
            pScoreT1.Add(pobj.GetComponent<NetworkObject>(), 0);
            pobj.GetComponent<ServerHealthManager>().team = "team1";
        }
        foreach (var pobj in Team2)
        {
            DoTeamColours(pobj.GetComponent<NetworkObject>().OwnerId, Team1Colour, Team2Colour, "team2");
            pScoreT2.Add(pobj.GetComponent<NetworkObject>(), 0);
            pobj.GetComponent<ServerHealthManager>().team = "team2";
        }

        switch (Team1.Count)
        {
            case 1:
                endScoreTeam1 = 500;
                break;
            case 2:
                endScoreTeam1 = 1300;
                break;
            case 3:
                endScoreTeam1 = 2500;
                break;
        }
        switch (Team2.Count)
        {
            case 1:
                endScoreTeam2 = 500;
                break;
            case 2:
                endScoreTeam2 = 1300;
                break;
            case 3:
                endScoreTeam2 = 2500;
                break;
        }
    }

    [ObserversRpc]
    private void DoTeamColours(int _id, Color _color1, Color _color2, string _team)
    {
        List<UnityEngine.SceneManagement.Scene> loadedScenes = new();
        for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
        {
            loadedScenes.Add(UnityEngine.SceneManagement.SceneManager.GetSceneAt(i));
        }

        Color _color = new();
        switch (_team)
        {
            case "team1":
                _color = _color1;
                break;
            case "team2":
                _color = _color2;
                break;
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
                        Transform score = pobj.transform.Find("UI/Score");
                        score.transform.Find("T1Image").GetComponent<Image>().color = _color1;
                        score.transform.Find("T2Image").GetComponent<Image>().color = _color2;
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

    public override void OnScoreChange()
    {
        //calc new score
        scoreT1 = 0;
        foreach (var pair in pScoreT1)
        {
            scoreT1 += Mathf.RoundToInt(pair.Value);
        }

        scoreT2 = 0;
        foreach (var pair in pScoreT2)
        {
            scoreT2 += Mathf.RoundToInt(pair.Value);
        }

        //do scoreboards
        SyncScore(scoreT1, scoreT2);

        //check for end game
        if (scoreT1 >= endScoreTeam1 && gameIsRunning) FinishGame("team1");
        if (scoreT2 >= endScoreTeam2 && gameIsRunning) FinishGame("team2");
    }

    [ObserversRpc]
    private void SyncScore(int t1score, int t2score)
    {
        for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
        {
            if (UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).name != "Lobbies")
            {
                foreach (var obj in UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).GetRootGameObjects())
                {
                    if (obj.CompareTag("Player"))
                    {
                        obj.transform.Find("UI/Score/T1Value").GetComponent<TMP_Text>().text = t1score.ToString();
                        obj.transform.Find("UI/Score/T2Value").GetComponent<TMP_Text>().text = t2score.ToString();
                    }
                }
            }
        }
    }
}
