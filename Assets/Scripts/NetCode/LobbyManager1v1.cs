using FishNet.Connection;
using FishNet.Object;
using System;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEngine;

public class LobbyManager1v1 : NetworkBehaviour
{
    [System.NonSerialized] public List<Status> readyStatus = new List<Status>();

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
        _manager.readyStatus.Add(new Status() { PlayerId = _id, PlayerStatus = _isReady});
    }

    private void Start()
    {
        timer = timerMax;
        if (Owner.IsLocalClient) { conn = LocalConnection; SyncCon(this, conn); }
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
            foreach(GameObject obj in GameObject.FindGameObjectsWithTag("Player"))
            {
                Debug.Log(obj.name);
                playerAmount++;
            }
            if (playerAmount == 2) lobbyIsFull = true;
            else lobbyIsFull = false;
            
            if(timer > 0 && lobbyIsFull)
            {
                timer -= Time.deltaTime;
                timerVal.text = Mathf.RoundToInt(timer).ToString();
            }
            else
            {
                //start game
                Debug.Log($"started game");
            }

            if(!readyStatus.Contains(new Status() { PlayerStatus = false}) && lobbyIsFull)
            {
                //start game cancalable
                Debug.Log($"started game");
            }

            foreach(var thing in readyStatus)
            {
                Debug.Log($"id: {thing.PlayerId}  status: {thing.PlayerStatus}");
            }
        }
    }
}

public class Status : IEquatable<Status>
{
    public int PlayerId { get; set; }

    public bool PlayerStatus { get; set; }

    public override string ToString()
    {
        return "ID: " + PlayerStatus + "   Name: " + PlayerId;
    }
    public override bool Equals(object obj)
    {
        if (obj == null) return false;
        Status objAsPart = obj as Status;
        if (objAsPart == null) return false;
        else return Equals(objAsPart);
    }
    public bool Equals(Status other)
    {
        if (other == null) return false;
        return (this.PlayerStatus.Equals(other.PlayerStatus));
    }
}
