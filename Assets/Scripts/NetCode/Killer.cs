using FishNet.Connection;
using FishNet.Object;
using UnityEngine;

public class Killer : NetworkBehaviour
{
    [ObserversRpc]
    public void DoDeathCounterRpc(int _id, GameObject _obj, GameObject __shooter)
    {
        if (base.OwnerId == _id && base.IsOwner) _obj.GetComponentInChildren<UI>().deathCounterValue++;
    }

    [TargetRpc]
    public void DoKillCounterRpc(NetworkConnection __conn)
    {
        Debug.Log($"urmom");
    }
}
