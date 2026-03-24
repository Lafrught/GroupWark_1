using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public enum EnemyState { Patrol, Chase, Search }

    [Header("äÓñ{ê›íË")]
    public Transform player;
    public EnemyState currentState;

    private NavMeshAgent agent;

    [Header("Patrolê›íË")]
    public Transform patrolCenterObject;
    public float patrolRadius = 10f;
    public float patrolSpeed = 2f;

    [Header("Searchê›íË")]
    public float searchRadius = 5f;
    public float searchTime = 5f;
    public float searchSpeed = 3f;

    [Header("Chaseê›íË")]
    public float lostTime = 3f;
    public float chaseSpeed = 5f;

    [Header("éãäEê›íË")]
    public float viewDistance = 10f;
    public float viewAngle = 60f;
    public LayerMask obstacleMask;

    [Header("Animatorê›íË")]
    public Animator animator;

    private float lostTimer = 0f;
    private float searchTimer = 0f;
    private Vector3 lastKnownPosition;
    private Vector3 patrolCenter;

    void Awake()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
    }

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        currentState = EnemyState.Patrol;

        patrolCenter = patrolCenterObject != null ? patrolCenterObject.position : transform.position;
        SetRandomPatrolDestination();
    }

    void Update()
    {
        // èÛë‘èàóù
        switch (currentState)
        {
            case EnemyState.Patrol:
                agent.speed = patrolSpeed;
                Patrol();
                if (CanSeePlayer())
                {
                    currentState = EnemyState.Chase;
                    lastKnownPosition = player.position;
                }
                break;

            case EnemyState.Chase:
                agent.speed = chaseSpeed;
                Chase();
                if (CanSeePlayer())
                {
                    lastKnownPosition = player.position;
                    lostTimer = 0f;
                }
                else
                {
                    lostTimer += Time.deltaTime;
                    if (lostTimer >= lostTime)
                    {
                        currentState = EnemyState.Search;
                        searchTimer = 0f;
                        agent.SetDestination(lastKnownPosition);
                    }
                }
                break;

            case EnemyState.Search:
                agent.speed = searchSpeed;
                Search();
                if (CanSeePlayer())
                {
                    currentState = EnemyState.Chase;
                }
                break;
        }

        UpdateAnimatorBools();
    }

    // Animator ÇÃ Bool ÉtÉâÉOÇîrëºÇ≈çXêV
    void UpdateAnimatorBools()
    {
        if (animator == null) return;

        animator.SetBool("isPatrolling", currentState == EnemyState.Patrol);
        animator.SetBool("isChasing", currentState == EnemyState.Chase);
        animator.SetBool("isSearching", currentState == EnemyState.Search);
    }

    // =================
    // èÛë‘èàóù
    // =================
    void Patrol()
    {
        if (!agent.hasPath || agent.remainingDistance < 0.5f)
            SetRandomPatrolDestination();
    }

    void SetRandomPatrolDestination()
    {
        Vector3 centerPos = patrolCenterObject != null ? patrolCenterObject.position : patrolCenter;
        Vector3 randomPos = centerPos + Random.insideUnitSphere * patrolRadius;
        randomPos.y = centerPos.y;

        if (NavMesh.SamplePosition(randomPos, out NavMeshHit hit, patrolRadius, NavMesh.AllAreas))
            agent.SetDestination(hit.position);
    }

    void Chase()
    {
        agent.SetDestination(player.position);
    }

    void Search()
    {
        searchTimer += Time.deltaTime;

        if (!agent.hasPath || agent.remainingDistance < 0.5f)
        {
            Vector3 randomPos = lastKnownPosition + Random.insideUnitSphere * searchRadius;
            randomPos.y = patrolCenter.y;

            if (NavMesh.SamplePosition(randomPos, out NavMeshHit hit, searchRadius, NavMesh.AllAreas))
                agent.SetDestination(hit.position);
        }

        if (searchTimer >= searchTime)
            currentState = EnemyState.Patrol;
    }

    // =================
    // éãäEîªíË
    // =================
    bool CanSeePlayer()
    {
        Vector3 dir = player.position - transform.position;
        float distance = dir.magnitude;
        if (distance > viewDistance) return false;

        float angle = Vector3.Angle(transform.forward, dir);
        if (angle > viewAngle / 2f) return false;

        Ray ray = new Ray(transform.position + Vector3.up * 1.5f, dir.normalized);
        if (Physics.Raycast(ray, out RaycastHit hit, viewDistance, ~obstacleMask))
            return hit.transform == player;

        return false;
    }

    // =================
    // ÉfÉoÉbÉOópGizmos
    // =================
    void OnDrawGizmos()
    {
        // éãäE
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewDistance);

        Vector3 left = Quaternion.Euler(0, -viewAngle / 2, 0) * transform.forward;
        Vector3 right = Quaternion.Euler(0, viewAngle / 2, 0) * transform.forward;
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + left * viewDistance);
        Gizmos.DrawLine(transform.position, transform.position + right * viewDistance);

        // ç≈å„Ç…å©ÇΩà íu
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(lastKnownPosition, 0.3f);

        // íTçıîÕàÕ
        Gizmos.color = new Color(1, 0, 0, 0.2f);
        Gizmos.DrawWireSphere(lastKnownPosition, searchRadius);

        // úpújîÕàÕ
        Vector3 centerPos = patrolCenterObject != null ? patrolCenterObject.position : patrolCenter;
        Gizmos.color = new Color(0, 1, 0, 0.2f);
        Gizmos.DrawWireSphere(centerPos, patrolRadius);
    }
}