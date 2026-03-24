using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0, 10, -10);

    private void LateUpdate()
    {
        transform.position = target.position + offset;
        transform.rotation = Quaternion.Euler(45f, 0f, 0f);
    }
}