using UnityEngine;
using UnityEngine.AI;

public class EnemyAnimationController : MonoBehaviour
{
    public Animator animator;       // 子モデルのAnimator
    private NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        if (animator == null || agent == null) return;

        // 現在のAnimatorステートを取得
        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);

        if (state.IsName("HitBack"))
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;   // これで瞬時に止まる
        }
        else
        {
            agent.isStopped = false;
        }
    }
}