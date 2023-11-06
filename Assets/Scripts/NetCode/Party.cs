using FishNet.Connection;
using FishNet.Managing.Scened;
using FishNet.Object;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Party : NetworkBehaviour
{
    public List<string> scenesToExclude = new();
    public List<NetworkObject> party = new();
    public bool isPartyLeader = true;
    public int id = -1;

    public override void OnStartNetwork()
    {
        base.SceneManager.OnLoadEnd += OnSceneLoaded;
        
        if (base.IsServerStarted)
        {
            party.Add(GetComponent<NetworkObject>());
        }

        if (Owner.IsLocalClient)
        {
            id = LocalConnection.ClientId;
            SyncId(id);
        }
    }

    public override void OnStopNetwork()
    {
        base.SceneManager.OnLoadEnd -= OnSceneLoaded;
    }

    [ServerRpc]
    private void SyncId(int _id)
    {
        id = _id;
    }

    private void OnSceneLoaded(SceneLoadEndEventArgs args)
    {
        foreach (var scene in args.LoadedScenes)
        {
            if (base.IsServerStarted)
            {
                if (!scenesToExclude.Contains(scene.name))
                {
                    SendPartyToGameSetup(party, id);
                }
                else if (scene.name == "Lobbies")
                {
                    foreach (var butn in FindObjectsOfType<LobbyButn>())
                    {
                        Debug.Log("Setting party");
                        if (!butn.party.ContainsKey(OwnerId)) butn.party.Add(OwnerId, party);
                        else butn.party[OwnerId] = party;
                    }
                }
            }

            if (base.IsClientStarted)
            {
                if (scene.name == "Lobbies")
                {
                    Invoke(nameof(Wait), 0.7f);
                }
            }
        }
    }

    private void Wait()
    {
        FindObjectOfType<PartyInviteManager>(true).SpawnPartyItem(GetComponent<InitializePlayer>().playerName);
    }

    private void SendPartyToGameSetup(List<NetworkObject> _party, int _id)
    {
        if (!isPartyLeader) return;
        Debug.Log($"party count: {_party.Count}");
        if(this.isActiveAndEnabled) foreach (var obj in gameObject.scene.GetRootGameObjects())
        {
            if (obj.name == "GameSetup")
            {
                GameSetup gm = obj.GetComponent<GameSetup>();
                if (!gm.parties.ContainsKey(_id)) gm.parties.Add(_id, _party);
                else gm.parties[_id] = _party;
            }
        }
    }
}
