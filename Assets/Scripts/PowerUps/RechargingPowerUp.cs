using UnityEngine;

public class RechargingPowerUp : PowerUpBase
{
    [SerializeField] private float powerUpDuration;
    [SerializeField] private GameObject rechargingVFX;
   

    private void OnTriggerEnter(Collider other)
    {
        if (!isDissolving)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                PlayerAttack player = other.GetComponent<PlayerAttack>();
                if (player != null)
                    player.RecharchingPowerUp(powerUpDuration);
                GameObject vfx = ObjectPoolManager.Instance.GetPooledObject(rechargingVFX, other.transform.position, other.transform.rotation);
                vfx.transform.SetParent(other.transform);
                powerUpSpawner.PowerUpCollected(PowerUps.recharging, index);
                _startTime = Time.time;
                isFalling = false;
                isDissolving = true;
            }
            if (other.gameObject.CompareTag("Enemy"))
            {
                EnemyAttack enemy = other.GetComponent<EnemyAttack>();
                if (enemy != null)
                    enemy.RecharchingPowerUp(powerUpDuration);
                GameObject vfx = ObjectPoolManager.Instance.GetPooledObject(rechargingVFX, other.transform.position, other.transform.rotation);
                vfx.transform.SetParent(other.transform);
                powerUpSpawner.PowerUpCollected(PowerUps.recharging, index);
                _startTime = Time.time;
                isFalling = false;
                isDissolving = true;
            }
        }         
    }
}
