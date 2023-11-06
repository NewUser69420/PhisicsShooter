using FishNet.Connection;
using FishNet.Object;
using System.Collections.Generic;
using System.Xml.Linq;
using TMPro;
using UnityEngine;

public class PartyInviteManager : NetworkBehaviour
{
    [SerializeField] private TMP_InputField nameVal;

    [SerializeField] private GameObject PartyItemPrefab;

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

                foreach (var player in obj.GetComponent<Party>().party)
                {
                    foreach (var pobj in obj.GetComponent<Party>().party)
                    {
                        SyncPartyItem(player.Owner, pobj.GetComponent<ScoreTracker>().playerName);
                    }
                }

                foreach (var butn in FindObjectsOfType<LobbyButn>(true))
                {
                    GiveParty(obj.GetComponent<Party>().party, butn, obj.OwnerId);
                }
            }
        }
    }

    [TargetRpc]
    private void SyncPartyItem(NetworkConnection conn, string _pname)
    {
        List<string> pitems = new();
        foreach (Transform child in transform.Find("Holder")) { pitems.Add(child.GetComponentInChildren<TMP_Text>().text); }

        if(!pitems.Contains(_pname)) SpawnPartyItem(_pname);
    }

    public void GiveParty(List<NetworkObject> _party, LobbyButn _butn, int _id)
    {
        if (!_butn.party.ContainsKey(_id)) _butn.party.Add(_id, _party);
        else _butn.party[_id] = _party;
        Debug.Log("Setting party");
    }

    public void SpawnPartyItem(string pname)
    {
        if (pname == "playername not set") return;
        GameObject PartyItem = Instantiate(PartyItemPrefab, GameObject.Find("Lobbies/Party/Holder").transform);
        PartyItem.GetComponentInChildren<TMP_Text>().text = pname;
        
        foreach(var obj in LocalConnection.Objects) 
        {
            if (obj.CompareTag("Player"))
            {
                if (obj.GetComponent<ScoreTracker>().playerName == pname || !obj.GetComponent<Party>().isPartyLeader) PartyItem.transform.Find("KickButn").gameObject.SetActive(false);
            }
        }
    }

    public void AskToKickPlayer(string _name)
    {
        foreach (var obj in LocalConnection.Objects)
        {
            if(obj.CompareTag("Player")) KickPlayer(_name, obj);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void KickPlayer(string __name, NetworkObject pobj)
    {
        NetworkObject objtoremove = null;
        foreach (var player in pobj.GetComponent<Party>().party)
        {
            if (player.GetComponent<InitializePlayer>().playerName == __name) objtoremove = player;

            if (player.GetComponent<InitializePlayer>().playerName == __name) { DestroyParty(player.Owner, __name); player.GetComponent<Party>().isPartyLeader = true; }
            else DestroyPartyItem(player.Owner, __name);
        }

        pobj.GetComponent<Party>().party.Remove(objtoremove);

        foreach (var butn in FindObjectsOfType<LobbyButn>(true))
        {
            GiveParty(pobj.GetComponent<Party>().party, butn, pobj.OwnerId);
        }
    }

    [TargetRpc]
    private void DestroyPartyItem(NetworkConnection conn, string oof)
    {
        foreach (Transform child in GameObject.Find("Lobbies/Party/Holder").transform)
        {
            if (child.GetComponentInChildren<TMP_Text>().text == oof) Kill(child.gameObject);
        }
    }

    [TargetRpc]
    private void DestroyParty(NetworkConnection conn, string weebName)
    {
        foreach (Transform child in GameObject.Find("Lobbies/Party/Holder").transform)
        {
            if(child.GetComponentInChildren<TMP_Text>().text != weebName) Kill(child.gameObject);
        }
        foreach (var butn in FindObjectsOfType<LobbyButn>())
        {
            butn.GetComponent<LobbyButn>().isPartyLeader = true;
        }
    }

    private void Kill(GameObject weeb)
    {
        Destroy(weeb);
    }
}