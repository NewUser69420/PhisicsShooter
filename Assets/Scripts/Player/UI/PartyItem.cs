using TMPro;
using UnityEngine;

public class PartyItem : MonoBehaviour
{
    [System.NonSerialized] public GameObject LocalPlayer;

    public void KickPlayer()
    {
        Debug.Log($"Kicking Player: {GetComponentInChildren<TMP_Text>().text}");

        FindObjectOfType<PartyInviteManager>().AskToKickPlayer(GetComponentInChildren<TMP_Text>().text);
    }
}
