using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    public Transform ShootPos;
    public float MoveSpeed = 2f;    //移动速度
    public float jumpH = 2f;        //跳跃高度

    private Animator animator;
    private CharacterController playerContor;
    private float FBSpeed;          //前后-1 1
    private float RLSpeed;          //左右-1 1
    private Vector3 velocity = new Vector3(0,-2,0);       //值存y方向的速度
    private float gravity = -20;    //重力，向下
    private bool isGround = true;
    private float time;
    private float FireTime = 0.1f;

    private float HP = 1000;         //血量
    private float baseDamage = 10;   //基础攻击力

    void Start()
    {
        animator = GetComponent<Animator>();
        playerContor = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        time += Time.deltaTime;

        //角色移动
        FBSpeed = Input.GetAxis("Vertical");
        RLSpeed = Input.GetAxis("Horizontal");
        
        //加速
        if (Input.GetKey(KeyCode.LeftShift))
        {
            FBSpeed *= 2;
        }

        FBSpeed = Mathf.Clamp(FBSpeed, -1, 2);
        animator.SetFloat("XSpeed", FBSpeed); //改变animator里的变量
        animator.SetFloat("YSpeed", RLSpeed);


        isGround = playerContor.isGrounded;
        if(isGround && velocity.y < 0f)
        {
            //把角色压在地面
            velocity.y = -2f;
        }

        Vector3 move = (transform.forward * FBSpeed + transform.right * RLSpeed) * MoveSpeed;

        //起跳
        if (Input.GetKeyDown(KeyCode.Space) && isGround)
        {

            //根据重力加速度公式v=sqrt(2gh),gravity为负数，加个负号
            velocity.y = Mathf.Sqrt(-2f * gravity * jumpH);
            

        }
        //一直有重力
        velocity.y += gravity * Time.deltaTime;
        velocity.y = Mathf.Max(velocity.y, -50f); //防止长时间下落，累加异常

        playerContor.Move((move + velocity) * Time.deltaTime);
        animator.SetFloat("velocityY", velocity.y);


        

        if (Input.GetMouseButton(0) && time > FireTime)
        {
            time = 0f;
            RaycastHit hit;
            //射击
            if(Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition),out hit,10000,1 << LayerMask.NameToLayer("Enemy")))
            {
                
                Enemy e = hit.collider.GetComponent<Enemy>();
                if(e != null)
                    e.ReduceHp(baseDamage,transform.position);

            }

        }


    }

    public void ReduceHp(float atk)
    {
        HP -= atk;
        print(gameObject.name + "还剩" + HP + "点血");
    }



}
