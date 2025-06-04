using UnityEngine;

public class SpeedPowerUp : PowerUpBase
{
    [SerializeField] private float powerUpDuration;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player") || other.gameObject.CompareTag("Enemy"))
        {
            TankBase tank = other.GetComponent<TankBase>();
            if (tank != null)
                tank.SpeedPowerUp(powerUpDuration);
            powerUpSpawner.PowerUpCollected(PowerUps.speed, index);
            Destroy(gameObject);
        }
    }
}
