using FishNet.Connection;
using FishNet.Managing.Scened;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class LobbyButn : NetworkBehaviour
{
    private NetworkObject Player;

    private void Start()
    {
        if(!base.IsServer) StartCoroutine(Wait());
    }

    IEnumerator Wait()
    {
        yield return new WaitForSeconds(1);
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Player"))
        {
            if (obj.GetComponent<NetworkObject>().OwnerId == LocalConnection.ClientId) Player = obj.GetComponent<NetworkObject>();
        }
    }

    public void OnButnClick()
    {
        if (base.IsServer) return;
        switch(gameObject.name)
        {
            case "B1v1":
                JoinLobby(LocalConnection, "1v1Lobby", Player);
                break;
            case "B2v2":
                JoinLobby(LocalConnection, "2v2Lobby", Player);
                break;
            case "B3v3":
                JoinLobby(LocalConnection, "3v3Lobby", Player);
                break;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void JoinLobby(NetworkConnection _conn, string _lobbyName, NetworkObject _Player)
    {
        List<NetworkObject> objsToKeep = new();
        objsToKeep.Add(_Player);
        List<int> checklist = new();
        
        //set playermax
        int playerMax = 0;
        switch(_lobbyName)
        {
            case "1v1Lobby":
                playerMax = 2;
                break;
            case "2v2Lobby":
                playerMax = 4;
                break;
            case "3v3Lobby":
                playerMax = 6;
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
                var stackedSceneHandle = pair.Key.handle;
                checklist.Add(0);
                //join scene if not full
                if (pair.Value.Count < playerMax)
                {
                    //join this scene
                    SceneLookupData lookup = new SceneLookupData(stackedSceneHandle, _lobbyName);
                    SceneLoadData sld = new SceneLoadData(lookup);
                    sld.Options.AllowStacking = true;
                    sld.MovedNetworkObjects = objsToKeep.ToArray();
                    sld.Options.LocalPhysics = LocalPhysicsMode.Physics3D; //be carefull, might cause bugs. do more research
                    base.SceneManager.LoadConnectionScenes(_conn, sld);
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
            sld.Options.AllowStacking = false;
            sld.MovedNetworkObjects = objsToKeep.ToArray();
            sld.Options.LocalPhysics = LocalPhysicsMode.Physics3D; //be carefull, might cause bugs. do more research
            base.SceneManager.LoadConnectionScenes(_conn, sld);
            Debug.Log($"No lobby exists, Making own");
            return;
        }

        //if still here (should only be bc other scenes are full) make and join scene
        SceneLookupData lookupp = new SceneLookupData(0, _lobbyName);
        SceneLoadData sldd = new SceneLoadData(lookupp);
        sldd.Options.AllowStacking = false;
        sldd.MovedNetworkObjects = objsToKeep.ToArray();
        sldd.Options.LocalPhysics = LocalPhysicsMode.Physics3D; //be carefull, might cause bugs. do more research
        base.SceneManager.LoadConnectionScenes(_conn, sldd);
        Debug.Log($"Lobbies full, Making own");
        return;
    }
}
