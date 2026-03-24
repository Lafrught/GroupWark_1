using UnityEngine;

public class EnemyBackTrigger : MonoBehaviour
{
    [HideInInspector]
    public bool canBeHitFromBack = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            canBeHitFromBack = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            canBeHitFromBack = false;
        }
    }
}