using UnityEngine;
using UnityEngine.UI;

public class ItemIconData : MonoBehaviour
{
    public string itemName;  // 名前
    public Sprite icon;      // アイコン画像
    public int price; // 売却価格

    public GameObject dropPrefab; // 落とす用の見た目
}