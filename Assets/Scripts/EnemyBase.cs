using UnityEngine;

public class EnemyBase : MonoBehaviour
{
    private void OnTriggerStay(Collider other)
    {
        if (other.transform.CompareTag("Player") && GameManager.instance.playerHasTheFlag)
        {
            GameManager.instance.OnFlagDelivered();
        }
    }
}
