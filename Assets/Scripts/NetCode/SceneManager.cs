using FishNet.Managing.Scened;
using FishNet;
using FishNet.Object;

public class SceneManager : NetworkBehaviour
{
    ////private List<NetworkObject> objsToKeep = new List<NetworkObject>(); 

    //private void Awake()
    //{

    //    InstanceFinder.ServerManager.OnAuthenticationResult += ChangeSceneForServer;
    //}

    //public void ChangeSceneForServer(NetworkConnection conn, bool bl)
    //{
    //    //GetObjsToKeep();
    //    SceneLoadData sld = new SceneLoadData("Lobbies");
    //    //sld.MovedNetworkObjects = objsToKeep.ToArray();
    //    sld.ReplaceScenes = ReplaceOption.All;
    //    InstanceFinder.SceneManager.LoadGlobalScenes(sld);
    //}

    //private void GetObjsToKeep()
    //{
    //    foreach (NetworkObject obj in InstanceFinder.ServerManager.Objects.SceneObjects.Values)
    //    {
    //        //if (obj.gameObject.name == "ServerHealthManager") objsToKeep.Add(obj);
    //    }
    //}

    public override void OnStartNetwork()
    {
        if(InstanceFinder.IsServer)
        {
            SceneLoadData sld = new SceneLoadData("Lobbies");
            sld.ReplaceScenes = ReplaceOption.All;
            InstanceFinder.SceneManager.LoadGlobalScenes(sld);
        }
    }
}