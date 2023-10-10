using FishNet.Connection;
using FishNet.Object;
using FishNet.Managing.Scened;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.Linq;
using FishNet;
using FishNet.Transporting;
using Unity.VisualScripting;

public class LobbyManager : NetworkBehaviour
{
    [System.NonSerialized] public Dictionary<int, bool> readyStatus = new Dictionary<int, bool>();

    List<NetworkConnection> conns = new List<NetworkConnection>();
    List<NetworkObject> nobjsToLoad = new List<NetworkObject>();

    [SerializeField] private TMP_Text timerVal;

    [SerializeField] private GameObject LoadingScreen;

    private UnityEngine.SceneManagement.Scene thisLobbyScene;

    private List<NetworkConnection> conn = new();

    private int playerAmount;

    private bool lobbyIsFull;
    private bool isReady = false;
    private float timer;
    private float timerMax = 60;

    

    public void PressedReady()
    {
        isReady = !isReady;
        SyncDataWithServer(this, isReady, LocalConnection.ClientId);
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
        base.ServerManager.OnRemoteConnectionState += OnConnectionChange;
        
        timer = timerMax;
        timerVal.text = Mathf.RoundToInt(timer).ToString();
        if (!base.IsServer)
        {
            Debug.Log("Trying to sync conn");
            conn.Add(LocalConnection);
            SyncCon(gameObject, conn);
            GameObject.Find("Lobbies").SetActive(false);
        }

        if (base.IsServer) Invoke(nameof(SetThisSceneVar), 1f);
        Invoke(nameof(TurnLoadingScreenOff), 1.5f);
    }

    private void TurnLoadingScreenOff()
    {
        LoadingScreen.SetActive(false);
    }

    private void SetThisSceneVar()
    {
        if (conn[0].Scenes == null) return;
        foreach(var scene in conn[0].Scenes)
        {
            if (scene.name != "Lobbies") thisLobbyScene = scene;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SyncCon(GameObject _manager, List<NetworkConnection> _conn)
    {
        _manager.GetComponent<LobbyManager>().conn = _conn;
    }

    private void Update()
    {
        if(base.IsServer)
        {         
            playerAmount = 0;
            foreach (GameObject obj in gameObject.scene.GetRootGameObjects())
            {
                if (obj == null) return;
                if(obj.tag == "Player")
                {
                    playerAmount++;
                }
            }
            if (playerAmount >= 2) lobbyIsFull = true;
            else lobbyIsFull = false;
            
            if(timer > 0 && lobbyIsFull)
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
                base.SceneManager.LoadConnectionScenes(conns.ToArray(), sld);

                Invoke(nameof(UnloadLobbyScene), 0.5f);
            }
            if (timer > 0 && !lobbyIsFull) { timer = timerMax; SyncTimerClientRpc(timerVal.gameObject, timer); }

            if(readyStatus.Count > 1 && !readyStatus.ContainsValue(false) && lobbyIsFull)
            {
                //get players to load
                foreach (GameObject obj in gameObject.scene.GetRootGameObjects())
                {
                    if(obj.CompareTag("Player"))
                    {
                        conns.Add(obj.GetComponent<NetworkObject>().Owner);
                        nobjsToLoad.Add(obj.GetComponent<NetworkObject>());
                    }
                }
                //start game cancalable (still have to make cancalable)
                EnableLoadingScreen();
                TurnLoadingScreenOnClient();

                Debug.Log($"started game");
                SceneLookupData lookup = new SceneLookupData(0, "SampleScene");
                SceneLoadData sld = new SceneLoadData(lookup);
                sld.Options.AllowStacking = false;
                sld.MovedNetworkObjects = nobjsToLoad.ToArray();
                //sld.Options.LocalPhysics = LocalPhysicsMode.Physics3D; //be carefull, might cause bugs. do more research
                base.SceneManager.LoadConnectionScenes(conns.ToArray(), sld);

                Invoke(nameof(UnloadLobbyScene), 0.5f);
            }

            foreach(var item in readyStatus)
            {
                Debug.Log($"Key: {item.Key} + Value: {item.Value}");
            }
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
        SceneUnloadData sud = new SceneUnloadData(thisLobbyScene);
        base.SceneManager.UnloadConnectionScenes(conns.ToArray(), sud);
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
                Debug.Log("Test0");
                if (child.name == "PlayerItem(Clone)" && child.GetComponent<PlayerItem>().ownerId == conn.ClientId)
                {
                    Debug.Log($"Test1");
                    if (child != null) base.Despawn(child.gameObject);
                    if (child != null) Destroy(child);

                    readyStatus.Remove(key: conn.ClientId);
                }
            }

            base.ServerManager.OnRemoteConnectionState -= OnConnectionChange;
        }
    }
}
