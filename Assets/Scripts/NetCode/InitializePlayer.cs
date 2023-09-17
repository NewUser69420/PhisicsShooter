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

    private void Start()
    {
        InstanceFinder.SceneManager.OnLoadEnd += OnSceneLoaded;
        GetComponent<TempPredictedPlayerController>().activated = true;
        MainMenuUI = GameObject.Find("MainMenuUI");

        //if (InstanceFinder.ServerManager.Clients.Count >= 3)
        //{
        //    if (InstanceFinder.IsServer) return;
        //    InstanceFinder.ServerManager.StopConnection(true);
        //    Debug.Log("Too Many Players");
        //    MainMenuUI.GetComponent<MainMenu>().LoadingScreen.gameObject.SetActive(false);
        //    MainMenuUI.GetComponent<MainMenu>().HomeScreen.gameObject.SetActive(true);
        //}

        if(base.IsClient)
        {
            transform.Find("Character/Body").gameObject.layer = LayerMask.NameToLayer("Character");
        }
    }

    private void OnSceneLoaded(SceneLoadEndEventArgs objj)
    {
        if (!base.IsClient) return;

        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Player"))
        {
            if (!base.IsOwner) return;
            InitializePlayerServer(obj, base.LocalConnection);
        }

        GameObject Cam = Instantiate(CamPrefab);
    }

    [ServerRpc]
    private void InitializePlayerServer(GameObject _player, NetworkConnection _conn)
    {
        _player.GetComponent<Rigidbody>().velocity = Vector3.zero;
        _player.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        _player.transform.position = spawnPos;
    }
}
