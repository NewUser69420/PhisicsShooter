using BayatGames.SaveGameFree;
using BayatGames.SaveGameFree.Examples;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreTracker : MonoBehaviour
{
    public string playerName;
    public int wins = -1;
    public int losses = -1;
    public float score = -1;

    public void Setup()
    {
        PlayerData pData = SaveGame.Load<PlayerData>(playerName);
        wins = pData.wins;
        losses = pData.losses;
    }

    public void OnScoreChange(string matchState)
    {
        switch (matchState)
        {
            case "win":
                wins += 1;
                break;
            case "loose":
                losses += 1;
                break;
        }

        //calc score
        if (losses != 0) score = (float) wins / (float) losses;
        else score = wins;

        //save new score
        PlayerData pData = SaveGame.Load<PlayerData>(playerName);
        pData.wins = wins;
        pData.losses = losses;
        pData.score = score;

        SaveGame.Save<PlayerData>(playerName, pData);

        Debug.Log($"{playerName}, wins: {wins}, losses: {losses}, score: {score}");
    }
}
