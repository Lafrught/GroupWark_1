using UnityEngine;
using System.Collections;

public class EnemyAttackHitbox : MonoBehaviour
{
    public int damage = 10;

    [Header("ヒットエフェクト（ヒットストップ時のみ）")]
    public GameObject hitEffect;

    [Header("ヒットストップ")]
    public float hitStopScale = 0.1f;
    public float hitStopDuration = 0.05f;

    private bool hasHit = false;

    public void ResetHit()
    {
        hasHit = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasHit) return;

        if (other.CompareTag("Player"))
        {
            hasHit = true;

            var hp = other.GetComponent<PlayerHP>();
            if (hp != null)
            {
                hp.TakeDamage(damage);
            }

            // ヒットストップと同時に演出
            StartCoroutine(HitStopAndEffect(other));
        }
    }

    IEnumerator HitStopAndEffect(Collider player)
    {
        // ヒットストップ開始と同時にエフェクト
        if (hitEffect != null)
        {
            Vector3 pos = player.ClosestPoint(transform.position);
            Instantiate(hitEffect, pos, Quaternion.identity);
        }

        float original = Time.timeScale;

        Time.timeScale = hitStopScale;
        yield return new WaitForSecondsRealtime(hitStopDuration);
        Time.timeScale = original;
    }
}