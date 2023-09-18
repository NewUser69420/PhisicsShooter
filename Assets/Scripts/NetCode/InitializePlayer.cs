using FishNet;
using FishNet.Connection;
using FishNet.Demo.AdditiveScenes;
using FishNet.Managing.Scened;
using FishNet.Object;
using Unity.VisualScripting;
using UnityEngine;

public class InitializePlayer : NetworkBehaviour
{
    public Vector3 spawnPos;

    public GameObject MainMenuUI;

    public GameObject CamPrefab;

    private NetworkConnection conn;
    private GameObject obj;

    private void Awake()
    {
        InstanceFinder.SceneManager.OnLoadEnd += OnSceneLoaded;
        MainMenuUI = GameObject.Find("MainMenuUI");

        Cursor.lockState = CursorLockMode.Locked;

        if(base.IsClient)
        {
            SetGameLayerRecursive(this.gameObject, 6);
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


    private void OnSceneLoaded(SceneLoadEndEventArgs arg)
    {
        InitializePlayerServerRpc();
    }

    [ServerRpc]

    private void InitializePlayerServerRpc()
    {
        foreach (var conn in InstanceFinder.ServerManager.Clients.Values)
        {
            foreach (var objj in conn.Objects)
            {
                if (objj.gameObject.tag == "Player") obj = objj.gameObject;
            }

            GameObject Cam = Instantiate(CamPrefab);
            base.Spawn(Cam, conn);

            obj.GetComponent<PredictedPlayerMover>().activated = true;
            obj.GetComponent<PredictedPlayerMover>()._cam = Cam;

            obj.transform.position = spawnPos;

            InitializePlayerClientRpc(conn, obj, Cam);
        }
    }

    [TargetRpc]
    private void InitializePlayerClientRpc(NetworkConnection _conn, GameObject _player, GameObject _Cam)
    {
        _player.GetComponent<PredictedPlayerMover>().activated = true;
        _player.GetComponent<PredictedPlayerMover>()._cam = _Cam;
    }
}
