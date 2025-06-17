using UnityEngine;

public class WeaponPowerUp : PowerUpBase
{
    [SerializeField] private GameObject weaponVFX;
    private void OnTriggerStay(Collider other)
    {
        if (!isDissolving)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                PlayerAttack player = other.GetComponent<PlayerAttack>();
                player.OnWeaponPowerUp();
                GameObject vfx = Instantiate(weaponVFX, other.transform.position, other.transform.rotation);
                vfx.transform.SetParent(other.transform);
                powerUpSpawner.PowerUpCollected(PowerUps.weapon, index);
                _startTime = Time.time;
                isFalling = false;
                isDissolving = true;
            }
            else if (other.gameObject.CompareTag("Enemy"))
            {
                GameObject vfx = Instantiate(weaponVFX, other.transform.position, other.transform.rotation);
                vfx.transform.SetParent(other.transform);
                powerUpSpawner.PowerUpCollected(PowerUps.weapon, index);
                _startTime = Time.time;
                isFalling = false;
                isDissolving = true;
            }
        }           
    }
}
