using FishNet;
using FishNet.Managing.Client;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScoreBoardItemTracker : MonoBehaviour
{
    [SerializeField] private TMP_Text killsText;
    [SerializeField] private TMP_Text deathsText;
    [SerializeField] private TMP_Text nameText;

    [System.NonSerialized] public int kills;
    [System.NonSerialized ] public int deaths;
    [System.NonSerialized] public string nameValue;
    [System.NonSerialized] public int id = -1;

    private List<int> deathCounter = new List<int>();

    private void Update()
    {
        killsText.text = kills.ToString();
        deathsText.text = deaths.ToString();
        nameText.text = nameValue;
            
        deathCounter.Clear();
        foreach(var client in InstanceFinder.ClientManager.Clients)
        {
            if (client.Value.ClientId == id) deathCounter.Insert(0, 1);
        }

        if(deathCounter.Count == 0) Destroy(gameObject);
    }
}
