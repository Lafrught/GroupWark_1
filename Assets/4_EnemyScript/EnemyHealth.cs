using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class EnemyHealth : MonoBehaviour
{
    public int maxHealth = 50;
    private int currentHealth;

    [Header("アニメーション設定")]
    public string hitAnimation = "HitBack"; // 被ダメージ用
    public string dieAnimation = "Die";     // 死亡用

    private Animator animator;
    private bool isAnimating = false;       // アニメーション中フラグ
    private NavMeshAgent agent;

    private void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponentInChildren<Animator>();
        agent = GetComponent<NavMeshAgent>();

        if (animator == null)
            Debug.LogWarning(gameObject.name + " に Animator が見つかりません");

        if (agent == null)
            Debug.LogWarning(gameObject.name + " に NavMeshAgent が見つかりません");
    }

    public void TakeDamage(int amount)
    {
        if (isAnimating) return;

        currentHealth -= amount;
        Debug.Log(gameObject.name + " が " + amount + " ダメージを受けた");

        // Coroutine で 0.5 秒遅延してアニメーション再生
        StartCoroutine(PlayAnimationWithDelay(hitAnimation, 0.5f));

        if (currentHealth <= 0)
        {
            StartCoroutine(PlayAnimationWithDelay(dieAnimation, 0.5f, destroyAfter: true));
        }
    }

    // Coroutine で遅延再生
    private IEnumerator PlayAnimationWithDelay(string animName, float delay, bool destroyAfter = false)
    {
        isAnimating = true;

        // 移動停止
        if (agent != null)
            agent.isStopped = true;

        // 遅延
        yield return new WaitForSeconds(delay);

        // アニメーション再生
        if (animator != null)
        {
            animator.Play(animName);

            // 再生時間取得
            float animLength = animator.GetCurrentAnimatorStateInfo(0).length;

            if (destroyAfter)
            {
                Destroy(gameObject, animLength);
            }
            else
            {
                // アニメーション終了後フラグリセット
                yield return new WaitForSeconds(animLength);
                EndAnimation();
            }
        }
        else
        {
            EndAnimation();
        }
    }

    private void EndAnimation()
    {
        isAnimating = false;

        if (agent != null)
            agent.isStopped = false;
    }
}