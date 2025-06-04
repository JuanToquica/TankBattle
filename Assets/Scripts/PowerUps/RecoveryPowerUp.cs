using UnityEngine;

public class RecoveryPowerUp : PowerUpBase
{
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            PlayerHealth health = other.GetComponent<PlayerHealth>();
            if (health != null) { }
                health.RegainHealth();
            powerUpSpawner.PowerUpCollected(PowerUps.recovery, index);
            Destroy(gameObject);
        }
        if (other.gameObject.CompareTag("Enemy"))
        {
            EnemyHealth health = other.GetComponent<EnemyHealth>();
            if (health != null) { }
                health.RegainHealth();
            powerUpSpawner.PowerUpCollected(PowerUps.recovery, index);
            Destroy(gameObject);
        }
    }
}
