using UnityEngine;

public class RechargingPowerUp : MonoBehaviour
{
    [SerializeField] private float powerUpDuration;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            PlayerAttack player = other.GetComponent<PlayerAttack>();
            if (player != null)
                player.RecharchingPowerUp(powerUpDuration);
            Destroy(gameObject);
        }
    }
}
