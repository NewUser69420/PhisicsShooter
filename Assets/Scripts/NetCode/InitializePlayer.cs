using FishNet;
using FishNet.Managing.Scened;
using FishNet.Object;
using UnityEngine;

public class InitializePlayer : NetworkBehaviour
{
    public Vector3 spawnPos;

    public GameObject MainMenuUI;

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
            transform.Find("CMCam").gameObject.SetActive(true);
        }
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
