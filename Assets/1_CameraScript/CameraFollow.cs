using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0, 10, -10);

    private void LateUpdate()
    {
        if (target == null) return;

        // 位置追従
        transform.position = target.position + offset;

        // ターゲットを見る
        transform.LookAt(target.position);
    }
}