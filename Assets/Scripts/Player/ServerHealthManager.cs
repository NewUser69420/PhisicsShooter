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

    private NetworkConnection Conn;
    private bool invinceble;

    public override void OnStartNetwork()
    {
        if (base.IsClient)
        {
            GiveLocalConn(base.LocalConnection);
        }
        
        if (!IsServer) { return; }

        totalHealth = maxHealth;

        SetMaxHealth();
    }

    [ServerRpc]
    private void GiveLocalConn(NetworkConnection _conn)
    {
        Conn = _conn;
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

    public void OnHealthChange(int index, float newItem, float oldItem, NetworkConnection shooter)
    {
        //check for below 0
        if (newItem < 0) health[index] = 0;

        //calc total health
        totalHealth += (newItem - oldItem);

        //check for death
        if (newItem <= 0 && !invinceble) DoDeath(Conn, this.gameObject, shooter);
    }

    private void DoDeath(NetworkConnection _killedCon, GameObject pobj, NetworkConnection _shooterConn)
    {
        StartCoroutine(Wait());
        
        //do death
        SetMaxHealth();

        pobj.transform.position = new Vector3(0, 5, 0);

        //add deathcounter on client
        GameObject _shooterObj = null;
        foreach (NetworkObject obj in _shooterConn.Objects)
        {
            if (obj.tag == "Player") _shooterObj = obj.gameObject;
        }
        FindObjectOfType<Killer>().DoDeathCounterRpc(_killedCon, pobj);

        //add killcounter on client
        FindObjectOfType<Killer>().DoKillCounterRpc(_shooterConn, _shooterObj);
    }

    IEnumerator Wait()
    {
        invinceble = true;
        yield return new WaitForSeconds(0.1f);
        invinceble = false;
    }
}
