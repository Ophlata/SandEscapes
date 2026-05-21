using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyChaser : MonoBehaviour
{
    [Header("Цель")]
    public Transform player;

    [Header("Обнаружение")]
    public float detectionRadius = 15f;
    [Range(0f, 180f)]
    public float fieldOfViewAngle = 90f;
    public LayerMask obstacleMask;

    [Header("Скорость")]
    public float patrolSpeed = 2f;
    public float chaseSpeed = 5f;

    [Header("Патруль")]
    public Transform[] patrolPoints;
    public float waitAtPointTime = 2f;

    [Header("Скример / проигрыш")]
    public GameObject screamerPanel;
    public AudioSource screamerSound;
    public bool stopTimeOnScreamer = true;

    private NavMeshAgent agent;
    private Animator animator;

    private int patrolIndex;
    private float waitTimer;
    private bool gameEnded = false;

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

        if (screamerPanel != null)
            screamerPanel.SetActive(false);
    }

    void Start()
    {
        agent.speed = patrolSpeed;

        if (patrolPoints.Length > 0)
            agent.SetDestination(patrolPoints[0].position);
    }

    void Update()
    {
        if (gameEnded || player == null) return;

        if (CanSeePlayer())
        {
            TriggerScreamer();
            return;
        }

        Patrol();

        if (animator != null)
            animator.SetFloat("Speed", agent.velocity.magnitude);
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

    bool CanSeePlayer()
    {
        Vector3 toPlayer = player.position - transform.position;
        float distance = toPlayer.magnitude;

        if (distance > detectionRadius)
            return false;

        float angle = Vector3.Angle(transform.forward, toPlayer);
        if (angle > fieldOfViewAngle)
            return false;

        if (Physics.Raycast(
            transform.position + Vector3.up,
            toPlayer.normalized,
            out RaycastHit hit,
            distance,
            obstacleMask))
        {
            return false;
        }

        return true;
    }

    void TriggerScreamer()
    {
        gameEnded = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (agent != null)
            agent.isStopped = true;

        if (animator != null)
            animator.SetFloat("Speed", 0f);

        if (screamerPanel != null)
            screamerPanel.SetActive(true);

        if (screamerSound != null)
            screamerSound.Play();

        Time.timeScale = 1f;

        Debug.Log("Монстр увидел игрока. Проигрыш.");
    }
}