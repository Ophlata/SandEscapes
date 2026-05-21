using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyChaser : MonoBehaviour
{
    [Header("Цель")]
    public Transform player;

    [Header("Радиусы")]
    public float detectionRadius = 15f;
    public float attackRadius = 2f;

    [Header("Скорость")]
    public float patrolSpeed = 2f;
    public float chaseSpeed = 5f;

    [Header("Атака")]
    public int attackDamage = 10;
    public float attackCooldown = 1.5f;

    [Header("Патруль")]
    public Transform[] patrolPoints;
    public float waitAtPointTime = 2f;

    private NavMeshAgent agent;
    private Animator animator;
    private HealthSystem playerHealth;

    private int patrolIndex;
    private float waitTimer;
    private float attackTimer;

    private enum State { Patrol, Chase, Attack }
    private State state = State.Patrol;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();

        if (player == null)
        {
            GameObject foundPlayer = GameObject.FindGameObjectWithTag("Player");
            if (foundPlayer != null)
                player = foundPlayer.transform;
        }

        if (player != null)
            playerHealth = player.GetComponent<HealthSystem>();
    }

    void Start()
    {
        agent.speed = patrolSpeed;

        if (patrolPoints.Length > 0)
            agent.SetDestination(patrolPoints[0].position);
    }

    void Update()
    {
        if (player == null) return;

        attackTimer -= Time.deltaTime;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= attackRadius)
            state = State.Attack;
        else if (distance <= detectionRadius)
            state = State.Chase;
        else
            state = State.Patrol;

        switch (state)
        {
            case State.Patrol:
                Patrol();
                break;

            case State.Chase:
                Chase();
                break;

            case State.Attack:
                Attack();
                break;
        }

        UpdateAnimator();
    }

    void Patrol()
    {
        agent.isStopped = false;
        agent.speed = patrolSpeed;

        if (patrolPoints.Length == 0)
        {
            agent.SetDestination(transform.position);
            return;
        }

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            waitTimer += Time.deltaTime;

            if (waitTimer >= waitAtPointTime)
            {
                waitTimer = 0f;
                patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
                agent.SetDestination(patrolPoints[patrolIndex].position);
            }
        }
    }

    void Chase()
    {
        agent.isStopped = false;
        agent.speed = chaseSpeed;
        agent.SetDestination(player.position);
    }

    void Attack()
    {
        agent.isStopped = true;

        Vector3 lookPos = new Vector3(player.position.x, transform.position.y, player.position.z);
        transform.LookAt(lookPos);

        if (attackTimer <= 0f)
        {
            attackTimer = attackCooldown;

            if (animator != null)
                animator.SetTrigger("Attack");

            if (playerHealth != null)
                playerHealth.TakeDamage(attackDamage);
        }
    }

    void UpdateAnimator()
    {
        if (animator == null) return;

        float speed = agent.velocity.magnitude;
        animator.SetFloat("Speed", speed);
    }
}