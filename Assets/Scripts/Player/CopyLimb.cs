using FishNet.Object;
using UnityEngine;

public class CopyLimb : NetworkBehaviour
{
    public Transform Player;
    public Transform targetLimb;
    public ConfigurableJoint m_ConfigurableJoint;

    Quaternion targetInitalRotation;

    public void Awake(){                
        if(!base.IsClient) return;
        this.m_ConfigurableJoint = this.GetComponent<ConfigurableJoint>();
        this.targetInitalRotation = this.targetLimb.transform.localRotation;
    }

    private Quaternion copyRotation(){
        return Quaternion.Inverse(this.targetLimb.localRotation) * this.targetInitalRotation;
    }

    void FixedUpdate(){
        if(!base.IsClient) return;
        // this.m_ConfigurableJoint.targetRotation = copyRotation();
        SetTargetRotationServer(copyRotation(), gameObject.name, Player);
    }

    [ServerRpc]
    private void SetTargetRotationServer(Quaternion _copyRotation, string objName, Transform _Player){
        
        var children = _Player.transform.Find("Phisical/Armature").GetComponentsInChildren<Transform>(includeInactive: true);
        foreach (var child in children)
        {
            if(child.name == objName) child.GetComponent<ConfigurableJoint>().targetRotation = _copyRotation;
        }
    }
}