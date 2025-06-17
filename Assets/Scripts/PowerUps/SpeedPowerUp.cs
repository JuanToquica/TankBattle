using UnityEngine;

public class SpeedPowerUp : PowerUpBase
{
    [SerializeField] private float powerUpDuration;
    [SerializeField] private GameObject speedVFX;
    
    private void OnTriggerEnter(Collider other)
    {
        if (!isDissolving)
        {
            if (other.gameObject.CompareTag("Player") || other.gameObject.CompareTag("Enemy"))
            {
                TankBase tank = other.GetComponent<TankBase>();
                if (tank != null)
                    tank.SpeedPowerUp(powerUpDuration);
                GameObject vfx = Instantiate(speedVFX, other.transform.position, other.transform.rotation);
                vfx.transform.SetParent(other.transform);
                powerUpSpawner.PowerUpCollected(PowerUps.speed, index);
                _startTime = Time.time;
                isFalling = false;
                isDissolving = true;
            }
        }           
    }
}
