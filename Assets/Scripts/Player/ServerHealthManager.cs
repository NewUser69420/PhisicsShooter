using FishNet.Connection;
using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ServerHealthManager : NetworkBehaviour
{
    public List<float> health = new List<float>();

    public float totalHealth;
    public float maxHealth;
    public string team;
    public float score;
    public int deaths;
    public int kills;

    public Vector3 spawnPosition;

    private NetworkConnection Conn;
    private bool invinceble;


    public override void OnStartNetwork()
    {
        if (Owner.IsLocalClient)
        {
            GiveLocalConn(base.LocalConnection);
        }
        
        if (!IsServerStarted) { return; }

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
        health.Clear();
        
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

    public void OnHealthChange(int index, float newItem, float oldItem, NetworkConnection shooter, string _team)
    {
        if (_team == team) { Debug.Log($"Friendly fire"); return; }
        
        Debug.Log("health changed");
        //check for below 0
        if (newItem < 0) health[index] = 0;

        //calc total health
        totalHealth += (newItem - oldItem);

        //check for death
        if (newItem <= 0 && !invinceble) DoDeath(Conn, this.gameObject, shooter);
    }

    private void DoDeath(NetworkConnection _killedCon, GameObject pobj, NetworkConnection _shooterConn)
    {
        Debug.Log("Doing death");
        StartCoroutine(Wait());

        //make sound
        GetComponent<PlayerAudioManager>().Play("Death");
        SyncSound("Death");

        //do death
        SetMaxHealth();

        pobj.transform.position = spawnPosition;

        pobj.GetComponentInChildren<IK_foot>().ResetLeg();

        //add deathcounter on client
        GameObject _shooterObj = null;
        foreach (NetworkObject obj in _shooterConn.Objects)
        {
            if (obj.tag == "Player") _shooterObj = obj.gameObject;
        }
        FindObjectOfType<Killer>().DoDeathCounterRpc(_killedCon, pobj);

        //add killcounter on client
        FindObjectOfType<Killer>().DoKillCounterRpc(_shooterConn, _shooterObj);

        //add deathcounter on all instances of pobj's scoreboard
        AddDeathCounterToScoreboard(pobj.GetComponent<NetworkObject>().OwnerId);

        //add killcounter on all instances of shooter's scoreobard
        AddKillCounterToScoreboard(_shooterObj.GetComponent<NetworkObject>().OwnerId);

        //set kill/death counter on server
        _shooterObj.GetComponent<ServerHealthManager>().kills++;
        _shooterObj.GetComponent<ServerHealthManager>().OnKDChange("kill");
        deaths++;
        OnKDChange("death");
    }

    [ObserversRpc]
    private void AddDeathCounterToScoreboard(int id)
    {
        foreach (NetworkObject nobj in LocalConnection.Objects)
        {
            if(nobj.tag == "Player")
            {
                foreach(Transform item in nobj.transform.Find("UI/ScoreBoard/Holder"))
                {
                    if(item.GetComponent<ScoreBoardItemTracker>().id == id)
                    {
                        item.GetComponent<ScoreBoardItemTracker>().deaths++;
                    }
                }
            }
        }
    }

    [ObserversRpc]
    private void AddKillCounterToScoreboard(int id)
    {
        foreach (NetworkObject nobj in LocalConnection.Objects)
        {
            if (nobj.tag == "Player")
            {
                foreach (Transform item in nobj.transform.Find("UI/ScoreBoard/Holder"))
                {
                    if (item.GetComponent<ScoreBoardItemTracker>().id == id)
                    {
                        item.GetComponent<ScoreBoardItemTracker>().kills++;
                    }
                }
            }
        }
    }

    IEnumerator Wait()
    {
        invinceble = true;
        yield return new WaitForSeconds(0.1f);
        invinceble = false;
    }

    [ObserversRpc]
    private void SyncSound(string sound)
    {
        GetComponent<PlayerAudioManager>().Play(sound);  
    }

    private void OnKDChange(string type)
    {
        //set new score
        switch (type)
        {
            case "kill":
                score += 3 * Mathf.Sqrt(15 * kills);
                break;
            case "death":
                score -= Mathf.Pow(deaths, 2) / 5;
                if (score < 0) score = 0;
                break;
        }

        //sync score with gamemode obj
        foreach (var obj in gameObject.scene.GetRootGameObjects())
        {
            if (obj.CompareTag("Gamemode"))
            {
                switch (team)
                {
                    case "team1":
                        for (int i = 0; i < obj.GetComponent<GamemodeBase>().pScoreT1.Count; i++)
                        {
                            if (obj.GetComponent<GamemodeBase>().pScoreT1.ElementAt(i).Key == this.GetComponent<NetworkObject>())
                                obj.GetComponent<GamemodeBase>().pScoreT1[this.GetComponent<NetworkObject>()] = score;
                        }
                        break;
                    case "team2":
                        for (int i = 0; i < obj.GetComponent<GamemodeBase>().pScoreT2.Count; i++)
                        {
                            if (obj.GetComponent<GamemodeBase>().pScoreT2.ElementAt(i).Key == this.GetComponent<NetworkObject>())
                                obj.GetComponent<GamemodeBase>().pScoreT2[this.GetComponent<NetworkObject>()] = score;
                        }
                        break;
                }

                obj.GetComponent<GamemodeBase>().OnScoreChange();
            }
        }
    }
}
