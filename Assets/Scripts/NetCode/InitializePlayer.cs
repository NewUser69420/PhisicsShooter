using FishNet;
using FishNet.Managing.Scened;
using FishNet.Object;
using Unity.VisualScripting;
using UnityEngine;

public class InitializePlayer : NetworkBehaviour
{
    public Vector3 spawnPos;

    private void Start()
    {
        InstanceFinder.SceneManager.OnLoadEnd += OnSceneLoaded;
        GetComponent<TempPredictedPlayerController>().activated = true;
    }

    private void OnSceneLoaded(SceneLoadEndEventArgs objj)
    {
        if (!base.IsClient) return;

        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Player"))
        {
            if (!base.IsOwner) return;
            InitializePlayerServer(obj);
        }
    }

    [ServerRpc]
    private void InitializePlayerServer(GameObject _obj)
    {
        _obj.GetComponent<Rigidbody>().velocity = Vector3.zero;
        _obj.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        _obj.transform.position = spawnPos;
    }
}
