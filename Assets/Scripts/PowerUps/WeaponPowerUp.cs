using UnityEngine;

public class WeaponPowerUp : PowerUpBase
{
    [SerializeField] private GameObject weaponVFX;
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            GameObject vfx = Instantiate(weaponVFX, other.transform.position, other.transform.rotation);
            vfx.transform.SetParent(other.transform);
            powerUpSpawner.PowerUpCollected(PowerUps.weapon, index);
            Destroy(gameObject);
        }
        if (other.gameObject.CompareTag("Enemy"))
        {
            GameObject vfx = Instantiate(weaponVFX, other.transform.position, other.transform.rotation);
            vfx.transform.SetParent(other.transform);
            powerUpSpawner.PowerUpCollected(PowerUps.weapon, index);
            Destroy(gameObject);
        }
    }
}
