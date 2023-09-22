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
        health.Insert(0, (maxHealth * 0.1f));
        health.Insert(1, (maxHealth * 0.0333f));
        health.Insert(2, (maxHealth * 0.0333f));
        health.Insert(3, (maxHealth * 0.0333f));
        health.Insert(4, (maxHealth * 0.0333f));
        health.Insert(5, (maxHealth * 0.0333f));
        health.Insert(6, (maxHealth * 0.0333f));
        health.Insert(7, (maxHealth * 0.05f));
        health.Insert(8, (maxHealth * 0.15f));
        health.Insert(9, (maxHealth * 0.2f));
        health.Insert(10, (maxHealth * 0.025f));
        health.Insert(11, (maxHealth * 0.025f));
        health.Insert(12, (maxHealth * 0.025f));
        health.Insert(13, (maxHealth * 0.025f));
        health.Insert(14, (maxHealth * 0.025f));
        health.Insert(15, (maxHealth * 0.025f));
        health.Insert(16, (maxHealth * 0.025f));
        health.Insert(17, (maxHealth * 0.025f));
        health.Insert(18, (maxHealth * 0.05f));
        health.Insert(19, (maxHealth * 0.05f));
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
        if (newItem <= 0 && (index == 0 || index == 7 || index == 8 || index == 9 || index == 18 || index == 19)) DoDeath(base.ClientManager.Connection, this.gameObject);
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
