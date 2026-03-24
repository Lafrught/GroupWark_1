using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimationController : MonoBehaviour
{
    private Animator animator;

    private Vector3 lastPosition;

    [Header("速度調整")]
    [SerializeField] private float speedMultiplier = 1f;
    [SerializeField] private float dampTime = 0.1f;

    void Start()
    {
        animator = GetComponent<Animator>();
        lastPosition = transform.position;
    }

    void Update()
    {
        // フレーム間の移動量から速度を計算
        Vector3 delta = transform.position - lastPosition;
        float speed = delta.magnitude / Time.deltaTime;

        // 0?1くらいに正規化（適宜調整）
        float normalizedSpeed = speed * speedMultiplier;

        // Animator に渡す（スムーズに変化）
        animator.SetFloat("Speed", normalizedSpeed, dampTime, Time.deltaTime);

        lastPosition = transform.position;
    }
}