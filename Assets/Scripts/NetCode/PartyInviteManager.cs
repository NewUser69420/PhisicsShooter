using FishNet.Connection;
using FishNet.Object;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PartyInviteManager : NetworkBehaviour
{
    [SerializeField] private TMP_InputField nameVal;

    [System.NonSerialized] public string playerName = "playerName not set";
    [System.NonSerialized] public NetworkObject Player;

    private string invitedName;

    public void SendInvite()
    {
        NetworkObject pobj = null;
        foreach (var obj in LocalConnection.Objects)
        {
            if (obj.CompareTag("Player")) pobj = obj;
        }

        invitedName = nameVal.text;
        if (pobj.GetComponent<Party>().party.Count < 3 && nameVal.text != playerName) InviteWithName(invitedName, LocalConnection);
        else { GameObject.Find("Lobbies/Party/ToManyFriendsError").SetActive(true); Invoke(nameof(TurnOffError), 2f); }
    }

    private void TurnOffError()
    {
        GameObject.Find("Lobbies/Party/ToManyFriendsError").SetActive(false);
    }

    [ServerRpc(RequireOwnership = false)]
    private void InviteWithName(string _name, NetworkConnection senderConn)
    {
        NetworkObject _Player = null;
        foreach(var obj in senderConn.Objects) { Debug.Log(obj.name); if (obj.CompareTag("Player")) _Player = obj; }
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
        receiver.transform.Find("Object").gameObject.SetActive(true);
        receiver.transform.Find("Object/InviteText").GetComponent<TMP_Text>().text = __Player.GetComponent<ScoreTracker>().playerName + "has invited you to a party";
        receiver.GetComponent<FriendInviteReceiver>().inviteSender = __Player;
    }

    public void AskToSendYes(NetworkObject _sobj, NetworkObject pobj)
    {
        AskToSendYesServer(_sobj, pobj);
    }

    [ServerRpc(RequireOwnership = false)]
    private void AskToSendYesServer(NetworkObject __sobj, NetworkObject _pobj)
    {
        foreach (var obj in __sobj.Owner.Objects)
        {
            if (obj.CompareTag("Player")) 
            { 
                obj.GetComponent<Party>().party.Add(_pobj);


                foreach (var butn in FindObjectsOfType<LobbyButn>(true))
                {
                    GiveParty(obj.GetComponent<Party>().party, butn, obj.OwnerId);
                }
            }
        }
    }

    public void GiveParty(List<NetworkObject> _party, LobbyButn _butn, int _id)
    {
        //if (!_butn.party.TryAdd(_id, _party)) { _butn.party.Remove(_id); _butn.party.Add(_id, _party); }
        if (!_butn.party.ContainsKey(_id)) _butn.party.Add(_id, _party);
        else _butn.party[_id] = _party;
        Debug.Log("Setting party");
    }
}