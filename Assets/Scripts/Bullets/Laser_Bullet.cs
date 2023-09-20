using System.Collections;
using UnityEngine;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Component.Prediction;
using FishNet.Managing.Timing;

public class Laser_Bullet : NetworkBehaviour
{
    [SyncVar(OnChange = nameof(_startingForce_OnChange))]

    private Vector3 _startingForce;

    private uint _stopTick = TimeManager.UNSET_TICK;

    //private ServerHealthManager SHM;

    public int damage = 50;
    private bool canDoDamage = true;

    void Awake(){
        //SHM = FindObjectOfType<ServerHealthManager>();
    }
    
    public override void OnStartNetwork(){
        StartCoroutine (Wait());

        uint timeToTicks = base.TimeManager.TimeToTicks(0.65f);

        if (base.IsServer || base.Owner.IsLocalClient)
        {
            _stopTick = base.TimeManager.LocalTick + timeToTicks;
        }
       
        else
        {
            uint passed = (uint)Mathf.Max(1, base.TimeManager.Tick - base.TimeManager.LastPacketTick);
            long stopTick = (base.TimeManager.Tick + timeToTicks - passed - 1);
            if (stopTick > 0)
                _stopTick = (uint)stopTick;
            else
                _stopTick = 1;
        }

        base.TimeManager.OnTick += TimeManager_OnTick;
    }

    public override void OnStopNetwork()
    {
        base.TimeManager.OnTick -= TimeManager_OnTick;
    }

    private void TimeManager_OnTick()
    {
        if (_stopTick > 0 && base.TimeManager.LocalTick >= _stopTick)
        {
            Rigidbody rb = GetComponent<Rigidbody>();
            rb.isKinematic = true;
        }
    }

    IEnumerator Wait(){
        yield return new WaitForSeconds(2f);
        if(this.IsSpawned) DespawnBullet();
    }

    public void SetStartingForce(Vector3 value)
    {
        if (!base.IsSpawned)
            SetVelocity(value);

        _startingForce = value;
    }

    private void _startingForce_OnChange(Vector3 prev, Vector3 next, bool asServer)
    {
        SetVelocity(next);
    }

    public void SetVelocity(Vector3 value)
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.velocity = value;
    }

    void OnCollisionEnter(Collision collision){
        if(!IsServer) return;
        if(collision.gameObject.layer == LayerMask.NameToLayer("OtherCharacter") || collision.gameObject.layer == LayerMask.NameToLayer("Character")){
            if(!canDoDamage) return;
            canDoDamage = !canDoDamage;

            string name = collision.gameObject.name;
            // if(collision.gameObject.GetComponentInParent<NetworkObject>() == null) return;
            var id = collision.gameObject.GetComponent<NetworkObject>().ClientManager.Connection;
            //switch(id){
            //    case 0 :
            //        switch(name){
            //            case "Hips" :
            //                SHM.Player0[0] -= damage;
            //                break;
            //            case "Left Thigh" :
            //                SHM.Player0[1] -= damage;
            //                break;
            //            case "Left Leg" :
            //                SHM.Player0[2] -= damage;
            //                break;
            //            case "Left Foot" :
            //                SHM.Player0[3] -= damage;
            //                break;
            //            case "Right Thigh" :
            //                SHM.Player0[4] -= damage;
            //                break;
            //            case "Right Leg" :
            //                SHM.Player0[5] -= damage;
            //                break;
            //            case "Right Foot" :
            //                SHM.Player0[6] -= damage;
            //                break;
            //            case "Spine 1" :
            //                SHM.Player0[7] -= damage;
            //                break;
            //            case "Spine 2" :
            //                SHM.Player0[8] -= damage;
            //                break;
            //            case "Spine 3" :
            //                SHM.Player0[9] -= damage;
            //                break;
            //            case "Left Shoulder" :
            //                SHM.Player0[10] -= damage;
            //                break;
            //            case "Left Arm" :
            //                SHM.Player0[11] -= damage;
            //                break;
            //            case "Left Forearm" :
            //                SHM.Player0[12] -= damage;
            //                break;
            //            case "Left Hand" :
            //                SHM.Player0[13] -= damage;
            //                break;
            //            case "Right Shoulder" :
            //                SHM.Player0[14] -= damage;
            //                break;
            //            case "Right Arm" :
            //                SHM.Player0[15] -= damage;
            //                break;
            //            case "Right Forearm" :
            //                SHM.Player0[16] -= damage;
            //                break;
            //            case "Right Hand" :
            //                SHM.Player0[17] -= damage;
            //                break;
            //            case "Neck" :
            //                SHM.Player0[18] -= damage;
            //                break;
            //            case "Head" :
            //                SHM.Player0[19] -= damage;
            //                break;
            //            default :
            //                Debug.Log("hit non-damageble object");
            //                break;
            //        }       
            //        break;
            //    case 1 :
            //        switch(name){
            //            case "Hips" :
            //                SHM.Player1[0] -= damage;
            //                break;
            //            case "Left Thigh" :
            //                SHM.Player1[1] -= damage;
            //                break;
            //            case "Left Leg" :
            //                SHM.Player1[2] -= damage;
            //                break;
            //            case "Left Foot" :
            //                SHM.Player1[3] -= damage;
            //                break;
            //            case "Right Thigh" :
            //                SHM.Player1[4] -= damage;
            //                break;
            //            case "Right Leg" :
            //                SHM.Player1[5] -= damage;
            //                break;
            //            case "Right Foot" :
            //                SHM.Player1[6] -= damage;
            //                break;
            //            case "Spine 1" :
            //                SHM.Player1[7] -= damage;
            //                break;
            //            case "Spine 2" :
            //                SHM.Player1[8] -= damage;
            //                break;
            //            case "Spine 3" :
            //                SHM.Player1[9] -= damage;
            //                break;
            //            case "Left Shoulder" :
            //                SHM.Player1[10] -= damage;
            //                break;
            //            case "Left Arm" :
            //                SHM.Player1[11] -= damage;
            //                break;
            //            case "Left Forearm" :
            //                SHM.Player1[12] -= damage;
            //                break;
            //            case "Left Hand" :
            //                SHM.Player1[13] -= damage;
            //                break;
            //            case "Right Shoulder" :
            //                SHM.Player1[14] -= damage;
            //                break;
            //            case "Right Arm" :
            //                SHM.Player1[15] -= damage;
            //                break;
            //            case "Right Forearm" :
            //                SHM.Player1[16] -= damage;
            //                break;
            //            case "Right Hand" :
            //                SHM.Player1[17] -= damage;
            //                break;
            //            case "Neck" :
            //                SHM.Player1[18] -= damage;
            //                break;
            //            case "Head" :
            //                SHM.Player1[19] -= damage;
            //                break;
            //            default :
            //                Debug.Log("hit non-damageble object");
            //                break;
            //        }       
            //        break;
            //    case 2 :
            //        switch(name){
            //            case "Hips" :
            //                SHM.Player2[0] -= damage;
            //                break;
            //            case "Left Thigh" :
            //                SHM.Player2[1] -= damage;
            //                break;
            //            case "Left Leg" :
            //                SHM.Player2[2] -= damage;
            //                break;
            //            case "Left Foot" :
            //                SHM.Player2[3] -= damage;
            //                break;
            //            case "Right Thigh" :
            //                SHM.Player2[4] -= damage;
            //                break;
            //            case "Right Leg" :
            //                SHM.Player2[5] -= damage;
            //                break;
            //            case "Right Foot" :
            //                SHM.Player2[6] -= damage;
            //                break;
            //            case "Spine 1" :
            //                SHM.Player2[7] -= damage;
            //                break;
            //            case "Spine 2" :
            //                SHM.Player2[8] -= damage;
            //                break;
            //            case "Spine 3" :
            //                SHM.Player2[9] -= damage;
            //                break;
            //            case "Left Shoulder" :
            //                SHM.Player2[10] -= damage;
            //                break;
            //            case "Left Arm" :
            //                SHM.Player2[11] -= damage;
            //                break;
            //            case "Left Forearm" :
            //                SHM.Player2[12] -= damage;
            //                break;
            //            case "Left Hand" :
            //                SHM.Player2[13] -= damage;
            //                break;
            //            case "Right Shoulder" :
            //                SHM.Player2[14] -= damage;
            //                break;
            //            case "Right Arm" :
            //                SHM.Player2[15] -= damage;
            //                break;
            //            case "Right Forearm" :
            //                SHM.Player2[16] -= damage;
            //                break;
            //            case "Right Hand" :
            //                SHM.Player2[17] -= damage;
            //                break;
            //            case "Neck" :
            //                SHM.Player2[18] -= damage;
            //                break;
            //            case "Head" :
            //                SHM.Player2[19] -= damage;
            //                break;
            //            default :
            //                Debug.Log("hit non-damageble object");
            //                break;
            //        }       
            //        break;
            //    case 3 :
            //        switch(name){
            //            case "Hips" :
            //                SHM.Player3[0] -= damage;
            //                break;
            //            case "Left Thigh" :
            //                SHM.Player3[1] -= damage;
            //                break;
            //            case "Left Leg" :
            //                SHM.Player3[2] -= damage;
            //                break;
            //            case "Left Foot" :
            //                SHM.Player3[3] -= damage;
            //                break;
            //            case "Right Thigh" :
            //                SHM.Player3[4] -= damage;
            //                break;
            //            case "Right Leg" :
            //                SHM.Player3[5] -= damage;
            //                break;
            //            case "Right Foot" :
            //                SHM.Player3[6] -= damage;
            //                break;
            //            case "Spine 1" :
            //                SHM.Player3[7] -= damage;
            //                break;
            //            case "Spine 2" :
            //                SHM.Player3[8] -= damage;
            //                break;
            //            case "Spine 3" :
            //                SHM.Player3[9] -= damage;
            //                break;
            //            case "Left Shoulder" :
            //                SHM.Player3[10] -= damage;
            //                break;
            //            case "Left Arm" :
            //                SHM.Player3[11] -= damage;
            //                break;
            //            case "Left Forearm" :
            //                SHM.Player3[12] -= damage;
            //                break;
            //            case "Left Hand" :
            //                SHM.Player3[13] -= damage;
            //                break;
            //            case "Right Shoulder" :
            //                SHM.Player3[14] -= damage;
            //                break;
            //            case "Right Arm" :
            //                SHM.Player3[15] -= damage;
            //                break;
            //            case "Right Forearm" :
            //                SHM.Player3[16] -= damage;
            //                break;
            //            case "Right Hand" :
            //                SHM.Player3[17] -= damage;
            //                break;
            //            case "Neck" :
            //                SHM.Player3[18] -= damage;
            //                break;
            //            case "Head" :
            //                SHM.Player3[19] -= damage;
            //                break;
            //            default :
            //                Debug.Log("hit non-damageble object");
            //                break;
            //        }       
            //        break;
            //    case 4 :
            //        switch(name){
            //            case "Hips" :
            //                SHM.Player4[0] -= damage;
            //                break;
            //            case "Left Thigh" :
            //                SHM.Player4[1] -= damage;
            //                break;
            //            case "Left Leg" :
            //                SHM.Player4[2] -= damage;
            //                break;
            //            case "Left Foot" :
            //                SHM.Player4[3] -= damage;
            //                break;
            //            case "Right Thigh" :
            //                SHM.Player4[4] -= damage;
            //                break;
            //            case "Right Leg" :
            //                SHM.Player4[5] -= damage;
            //                break;
            //            case "Right Foot" :
            //                SHM.Player4[6] -= damage;
            //                break;
            //            case "Spine 1" :
            //                SHM.Player4[7] -= damage;
            //                break;
            //            case "Spine 2" :
            //                SHM.Player4[8] -= damage;
            //                break;
            //            case "Spine 3" :
            //                SHM.Player4[9] -= damage;
            //                break;
            //            case "Left Shoulder" :
            //                SHM.Player4[10] -= damage;
            //                break;
            //            case "Left Arm" :
            //                SHM.Player4[11] -= damage;
            //                break;
            //            case "Left Forearm" :
            //                SHM.Player4[12] -= damage;
            //                break;
            //            case "Left Hand" :
            //                SHM.Player4[13] -= damage;
            //                break;
            //            case "Right Shoulder" :
            //                SHM.Player4[14] -= damage;
            //                break;
            //            case "Right Arm" :
            //                SHM.Player4[15] -= damage;
            //                break;
            //            case "Right Forearm" :
            //                SHM.Player4[16] -= damage;
            //                break;
            //            case "Right Hand" :
            //                SHM.Player4[17] -= damage;
            //                break;
            //            case "Neck" :
            //                SHM.Player4[18] -= damage;
            //                break;
            //            case "Head" :
            //                SHM.Player4[19] -= damage;
            //                break;
            //            default :
            //                Debug.Log("hit non-damageble object");
            //                break;
            //        }       
            //        break;
            //    case 5 :
            //        switch(name){
            //            case "Hips" :
            //                SHM.Player5[0] -= damage;
            //                break;
            //            case "Left Thigh" :
            //                SHM.Player5[1] -= damage;
            //                break;
            //            case "Left Leg" :
            //                SHM.Player5[2] -= damage;
            //                break;
            //            case "Left Foot" :
            //                SHM.Player5[3] -= damage;
            //                break;
            //            case "Right Thigh" :
            //                SHM.Player5[4] -= damage;
            //                break;
            //            case "Right Leg" :
            //                SHM.Player5[5] -= damage;
            //                break;
            //            case "Right Foot" :
            //                SHM.Player5[6] -= damage;
            //                break;
            //            case "Spine 1" :
            //                SHM.Player5[7] -= damage;
            //                break;
            //            case "Spine 2" :
            //                SHM.Player5[8] -= damage;
            //                break;
            //            case "Spine 3" :
            //                SHM.Player5[9] -= damage;
            //                break;
            //            case "Left Shoulder" :
            //                SHM.Player5[10] -= damage;
            //                break;
            //            case "Left Arm" :
            //                SHM.Player5[11] -= damage;
            //                break;
            //            case "Left Forearm" :
            //                SHM.Player5[12] -= damage;
            //                break;
            //            case "Left Hand" :
            //                SHM.Player5[13] -= damage;
            //                break;
            //            case "Right Shoulder" :
            //                SHM.Player5[14] -= damage;
            //                break;
            //            case "Right Arm" :
            //                SHM.Player5[15] -= damage;
            //                break;
            //            case "Right Forearm" :
            //                SHM.Player5[16] -= damage;
            //                break;
            //            case "Right Hand" :
            //                SHM.Player5[17] -= damage;
            //                break;
            //            case "Neck" :
            //                SHM.Player5[18] -= damage;
            //                break;
            //            case "Head" :
            //                SHM.Player5[19] -= damage;
            //                break;
            //            default :
            //                Debug.Log("hit non-damageble object");
            //                break;
            //        }       
            //        break;
            //}
            if(this.IsSpawned) DespawnBullet();
        }
        
        if(this.IsSpawned) {Invoke(nameof(DespawnBullet), 0.1f);}
    }

    private void DespawnBullet(){
        if(!base.IsServer) return;
        this.GetComponent<NetworkObject>().Despawn();
        Destroy(this);
    }
}
