using FishNet;
using FishNet.Connection;
using FishNet.Demo.AdditiveScenes;
using FishNet.Managing.Client;
using FishNet.Managing.Scened;
using FishNet.Object;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class InitializePlayer : NetworkBehaviour
{
    public Vector3 spawnPos;

    public string playerName = "playername not set";

    public GameObject MainMenuUI;
    public Transform UI;
    public Transform PlayerName;

    public GameObject ScoreboardItemPrefab;

    private GameObject obj;

    public override void OnStartNetwork()
    {
        MainMenuUI = GameObject.Find("MainMenuUI");
        
        if (base.Owner.IsLocalClient)
        {
            SetGameLayerRecursive(this.gameObject, 6);

            playerName = MainMenuUI.GetComponent<MainMenu>().playerName;
            PlayerName.GetComponent<TMP_Text>().text = playerName;
            SyncNameServer(playerName, PlayerName.gameObject);

            //SetGameLayerRecursive(transform.Find("Character/Armature/Hips").gameObject, 12);

            InitializePlayerServerRpc(base.LocalConnection);
            
            Cursor.lockState = CursorLockMode.Locked;

            //make scoreboard item and activate player
            StartCoroutine(Wait2());
        }

        if (base.IsServer)
        {
            StartCoroutine(Wait());
        }
    }

    IEnumerator Wait2()
    {
        yield return new WaitForSeconds(1f);
        
        foreach (NetworkConnection client in base.ClientManager.Clients.Values)
        {
            SyncScoreboardServer(ScoreboardItemPrefab, client.ClientId, client);
        }

        //activate player
        GetComponent<PredictedPlayerController>()._activated = true;
        UI.gameObject.SetActive(true);
    }

    [ServerRpc]
    private void SyncScoreboardServer(GameObject obj, int id, NetworkConnection conn)
    {
        string _playerName = "input name";
        foreach(NetworkObject ___obj in conn.Objects)
        {
            if (___obj.tag == "Player") _playerName = ___obj.transform.Find("NameCanvas/PlayerName").GetComponent<TMP_Text>().text;
        }
        Debug.Log($"spawning scoreboardItem with name: {_playerName}");
        SyncScoreboardClient(obj , id, _playerName);
    }

    [ObserversRpc]
    private void SyncScoreboardClient(GameObject _obj, int _id, string __playerName)
    {
        Debug.Log($"spawning scoreboardItem for id: {_id}, name: {__playerName} on client: {LocalConnection.ClientId}");
        
        foreach(var __obj in LocalConnection.Objects)
        {
            if(__obj.tag == "Player")
            {
                bool hasItem = false;
                foreach(Transform child in __obj.transform.Find("UI/ScoreBoard/Holder"))
                {
                    if(child.GetComponent<ScoreBoardItemTracker>().id == _id) hasItem = true;
                }
                
                if(!hasItem)
                {
                    GameObject objj = Instantiate(_obj, __obj.transform.Find("UI/ScoreBoard/Holder"));
                    objj.GetComponent<ScoreBoardItemTracker>().id = _id;
                    objj.GetComponent<ScoreBoardItemTracker>().nameValue = __playerName;
                }
            }
        }
    }

    IEnumerator Wait()
    {
        yield return new WaitForSeconds(0.5f);
        
        List<GameObject> pobjs = new List<GameObject>();
        GameObject obj = null;
        String pName = null;
        int id = 0;
        foreach (var client in base.ServerManager.Clients)
        {
            foreach (var pobj in client.Value.Objects)
            {
                if (pobj.gameObject.tag == "Player")
                {
                    obj = pobj.transform.Find("NameCanvas/PlayerName").gameObject;
                    if (pobj.GetComponent<NetworkObject>().OwnerId == client.Value.ClientId) pName = pobj.transform.Find("NameCanvas/PlayerName").GetComponent<TMP_Text>().text;
                    else Debug.Log($"No match: {pobj.GetComponent<NetworkObject>().OwnerId} != {client.Value.ClientId}");
                }
            }
            id = client.Value.ClientId;

            Debug.Log($"sending client rpc with obj:{obj.name} + name:{pName} + id:{id}");
            SyncNameClient(obj, pName, id);
        }
    }

    [ServerRpc]
    private void SyncNameServer(String _name, GameObject _obj)
    {
        _obj.GetComponent<TMP_Text>().text = _name;
    }

    [ObserversRpc]
    private void SyncNameClient(GameObject _obj, String _name, int id)
    {
        foreach(var client in base.ClientManager.Clients)
        {
            if(client.Value.ClientId == id)
            {
                _obj.GetComponent<TMP_Text>().text = _name;
            }
        }
    }

    private void SetGameLayerRecursive(GameObject _go, int _layer)
    {
        _go.layer = _layer;
        foreach (Transform child in _go.transform)
        {
            child.gameObject.layer = _layer;

            Transform _HasChildren = child.GetComponentInChildren<Transform>();
            if (_HasChildren != null)
                SetGameLayerRecursive(child.gameObject, _layer);
        }
    }

    [ServerRpc]
    private void InitializePlayerServerRpc(NetworkConnection _conn)
    {
        foreach (var objj in _conn.Objects)
        {
            if (objj.gameObject.tag == "Player") obj = objj.gameObject;
        }

        obj.GetComponent<PredictedPlayerController>()._activated = true;

        obj.transform.position = spawnPos;
    }
}
