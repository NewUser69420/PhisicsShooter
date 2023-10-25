using FishNet.Connection;
using FishNet.Managing.Scened;
using FishNet.Object;
using FishNet.Transporting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.ProBuilder.Shapes;

public class InitializePlayer : NetworkBehaviour
{        
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
        Invoke(nameof(DoStartNetwork), 1f);
    }

    private void DoStartNetwork()
    {
        MainMenuUI = GameObject.Find("MainMenuUI");

        conId = OwnerId;

        base.SceneManager.OnLoadEnd += OnLoadScene;
        

        if (base.Owner.IsLocalClient)
        {
            SetGameLayerRecursive(this.gameObject, 6);

            if (MainMenuUI != null) { playerName = MainMenuUI.GetComponent<MainMenu>().playerName; SyncPlayerName(playerName); }

            PlayerName.GetComponent<TMP_Text>().text = playerName;
            SyncNameServer(playerName, PlayerName.gameObject);
        }
    }

    [ServerRpc]
    private void SyncPlayerName(string _name)
    {
        playerName = _name;
    }

    public void OnLoadScene(SceneLoadEndEventArgs args)
    {
        foreach (var _scene in args.LoadedScenes)
        {
            if (_scene.name == "1v1Lobby" || _scene.name == "2v2Lobby" || _scene.name == "3v3Lobby")
            {
                if (Owner.IsLocalClient) StartCoroutine(Wait3());
            }
            if (_scene.name != "Lobbies" && base.IsClient) UnityEngine.SceneManagement.SceneManager.SetActiveScene(_scene);
        }
    }

    IEnumerator Wait3()
    {
        yield return new WaitForSeconds(1f);

        SyncPlayerItemServer(OwnerId, PlayerItemPrefab, playerName);
    }

    [ServerRpc]
    private void SyncPlayerItemServer(int _id, GameObject prefab, string _playerName)
    {
        Transform Parent = null;
        foreach (GameObject obj in gameObject.scene.GetRootGameObjects())
        {
            if (obj.name == "LobbyManager") Parent = obj.transform.Find("PlayerHolder");
        }
        GameObject _PlayerItem = Instantiate(prefab, Parent);
        _PlayerItem.GetComponentInChildren<TMP_Text>().text = _playerName;
        _PlayerItem.GetComponent<PlayerItem>().ownerId = _id;
        PlayerItems.Add(_PlayerItem);
        base.Spawn(_PlayerItem);
        SyncPlayerItemClient(_id, PlayerItems, _playerName);
    }

    [ObserversRpc]
    private void SyncPlayerItemClient(int __id, List<GameObject> __PlayerItems, string __playerName)
    {
        foreach (var _obj in __PlayerItems)
        {
            if (_obj == null) { Debug.Log("Can't Fix lobby Player Items");  return; }
                Debug.Log($"playeritem id = {_obj.GetComponent<PlayerItem>().ownerId}");
            foreach (var __obj in gameObject.scene.GetRootGameObjects())
            {
                if (__obj.name == "LobbyManager") { _obj.transform.SetParent(__obj.transform.Find("PlayerHolder")); _obj.GetComponentInChildren<TMP_Text>().text = __playerName; _obj.GetComponent<PlayerItem>().ownerId = __id; }
            }
        }
    }

    [TargetRpc]
    public void InitializeThePlayerOnClient(NetworkConnection _conn)
    {
        if (base.IsOwner)
        {
            InitializePlayerServerRpc(base.LocalConnection);

            Cursor.lockState = CursorLockMode.Locked;

            //make scoreboard item and activate player
            StartCoroutine(Wait2());

            GameObject.Find("MainMenuUI").SetActive(false);
            foreach(var obj in gameObject.scene.GetRootGameObjects())
            {
                if(obj.name == "EventSystem") obj.SetActive(true);
            }

            StartCoroutine(Wait());
        }
    }

    IEnumerator Wait2()
    {
        yield return new WaitForSeconds(1f);

        foreach (var obj in gameObject.scene.GetRootGameObjects())
        {
            if(obj.CompareTag("Player")) { SyncScoreboardServer(ScoreboardItemPrefab, obj.GetComponent<NetworkObject>().OwnerId, obj.GetComponent<NetworkObject>().Owner); }
        }

        //activate ui
        Invoke(nameof(SetUIActive), 2f);
    }

    private void SetUIActive()
    {
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
        yield return new WaitForSeconds(1f);

        DoName();
    }

    [ServerRpc]
    private void DoName()
    {
        List<GameObject> pobjs = new List<GameObject>();
        GameObject obj = null;
        String pName = null;
        int id = 0;
        foreach (var pair in base.SceneManager.SceneConnections)
        {
            if (pair.Key == gameObject.scene)
            {
                foreach (var conn in pair.Value)
                {
                    foreach (var pobj in conn.Objects)
                    {
                        if (pobj.gameObject.tag == "Player")
                        {
                            obj = pobj.transform.Find("NameCanvas/PlayerName").gameObject;
                            if (pobj.GetComponent<NetworkObject>().OwnerId == conn.ClientId) pName = pobj.transform.Find("NameCanvas/PlayerName").GetComponent<TMP_Text>().text;
                            else Debug.Log($"No match: {pobj.GetComponent<NetworkObject>().OwnerId} != {conn.ClientId}");
                        }
                    }
                    id = conn.ClientId;

                    Debug.Log($"sending client rpc with obj:{obj.name} + name:{pName} + id:{id}");
                    SyncNameClient(obj, pName, id);
                }
            }
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
    }
}
