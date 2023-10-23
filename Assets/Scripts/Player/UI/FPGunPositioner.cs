using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class FPGunPositioner : MonoBehaviour
{
    [SerializeField] private Transform Gun;
    [SerializeField] private Transform Player;
    [SerializeField] private LayerMask whatIsWall;
    [SerializeField] private float smooth;
    [SerializeField] private float posSmooth;
    [SerializeField] private float swayMultiplier;

    private PlayerControlls playerControlls;
    private float timer;
    private float timerMax = 0.8f;
    private float posSmoothVal;
    private Vector3 randomPos;
    private Vector3 defPosDefault;
    private Vector3 defPos;

    private void Start()
    {
        playerControlls = new PlayerControlls();
        playerControlls.Enable();
        defPosDefault = Gun.localPosition;
    }

    void Update()
    {
        //get input
        float mouseX = playerControlls.OnFoot.Look.ReadValue<Vector2>().x * swayMultiplier;
        float mouseY = playerControlls.OnFoot.Look.ReadValue<Vector2>().y * swayMultiplier;

        //calc target rot
        Quaternion rotationX = Quaternion.AngleAxis(-mouseY, Vector3.right);
        Quaternion rotationY = Quaternion.AngleAxis(mouseX, Vector3.up);

        Quaternion targetRotation = rotationX * rotationY;

        //rotate
        Gun.localRotation = Quaternion.Slerp(Gun.localRotation, targetRotation, smooth * Time.deltaTime);

        //set defpos
        RaycastHit hit;
        if (Physics.Raycast(Player.position, Player.right, out hit, 1f, whatIsWall))
        {
            defPos = new Vector3(defPosDefault.x - 0.3f, defPosDefault.y, defPosDefault.z);
        }
        else defPos = defPosDefault;
        Debug.DrawRay(Player.position, Player.right * 1, Color.red);

        //cal movement point
        if (timer > 0)
        {
            timer -= Time.deltaTime;
        }
        else
        {
            timer = timerMax;
            float randomX = Random.Range(-0.08f, 0.08f);
            float randomY = Random.Range(-0.08f, 0.08f);
            randomPos = new Vector3(randomX, randomY, 0);
        }

        //set speed of anim
        if (Vector3.Distance(Gun.localPosition, defPos) > 0.1f) posSmoothVal = 4f;
        else posSmoothVal = posSmooth; 

        //positionate
        Gun.localPosition = Vector3.Slerp(Gun.localPosition, randomPos + defPos, posSmoothVal * Time.deltaTime);
    }
}
