using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class GENO : MonoBehaviour
{
    public NavMeshAgent agent;
    public Transform player;
    public LayerMask whatIsGround, whatIsPlayer;
    public int enemyHealth;
    public Animator anim;
    public float enemySpeed = 1f;
    public bool isPatroling = false;
    public float patrolSpeed = 1f;
    public int attackDamage;
    public bool isDead;
    private MoveBehaviour playerBehaviour;
    public Slider healthBar;

    public Vector3 walkPoint;
    bool walkPointSet;
    public float walkPointRange;

    public float timeBetweenAttacks;
    bool alreadyAttacked;

    public float maxAngularSpeed = 120f;

    public Collider weaponCollider;

    public float sightRange, attackRange;
    public bool playerInSightRange, playerInAttackRange;

    private bool hitAudioPlayed = false;


    private void Awake()
    {
        player = GameObject.Find("brutus").transform;
        agent = GetComponent<NavMeshAgent>();
        agent.angularSpeed = maxAngularSpeed;
        anim = GetComponent<Animator>();
        playerBehaviour = player.GetComponent<MoveBehaviour>();
    }
    void Update()
    {
        if (!isDead)
        {
            playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);
            playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);



            if (!playerInSightRange && !playerInAttackRange) Patroling();
            else if (playerInSightRange && !playerInAttackRange) ChasePlayer();
            else if (playerInSightRange && playerInAttackRange) AttackPlayer();
        }

        healthBar.value = enemyHealth;

    }
    private void Patroling()
    {
        if (!isDead)
        {
            anim.SetBool("IsPatroling", true);
            if (!walkPointSet) SearchWalkPoint();

            if (walkPointSet)
                agent.SetDestination(walkPoint);

            Vector3 distanceToWalkPoint = transform.position - walkPoint;

            if (distanceToWalkPoint.magnitude < 1f)
                walkPointSet = false;
            anim.SetBool("IsRunning", false);
        }
    }
    private void SearchWalkPoint()
    {
        float randomZ = Random.Range(-walkPointRange, walkPointRange);
        float randomX = Random.Range(-walkPointRange, walkPointRange);

        walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

        if (Physics.Raycast(walkPoint, -transform.up, 1f, whatIsGround))
            walkPointSet = true;
        agent.speed = patrolSpeed;
    }
    private void ChasePlayer()
    {
        if (!isDead)
        {
            agent.SetDestination(player.position);
            anim.SetBool("IsRunning", true);
            anim.SetBool("IsPatroling", false);
            agent.speed = enemySpeed;
        }
    }

    private void AttackPlayer()
    {
        if (!isDead)
        {
            anim.SetBool("IsPatroling", false);
            anim.SetBool("IsRunning", false);
            agent.SetDestination(transform.position);
            transform.LookAt(player);

            if (!alreadyAttacked && playerBehaviour != null && !playerBehaviour.IsDead()) 
            {
                anim.SetTrigger("horizAttack");
                if (!hitAudioPlayed)
                {
                    FindAnyObjectByType<AudioManager>().Play("Hit received");
                    hitAudioPlayed = true;  
                }
                alreadyAttacked = true;
                Invoke(nameof(ResetAttack), timeBetweenAttacks);
            }
        }
    }
    private void ResetAttack()
    {
        alreadyAttacked = false;
        hitAudioPlayed = false;
    }
    public void TakeDamage(int damage)
    {
        enemyHealth -= damage;
        if (enemyHealth <= 0) Invoke("DestroyEnemy", 0.1f);
    }
    private void DestroyEnemy()
    {
        isDead = true;
        anim.SetTrigger("death");
        FindAnyObjectByType<AudioManager>().Play("enemy dead");
        GetComponent<Collider>().enabled = false;
        agent.isStopped = true;
        Destroy(healthBar.gameObject);
        FindObjectOfType<MoveBehaviour>().EnemyKilled();
        FindObjectOfType<MoveBehaviour>().UpdateKillText();
    }
}