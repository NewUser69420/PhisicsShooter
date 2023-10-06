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
    public string sceneName;
    
    public Vector3 spawnPos;

    public string playerName = "playername not set";

    public GameObject MainMenuUI;
    public Transform UI;
    public Transform PlayerName;

    public GameObject ScoreboardItemPrefab;
    public GameObject PlayerItemPrefab;

    private GameObject obj;

    public override void OnStartNetwork()
    {
        MainMenuUI = GameObject.Find("MainMenuUI");

        base.SceneManager.OnLoadEnd += OnInitializePlayer;
        
        if(base.Owner.IsLocalClient)
        {
            SetGameLayerRecursive(this.gameObject, 6);

            playerName = MainMenuUI.GetComponent<MainMenu>().playerName;
            PlayerName.GetComponent<TMP_Text>().text = playerName;
            SyncNameServer(playerName, PlayerName.gameObject);
        }
    }

    public void OnInitializePlayer(SceneLoadEndEventArgs args)
    {
        foreach(var scene in args.LoadedScenes)
        {
            if (scene.name == sceneName)
            {
                InitializeThePlayerOnClient(Owner);
            }
            else if (scene.name == "1v1Lobby" || scene.name == "2v2Lobby" || scene.name == "3v3Lobby")
            {
                StartCoroutine(Wait3());
            }
        }
    }

    IEnumerator Wait3()
    {
        yield return new WaitForSeconds(0.5f);
        GameObject PlayerItem = Instantiate(PlayerItemPrefab, GameObject.Find("LobbyManager/PlayerHolder").transform);
        PlayerItem.GetComponentInChildren<TMP_Text>().text = playerName;

        SyncPlayerItemServer(PlayerItemPrefab, playerName);
    }

    [ServerRpc]
    private void SyncPlayerItemServer(GameObject prefab, string _playerName)
    {
        GameObject _PlayerItem = Instantiate(prefab, GameObject.Find("LobbyManager/PlayerHolder").transform);
        _PlayerItem.GetComponentInChildren<TMP_Text>().text = _playerName;

        SyncPlayerItemClient(prefab, _playerName);
    }

    [ObserversRpc]
    private void SyncPlayerItemClient(GameObject _prefab, string __playerName)
    {
        GameObject __PlayerItem = Instantiate(_prefab, GameObject.Find("LobbyManager/PlayerHolder").transform);
        __PlayerItem.GetComponentInChildren<TMP_Text>().text = __playerName;
    }

    [TargetRpc]
    private void InitializeThePlayerOnClient(NetworkConnection _conn)
    {
        if (base.Owner.IsLocalClient)
        {
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
        int _kills = 0;
        int _deaths = 0;
        foreach(NetworkObject ___obj in conn.Objects)
        {
            if (___obj.tag == "Player")
            {
                _playerName = ___obj.transform.Find("NameCanvas/PlayerName").GetComponent<TMP_Text>().text;
                _kills = ___obj.GetComponent<ServerHealthManager>().kills;
                _deaths = ___obj.GetComponent<ServerHealthManager>().deaths;
            }
        }
        Debug.Log($"spawning scoreboardItem with name: {_playerName} and k/d: {_kills}/{_deaths}");
        SyncScoreboardClient(obj , id, _playerName, _kills, _deaths);
    }

    [ObserversRpc]
    private void SyncScoreboardClient(GameObject _obj, int _id, string __playerName, int __kills, int __deaths)
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
                    objj.GetComponent<ScoreBoardItemTracker>().kills = __kills;
                    objj.GetComponent<ScoreBoardItemTracker>().deaths = __deaths;
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
        if(_go.layer != 13) _go.layer = _layer;
        foreach (Transform child in _go.transform)
        {
            if(child.gameObject.layer != 13) child.gameObject.layer = _layer;

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
