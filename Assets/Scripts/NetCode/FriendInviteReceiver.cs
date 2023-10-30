using FishNet.Connection;
using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FriendInviteReceiver : MonoBehaviour
{
    [System.NonSerialized] public NetworkObject inviteSender;
    private NetworkObject Player;

    private void Awake()
    {
        DontDestroyOnLoad(this);

        Player = GameObject.FindGameObjectWithTag("Player").GetComponent<NetworkObject>();

        gameObject.SetActive(false);
    }

    public void AcceptInvite()
    {
        FindObjectOfType<AudioManger>().Play("click1");
        FindObjectOfType<PartyInviteManager>().AskToSendYes(inviteSender, Player);
        gameObject.SetActive(false);
    }

    public void DeclineInvite()
    {
        FindObjectOfType<AudioManger>().Play("click2");
        gameObject.SetActive(false);
    }
}
