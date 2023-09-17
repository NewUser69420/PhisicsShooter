using FishNet;
using FishNet.Object;
using UnityEngine;
using FishNet.Managing.Scened;
using System.Collections.Generic;
using System;
using FishNet.Connection;
using FishNet.Transporting;

public class MainMenuSceneChanger : NetworkBehaviour
{
    private List<NetworkObject> objToKeep = new List<NetworkObject>();
    private NetworkConnection conn;

    private void Awake()
    {
        if (base.IsClient) return;
        InstanceFinder.ServerManager.OnRemoteConnectionState += OnClientConnect;
    }

    //change scene when connected to server    
    public void OnClientConnect(NetworkConnection connection, RemoteConnectionStateArgs args)
    {
        if (base.IsClient) return;
        
        GetReferanceToPlayers();
        conn = connection;
        ChangeScenes(objToKeep.ToArray(), conn);
    }

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
}