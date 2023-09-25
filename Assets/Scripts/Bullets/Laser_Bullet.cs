using System.Collections;
using UnityEngine;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Component.Prediction;
using FishNet.Managing.Timing;
using System.Linq;
using FishNet.Connection;
using Unity.VisualScripting;

public class Laser_Bullet : NetworkBehaviour
{
    [SyncVar(OnChange = nameof(_startingForce_OnChange))]

    private Vector3 _startingForce;

    private uint _stopTick = TimeManager.UNSET_TICK;

    private ServerHealthManager SHealth;

    [System.NonSerialized] public GameObject Player;

    [System.NonSerialized] public GameObject lastHitObject;

    public int damage = 50;
    private bool canDoDamage = true;
    
    public override void OnStartNetwork(){
        StartCoroutine (Wait());

        //Make sound
        GetComponent<AudioSource>().pitch = Random.Range(0.5f, 1.5f);
        GetComponent<AudioSource>().Play();

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

    void OnTriggerEnter(Collider collider){
        if(base.IsOwner)
        {
            FindObjectOfType<AudioManger>().Play("HitMarker");
        }
        
        if (!IsServer) return;
        if (collider.gameObject.layer == 12)
        {

            if(!canDoDamage) return;
            canDoDamage = !canDoDamage;

            string name = collider.gameObject.name;
            var id = collider.gameObject.GetComponentInParent<NetworkObject>().ClientManager.Connection.ClientId;
            lastHitObject = collider.gameObject;

            SHealth = collider.GetComponentInParent<ServerHealthManager>();


            //do damage
            switch (name)
            {
                case "Hips":
                    SHealth.OnHealthChange(0, SHealth.health[0] - damage, SHealth.health[0], Player);
                    SHealth.health[0] -= damage;
                    break;
                case "Left Thigh":
                    SHealth.OnHealthChange(1, SHealth.health[1] - damage, SHealth.health[1], Player);
                    SHealth.health[1] -= damage;
                    break;
                case "Left Leg":
                    SHealth.OnHealthChange(2, SHealth.health[2] - damage, SHealth.health[2], Player);
                    SHealth.health[2] -= damage;
                    break;
                case "Left Foot":
                    SHealth.OnHealthChange(3, SHealth.health[3] - damage, SHealth.health[3], Player);
                    SHealth.health[3] -= damage;
                    break;
                case "Right Thigh":
                    SHealth.OnHealthChange(4, SHealth.health[4] - damage, SHealth.health[4], Player);
                    SHealth.health[4] -= damage;
                    break;
                case "Right Leg":
                    SHealth.OnHealthChange(5, SHealth.health[5] - damage, SHealth.health[5], Player);
                    SHealth.health[5] -= damage;
                    break;
                case "Right Foot":
                    SHealth.OnHealthChange(6, SHealth.health[6] - damage, SHealth.health[6], Player);
                    SHealth.health[6] -= damage;
                    break;
                case "Spine 1":
                    SHealth.OnHealthChange(7, SHealth.health[7] - damage, SHealth.health[7], Player);
                    SHealth.health[7] -= damage;
                    break;
                case "Spine 2":
                    SHealth.OnHealthChange(8, SHealth.health[8] - damage, SHealth.health[8], Player);
                    SHealth.health[8] -= damage;
                    break;
                case "Spine 3":
                    SHealth.OnHealthChange(9, SHealth.health[9] - damage, SHealth.health[9], Player);
                    SHealth.health[9] -= damage;
                    break;
                case "Left Shoulder":
                    SHealth.OnHealthChange(10, SHealth.health[10] - damage, SHealth.health[10], Player);
                    SHealth.health[10] -= damage;
                    break;
                case "Left Arm":
                    SHealth.OnHealthChange(11, SHealth.health[11] - damage, SHealth.health[11], Player);
                    SHealth.health[11] -= damage;
                    break;
                case "Left Forearm":
                    SHealth.OnHealthChange(12, SHealth.health[12] - damage, SHealth.health[12], Player);
                    SHealth.health[12] -= damage;
                    break;
                case "Left Hand":
                    SHealth.OnHealthChange(13, SHealth.health[13] - damage, SHealth.health[13], Player);
                    SHealth.health[13] -= damage;
                    break;
                case "Right Shoulder":
                    SHealth.OnHealthChange(14, SHealth.health[14] - damage, SHealth.health[14], Player);
                    SHealth.health[14] -= damage;
                    break;
                case "Right Arm":
                    SHealth.OnHealthChange(15, SHealth.health[15] - damage, SHealth.health[15], Player);
                    SHealth.health[15] -= damage;
                    break;
                case "Right Forearm":
                    SHealth.OnHealthChange(16, SHealth.health[16] - damage, SHealth.health[16], Player);
                    SHealth.health[16] -= damage;
                    break;
                case "Right Hand":
                    SHealth.OnHealthChange(17, SHealth.health[17] - damage, SHealth.health[17], Player);
                    SHealth.health[17] -= damage;
                    break;
                case "Neck":
                    SHealth.OnHealthChange(18, SHealth.health[18] - damage, SHealth.health[18], Player);
                    SHealth.health[18] -= damage;
                    break;
                case "Head":
                    SHealth.OnHealthChange(19, SHealth.health[19] - damage, SHealth.health[19], Player);
                    SHealth.health[19] -= damage;
                    break;
            }

            if (this.IsSpawned) DespawnBullet();
        }
        
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(base.IsServer) if (this.IsSpawned) { Invoke(nameof(DespawnBullet), 0.01f); }
        if(base.IsClient) if (this.IsSpawned) { Invoke(nameof(TurnOffBullet), 0.01f); }
    }

    private void DespawnBullet(){
        if(!base.IsServer) return;
        this.GetComponent<NetworkObject>().Despawn();
        Destroy(this);
    }

    private void TurnOffBullet()
    {
        transform.Find("Capsule").gameObject.SetActive(false);
    }
}
