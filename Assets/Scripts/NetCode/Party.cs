using FishNet.Managing.Scened;
using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Party : NetworkBehaviour
{
    public List<string> scenesToExclude = new();
    public List<NetworkObject> party = new();

    private void OnEnable()
    {
        base.SceneManager.OnLoadEnd += OnSceneLoaded;
    }

    private void OnDisable()
    {
        base.SceneManager.OnLoadEnd -= OnSceneLoaded;
    }

    private void OnSceneLoaded(SceneLoadEndEventArgs args)
    {
        if (base.IsClient)
        {
            foreach (var scene in args.LoadedScenes)
            {
                if (!scenesToExclude.Contains(scene.name))
                {
                    SendPartyToGameSetup(party, scene);
                }
            }
        }
    }

    [ServerRpc]
    private void SendPartyToGameSetup(List<NetworkObject> _party, UnityEngine.SceneManagement.Scene _scene)
    {
        foreach (var obj in _scene.GetRootGameObjects())
        {
            if (obj.name == "GameSetup") { GameSetup gm = obj.GetComponent<GameSetup>(); gm.parties.Add(gm.parties.Count, _party); }
        }
    }
}
