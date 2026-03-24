using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private SimpleInventory inventory;
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private Transform slotParent;

    private List<Image> slotImages = new List<Image>();

    private void Start()
    {
        inventory.OnInventoryChanged += RefreshUI;
        CreateSlots();
        RefreshUI();
    }

    private void CreateSlots()
    {
        foreach (Transform child in slotParent)
            Destroy(child.gameObject);

        slotImages.Clear();

        for (int i = 0; i < inventory.GetMaxSlots(); i++)
        {
            GameObject slot = Instantiate(slotPrefab, slotParent);
            Image img = slot.GetComponentInChildren<Image>();
            slotImages.Add(img);
        }
    }

    private void RefreshUI()
    {
        if (slotImages.Count != inventory.GetMaxSlots())
        {
            CreateSlots();
        }

        var items = inventory.GetItems();

        for (int i = 0; i < slotImages.Count; i++)
        {
            if (i < items.Count && items[i] != null)
            {
                slotImages[i].sprite = items[i].icon;
                slotImages[i].enabled = true;
            }
            else
            {
                slotImages[i].sprite = null;
                slotImages[i].enabled = false;
            }
        }
    }
}