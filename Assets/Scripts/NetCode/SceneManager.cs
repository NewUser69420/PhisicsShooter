using FishNet.Managing.Scened;
using FishNet;
using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Connection;
using Unity.VisualScripting;

public class SceneManager : MonoBehaviour
{
    //private List<NetworkObject> objsToKeep = new List<NetworkObject>(); 

    private void Awake()
    {
        InstanceFinder.ServerManager.OnAuthenticationResult += ChangeSceneForServer;
    }

    public void ChangeSceneForServer(NetworkConnection conn, bool bl)
    {
        //GetObjsToKeep();
        SceneLoadData sld = new SceneLoadData("Lobbies");
        //sld.MovedNetworkObjects = objsToKeep.ToArray();
        sld.ReplaceScenes = ReplaceOption.All;
        InstanceFinder.SceneManager.LoadGlobalScenes(sld);
    }

    private void GetObjsToKeep()
    {
        foreach (NetworkObject obj in InstanceFinder.ServerManager.Objects.SceneObjects.Values)
        {
            //if (obj.gameObject.name == "ServerHealthManager") objsToKeep.Add(obj);
        }
    }
}