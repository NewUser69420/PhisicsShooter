using FishNet;
using FishNet.Demo.AdditiveScenes;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Transporting;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SetupTestPlayer : MonoBehaviour
{
    public GameObject Player;
    [SerializeField] private Vector3 Spawnposition;


    private void Awake()
    {
        transform.Find("SpawnPos").position = Spawnposition;

        Instantiate(Player, Spawnposition, Quaternion.identity);
    }
}
