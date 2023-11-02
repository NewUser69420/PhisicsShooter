using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using FishNet.Object;
using FishNet.Connection;

public class PartyInviteManager : NetworkBehaviour
{
    [SerializeField] private TMP_InputField nameVal;

    [System.NonSerialized] public string playerName = "playerName not set";
    [System.NonSerialized] public NetworkObject Player;

    public List<NetworkObject> party = new();

    private string invitedName;

    public override void OnStartNetwork()
    {
        if (base.IsClient)
        {
            foreach (var obj in LocalConnection.Objects)
            {
                if (obj.CompareTag("Player")) Player = obj;
            }
            party.Add(Player);
            OnPartyChange();
        }
    }

    public void SendInvite()
    {
        invitedName = nameVal.text;
        if(party.Count < 4) InviteWithName(invitedName, Player);
    }

    [ServerRpc(RequireOwnership = false)]
    private void InviteWithName(string _name, NetworkObject _Player)
    {
        foreach (var p in FindObjectsOfType<ScoreTracker>())
        {
            if (p.playerName == _name)
            {
                Invite(p.GetComponent<NetworkObject>().Owner, _Player);
            }
        }
    }

    [TargetRpc]
    private void Invite(NetworkConnection conn, NetworkObject __Player)
    {
        GameObject receiver = FindObjectOfType<FriendInviteReceiver>(true).gameObject;
        receiver.SetActive(true);
        receiver.transform.Find("InviteText").GetComponent<TMP_Text>().text = __Player.GetComponent<ScoreTracker>().playerName + "has invited you to a party";
        receiver.GetComponent<FriendInviteReceiver>().inviteSender = __Player;
    }

    public void AskToSendYes(NetworkObject _sobj, NetworkObject _pobj)
    {
        AskToSendYesServer(_sobj, _pobj);
    }

    [ServerRpc(RequireOwnership = false)]
    private void AskToSendYesServer(NetworkObject __sobj, NetworkObject __pobj)
    {
        SendYes(__sobj.Owner, __pobj);
    }

    [TargetRpc]
    private void SendYes(NetworkConnection conn, NetworkObject ___pobj)
    {
        party.Add(___pobj);
        OnPartyChange();
    }

    public void OnPartyChange()
    {
        foreach (var butn in FindObjectsOfType<LobbyButn>())
        {
            GiveParty(party, butn);
        }
    }

    [ServerRpc]
    private void GiveParty(List<NetworkObject> _party, LobbyButn _butn)
    {
        _butn.party = _party;   
    }
}