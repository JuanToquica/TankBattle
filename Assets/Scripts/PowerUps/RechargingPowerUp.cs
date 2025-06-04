using UnityEngine;

public class RechargingPowerUp : PowerUpBase
{
    [SerializeField] private float powerUpDuration;
   

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            PlayerAttack player = other.GetComponent<PlayerAttack>();
            if (player != null)
                player.RecharchingPowerUp(powerUpDuration);
            powerUpSpawner.PowerUpCollected(PowerUps.recharging, index);
            Destroy(gameObject);
        }
        if (other.gameObject.CompareTag("Enemy"))
        {
            EnemyAttack enemy = other.GetComponent<EnemyAttack>();
            if (enemy != null)
                enemy.RecharchingPowerUp(powerUpDuration);
            powerUpSpawner.PowerUpCollected(PowerUps.recharging, index);
            Destroy(gameObject);
        }
    }
}
