using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference dashAction;

    [Header("Movement")]
    [SerializeField] private float speed = 4f;
    [SerializeField] private float dashSpeed = 8f;
    [SerializeField] private float rotationSpeed = 8f;

    [Header("Stamina")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float staminaDecreaseRate = 20f;

    [Header("UI")]
    [SerializeField] private Text staminaText; // 

    private Rigidbody rb;
    private Vector2 input;
    private bool isDashing;

    private float currentStamina;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        currentStamina = maxStamina;

        UpdateStaminaText();
    }

    private void OnEnable()
    {
        moveAction.action.Enable();
        dashAction.action.Enable();
    }

    private void OnDisable()
    {
        moveAction.action.Disable();
        dashAction.action.Disable();
    }

    private void Update()
    {
        // 入力取得
        input = moveAction.action.ReadValue<Vector2>();

        bool isMoving = input != Vector2.zero;

        // ダッシュ判定（スタミナがある時のみ）
        bool dashInput = dashAction.action.IsPressed();
        isDashing = dashInput && currentStamina > 0f;

        //スタミナ減少処理
        if (isMoving && currentStamina > 0f)
        {
            float decreaseRate = isDashing
                ? staminaDecreaseRate              // ダッシュ時
                : staminaDecreaseRate * 0.5f;      // 歩き時（半分）

            currentStamina -= decreaseRate * Time.deltaTime;
            currentStamina = Mathf.Max(currentStamina, 0f);
        }

        // UI更新
        UpdateStaminaText();
    }

    private void FixedUpdate()
    {
        Vector3 move = new Vector3(input.x, 0, input.y);

        float currentSpeed = isDashing ? dashSpeed : speed;

        // 移動
        rb.MovePosition(
            rb.position + move * currentSpeed * Time.fixedDeltaTime
        );

        // 回転
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

    private void UpdateStaminaText()
    {
        if (staminaText == null) return;

        int staminaInt = Mathf.CeilToInt(currentStamina);
        staminaText.text =  staminaInt + " / " + maxStamina;

        // 段階で色変更
        if (staminaInt >= 200)
        {
            staminaText.color = Color.white;
        }
        else if (staminaInt >= 50)
        {
            staminaText.color = Color.yellow;
        }
        else
        {
            staminaText.color = Color.red;
        }
    }
}