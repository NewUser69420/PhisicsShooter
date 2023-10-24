using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamemodeBase : NetworkBehaviour
{
    public List<NetworkObject> Team1 = new();
    public List<NetworkObject> Team2 = new();

    public Color Team1Colour;
    public Color Team2Colour;

    [System.NonSerialized] public Dictionary<NetworkObject, float> pScoreT1 = new();
    [System.NonSerialized] public Dictionary<NetworkObject, float> pScoreT2 = new();
    public int scoreT1;
    public int scoreT2;
    public int endScore;

    [System.NonSerialized] public bool gameIsRunning = true;

    protected virtual void MakeTeams()
    {
        //override this in specific gamemode script
    }

    public virtual void OnScoreChange()
    {
        //override this in specific gamemode script
    }

    public void FinishGame(string team)
    {
        gameIsRunning = false;
        Debug.Log($"Team {team} Won!");
    }
}
