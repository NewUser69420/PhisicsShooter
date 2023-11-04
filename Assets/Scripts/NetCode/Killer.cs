using FishNet.Connection;
using FishNet.Demo.AdditiveScenes;
using FishNet.Object;
using UnityEngine;

public class Killer : NetworkBehaviour
{
    [TargetRpc]
    public void DoDeathCounterRpc(NetworkConnection conn, GameObject _obj)
    {
        if(_obj.GetComponentInChildren<UI>() != null) _obj.GetComponentInChildren<UI>().deathCounterValue++;
    }

    [TargetRpc]
    public void DoKillCounterRpc(NetworkConnection conn, GameObject __shooter)
    {
        if (__shooter.GetComponentInChildren<UI>() != null) __shooter.GetComponentInChildren<UI>().killCounterValue++;
    }
}
