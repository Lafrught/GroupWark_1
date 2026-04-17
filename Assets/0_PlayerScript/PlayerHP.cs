using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerHP : MonoBehaviour
{
    [Header("HP")]
    [SerializeField] private int maxHP = 100;
    [SerializeField] private int currentHP = 100;

    [Header("UI")]
    [SerializeField] private Text hpText;

    [Header("ダメージ設定")]
    [SerializeField] private float invincibleTime = 0.5f;

    [Header("Animator")]
    public Animator animator; // ★追加

    private bool isInvincible = false;

    private void Start()
    {
        currentHP = maxHP;
        UpdateHPText();
    }

    // =========================
    // ダメージ処理
    // =========================
    public void TakeDamage(int damage)
    {
        if (isInvincible) return;

        currentHP -= damage;
        currentHP = Mathf.Max(currentHP, 0);

        UpdateHPText();

        // 被弾アニメーション追加
        if (animator != null)
        {
            animator.SetTrigger("hit");
        }

        StartCoroutine(InvincibleRoutine());

        if (currentHP <= 0)
        {
            Die();
        }
    }

    IEnumerator InvincibleRoutine()
    {
        isInvincible = true;
        yield return new WaitForSeconds(invincibleTime);
        isInvincible = false;
    }

    // =========================
    // UI更新
    // =========================
    private void UpdateHPText()
    {
        if (hpText == null) return;

        hpText.text = currentHP + " / " + maxHP;

        if (currentHP >= 50)
        {
            hpText.color = Color.white;
        }
        else if (currentHP >= 20)
        {
            hpText.color = Color.yellow;
        }
        else
        {
            hpText.color = Color.red;
        }
    }

    // =========================
    // 死亡処理
    // =========================
    void Die()
    {
        Debug.Log("Player Dead");

        gameObject.SetActive(false);
    }
}
