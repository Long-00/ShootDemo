using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    public List<Transform> patrols = new List<Transform>(); //存放巡逻的目标点


    private Animator animator;
    private NavMeshAgent agent;
    private HashSet<Transform> PTarget = new(); //是否有玩家进入范围，HashSet防止重复加入
    private int patrolIndex = 0;               //巡逻点下标
    private int playerLayer;                    //玩家层级

    private float HP = 1000;                    //血量
    private float baseDamage = 10;              //基础攻击力

    private void Awake()
    {
        playerLayer = LayerMask.NameToLayer("Player");
    }

    void Start()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        animator.SetFloat("Dis", 3f);

    }

    private void OnTriggerEnter(Collider other)
    {
        //判断是否是玩家层
        if(other.gameObject.layer == playerLayer)
        {
            PTarget.Add(other.gameObject.transform);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        //玩家离开触发器，移除玩家
        if (other.gameObject.layer == playerLayer)
        {
            PTarget.Remove(other.gameObject.transform);
        }
    }

    //判断HashSet里是否有有效值
    private bool HashHasValue()
    {
        foreach(var t in PTarget)
        {
            if (t != null)
                return true;
        }
        return false;
    }

    
    void Update()
    {
        
        if (HashHasValue())
        {
            //如果HashSet里有玩家，则追踪玩家
            foreach (var t in PTarget)
            {
                if (t == null)
                    continue;

                agent.SetDestination(t.position);
                if (Vector3.Distance(transform.position, t.position) <= 2)
                {
                    transform.rotation = Quaternion.LookRotation(new Vector3(t.position.x,transform.position.y,t.position.z) - transform.position);
                }
                animator.SetFloat("Dis", Vector3.Distance(transform.position, t.position));
            }
        }
        else
        {


            //巡逻
            if (agent.velocity == Vector3.zero)
            {

                agent.SetDestination(patrols[patrolIndex % patrols.Count].position);

                if (!agent.pathPending)
                {
                    patrolIndex++;
                }

            }
        }
        
            
        

    }

    Collider[] colliders = new Collider[10];
    public void attack()
    {
        int HitCount = Physics.OverlapBoxNonAlloc(transform.position + transform.forward, new Vector3(1f,0.5f,0.5f), colliders, transform.rotation, 1 << playerLayer, QueryTriggerInteraction.Ignore);
        if (HitCount > 0)
        {
            for(int i = 0;i < HitCount; i++)
            {
                Collider c = colliders[i];
                if(c != null)
                {
                    PlayerControl pC =c.GetComponent<PlayerControl>();
                    if (pC != null)
                        pC.ReduceHp(baseDamage);
                    
                }

            }
        }

    }

    /// <summary>
    /// 敌人扣血
    /// </summary>
    /// <param name="atk">攻击的扣除血量</param>
    /// <param name="Ta">自己的方向，判断受击反馈</param>
    public void ReduceHp(float atk,Vector3 Self)
    {
        if(HP - atk <= 0)
        {
            animator.SetBool("dead", true);
            Invoke("Dead" , 1);
            enabled = false;

        }
        else
        {
            HP -= atk;
            //受击反馈
            this.transform.position -= Vector3.Normalize(Self - transform.position) * 0.1f;
            print(gameObject.name + "还剩" + HP + "点血");
        }

        
    }

    public void Dead()
    {
        Destroy(this.gameObject);
    }

}
