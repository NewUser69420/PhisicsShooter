using FishNet;
using FishNet.Connection;
using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FriendInviteReceiver : NetworkBehaviour
{
    [System.NonSerialized] public NetworkObject inviteSender;

    public void AcceptInvite()
    {
        FindObjectOfType<AudioManger>().Play("click1");
        NetworkObject pobj = null;
        foreach (var obj in LocalConnection.Objects)
        {
            if(obj.CompareTag("Player")) pobj = obj;
        }
        FindObjectOfType<PartyInviteManager>().AskToSendYes(inviteSender, pobj);
        DisablePartyLeader(pobj);
        pobj.GetComponent<Party>().isPartyLeader = false;
        foreach(var thing in FindObjectsOfType<LobbyButn>()) thing.isPartyLeader = false;
        gameObject.SetActive(false);
    }

    [ServerRpc(RequireOwnership = false)]
    private void DisablePartyLeader(NetworkObject _pobj)
    {
        _pobj.GetComponent<Party>().isPartyLeader = false;
    }

    public void DeclineInvite()
    {
        FindObjectOfType<AudioManger>().Play("click2");
        gameObject.SetActive(false);
    }
}
