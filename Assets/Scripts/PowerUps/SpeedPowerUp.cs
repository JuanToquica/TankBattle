using UnityEngine;

public class SpeedPowerUp : MonoBehaviour
{
    [SerializeField] private float powerUpDuration;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player") || other.gameObject.CompareTag("Enemy"))
        {
            TankBase tank = other.GetComponent<TankBase>();
            if (tank != null)
                tank.SpeedPowerUp(powerUpDuration);
            Destroy(gameObject);
        }
    }
}
