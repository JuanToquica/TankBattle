using UnityEngine;

public class RecoveryPowerUp : MonoBehaviour
{
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            PlayerHealth health = other.GetComponent<PlayerHealth>();
            if (health != null) { }
                health.RegainHealth();
            Destroy(gameObject);
        }
        if (other.gameObject.CompareTag("Enemy"))
        {
            EnemyHealth health = other.GetComponent<EnemyHealth>();
            if (health != null) { }
                health.RegainHealth();
            Destroy(gameObject);
        }
    }
}
