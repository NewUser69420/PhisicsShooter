using FishNet.Connection;
using FishNet.Managing.Scened;
using FishNet.Object;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class InitializePlayer : NetworkBehaviour
{    
    [SerializeField] private List<NetworkObject> sceneObjects = new();

    [SerializeField]  private Vector3 spawnPos;
    [SerializeField] private int conId = -1;

    [System.NonSerialized]  public string playerName = "playername not set";

    [SerializeField] private GameObject MainMenuUI;
    [SerializeField] private Transform UI;
    [SerializeField] private Transform PlayerName;

    [SerializeField] private GameObject ScoreboardItemPrefab;
    [SerializeField] private GameObject PlayerItemPrefab;

    private GameObject obj;
    private List<GameObject> PlayerItems = new List<GameObject>();

    public override void OnStartNetwork()
    {
        Invoke(nameof(DoStartNetwork), 0.5f);
    }

    private void DoStartNetwork()
    {
        MainMenuUI = GameObject.Find("MainMenuUI");

        if (base.IsServer) DoSceneObjList();

        conId = OwnerId;

        base.SceneManager.OnLoadEnd += OnLoadScene;

        if (base.Owner.IsLocalClient)
        {
            SetGameLayerRecursive(this.gameObject, 6);

            playerName = MainMenuUI.GetComponent<MainMenu>().playerName;
            PlayerName.GetComponent<TMP_Text>().text = playerName;
            SyncNameServer(playerName, PlayerName.gameObject);
        }
    }

    private void DoSceneObjList()
    {
        sceneObjects.Clear();
        foreach (var pair in base.SceneManager.SceneConnections)
        {
            Debug.Log($"{pair.Key.name} + {gameObject.scene.name}");
            if (pair.Key == gameObject.scene)
            {
                foreach (var obj in pair.Key.GetRootGameObjects())
                {
                    if (obj.GetComponent<NetworkObject>() != null)
                    {
                        Debug.Log($"added obj: {obj.name}");
                        sceneObjects.Add(obj.GetComponent<NetworkObject>());
                    }
                }
            }
        }
        SyncSceneObjectList(sceneObjects, this);
    }

    [ObserversRpc]
    private void SyncSceneObjectList(List<NetworkObject> _list, InitializePlayer _script)
    {
        _script.sceneObjects = _list;
    }

    public void OnLoadScene(SceneLoadEndEventArgs args)
    {
        if (base.IsServer) Invoke(nameof(DoSceneObjList), 0.5f);

        foreach (var _scene in args.LoadedScenes)
        {
            if (_scene.name == "1v1Lobby" || _scene.name == "2v2Lobby" || _scene.name == "3v3Lobby")
            {
                if (Owner.IsLocalClient) StartCoroutine(Wait3());
                if (Owner.IsLocalClient || base.IsServer) StartCoroutine(Wait4(sceneObjects));
            }
            if (_scene.name != "Lobbies" && base.IsClient) UnityEngine.SceneManagement.SceneManager.SetActiveScene(_scene);
        }
    }

    IEnumerator Wait3()
    {
        yield return new WaitForSeconds(1f);

        SyncPlayerItemServer(PlayerItemPrefab, playerName);
    }

    IEnumerator Wait4(List<NetworkObject> _sceneObjects)
    {
        yield return new WaitForSeconds(1f);
        foreach (NetworkObject obj in _sceneObjects)
        {
            if (obj == null) break;
            if (obj.name == "LobbyManager")
            {
                obj.GetComponent<LobbyManager>().sceneObjects = _sceneObjects;
            }
        }
    }

    [ServerRpc]
    private void SyncPlayerItemServer(GameObject prefab, string _playerName)
    {
        Transform Parent = null;
        foreach (NetworkObject obj in sceneObjects)
        {
            if (obj.name == "LobbyManager") Parent = obj.transform.Find("PlayerHolder");
        }
        GameObject _PlayerItem = Instantiate(prefab, Parent);
        _PlayerItem.GetComponentInChildren<TMP_Text>().text = _playerName;
        PlayerItems.Add(_PlayerItem);
        base.Spawn(_PlayerItem);
        SyncPlayerItemClient(PlayerItems, _playerName);
    }

    [ObserversRpc]
    private void SyncPlayerItemClient(List<GameObject> __PlayerItems, string __playerName)
    {
        foreach(var _obj in __PlayerItems)
        {
            Debug.Log($"Test2");
            foreach (NetworkObject __obj in sceneObjects)
            {
                Debug.Log($"Test3");
                if (__obj.name == "LobbyManager") { _obj.transform.SetParent(__obj.transform.Find("PlayerHolder")); __obj.GetComponentInChildren<TMP_Text>().text = __playerName; }
            }
        }
    }

    [TargetRpc]
    public void InitializeThePlayerOnClient(NetworkConnection _conn)
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
