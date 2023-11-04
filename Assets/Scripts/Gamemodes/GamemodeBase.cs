using FishNet.Connection;
using FishNet.Demo.AdditiveScenes;
using FishNet.Managing.Scened;
using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class GamemodeBase : NetworkBehaviour
{
    public List<NetworkObject> Team1 = new();
    public List<NetworkObject> Team2 = new();

    public Color Team1Colour;
    public Color Team2Colour;

    public Dictionary<int, List<NetworkObject>> parties = new();

    [System.NonSerialized] public Dictionary<NetworkObject, float> pScoreT1 = new();
    [System.NonSerialized] public Dictionary<NetworkObject, float> pScoreT2 = new();
    public int endScoreTeam1;
    public int endScoreTeam2;
    public int scoreT1;
    public int scoreT2;
    public string lobbyName;

    [System.NonSerialized] public bool gameIsRunning = true;

    private List<NetworkObject> playerobjs = new();
    private List<NetworkConnection> playerconns = new();

    [System.NonSerialized] public GameObject PlayerPrefab;

    protected virtual void SetPlayerPrefab()
    {
        //override this in specific gamemode script
    }

    protected virtual void MakeTeams()
    {
        //override this in specific gamemode script
    }

    public virtual void OnScoreChange()
    {
        //override this in specific gamemode script
    }

    public void FinishGame(string team)
    {
        gameIsRunning = false;
        Debug.Log($"Team {team} Won!");

        //set score on server
        foreach (var pobj in Team1)
        {
            pobj.GetComponent<ScoreTracker>().Setup();
            if(team == "team1") { pobj.GetComponent<ScoreTracker>().OnScoreChange("win"); }
            else { pobj.GetComponent<ScoreTracker>().OnScoreChange("loose"); }
        }
        foreach (var pobj in Team2)
        {
            pobj.GetComponent<ScoreTracker>().Setup();
            if (team == "team2") { pobj.GetComponent<ScoreTracker>().OnScoreChange("win"); }
            else { pobj.GetComponent<ScoreTracker>().OnScoreChange("loose"); }
        }

        DoEndGameScreenClient(team);

        Invoke(nameof(StartNewGame), 3f);
    }

    [ObserversRpc]
    private void DoEndGameScreenClient(string _team)
    {
        for(int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
        {
            if (UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).name != "Lobbies" && UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).name != "ServerMenu")
            {
                foreach (var obj in UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).GetRootGameObjects())
                {
                    if (obj.name == "Canvas") { obj.transform.Find("EndGameScreen").gameObject.SetActive(true); obj.transform.Find("EndGameScreen/ValueText").GetComponent<TMP_Text>().text = _team; }
                    if (obj.CompareTag("Player")) obj.transform.Find("UI").gameObject.SetActive(false);
                }
            }
        }
        
        
    }

    private void StartNewGame()
    {
        foreach (var obj in gameObject.scene.GetRootGameObjects())
        {
            if (obj.CompareTag("Player")) { DeInitializePlayer(obj, obj.GetComponent<NetworkObject>().Owner); DeInitializePlayerClient(obj.GetComponent<NetworkObject>().Owner); }
        }

        Invoke(nameof(NewLobby), 1f);
    }

    private void NewLobby()
    {
        SceneLookupData lookup = new SceneLookupData(0, lobbyName);
        SceneLoadData sld = new SceneLoadData(lookup);
        sld.Options.AllowStacking = true;
        sld.MovedNetworkObjects = playerobjs.ToArray();
        sld.Options.LocalPhysics = UnityEngine.SceneManagement.LocalPhysicsMode.Physics3D;
        base.SceneManager.LoadConnectionScenes(playerconns.ToArray(), sld);

        Invoke(nameof(UnloadLobbyScene), 1f);
    }

    private void UnloadLobbyScene()
    {
        //get rid of lobby scene
        Debug.Log($"Unloading lobby scene for {playerconns.Count} connections");
        SceneUnloadData sud = new SceneUnloadData(gameObject.scene);
        base.SceneManager.UnloadConnectionScenes(playerconns.ToArray(), sud);
    }

    private void DeInitializePlayer(GameObject pobj, NetworkConnection playerConn)
    {        
        UnityEngine.SceneManagement.SceneManager.SetActiveScene(gameObject.scene);

        string name = pobj.GetComponent<InitializePlayer>().playerName;
        List<int> partyIDs = new List<int>();
        for(int i = 0; i < pobj.GetComponent<Party>().party.Count; i++)
        {
            if(pobj.GetComponent<Party>().party[i].OwnerId != pobj.GetComponent<NetworkObject>().OwnerId) partyIDs.Add(pobj.GetComponent<Party>().party[i].OwnerId);
            Debug.Log($"party owner: {pobj.GetComponent<NetworkObject>().OwnerId}, party item: {pobj.GetComponent<Party>().party[i].OwnerId}");
        }

        base.ServerManager.Despawn(pobj);

        GameObject Player = Instantiate(PlayerPrefab);
        base.ServerManager.Spawn(Player, playerConn);

        playerobjs.Add(Player.GetComponent<NetworkObject>());
        playerconns.Add(Player.GetComponent<NetworkObject>().Owner);

        FixName(Player.GetComponent<NetworkObject>().Owner, Player, name);
        StartCoroutine(Wait(partyIDs, Player));
    }

    IEnumerator Wait(List<int> _partyIDs, GameObject _Player)
    {
        yield return new WaitForSeconds(0.5f);
        
        foreach (var id in _partyIDs)
        {
            foreach (var client in base.ServerManager.Clients)
            {
                if (client.Value.ClientId == id)
                {
                    foreach (var obj in client.Value.Objects)
                    {
                        if (obj.CompareTag("Player")) _Player.GetComponent<Party>().party.Add(obj);
                    }
                }
            }
        }
    }

    [TargetRpc]
    private void DeInitializePlayerClient(NetworkConnection conn)
    {
        for(int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
        {
            if (UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).name != "Lobbies" && UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).name != "ServerMenu") { foreach (var obj in UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).GetRootGameObjects()) { if (obj.name == "Canvas") obj.transform.Find("LoadingScreen").gameObject.SetActive(true); } }
        }
    }

    [TargetRpc]
    private void FixName(NetworkConnection conn, GameObject pobj, string _name)
    {
        pobj.GetComponent<InitializePlayer>().playerName = _name;
    }
}
