using FishNet.Managing.Scened;
using FishNet;
using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Connection;

public class SceneManager : MonoBehaviour
{
    //public void ChangeScenes(NetworkObject[] _objs, NetworkConnection _conn)
    //{
    //    SceneLoadData sld = new SceneLoadData("SampleScene");
    //    sld.MovedNetworkObjects = _objs;
    //    sld.ReplaceScenes = ReplaceOption.All;
    //    InstanceFinder.SceneManager.LoadConnectionScenes(_conn, sld);
    //}

    private void Awake()
    {
        InstanceFinder.ServerManager.OnAuthenticationResult += ChangeSceneForServer;
    }

    public void ChangeSceneForServer(NetworkConnection conn, bool bl)
    {
        SceneLoadData sld = new SceneLoadData("SampleScene");
        sld.ReplaceScenes = ReplaceOption.All;
        InstanceFinder.SceneManager.LoadGlobalScenes(sld);
    }
}