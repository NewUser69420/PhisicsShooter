using FishNet.Managing.Scened;
using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testscript : NetworkBehaviour
{
    [SerializeField] private int Id;
    public override void OnStartNetwork()
    {
        Id = OwnerId;
    }
}
