using FishNet.Connection;
using FishNet.Object;
using FishNet.Managing.Scened;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using FishNet;
using FishNet.Transporting;
using UnityEngine.UI;

public class LobbyManager : NetworkBehaviour
{
    [System.NonSerialized] public Dictionary<int, bool> readyStatus = new Dictionary<int, bool>();

    List<NetworkConnection> connss = new List<NetworkConnection>();
    List<NetworkObject> nobjsToLoad = new List<NetworkObject>();

    [SerializeField] private TMP_Text timerVal;

    [SerializeField] private GameObject LoadingScreen;

    [SerializeField] private GameObject CancleTimerVal;

    [SerializeField] private GameObject ReadyButn;

    [SerializeField] private Color readyColour;
    [SerializeField] private Color unReadyColour;

    private UnityEngine.SceneManagement.Scene thisLobbyScene;

    private List<NetworkConnection> conns = new();

    private int playerAmount;

    private bool lobbyIsFull;
    private bool isReady;
    private float timer;
    private float timerMax = 60;
    private bool startedGameCancalable;
    private bool startedGame;
    private float cancleTimer = 5;
    private float cancleTimerMax = 5;

    private int minPlayerCount = -1;
    private int maxPlayerCount = -1;

    public void PressedReady()
    {
        isReady = !isReady;

        if (isReady) ReadyButn.GetComponent<Image>().color = readyColour;
        else ReadyButn.GetComponent<Image>().color = unReadyColour;

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

        BackToMMServer(LocalConnection);
    }

    [ServerRpc(RequireOwnership = false)]
    private void BackToMMServer(NetworkConnection conn)
    {
        //remove lobby player item
        foreach (Transform child in transform.Find("PlayerHolder"))
        {
            if (child.GetComponent<PlayerItem>().ownerId == conn.ClientId) { base.ServerManager.Despawn(child.GetComponent<NetworkObject>()); Destroy(child); }
        }

        //go to sm scene
        NetworkObject Player = null;
        foreach (var obj in conn.Objects) if (obj.CompareTag("Player")) Player = obj;

        SceneLoadData sld = new SceneLoadData("Lobbies");
        sld.Options.AllowStacking = false;
        sld.MovedNetworkObjects = new NetworkObject[] { Player};
        base.SceneManager.LoadConnectionScenes(conn, sld);

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
        FindObjectOfType<MainMenu>(true).gameObject.SetActive(true);

        Cursor.lockState = CursorLockMode.None;

        ReadyButn.GetComponent<Image>().color = unReadyColour;

        timer = timerMax;
        timerVal.text = Mathf.RoundToInt(timer).ToString();
        if (!base.IsServerStarted)
        {
            conns.Add(LocalConnection);
            SyncCon(gameObject, conns);
            if(GameObject.Find("Lobbies") != null) GameObject.Find("Lobbies").SetActive(false);
        }

        //if (base.IsClient)
        //{
        //    FindObjectOfType<MainMenu>(true).gameObject.SetActive(false);
        //}

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
        if(base.IsServerStarted)
        {
            //int thing = 0;
            //foreach (var pair in base.SceneManager.SceneConnections)
            //{
            //    if(pair.Key == gameObject.scene) foreach(var conn in pair.Value) thing++;
            //}
            //Debug.Log($"connections amount: {thing}");
            
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
                SyncTimerClientRpc(timer);
            }
            else if(timer <= 0 && !startedGameCancalable)
            {
                startedGameCancalable = true;
                StartGameCancalable();
                SyncStartedGameClient();
            }
            if (timer > 0 && !lobbyIsFull) { timer = timerMax; SyncTimerClientRpc(timer); }

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

        //set ServerMenu inactive on the client
        TurnServMenuOffClient();

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
        SceneLookupData lookup = new SceneLookupData(0, "MapV1_1");
        SceneLoadData sld = new SceneLoadData(lookup);
        sld.Options.AllowStacking = true;
        sld.Options.LocalPhysics = UnityEngine.SceneManagement.LocalPhysicsMode.Physics3D;
        sld.MovedNetworkObjects = nobjsToLoad.ToArray();
        InstanceFinder.SceneManager.LoadConnectionScenes(connss.ToArray(), sld);

        Invoke(nameof(SetupGameScene), 1f);
        Invoke(nameof(UnloadLobbyScene), 1f);
    }

    private void SetupGameScene()
    {
        if (!startedGameCancalable) return;

        UnityEngine.SceneManagement.Scene scene = new();
        foreach (var pair in base.SceneManager.SceneConnections)
        {
            if (pair.Value.Contains(conns[0]) && !(pair.Key.name == "1v1Lobby" || pair.Key.name == "2v2Lobby" || pair.Key.name == "3v3Lobby" || pair.Key.name == "Lobbies" || pair.Key.name == "ServerMenu")) { scene = pair.Key; Debug.Log($"found scene with name: {scene.name}"); }
        }
        foreach (var obj in scene.GetRootGameObjects())
        {
            if (obj.name == "GameSetup") { obj.GetComponent<GameSetup>().gamemode = "1v1Deathmatch"; obj.GetComponent<GameSetup>().lobbyName = gameObject.scene.name; }
        }
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
    private void SyncTimerClientRpc(float _timer)
    {
        GameObject.Find("LobbyManager/Timer/TimerValue").GetComponent<TMP_Text>().text = Mathf.RoundToInt(_timer).ToString();
    }

    private void EnableLoadingScreen()
    {
        LoadingScreen.SetActive(true);
    }

    private void OnConnectionChange(NetworkConnection conn, RemoteConnectionStateArgs args)
    {
        if (!this.isActiveAndEnabled) return;
        if(args.ConnectionState == RemoteConnectionState.Stopped && base.IsServerStarted)
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

    [ObserversRpc]
    private void TurnServMenuOffClient()
    {
        FindObjectOfType<ServerMenu>(true).gameObject.SetActive(false);
    }
}
