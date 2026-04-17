using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class EnemyAI : MonoBehaviour
{
    public enum EnemyState { Idle, Chase }

    [Header("プレイヤー")]
    public Transform player;

    private NavMeshAgent agent;

    [Header("移動")]
    public float chaseSpeed = 5f;

    [Header("索敵範囲")]
    public float detectRadius = 8f;
    public float loseRadius = 15f;

    [Header("攻撃")]
    public float attackRange = 2f;
    public float attackCooldown = 3f;
    public float attackDuration = 1.2f;

    private float attackTimer = 0f;

    [Header("攻撃コライダー")]
    public Collider attackCollider;

    [Header("エフェクト")]
    public GameObject attackEffect;

    [Header("ヒットストップ")]
    public float hitStopTime = 0.05f;

    [Header("ノックバック")]
    public float knockbackPower = 3f;

    [Header("Animator")]
    public Animator animator;

    private bool isAttacking = false;
    private EnemyState currentState = EnemyState.Idle;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        attackTimer += Time.deltaTime;

        if (isAttacking)
        {
            if (agent != null)
            {
                agent.isStopped = true;
                agent.velocity = Vector3.zero;
            }

            UpdateAnimator();
            return;
        }

        switch (GetState())
        {
            case EnemyState.Idle:
                Idle();

                if (IsInDetectRange())
                    Chase();
                break;

            case EnemyState.Chase:
                Chase();

                if (!IsInLoseRange())
                    agent.ResetPath();
                else
                    TryAttack();
                break;
        }

        UpdateAnimator();
    }

    EnemyState GetState()
    {
        float dist = DistanceToPlayer();

        if (dist <= detectRadius)
            return EnemyState.Chase;

        if (dist >= loseRadius)
            return EnemyState.Idle;

        return currentState;
    }

    void Chase()
    {
        currentState = EnemyState.Chase;

        float dist = DistanceToPlayer();

        agent.speed = chaseSpeed;

        if (dist > attackRange)
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);
        }
        else
        {
            agent.isStopped = true;
        }
    }

    void TryAttack()
    {
        float dist = DistanceToPlayer();

        if (dist <= attackRange && attackTimer >= attackCooldown)
            Attack();
    }

    void Attack()
    {
        attackTimer = 0f;
        isAttacking = true;

        if (agent != null)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }

        animator.SetTrigger("attack");

        // 攻撃エフェクト
        if (attackEffect != null)
        {
            Instantiate(attackEffect, transform.position + transform.forward, Quaternion.identity);
        }

        StartCoroutine(HitboxDelayRoutine());
        StartCoroutine(AttackRoutine());
    }

    IEnumerator HitboxDelayRoutine()
    {
        yield return new WaitForSeconds(1f); // 遅れてON

        if (attackCollider != null)
        {
            attackCollider.GetComponent<EnemyAttackHitbox>()?.ResetHit();
            attackCollider.enabled = true;

            // ここで同時に「早めOFF予約」
            StartCoroutine(HitboxAutoOff());
        }
    }

    IEnumerator HitboxAutoOff()
    {
        yield return new WaitForSeconds(0.6f); // 短いヒットフレーム

        if (attackCollider != null)
        {
            attackCollider.enabled = false;
        }
    }

    IEnumerator AttackRoutine()
    {
        yield return new WaitForSeconds(attackDuration);
        EndAttack();
    }

    public void EndAttack()
    {
        isAttacking = false;

        if (attackCollider != null)
            attackCollider.enabled = false;

        if (agent != null)
            agent.isStopped = false;
    }

    float DistanceToPlayer()
    {
        return Vector3.Distance(transform.position, player.position);
    }

    bool IsInDetectRange()
    {
        return DistanceToPlayer() <= detectRadius;
    }

    bool IsInLoseRange()
    {
        return DistanceToPlayer() <= loseRadius;
    }

    void Idle()
    {
        if (agent == null) return;

        currentState = EnemyState.Idle;
        agent.speed = 0f;
        agent.ResetPath();
    }

    void UpdateAnimator()
    {
        if (animator == null) return;

        animator.SetBool("isChasing", currentState == EnemyState.Chase);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectRadius);

        Gizmos.color = Color.gray;
        Gizmos.DrawWireSphere(transform.position, loseRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position, 0.2f);
    }
}