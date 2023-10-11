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

    private int maxPlayerCount = -1;

    

    public void PressedReady()
    {
        isReady = !isReady;

        if(startedGameCancalable)
        {
            //cancle game
            StopCoroutine(StartGameCancalable());
            CancleTimerVal.SetActive(false);
            cancleTimer = cancleTimerMax;
            startedGameCancalable = false;
        }

        SyncDataWithServer(this, isReady, LocalConnection.ClientId, startedGameCancalable);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SyncDataWithServer(LobbyManager _manager, bool _isReady, int _id, bool _startedGame)
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

        if(_startedGame)
        {
            StopCoroutine(StartGameCancalable());
        }
    }

    public override void OnStartNetwork()
    {     
        timer = timerMax;
        timerVal.text = Mathf.RoundToInt(timer).ToString();
        if (!base.IsServer)
        {
            Debug.Log("Trying to sync conn");
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
                maxPlayerCount = 2;
                break;
            case "2v2Lobby":
                maxPlayerCount = 4;
                break;
            case "3v3Lobby":
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

            if(readyStatus.Count > 1 && !readyStatus.ContainsValue(false) && lobbyIsFull)
            {
                //get players to load
                foreach (GameObject obj in gameObject.scene.GetRootGameObjects())
                {
                    if(obj.CompareTag("Player") && !connss.Contains(obj.GetComponent<NetworkObject>().Owner))
                    {
                        connss.Add(obj.GetComponent<NetworkObject>().Owner);
                        nobjsToLoad.Add(obj.GetComponent<NetworkObject>());
                    }
                }
                //start game cancalable (still have to make cancalable)
                StartCoroutine(StartGameCancalable());
                timer = timerMax;
                CancleTimerVal.SetActive(true);
                startedGameCancalable = true;
            }

            foreach(var item in readyStatus)
            {
                Debug.Log($"Key: {item.Key} + Value: {item.Value}");
            }
        }
    }

    [ObserversRpc]
    private void SyncCancleTimer(float _timer, GameObject obj1)
    {
        obj1.transform.Find("CanleTimerVal").gameObject.SetActive(true);
        obj1.transform.Find("CanleTimerVal").gameObject.GetComponent<TMP_Text>().text = Mathf.RoundToInt(_timer).ToString();
    }

    IEnumerator StartGameCancalable()
    {
        yield return new WaitForSeconds(5f);

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

    [ObserversRpc]
    private void TurnLoadingScreenOnClient()
    {
        EnableLoadingScreen();
    }

    private void UnloadLobbyScene()
    {
        UnityEngine.SceneManagement.Scene _Scene = gameObject.scene;
        foreach(var pair in base.SceneManager.SceneConnections)
        {
            if (pair.Value.Contains(conns[0]))
            {
                _Scene = pair.Key;
            }
        }
        
        //get rid of lobby scene
        Debug.Log($"Unloading lobby scene for {connss.Count} connections");
        SceneUnloadData sud = new SceneUnloadData(_Scene.handle);
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
