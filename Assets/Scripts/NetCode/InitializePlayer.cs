using FishNet;
using FishNet.Connection;
using FishNet.Managing.Scened;
using FishNet.Object;
using UnityEngine;

public class InitializePlayer : NetworkBehaviour
{
    public Vector3 spawnPos;

    public GameObject MainMenuUI;

    public GameObject CamPrefab;

    private NetworkConnection conn;
    private GameObject obj;

    public override void OnStartNetwork()
    {
        InstanceFinder.SceneManager.OnLoadEnd += OnSceneLoaded;
        
        MainMenuUI = GameObject.Find("MainMenuUI");

        if(base.IsClient)
        {
            SetGameLayerRecursive(this.gameObject, 6);
            ChangeScenesServerRpc(base.ClientManager.Connection);
        }
    }

    [ServerRpc]
    private void ChangeScenesServerRpc(NetworkConnection _conn)
    {
        GameObject.Find("NetcodeLogics").GetComponent<MainMenuSceneChanger>().IWantToChangeScenesNow(_conn);
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
        //Invoke(nameof(InitializePlayerServerRpc), 1f);
        InitializePlayerServerRpc();
        Cursor.lockState = CursorLockMode.Locked;
    }

    [ServerRpc]
    private void InitializePlayerServerRpc()
    {
        foreach (var conn in base.ServerManager.Clients.Values)
        {
            foreach (var objj in conn.Objects)
            {
                if (objj.gameObject.tag == "Player") obj = objj.gameObject;
            }

            obj.GetComponent<PredictedPlayerController>()._activated = true;

            obj.transform.position = spawnPos;

            InitializePlayerClientRpc(conn, obj);
        }
    }

    [TargetRpc]
    private void InitializePlayerClientRpc(NetworkConnection _conn, GameObject _player)
    {
        _player.GetComponent<PredictedPlayerController>()._activated = true;
    }
}
