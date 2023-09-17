using LiteNetLib;
using LiteNetLib.Layers;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;
using TMPro;
using FishNet;

public class ServerPlayerList : MonoBehaviour
{
    public int connectedPlayers = 0;
    public TMP_Text ConnectedPlayersText;

    private void Update()
    {
        connectedPlayers = InstanceFinder.ServerManager.Clients.Count;
        ConnectedPlayersText.text = connectedPlayers.ToString();
    }
}