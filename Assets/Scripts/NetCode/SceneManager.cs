using FishNet.Managing.Scened;
using FishNet;
using FishNet.Object;
using FishNet.Connection;
using UnityEngine;
using FishNet.Transporting;
using System.Collections;

public class SceneManager : NetworkBehaviour
{
    public override void OnStartNetwork()
    {
        if(base.IsServer)
        {
            SceneLookupData lookup = new SceneLookupData(0, "Lobbies");
            SceneLoadData sld = new SceneLoadData(lookup);
            sld.Options.AllowStacking = false;
            //sld.Options.LocalPhysics = UnityEngine.SceneManagement.LocalPhysicsMode.Physics3D;
            sld.ReplaceScenes = ReplaceOption.All;
            base.SceneManager.LoadGlobalScenes(sld);
        }
    }
}