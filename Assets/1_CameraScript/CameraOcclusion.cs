using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CameraOcclusionURP : MonoBehaviour
{
    [Header("対象プレイヤー")]
    [SerializeField] private Transform player;

    [Header("障害物検出")]
    [SerializeField] private LayerMask occlusionLayer;
    [SerializeField] private float capsuleRadius = 0.5f;

    [Header("透明化設定")]
    [SerializeField, Range(0f, 1f)] private float transparentAlpha = 0.7f; // 半透明のアルファ値

    // 現在透明化しているレンダラー
    private HashSet<Renderer> currentlyTransparent = new HashSet<Renderer>();

    // 元のマテリアルカラーを保存しておく
    private Dictionary<Material, Color> originalColors = new Dictionary<Material, Color>();

    // インスタンス化したマテリアルを保持（共有マテリアルを壊さない）
    private Dictionary<Renderer, Material[]> instantiatedMaterials = new Dictionary<Renderer, Material[]>();

    void LateUpdate()
    {
        if (!player) return;

        Vector3 dir = player.position - transform.position;
        float distance = dir.magnitude;

        // カメラ→プレイヤー間の障害物を検出
        RaycastHit[] hits = Physics.CapsuleCastAll(transform.position, transform.position, capsuleRadius, dir, distance, occlusionLayer);

        HashSet<Renderer> hitRenderers = new HashSet<Renderer>();

        foreach (var hit in hits)
        {
            Renderer rend = hit.collider.GetComponent<Renderer>();
            if (rend == null) continue;

            hitRenderers.Add(rend);

            if (!currentlyTransparent.Contains(rend))
            {
                // マテリアルをインスタンス化してオブジェクト単位で半透明化
                Material[] mats = rend.materials;
                Material[] newMats = new Material[mats.Length];
                for (int i = 0; i < mats.Length; i++)
                {
                    newMats[i] = new Material(mats[i]);
                    if (!originalColors.ContainsKey(newMats[i]))
                        originalColors[newMats[i]] = newMats[i].color;

                    SetMaterialTransparent(newMats[i], transparentAlpha);
                }
                rend.materials = newMats;
                instantiatedMaterials[rend] = newMats;
                currentlyTransparent.Add(rend);
            }
        }

        // Rayに当たらなくなったオブジェクトを元に戻す
        List<Renderer> toRestore = new List<Renderer>();
        foreach (var rend in currentlyTransparent)
        {
            if (!hitRenderers.Contains(rend))
            {
                if (instantiatedMaterials.TryGetValue(rend, out Material[] mats))
                {
                    foreach (var mat in mats)
                    {
                        if (originalColors.TryGetValue(mat, out Color original))
                        {
                            RestoreMaterial(mat, original);
                            originalColors.Remove(mat);
                        }
                    }
                }
                toRestore.Add(rend);
            }
        }

        foreach (var rend in toRestore)
        {
            currentlyTransparent.Remove(rend);
            instantiatedMaterials.Remove(rend);
        }

        // デバッグ用（線を引く）
        Debug.DrawLine(transform.position, player.position, Color.green);
        foreach (var hit in hits)
            Debug.DrawLine(transform.position, hit.point, Color.red);
    }

    // 透明化する処理（URP Lit Shader対応）
    private void SetMaterialTransparent(Material mat, float alpha)
    {
        Color c = mat.color;
        c.a = alpha; // 半透明
        mat.color = c;

        // URP Lit Shader の透明化設定
        mat.SetFloat("_Surface", 1f);                     // 1 = Transparent
        mat.SetFloat("_Blend", 0f);                       // Alpha Blending
        mat.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.SetOverrideTag("RenderType", "Transparent");
        mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        mat.EnableKeyword("_ALPHAPREMULTIPLY_ON");
    }

    // 元に戻す処理
    private void RestoreMaterial(Material mat, Color originalColor)
    {
        mat.color = originalColor;

        mat.SetFloat("_Surface", 0f); // 0 = Opaque
        mat.SetFloat("_Blend", 0f);
        mat.SetInt("_SrcBlend", (int)BlendMode.One);
        mat.SetInt("_DstBlend", (int)BlendMode.Zero);
        mat.SetInt("_ZWrite", 1);
        mat.SetOverrideTag("RenderType", "Opaque");
        mat.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
    }
}