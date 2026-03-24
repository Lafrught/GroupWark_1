using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference dashAction; // ← 追加

    [SerializeField] private float speed = 4f;
    [SerializeField] private float dashSpeed = 8f; // ← 追加
    [SerializeField] private float rotationSpeed = 8f;

    private Rigidbody rb;
    private Vector2 input;
    private bool isDashing;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        moveAction.action.Enable();
        dashAction.action.Enable(); // ← 追加
    }

    private void OnDisable()
    {
        moveAction.action.Disable();
        dashAction.action.Disable(); // ← 追加
    }

    private void Update()
    {
        // 入力取得
        input = moveAction.action.ReadValue<Vector2>();

        // ダッシュ入力（押している間だけダッシュ）
        isDashing = dashAction.action.IsPressed();
    }

    private void FixedUpdate()
    {
        // 入力をワールド方向に変換（XZ平面）
        Vector3 move = new Vector3(input.x, 0, input.y);

        // 速度切り替え
        float currentSpeed = isDashing ? dashSpeed : speed;

        // 移動
        rb.MovePosition(
            rb.position + move * currentSpeed * Time.fixedDeltaTime
        );

        // 回転（移動方向に向く）
        if (move != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(move);

            rb.MoveRotation(
                Quaternion.Slerp(
                    rb.rotation,
                    targetRotation,
                    rotationSpeed * Time.fixedDeltaTime
                )
            );
        }
    }
}