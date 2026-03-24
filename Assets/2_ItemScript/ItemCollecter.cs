using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ItemCollectorFrontOnly : MonoBehaviour
{
    [Header("回収範囲")]
    [SerializeField] private float collectRadius = 2f;

    [Header("プレイヤー正面の判定角度")]
    [SerializeField, Range(0f, 180f)] private float frontAngle = 60f;

    [Header("点滅設定")]
    [SerializeField] private Color blinkColor = Color.red;
    [SerializeField] private float blinkSpeed = 4f;

    [Header("インベントリ")]
    [SerializeField] private SimpleInventory inventory;

    [Header("アニメーション設定")]
    [SerializeField] private Animator animator;
    [SerializeField] private string pickupTriggerName = "Pickup";
    [SerializeField] private float collectDelay = 0.25f;

    [Header("入力ロック時間")]
    [SerializeField] private float inputLockTime = 0.3f;

    [Header("移動制御")]
    [SerializeField] private MonoBehaviour playerMovement; // ← 移動スクリプトを入れる

    private List<GameObject> nearbyItems = new List<GameObject>();
    private Dictionary<Renderer, Color> originalColors = new Dictionary<Renderer, Color>();

    private bool isPickingUp = false;

    void Update()
    {
        // アニメーション中はこのスクリプトも停止
        if (isPickingUp) return;

        UpdateNearbyItems();
        HandleBlink();

        if (Input.GetKeyDown(KeyCode.E))
        {
            TryCollect();
        }
    }

    private void TryCollect()
    {
        if (nearbyItems.Count == 0) return;

        isPickingUp = true;

        // 移動停止（これが今回の本質）
        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }

        // アニメーション再生
        if (animator != null && !string.IsNullOrEmpty(pickupTriggerName))
        {
            animator.SetTrigger(pickupTriggerName);
        }

        // アイテム取得タイミング
        if (collectDelay > 0f)
        {
            StartCoroutine(CollectWithDelay());
        }
        else
        {
            CollectItems();
        }

        // 入力＆移動解除
        StartCoroutine(UnlockAfterDelay());
    }

    private IEnumerator CollectWithDelay()
    {
        yield return new WaitForSeconds(collectDelay);
        CollectItems();
    }

    private IEnumerator UnlockAfterDelay()
    {
        yield return new WaitForSeconds(inputLockTime);

        isPickingUp = false;

        // 移動復帰
        if (playerMovement != null)
        {
            playerMovement.enabled = true;
        }
    }

    private void UpdateNearbyItems()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, collectRadius);
        HashSet<GameObject> currentNearby = new HashSet<GameObject>();

        Vector3 forward = transform.forward;

        foreach (var col in colliders)
        {
            if (!col.CompareTag("Item")) continue;

            Vector3 toItem = col.transform.position - transform.position;

            if (Vector3.Angle(forward, toItem) <= frontAngle * 0.5f)
            {
                currentNearby.Add(col.gameObject);

                if (!nearbyItems.Contains(col.gameObject))
                {
                    Renderer rend = col.GetComponent<Renderer>();
                    if (rend != null && !originalColors.ContainsKey(rend))
                    {
                        originalColors[rend] = rend.material.color;
                    }
                }
            }
        }

        List<GameObject> toRemove = new List<GameObject>();
        foreach (var item in nearbyItems)
        {
            if (!currentNearby.Contains(item))
            {
                Renderer rend = item.GetComponent<Renderer>();
                if (rend != null && originalColors.ContainsKey(rend))
                {
                    rend.material.color = originalColors[rend];
                    originalColors.Remove(rend);
                }
                toRemove.Add(item);
            }
        }

        foreach (var item in toRemove)
            nearbyItems.Remove(item);

        foreach (var item in currentNearby)
        {
            if (!nearbyItems.Contains(item))
                nearbyItems.Add(item);
        }
    }

    private void HandleBlink()
    {
        float t = Mathf.Sin(Time.time * blinkSpeed * Mathf.PI * 2) * 0.5f + 0.5f;
        Color blink = Color.Lerp(Color.white, blinkColor, t);

        foreach (var item in nearbyItems)
        {
            Renderer rend = item.GetComponent<Renderer>();
            if (rend != null)
            {
                rend.material.color = blink;
            }
        }
    }

    private void CollectItems()
    {
        foreach (var itemObj in nearbyItems)
        {
            Debug.Log("Collected: " + itemObj.name);

            ItemIconData itemData = itemObj.GetComponent<ItemIconData>();
            if (itemData != null)
            {
                bool added = inventory.AddItem(itemData);
                if (added)
                {
                    itemObj.SetActive(false);
                }
            }
        }

        nearbyItems.Clear();
        originalColors.Clear();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, collectRadius);

        Vector3 forward = transform.forward;
        Gizmos.color = new Color(1, 0, 0, 0.2f);

        Quaternion leftRot = Quaternion.Euler(0, -frontAngle * 0.5f, 0);
        Quaternion rightRot = Quaternion.Euler(0, frontAngle * 0.5f, 0);

        Gizmos.DrawLine(transform.position, transform.position + leftRot * forward * collectRadius);
        Gizmos.DrawLine(transform.position, transform.position + rightRot * forward * collectRadius);
    }
}