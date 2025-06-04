using UnityEngine;

public class WeaponPowerUp : PowerUpBase
{   
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            powerUpSpawner.PowerUpCollected(PowerUps.weapon, index);
            Destroy(gameObject);
        }
        if (other.gameObject.CompareTag("Enemy"))
        {
            powerUpSpawner.PowerUpCollected(PowerUps.weapon, index);
            Destroy(gameObject);
        }
    }
}
