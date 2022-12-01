using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace EnemyPlayer
{
    

    public class EnemyGolem : MonoBehaviour
{
    NavMeshAgent agent;

    //public Transform player;
    public LayerMask character;

    public Transform target;

    //AI?Start
    //Attack
    //public float timeBetwwenAttacks;
    //public bool alreadyAttacked;
    //State
    public float sightRange = 10f;

    public bool playerInSightRange;

    public bool playerInAttackRange;

    //AI Finish
    // Golem attack
    public Transform golemAttackPoint;

    public float golemAttackRange = 1f;

    public int golemAttackDamage = 10;

    public float golemAttackSpeed = 1f;

    private float golemAttackCooldown = 0f;

    //--
    public int maxGolemHealth = 200;

    public CallAfterDelay CallAfterDelay;

    int currentHealth;

    public int layerholder;

    

    //public static List<Player> playerList = new List<Player>();

    //public static List<Player> GetPlayerList(){
   //     return playerList;
    //}
    public static List <GameObject> playerList;

    PhotonView pw;
    // Start is called before the first frame update
    void Start()
    {
        //target = EnemyPlayer.instance.player.transform;
        agent = GetComponent<NavMeshAgent>();
        currentHealth = maxGolemHealth;
        pw = GetComponent<PhotonView>();
        layerholder = LayerMask.NameToLayer("nonTargetable");
        playerList = new List<GameObject>();
        
    }
    /*private void Awake() {
        PlayerAttack[] components = GameObject.FindObjectsOfType<PlayerAttack>();
        foreach(PlayerAttack comp in components)
         playerList.Add(comp.gameObject);
    }*/
    
    [PunRPC]
    public void TakeDamage(int damage)
    {
        currentHealth = currentHealth - damage;
        transform.GetComponent<Animator>().SetTrigger("Hit");

        if (currentHealth <= 0)
        {
            Die();
            gameObject.layer = layerholder;
            Debug.Log("Current layer: " + gameObject.layer);
            CallAfterDelay.Create(2.0f, Destroy);
        }
    }

    [PunRPC]
    void Destroy()
    {
        Destroy (gameObject);
    }

    void Die()
    {
        transform.GetComponent<CapsuleCollider>().enabled = false;
        transform.GetComponent<Animator>().SetBool("Death", true);
        Debug.Log("Enemy died!");
    }

    void gAttack()
    {
        if (golemAttackCooldown <= 0f)
        {
            transform.GetComponent<Animator>().SetTrigger("GolemAttack");
            Collider[] players =
                Physics
                    .OverlapSphere(golemAttackPoint.position,
                    golemAttackRange,
                    character);
            foreach (Collider character in players)
            {
                if (character.GetComponent<PlayerAttack>())
                {
                    character
                        .GetComponent<PlayerAttack>()
                        .playerTakeDamage(golemAttackDamage);
                }
            }
            golemAttackCooldown = 1f / golemAttackSpeed;
        }
    }
    

    void faceTarget()
    {
        Vector3 direction = (target.position - transform.position).normalized;
        Quaternion faceRotate =
            Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation =
            Quaternion
                .Slerp(transform.rotation, faceRotate, Time.deltaTime * 5);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, sightRange);
        if (golemAttackPoint == null) return;
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(golemAttackPoint.position, golemAttackRange);
    }
    

    void Update()
    {
        
        golemAttackCooldown -= Time.deltaTime;
    if(playerList.Count != 0){
        foreach(GameObject p in playerList){
        float distance = Vector3.Distance(p.transform.position, transform.position);
        if (distance <= sightRange)
        {
            agent.SetDestination(p.transform.position);
        }
        if (distance <= agent.stoppingDistance)
        {
            gAttack();
            
        }
      }
    }
      
        
    }
}
}