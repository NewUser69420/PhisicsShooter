using FishNet;
using FishNet.Object;
using UnityEngine;
using FishNet.Managing.Scened;
using System.Collections.Generic;
using System;
using FishNet.Connection;

public class MainMenuSceneChanger : NetworkBehaviour
{
    private List<NetworkObject> objToKeep = new List<NetworkObject>();
    private NetworkConnection conn;

    //change scene when connected to server
    public override void OnStartNetwork()
    {
        base.OnStartNetwork();
        if (base.IsServer) return;
        
        GetReferanceToPlayers();
        GetReferanceToLocalPlayer();
        ChangeScenes(objToKeep.ToArray(), conn);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ChangeScenes(NetworkObject[] _objs, NetworkConnection _conn)
    {
        SceneLoadData sld = new SceneLoadData("SampleScene");
        sld.MovedNetworkObjects = _objs;
        sld.ReplaceScenes = ReplaceOption.All;
        InstanceFinder.SceneManager.LoadConnectionScenes(_conn, sld);
    }

    private void GetReferanceToPlayers()
    {
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Player"))
        {
            objToKeep.Add(obj.GetComponent<NetworkObject>());
        }
    }

    private void GetReferanceToLocalPlayer()
    {
        conn = InstanceFinder.NetworkManager.ClientManager.Connection;
    }
}