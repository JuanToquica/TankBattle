using UnityEngine;

public class EnemyBase : MonoBehaviour
{
    [SerializeField] private GameObject vfx;
    private void OnTriggerStay(Collider other)
    {
        if (other.transform.CompareTag("Player") && GameManager.instance.playerHasTheFlag)
        {
            GameManager.instance.OnFlagDelivered();
            Instantiate(vfx, transform.position + transform.up, transform.rotation);
        }
    }
}
