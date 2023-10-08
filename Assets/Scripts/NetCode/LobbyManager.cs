using FishNet.Connection;
using FishNet.Object;
using FishNet.Managing.Scened;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LobbyManager : NetworkBehaviour
{
    public List<NetworkObject> sceneObjects = new();

    [System.NonSerialized] public Dictionary<int, bool> readyStatus = new Dictionary<int, bool>();

    [SerializeField] private TMP_Text timerVal;

    private NetworkConnection conn;

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
        timer = timerMax;
        timerVal.text = Mathf.RoundToInt(timer).ToString();
        if (Owner.IsLocalClient)
        {
            conn = LocalConnection; SyncCon(this, conn);
        }
    }

    [ServerRpc]
    private void SyncCon(LobbyManager _manager, NetworkConnection _conn)
    {
        _manager.conn = _conn;
    }

    private void Update()
    {
        if(base.IsServer)
        {         
            playerAmount = 0;
            foreach (NetworkObject obj in sceneObjects)
            {
                if(obj.tag == "Player")
                {
                    Debug.Log(obj.name + obj.OwnerId);
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
                Debug.Log($"started game");
            }

            Debug.Log($"readystatuscount: {readyStatus.Count} + doesreadystatus contain false: {readyStatus.ContainsValue(false)} + is lobby full: {lobbyIsFull}");
            if(readyStatus.Count > 1 && !readyStatus.ContainsValue(false) && lobbyIsFull)
            {
                List<NetworkConnection> connsToLoad = new List<NetworkConnection>();
                List<NetworkObject> nobjsToLoad = new List<NetworkObject>();

                foreach (NetworkObject obj in sceneObjects)
                {
                    if(obj.CompareTag("Player"))
                    {
                        connsToLoad.Add(obj.Owner);
                        nobjsToLoad.Add(obj);
                    }
                }

            //start game cancalable
            Debug.Log($"started game");
            SceneLookupData lookup = new SceneLookupData(0, "SampleScene");
            SceneLoadData sld = new SceneLoadData(lookup);
            sld.Options.AllowStacking = false;
            sld.MovedNetworkObjects = nobjsToLoad.ToArray();
            //sld.Options.LocalPhysics = LocalPhysicsMode.Physics3D; //be carefull, might cause bugs. do more research
            base.SceneManager.LoadConnectionScenes(connsToLoad.ToArray(), sld);
        }

            foreach(var thing in readyStatus)
            {
                Debug.Log($"id: {thing.Key}  status: {thing.Value}");
            }
        }
    }

    [ObserversRpc]
    private void SyncTimerClientRpc(GameObject _timerVal, float _timer)
    {
        _timerVal.GetComponent<TMP_Text>().text = Mathf.RoundToInt(_timer).ToString();
    }
}
