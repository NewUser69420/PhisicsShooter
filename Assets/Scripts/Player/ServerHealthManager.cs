using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ServerHealthManager : NetworkBehaviour
{
    public List<float> health = new List<float>();

    public float totalHealth;
    public float maxHealth;

    public override void OnStartNetwork()
    {
        if(!IsServer) { return; }
        
        //SendHealthDataToClientRpc(base.ClientManager.Connection, totalHealth, maxHealth);
        
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

    public void OnHealthChange(int index, float newItem, float oldItem)
    {
        //check for below 0
        if (newItem < 0) health[index] = 0;

        //calc total health
        totalHealth += (newItem - oldItem);

        //send total health to client
        //SendHealthDataToClientRpc(base.ClientManager.Connection, totalHealth, maxHealth);

        //check for death
        //if (newItem <= 0 && (index == 0 || index == 7 || index == 8 || index == 9 || index == 18 || index == 19)) DoDeath(base.ClientManager.Connection, this.gameObject);
        if (newItem <= 0) DoDeath(base.ClientManager.Connection, this.gameObject);
    }

    private void DoDeath(NetworkConnection _conn, GameObject pobj)
    {
        //do death
        Debug.Log($"{_conn.ClientId} + died");

        SetMaxHealth();

        pobj.transform.position = new Vector3 (0, 5, 0);
    }

    [TargetRpc]
    private void SendHealthDataToClientRpc(NetworkConnection _conn, float _totalHealth, float _maxHealth)
    {
        foreach(NetworkObject obj in  _conn.Objects)
        {
            if(obj.gameObject.tag == "Player")
            {
                obj.GetComponentInChildren<UI>().totalHealth = _totalHealth;
                obj.GetComponentInChildren<UI>().maxHealth = _maxHealth;
            }
        }   
    }
}
