using FishNet.Object;
using UnityEngine;

public class CamIdentifier : NetworkBehaviour
{
    public int id;

    private void Awake()
    {
        if (Owner.IsLocalClient) id = 0;
        else id = 1;
    }
}
