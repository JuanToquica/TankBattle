using UnityEngine;

public class RecoveryPowerUp : PowerUpBase
{
    [SerializeField] private GameObject healingVFX;

    private void OnTriggerStay(Collider other)
    {
        if (!isDissolving)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                PlayerHealth health = other.GetComponent<PlayerHealth>();
                if (health != null) { }
                health.RegainHealth();
                GameObject vfx = ObjectPoolManager.Instance.GetPooledObject(healingVFX, other.transform.position, other.transform.rotation);
                vfx.transform.SetParent(other.transform);
                powerUpSpawner.PowerUpCollected(PowerUps.recovery, index);
                _startTime = Time.time;
                isFalling = false;
                isDissolving = true;
            }
            if (other.gameObject.CompareTag("Enemy"))
            {
                EnemyHealth health = other.GetComponent<EnemyHealth>();
                if (health != null) { }
                health.RegainHealth();
                GameObject vfx = ObjectPoolManager.Instance.GetPooledObject(healingVFX, other.transform.position, other.transform.rotation);
                vfx.transform.SetParent(other.transform);
                powerUpSpawner.PowerUpCollected(PowerUps.recovery, index);
                _startTime = Time.time;
                isFalling = false;
                isDissolving = true;
            }
        }
        
    }
}
