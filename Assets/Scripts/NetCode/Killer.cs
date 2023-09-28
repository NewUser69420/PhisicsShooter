using FishNet.Connection;
using FishNet.Object;
using UnityEngine;

public class Killer : NetworkBehaviour
{
    [TargetRpc]
    public void DoDeathCounterRpc(NetworkConnection conn, GameObject _obj)
    {
        _obj.GetComponentInChildren<UI>().deathCounterValue++;
    }

    [TargetRpc]
    public void DoKillCounterRpc(NetworkConnection conn, GameObject __shooter)
    {
        __shooter.GetComponentInChildren<UI>().killCounterValue++;
    }
}
