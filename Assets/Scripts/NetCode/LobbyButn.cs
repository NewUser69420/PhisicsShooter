using FishNet.Connection;
using FishNet.Managing.Scened;
using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyButn : NetworkBehaviour
{
    private NetworkObject Player;

    [System.NonSerialized] public List<NetworkObject> party = new();

    [SerializeField] private GameObject LoadingScreen;

    public override void OnStartNetwork()
    {
        GameObject.Find("Lobbies").transform.Find("LoadingScreen").gameObject.SetActive(false);
        
        if (base.IsServer) return;
        StartCoroutine(Wait());
    }

    IEnumerator Wait()
    {
        yield return new WaitForSeconds(0.5f);
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Player"))
        {
            if (obj.GetComponent<NetworkObject>().OwnerId == base.LocalConnection.ClientId) { Player = obj.GetComponent<NetworkObject>(); Debug.Log($"player id = {Player.OwnerId}"); }
        }
        if (Player == null) Debug.Log($"error, cant find player object with id: {base.LocalConnection.ClientId}");
    }

    public void OnButnClick()
    {
        FindObjectOfType<AudioManger>().Play("click1");
        if (base.IsServer) return;
        switch (gameObject.name)
        {
            case "B1v1":
                JoinLobby("1v1Lobby", Player);
                break;
            case "B2v2":
                JoinLobby("2v2Lobby", Player);
                break;
            case "B3v3":
                JoinLobby("3v3Lobby", Player);
                break;
        }
    }

    //[ServerRpc(RequireOwnership = false)]
    //private void Test(NetworkObject nobj)
    //{
    //    DoLoadingScreenClientRpc(nobj.Owner);

    //    SceneLoadData sld = new SceneLoadData("1v1Lobby");
    //    sld.MovedNetworkObjects = new NetworkObject[] { nobj };
    //    sld.ReplaceScenes = ReplaceOption.None;
    //    base.SceneManager.LoadConnectionScenes(nobj.Owner, sld);
    //}

    //[TargetRpc]
    //private void SyncLocationOfPobjs(NetworkConnection _conn, NetworkObject _obj)
    //{
    //    StartCoroutine(Wait2(_obj));
    //}

    //IEnumerator Wait2(NetworkObject __obj)
    //{
    //    yield return new WaitForSeconds(1);
    //    if(__obj.GetComponent<InitializePlayer>() != null) UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(__obj.gameObject, UnityEngine.SceneManagement.SceneManager.GetSceneByName("1v1Lobby"));
    //}

    [ServerRpc(RequireOwnership = false)]
    private void JoinLobby(string _lobbyName, NetworkObject _Player)
    {
        List<int> checklist = new();

        List<NetworkObject> objsToKeep = new();
        objsToKeep = party;
        objsToKeep.Add(FindObjectOfType<Party>().NetworkObject);
        
        //set playermax
        int playerMax = 0;
        switch(_lobbyName)
        {
            case "1v1Lobby":
                playerMax = 2;
                if (party.Count > 2) return;
                break;
            case "2v2Lobby":
                playerMax = 4;
                if (party.Count > 2) return;
                break;
            case "3v3Lobby":
                playerMax = 6;
                if (party.Count > 3) return;
                break;
        }

        foreach (var pair in base.SceneManager.SceneConnections)
        {
            if (pair.Key.name != _lobbyName)
            {
                checklist.Add(1);
            }
            else if (pair.Key.name == _lobbyName)
            {
                checklist.Add(0);
                //join scene if not full
                if (pair.Value.Count < playerMax && (playerMax - pair.Value.Count >= party.Count))
                {
                    //join this scene
                    SceneLookupData lookup = new SceneLookupData(pair.Key.handle, _lobbyName);
                    SceneLoadData sld = new SceneLoadData(lookup);
                    sld.Options.AllowStacking = false;
                    sld.MovedNetworkObjects = objsToKeep.ToArray();
                    //sld.Options.LocalPhysics = LocalPhysicsMode.Physics3D; //be carefull, might cause bugs. do more research
                    base.SceneManager.LoadConnectionScenes(_Player.Owner, sld);
                    DoLoadingScreenClientRpc(_Player.Owner);
                    Debug.Log($"Joining lobby");
                    return;

                    ////old
                    //SceneLoadData sld = new SceneLoadData(pair.Key);
                    //sld.ReplaceScenes = ReplaceOption.None;
                    //sld.Options.AllowStacking = true;
                    //sld.MovedNetworkObjects = objsToKeep.ToArray();
                    //sld.Options.LocalPhysics = LocalPhysicsMode.Physics3D; //be carefull, might cause bugs. do more research
                    //base.SceneManager.LoadConnectionScenes(_conn, sld);
                }
            }
        }

        if (!checklist.Contains(0))
        {
            //no scenes yet make and join a scene
            SceneLookupData lookup = new SceneLookupData(0, _lobbyName);
            SceneLoadData sld = new SceneLoadData(lookup);
            sld.Options.AllowStacking = true;
            sld.MovedNetworkObjects = objsToKeep.ToArray();
            //sld.Options.LocalPhysics = LocalPhysicsMode.Physics3D; //be carefull, might cause bugs. do more research
            base.SceneManager.LoadConnectionScenes(_Player.Owner, sld);
            DoLoadingScreenClientRpc(_Player.Owner);
            Debug.Log($"No lobby exists, Making own");
            return;
        }

        //if still here (should only be bc other scenes are full) make and join scene
        SceneLookupData lookupp = new SceneLookupData(0, _lobbyName);
        SceneLoadData sldd = new SceneLoadData(lookupp);
        sldd.Options.AllowStacking = true;
        sldd.MovedNetworkObjects = objsToKeep.ToArray();
        //sldd.Options.LocalPhysics = LocalPhysicsMode.Physics3D; //be carefull, might cause bugs. do more research
        base.SceneManager.LoadConnectionScenes(_Player.Owner, sldd);
        DoLoadingScreenClientRpc(_Player.Owner);
        Debug.Log($"Lobbies full, Making own");
        return;
    }

    [TargetRpc]
    private void DoLoadingScreenClientRpc(NetworkConnection _conn)
    {
        GameObject.Find("Lobbies").transform.Find("LoadingScreen").gameObject.SetActive(true);
    }

    public void OnPressedBack()
    {
        FindObjectOfType<AudioManger>().Play("click2");
        //LoadingScreen.SetActive(true);

        FindObjectOfType<ServerMenu>(true).gameObject.SetActive(true);

        GameObject Player = null;
        foreach (var obj in LocalConnection.Objects) if (obj.CompareTag("Player")) Player = obj.gameObject;
        for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
        {
            if (UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).name == "ServerMenu") { UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(Player, UnityEngine.SceneManagement.SceneManager.GetSceneAt(i)); Debug.Log($"Moving player"); }
        }

        GoBackServer(LocalConnection);
    }

    [ServerRpc(RequireOwnership = false)]
    private void GoBackServer(NetworkConnection conn)
    {
        GameObject Player = null;
        foreach (var obj in conn.Objects) if (obj.CompareTag("Player")) Player = obj.gameObject;
        for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
        {
            if (UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).name == "ServerMenu") { UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(Player, UnityEngine.SceneManagement.SceneManager.GetSceneAt(i)); Debug.Log($"Moving player"); }
        }

        SceneUnloadData sud = new SceneUnloadData(gameObject.scene);
        base.SceneManager.UnloadConnectionScenes(conn, sud);
    }
}
