using FishNet;
using FishNet.Connection;
using FishNet.Managing.Scened;
using FishNet.Object;
using System.Collections.Generic;
using UnityEngine;

public class InitializePlayer : NetworkBehaviour
{
    public Vector3 spawnPos;

    public GameObject MainMenuUI;

    private GameObject obj;

    public override void OnStartNetwork()
    {
        MainMenuUI = GameObject.Find("MainMenuUI");

        if (base.Owner.IsLocalClient)
        {
            SetGameLayerRecursive(this.gameObject, 6);

            InitializePlayerServerRpc(base.LocalConnection);
            
            Cursor.lockState = CursorLockMode.Locked;

            GetComponent<PredictedPlayerController>()._activated = true;
        }
    }

    private void SetGameLayerRecursive(GameObject _go, int _layer)
    {
        _go.layer = _layer;
        foreach (Transform child in _go.transform)
        {
            child.gameObject.layer = _layer;

            Transform _HasChildren = child.GetComponentInChildren<Transform>();
            if (_HasChildren != null)
                SetGameLayerRecursive(child.gameObject, _layer);
        }
    }

    [ServerRpc]
    private void InitializePlayerServerRpc(NetworkConnection _conn)
    {
        foreach (var objj in _conn.Objects)
        {
            if (objj.gameObject.tag == "Player") obj = objj.gameObject;
        }

        obj.GetComponent<PredictedPlayerController>()._activated = true;

        obj.transform.position = spawnPos;
    }
}
