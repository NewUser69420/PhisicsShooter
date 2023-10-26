using BayatGames.SaveGameFree;
using BayatGames.SaveGameFree.Examples;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreTracker : MonoBehaviour
{
    public string playerName;
    public int wins;
    public int losses;
    public int score;

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
        score = wins / losses;

        //save new score
        PlayerData pData = SaveGame.Load<PlayerData>(playerName, SerializerDropdown.Singleton.ActiveSerializer);
        pData.wins = wins;
        pData.losses = losses;
        pData.score = score;

        SaveGame.Save<PlayerData>(playerName, pData);
    }
}
