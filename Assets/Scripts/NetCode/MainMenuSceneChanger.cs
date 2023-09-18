using FishNet;
using FishNet.Object;
using UnityEngine;
using FishNet.Managing.Scened;
using System.Collections.Generic;
using FishNet.Connection;

public class MainMenuSceneChanger : NetworkBehaviour
{
    private List<NetworkObject> objToKeep = new List<NetworkObject>();
    private NetworkConnection conn;

    public void IWantToChangeScenesNow(NetworkConnection connection)
    {
        conn = connection;
        ChangeScenes(GetReferanceToPlayers(), conn);
    }

    private void ChangeScenes(NetworkObject[] _objs, NetworkConnection _conn)
    {
        SceneLoadData sld = new SceneLoadData("SampleScene");
        sld.MovedNetworkObjects = _objs;
        sld.ReplaceScenes = ReplaceOption.All;
        InstanceFinder.SceneManager.LoadConnectionScenes(_conn, sld);
    }

    private NetworkObject[] GetReferanceToPlayers()
    {
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Player"))
        {
            objToKeep.Add(obj.GetComponent<NetworkObject>());
            return objToKeep.ToArray();
        }
        Debug.Log($"Can't find players to take to new scene");
        return null;
    }
}