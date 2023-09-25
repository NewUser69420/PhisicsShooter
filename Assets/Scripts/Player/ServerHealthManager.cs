using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;

public class ServerHealthManager : NetworkBehaviour
{
    public List<float> health = new List<float>();

    public float totalHealth;
    public float maxHealth;

    public override void OnStartNetwork()
    {
        if (!IsServer) { return; }

        totalHealth = maxHealth;

        SetMaxHealth();
    }

    private void SetMaxHealth()
    {
        health.Insert(0, Mathf.RoundToInt((maxHealth * 0.0469f)));
        health.Insert(1, Mathf.RoundToInt((maxHealth * 0.0587f)));
        health.Insert(2, Mathf.RoundToInt((maxHealth * 0.0646f)));
        health.Insert(3, Mathf.RoundToInt((maxHealth * 0.0704f)));
        health.Insert(4, Mathf.RoundToInt((maxHealth * 0.0587f)));
        health.Insert(5, Mathf.RoundToInt((maxHealth * 0.0646f)));
        health.Insert(6, Mathf.RoundToInt((maxHealth * 0.0704f)));
        health.Insert(7, Mathf.RoundToInt((maxHealth * 0.0235f)));
        health.Insert(8, Mathf.RoundToInt((maxHealth * 0.0176f)));
        health.Insert(9, Mathf.RoundToInt((maxHealth * 0.0117f)));
        health.Insert(10, Mathf.RoundToInt((maxHealth * 0.0587f)));
        health.Insert(11, Mathf.RoundToInt((maxHealth * 0.0587f)));
        health.Insert(12, Mathf.RoundToInt((maxHealth * 0.0646f)));
        health.Insert(13, Mathf.RoundToInt((maxHealth * 0.0704f)));
        health.Insert(14, Mathf.RoundToInt((maxHealth * 0.0587f)));
        health.Insert(15, Mathf.RoundToInt((maxHealth * 0.0587f)));
        health.Insert(16, Mathf.RoundToInt((maxHealth * 0.0646f)));
        health.Insert(17, Mathf.RoundToInt((maxHealth * 0.0704f)));
        health.Insert(18, Mathf.RoundToInt((maxHealth * 0.0059f)));
        health.Insert(19, Mathf.RoundToInt((maxHealth * 0.0023f)));
    }

    public void OnHealthChange(int index, float newItem, float oldItem, GameObject shooter)
    {
        //check for below 0
        if (newItem < 0) health[index] = 0;

        //calc total health
        totalHealth += (newItem - oldItem);

        //check for death
        if (newItem <= 0) DoDeath(base.OwnerId, this.gameObject, shooter);
    }

    private void DoDeath(int _id, GameObject pobj, GameObject _shooter)
    {
        //do death
        SetMaxHealth();

        pobj.transform.position = new Vector3(0, 5, 0);

        //add deathcounter on client
        FindObjectOfType<Killer>().DoDeathCounterRpc(_id, pobj, _shooter.gameObject);

        //add killcounter on client
        NetworkConnection _conn = null;
        _conn = _shooter.gameObject.GetComponent<NetworkObject>().LocalConnection;
        FindObjectOfType<Killer>().DoKillCounterRpc(_conn);
    }
}
