using FishNet.Object;
using FishNet.Transporting;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamemodeManager : NetworkBehaviour
{
    public override void OnStartNetwork()
    {
        //initialize player (move this as needed)
        foreach(var pair in base.SceneManager.SceneConnections)
        {
            foreach(var conn in pair.Value)
            {
                foreach(var obj in conn.Objects)
                {
                    if(obj.CompareTag("Player"))
                    {
                        obj.GetComponent<InitializePlayer>().InitializeThePlayerOnClient(obj.Owner);
                        obj.GetComponent<CameraWorker>().Initialize(obj.Owner);
                    }
                }
            }
        }
        Invoke(nameof(TurnOfLoadingScreen), 1f);
    }

    private void TurnOfLoadingScreen()
    {
        foreach(var pair in base.SceneManager.SceneConnections)
        {
            foreach(var conn in pair.Value)
            {
               foreach(var scene in conn.Scenes)
               {
                    if(scene.name != "Lobbies")
                    {
                        foreach(var obj in scene.GetRootGameObjects())
                        {
                            if (obj.name == "Canvas") obj.transform.Find("LoadingScreen").gameObject.SetActive(false);
                        }
                    }
               }
            }
        }
    }
}
