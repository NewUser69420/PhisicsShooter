using FishNet.Connection;
using FishNet.Object;
using FishNet.Managing.Scened;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using FishNet;
using FishNet.Transporting;
using System.Collections;
using UnityEngine.Animations;
using System.Reflection;

public class LobbyManager : NetworkBehaviour
{
    [System.NonSerialized] public Dictionary<int, bool> readyStatus = new Dictionary<int, bool>();

    List<NetworkConnection> connss = new List<NetworkConnection>();
    List<NetworkObject> nobjsToLoad = new List<NetworkObject>();

    [SerializeField] private TMP_Text timerVal;

    [SerializeField] private GameObject LoadingScreen;

    [SerializeField] private GameObject CancleTimerVal;

    private UnityEngine.SceneManagement.Scene thisLobbyScene;

    private List<NetworkConnection> conns = new();

    private int playerAmount;

    private bool lobbyIsFull;
    private bool isReady = false;
    private float timer;
    private float timerMax = 60;
    private bool startedGameCancalable = false;
    private float cancleTimer = 5;
    private float cancleTimerMax = 5;

    private int minPlayerCount = -1;
    private int maxPlayerCount = -1;

    

    public void PressedReady()
    {
        isReady = !isReady;

        if(startedGameCancalable)
        {
            //cancle game
            CancleTimerVal.SetActive(false);
            SyncStopGameWithServer();
            startedGameCancalable = false;
        }

        SyncDataWithServer(this, isReady, LocalConnection.ClientId);
    }

    public void PressedBackToMM()
    {
        FindObjectOfType<AudioManger>().Play("click2");
        GameObject LB = FindObjectOfType<LobbyButn>(true).transform.parent.parent.gameObject;
        LB.SetActive(true);
        LB.transform.Find("LoadingScreen").gameObject.SetActive(false);
        BackToMMServer(LocalConnection);
    }

    [ServerRpc(RequireOwnership = false)]
    private void BackToMMServer(NetworkConnection conn)
    {
        foreach (var obj in conn.Objects)
        {
            if (obj.CompareTag("Player")) UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(obj.gameObject, "Lobbies");
        }
        
        SceneUnloadData sud = new SceneUnloadData(gameObject.scene);
        base.SceneManager.UnloadConnectionScenes(conn, sud);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SyncStopGameWithServer()
    {
        startedGameCancalable = false;
        TurnCancleTimerOff();
    }

    [ObserversRpc]
    private void TurnCancleTimerOff()
    {
        CancleTimerVal.SetActive(false);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SyncDataWithServer(LobbyManager _manager, bool _isReady, int _id)
    {
        if(!_manager.readyStatus.ContainsKey(_id))
        {
            //make new instance
            _manager.readyStatus.Add(_id, _isReady);
        }
        else
        {
            //edit instance
            _manager.readyStatus[_id] = _isReady;
        }
    }

    public override void OnStartNetwork()
    {     
        timer = timerMax;
        timerVal.text = Mathf.RoundToInt(timer).ToString();
        if (!base.IsServer)
        {
            conns.Add(LocalConnection);
            SyncCon(gameObject, conns);
            GameObject.Find("Lobbies").SetActive(false);
        }

        Invoke(nameof(TurnLoadingScreenOff), 1.5f);
        thisLobbyScene = gameObject.scene;

        //set maxPlayerCount
        switch(gameObject.scene.name)
        {
            case "1v1Lobby":
                minPlayerCount = 1;
                maxPlayerCount = 2;
                break;
            case "2v2Lobby":
                minPlayerCount = 3;
                maxPlayerCount = 4;
                break;
            case "3v3Lobby":
                minPlayerCount = 5;
                maxPlayerCount = 6;
                break;
        }
    }

    private void OnEnable()
    {
        InstanceFinder.ServerManager.OnRemoteConnectionState += OnConnectionChange;
    }
    private void OnDisable()
    {
        InstanceFinder.ServerManager.OnRemoteConnectionState -= OnConnectionChange;
    }

    private void TurnLoadingScreenOff()
    {
        LoadingScreen.SetActive(false);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SyncCon(GameObject _manager, List<NetworkConnection> _conn)
    {
        _manager.GetComponent<LobbyManager>().conns = _conn;
    }

    private void Update()
    {
        if(base.IsServer)
        {
            if (cancleTimer > 0 && startedGameCancalable)
            {
                cancleTimer -= Time.deltaTime;
                CancleTimerVal.GetComponent<TMP_Text>().text = Mathf.RoundToInt(cancleTimer).ToString();
                SyncCancleTimer(cancleTimer, gameObject);
            }
            
            playerAmount = 0;
            foreach (GameObject obj in gameObject.scene.GetRootGameObjects())
            {
                if (obj == null) return;
                if(obj.tag == "Player")
                {
                    playerAmount++;
                }
            }
            if (playerAmount >= maxPlayerCount) lobbyIsFull = true;
            else lobbyIsFull = false;
            
            if(timer > 0 && lobbyIsFull && !startedGameCancalable)
            {
                timer -= Time.deltaTime;
                timerVal.text = Mathf.RoundToInt(timer).ToString();
                SyncTimerClientRpc(timerVal.gameObject, timer);
            }
            else if(timer <= 0)
            {
                //start game
                EnableLoadingScreen();
                TurnLoadingScreenOnClient();

                Debug.Log($"started game");
                SceneLookupData lookup = new SceneLookupData(0, "SampleScene");
                SceneLoadData sld = new SceneLoadData(lookup);
                sld.Options.AllowStacking = false;
                sld.MovedNetworkObjects = nobjsToLoad.ToArray();
                //sld.Options.LocalPhysics = LocalPhysicsMode.Physics3D; //be carefull, might cause bugs. do more research
                base.SceneManager.LoadConnectionScenes(connss.ToArray(), sld);

                Invoke(nameof(UnloadLobbyScene), 0.5f);
            }
            if (timer > 0 && !lobbyIsFull) { timer = timerMax; SyncTimerClientRpc(timerVal.gameObject, timer); }

            if (readyStatus.Count == maxPlayerCount && !readyStatus.ContainsValue(false) && !startedGameCancalable)
            {
                //start game cancalable (still have to make cancalable)
                Invoke(nameof(StartGameCancalable), 5f);
                timer = timerMax;
                cancleTimer = cancleTimerMax;
                CancleTimerVal.SetActive(true);
                startedGameCancalable = true;
                SyncStartedGameClient();
            }
            else if(readyStatus.Count == minPlayerCount && playerAmount != maxPlayerCount && !readyStatus.ContainsValue(false) && !startedGameCancalable)
            {
                //start game cancalable (still have to make cancalable)
                Invoke(nameof(StartGameCancalable), 5f);
                timer = timerMax;
                cancleTimer = cancleTimerMax;
                CancleTimerVal.SetActive(true);
                startedGameCancalable = true;
                SyncStartedGameClient();
            }

            foreach(var item in readyStatus)
            {
                Debug.Log($"Key: {item.Key} + Value: {item.Value}");
            }
        }
    }

    [ObserversRpc]
    private void SyncStartedGameClient()
    {
        startedGameCancalable = true;
    }

    [ObserversRpc]
    private void SyncCancleTimer(float _timer, GameObject obj1)
    {
        obj1.transform.Find("CanleTimerVal").gameObject.SetActive(true);
        obj1.transform.Find("CanleTimerVal").gameObject.GetComponent<TMP_Text>().text = Mathf.RoundToInt(_timer).ToString();
    }

    private void StartGameCancalable()
    {
        if (!startedGameCancalable) return;

        //get players to load
        foreach (GameObject obj in gameObject.scene.GetRootGameObjects())
        {
            if (obj.CompareTag("Player") && !connss.Contains(obj.GetComponent<NetworkObject>().Owner))
            {
                connss.Add(obj.GetComponent<NetworkObject>().Owner);
                nobjsToLoad.Add(obj.GetComponent<NetworkObject>());
            }
        }

        EnableLoadingScreen();
        TurnLoadingScreenOnClient();

        Debug.Log($"started game");
        SceneLookupData lookup = new SceneLookupData(0, "SampleScene");
        SceneLoadData sld = new SceneLoadData(lookup);
        sld.Options.AllowStacking = false;
        sld.MovedNetworkObjects = nobjsToLoad.ToArray();
        //sld.Options.LocalPhysics = LocalPhysicsMode.Physics3D; //be carefull, might cause bugs. do more research
        InstanceFinder.SceneManager.LoadConnectionScenes(connss.ToArray(), sld);

        Invoke(nameof(UnloadLobbyScene), 1f);
    }

    [ObserversRpc]
    private void TurnLoadingScreenOnClient()
    {
        EnableLoadingScreen();
    }

    private void UnloadLobbyScene()
    {        
        //get rid of lobby scene
        Debug.Log($"Unloading lobby scene for {connss.Count} connections");
        SceneUnloadData sud = new SceneUnloadData(thisLobbyScene);
        base.SceneManager.UnloadConnectionScenes(connss.ToArray(), sud);
    }

    [ObserversRpc]
    private void SyncTimerClientRpc(GameObject _timerVal, float _timer)
    {
        _timerVal.GetComponent<TMP_Text>().text = Mathf.RoundToInt(_timer).ToString();
    }

    private void EnableLoadingScreen()
    {
        LoadingScreen.SetActive(true);
    }

    private void OnConnectionChange(NetworkConnection conn, RemoteConnectionStateArgs args)
    {
        if (!this.isActiveAndEnabled) return;
        if(args.ConnectionState == RemoteConnectionState.Stopped && base.IsServer)
        {
            foreach(Transform child in gameObject.transform.Find("PlayerHolder"))
            {
                if (child.name == "PlayerItem(Clone)" && child.GetComponent<PlayerItem>().ownerId == conn.ClientId)
                {
                    if (child != null) base.Despawn(child.gameObject);
                    if (child != null) Destroy(child);

                    readyStatus.Remove(key: conn.ClientId);
                }
            }

            base.ServerManager.OnRemoteConnectionState -= OnConnectionChange;
        }
    }
}
