using FishNet.Connection;
using FishNet.Object;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LobbyManager1v1 : NetworkBehaviour
{
    private List<GameObject> sceneObjects = new();

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

    [ServerRpc]
    private void SyncDataWithServer(LobbyManager1v1 _manager, bool _isReady, int _id)
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

            foreach (var thing in base.SceneManager.SceneConnections)
            {
                if (thing.Key == gameObject.scene)
                {
                    foreach (var obj in thing.Key.GetRootGameObjects())
                    {
                        sceneObjects.Add(obj);
                    }
                }
            }
        }
    }

    [ServerRpc]
    private void SyncCon(LobbyManager1v1 _manager, NetworkConnection _conn)
    {
        _manager.conn = _conn;
    }

    private void Update()
    {
        if(base.IsServer)
        {
            playerAmount = 0;
            foreach (GameObject obj in sceneObjects)
            {
                if(obj.tag == "Player")
                {
                    Debug.Log(obj.name + obj.GetComponent<NetworkObject>().OwnerId);
                    playerAmount++;
                }
            }
            if (playerAmount == 2) lobbyIsFull = true;
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

            if(!readyStatus.ContainsValue(false) && lobbyIsFull)
            {
                //start game cancalable
                Debug.Log($"started game");
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
        Debug.Log($"TEST1");
        _timerVal.GetComponent<TMP_Text>().text = Mathf.RoundToInt(_timer).ToString();
    }
}
