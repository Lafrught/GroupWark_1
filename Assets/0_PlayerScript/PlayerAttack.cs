using UnityEngine;
using System.Collections;

public class PlayerAttack : MonoBehaviour
{
    [Header("攻撃設定")]
    public int damage = 10;
    public LayerMask enemyLayer;
    public float attackRadius = 1.5f;

    [Header("アニメーション設定")]
    public Animator animator;
    public string attackAnimationName = "Attack";
    public float attackDuration = 0.5f; // 攻撃アニメーション時間に合わせる

    [Header("エフェクト設定")]
    public Transform attackEffectPoint;   // エフェクトを出す位置（空オブジェクト）
    public GameObject attackEffectPrefab; // エフェクトのPrefab
    public float effectDelay = 0.3f;      // アニメーション開始からエフェクト発生までの遅延

    private bool isAttacking = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !isAttacking)
        {
            if (CanHitEnemy())
                StartCoroutine(DoAttack());
            else
                Debug.Log("攻撃可能な敵がいない");
        }
    }

    private IEnumerator DoAttack()
    {
        isAttacking = true;

        // 移動停止
        PlayerMovement movement = GetComponent<PlayerMovement>();
        if (movement != null) movement.enabled = false;

        // 攻撃アニメーション再生
        if (animator != null) animator.Play(attackAnimationName);

        // エフェクト発生（0.3秒遅延）
        if (attackEffectPrefab != null && attackEffectPoint != null)
            StartCoroutine(PlayEffectWithDelay());

        // 攻撃判定
        Attack();

        // 攻撃アニメーションの長さだけ待機
        yield return new WaitForSeconds(attackDuration);

        // 攻撃終了
        isAttacking = false;

        // 移動再開
        if (movement != null) movement.enabled = true;
    }

    private IEnumerator PlayEffectWithDelay()
    {
        yield return new WaitForSeconds(effectDelay);
        Instantiate(attackEffectPrefab, attackEffectPoint.position, attackEffectPoint.rotation);
    }

    void Attack()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, attackRadius, enemyLayer);

        foreach (var hit in hits)
        {
            EnemyBackTrigger backTrigger = hit.GetComponentInChildren<EnemyBackTrigger>();
            if (backTrigger != null && backTrigger.canBeHitFromBack)
            {
                EnemyAI enemyAI = hit.GetComponent<EnemyAI>();
                if (enemyAI != null && enemyAI.currentState == EnemyAI.EnemyState.Chase)
                    continue;

                EnemyHealth enemyHealth = hit.GetComponent<EnemyHealth>();
                if (enemyHealth != null)
                    enemyHealth.TakeDamage(damage);
            }
        }
    }

    bool CanHitEnemy()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, attackRadius, enemyLayer);
        foreach (var hit in hits)
        {
            EnemyBackTrigger backTrigger = hit.GetComponentInChildren<EnemyBackTrigger>();
            if (backTrigger != null && backTrigger.canBeHitFromBack)
            {
                EnemyAI enemyAI = hit.GetComponent<EnemyAI>();
                if (enemyAI == null || enemyAI.currentState != EnemyAI.EnemyState.Chase)
                    return true;
            }
        }
        return false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);
    }
}