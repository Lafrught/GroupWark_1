using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SellPoint : MonoBehaviour
{
    [SerializeField] private SimpleInventory inventory;
    [SerializeField] private PlayerMoney playerMoney;

    [Header("演出設定")]
    [SerializeField] private Transform dropPoint; // 足元位置
    [SerializeField] private GameObject sellEffect;
    [SerializeField] private float dropInterval = 0.15f;
    [SerializeField] private float effectDelay = 0.3f;

    private bool isPlayerNearby = false;
    private bool isSelling = false;

    private void Update()
    {
        if (isPlayerNearby && !isSelling && Input.GetKeyDown(KeyCode.E))
        {
            StartCoroutine(SellSequence());
        }
    }

    private IEnumerator SellSequence()
    {
        isSelling = true;

        var items = inventory.GetItems();

        if (items.Count == 0)
        {
            Debug.Log("売るものがない");
            isSelling = false;
            yield break;
        }

        List<GameObject> spawnedItems = new List<GameObject>();
        int total = 0;

        // 足元にドロップ
        for (int i = items.Count - 1; i >= 0; i--)
        {
            var item = items[i];

            if (item.dropPrefab != null)
            {
                Vector3 pos = dropPoint.position + Random.insideUnitSphere * 1f;
                pos.y = dropPoint.position.y;

                GameObject obj = Instantiate(item.dropPrefab, pos, Quaternion.identity);
                spawnedItems.Add(obj);
            }

            total += item.price;
            inventory.RemoveItem(i);

            yield return new WaitForSeconds(dropInterval);
        }

        // 少し待つ
        yield return new WaitForSeconds(effectDelay);

        // エフェクト再生
        if (sellEffect != null)
        {
            Instantiate(sellEffect, dropPoint.position, Quaternion.identity);
        }

        // アイテム削除
        foreach (var obj in spawnedItems)
        {
            Destroy(obj);
        }

        // お金加算
        playerMoney.AddMoney(total);

        Debug.Log("売却完了: " + total);

        isSelling = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
        }
    }
}