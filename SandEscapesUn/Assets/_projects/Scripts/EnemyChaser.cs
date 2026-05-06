using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// NPC-преследователь игрока.
/// Требования: NavMeshAgent на объекте, запечённый NavMesh на сцене.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class EnemyChaser : MonoBehaviour
{
    // ──────────────────────────────────────────
    // Настройки в Inspector
    // ──────────────────────────────────────────

    [Header("Цель")]
    [Tooltip("Перетащи Transform игрока сюда, или оставь пустым — найдёт по тегу 'Player'")]
    public Transform player;

    [Header("Обнаружение")]
    [Tooltip("Радиус, в котором NPC замечает игрока")]
    public float detectionRadius = 15f;

    [Tooltip("Угол обзора (влево/вправо от взгляда NPC)")]
    [Range(0f, 180f)]
    public float fieldOfViewAngle = 90f;

    [Tooltip("Слой препятствий для проверки прямой видимости")]
    public LayerMask obstacleMask;

    [Header("Погоня")]
    public float chaseSpeed = 5f;
    public float patrolSpeed = 2f;

    [Tooltip("Дистанция, с которой NPC останавливается рядом с игроком")]
    public float stoppingDistance = 1.5f;

    [Header("Патрулирование")]
    [Tooltip("Точки патрулирования. Если пусто — NPC стоит на месте")]
    public Transform[] patrolPoints;
    public float waitAtPointTime = 2f;

    [Header("Дебаг")]
    public bool drawGizmos = true;

    // ──────────────────────────────────────────
    // Приватные поля
    // ──────────────────────────────────────────

    private NavMeshAgent agent;
    private Animator animator;          // опционально
    private bool hasAnimator;

    private enum State { Patrol, Chase, Search }
    private State currentState = State.Patrol;

    private int currentPatrolIndex = 0;
    private float waitTimer = 0f;
    private bool isWaiting = false;

    private Vector3 lastKnownPosition;
    private float searchTimer = 0f;
    private float searchDuration = 5f;  // сколько секунд ищем после потери игрока

    // ──────────────────────────────────────────
    // Unity lifecycle
    // ──────────────────────────────────────────

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        hasAnimator = animator != null;

        if (player == null)
        {
            GameObject found = GameObject.FindWithTag("Player");
            if (found != null) player = found.transform;
        }
    }

    private void Start()
    {
        agent.speed = patrolSpeed;
        agent.stoppingDistance = stoppingDistance;

        // Ждём, пока агент встанет на NavMesh, затем идём к первой точке
        if (patrolPoints.Length > 0)
            StartCoroutine(WaitForNavMeshThenStart());
    }

    private System.Collections.IEnumerator WaitForNavMeshThenStart()
    {
        // Ждём до двух секунд, пока агент окажется на NavMesh
        float timeout = 2f;
        while (!agent.isOnNavMesh && timeout > 0f)
        {
            timeout -= Time.deltaTime;
            yield return null;
        }

        if (agent.isOnNavMesh)
            GoToPatrolPoint(0);
        else
            Debug.LogWarning($"[EnemyChaser] {name}: агент не попал на NavMesh! " +
                             "Убедись что объект стоит на запечённой поверхности.", this);
    }

    private void Update()
    {
        bool canSeePlayer = CanSeePlayer();

        switch (currentState)
        {
            case State.Patrol:
                HandlePatrol();
                if (canSeePlayer) EnterChase();
                break;

            case State.Chase:
                HandleChase(canSeePlayer);
                break;

            case State.Search:
                HandleSearch(canSeePlayer);
                break;
        }

        UpdateAnimator();
    }

    // ──────────────────────────────────────────
    // Состояния
    // ──────────────────────────────────────────

    private void HandlePatrol()
    {
        if (patrolPoints.Length == 0) return;

        if (isWaiting)
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0f)
            {
                isWaiting = false;
                currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
                GoToPatrolPoint(currentPatrolIndex);
            }
            return;
        }

        // Достигли точки?
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            isWaiting = true;
            waitTimer = waitAtPointTime;
        }
    }

    private void HandleChase(bool canSee)
    {
        if (!agent.isOnNavMesh) return;

        if (canSee)
        {
            lastKnownPosition = player.position;
            agent.SetDestination(player.position);
        }
        else
        {
            // Потеряли из виду — идём в последнюю известную позицию
            currentState = State.Search;
            searchTimer = searchDuration;
            agent.SetDestination(lastKnownPosition);
        }
    }

    private void HandleSearch(bool canSee)
    {
        if (canSee)
        {
            EnterChase();
            return;
        }

        searchTimer -= Time.deltaTime;

        // Достигли последней известной точки или истёк таймер
        bool reachedSpot = !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance;

        if (reachedSpot || searchTimer <= 0f)
        {
            // Возвращаемся к патрулированию
            currentState = State.Patrol;
            agent.speed = patrolSpeed;
            isWaiting = false;
            if (patrolPoints.Length > 0)
                GoToPatrolPoint(currentPatrolIndex);
        }
    }

    // ──────────────────────────────────────────
    // Вспомогательные методы
    // ──────────────────────────────────────────

    private void EnterChase()
    {
        currentState = State.Chase;
        agent.speed = chaseSpeed;
        isWaiting = false;
    }

    private void GoToPatrolPoint(int index)
    {
        if (patrolPoints.Length == 0 || !agent.isOnNavMesh) return;
        agent.speed = patrolSpeed;
        agent.SetDestination(patrolPoints[index].position);
    }

    /// <summary>
    /// Проверяет, видит ли NPC игрока (дистанция + угол обзора + препятствия).
    /// </summary>
    private bool CanSeePlayer()
    {
        if (player == null) return false;

        Vector3 toPlayer = player.position - transform.position;
        float distance = toPlayer.magnitude;

        if (distance > detectionRadius) return false;

        float angle = Vector3.Angle(transform.forward, toPlayer);
        if (angle > fieldOfViewAngle) return false;

        // Проверка прямой видимости (raycast)
        if (Physics.Raycast(transform.position + Vector3.up * 1f,
                            toPlayer.normalized,
                            out RaycastHit hit,
                            distance,
                            obstacleMask))
        {
            // Луч упёрся в препятствие до игрока
            return false;
        }

        return true;
    }

    private void UpdateAnimator()
    {
        if (!hasAnimator) return;

        float speed = agent.velocity.magnitude;
        animator.SetFloat("Speed", speed);
        animator.SetBool("IsChasing", currentState == State.Chase);
    }

    // ──────────────────────────────────────────
    // Gizmos — отображение в редакторе
    // ──────────────────────────────────────────

    private void OnDrawGizmosSelected()
    {
        if (!drawGizmos) return;

        // Радиус обнаружения
        Gizmos.color = new Color(1f, 1f, 0f, 0.2f);
        Gizmos.DrawSphere(transform.position, detectionRadius);

        // Угол обзора
        Gizmos.color = Color.yellow;
        Vector3 forward = Application.isPlaying ? transform.forward : transform.forward;
        Quaternion leftRot  = Quaternion.AngleAxis(-fieldOfViewAngle, Vector3.up);
        Quaternion rightRot = Quaternion.AngleAxis( fieldOfViewAngle, Vector3.up);
        Gizmos.DrawRay(transform.position, leftRot  * forward * detectionRadius);
        Gizmos.DrawRay(transform.position, rightRot * forward * detectionRadius);

        // Линия к игроку при погоне
        if (Application.isPlaying && currentState == State.Chase && player != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, player.position);
        }

        // Точки патрулирования
        if (patrolPoints == null) return;
        Gizmos.color = Color.cyan;
        for (int i = 0; i < patrolPoints.Length; i++)
        {
            if (patrolPoints[i] == null) continue;
            Gizmos.DrawSphere(patrolPoints[i].position, 0.3f);
            if (i + 1 < patrolPoints.Length && patrolPoints[i + 1] != null)
                Gizmos.DrawLine(patrolPoints[i].position, patrolPoints[i + 1].position);
        }
    }
}