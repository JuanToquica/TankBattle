using UnityEngine;

public class RecoveryPowerUp : MonoBehaviour
{
    public PowerUpSpawner powerUpSpawner;
    public Transform targetPoint;
    public int index;
    private bool isFalling = true;
    private float verticalSpeed;

    void Update()
    {
        if (!isFalling) return;

        verticalSpeed += 10 * Time.deltaTime;
        transform.position -= new Vector3(0, verticalSpeed * Time.deltaTime, 0);

        if (transform.position.y <= targetPoint.position.y)
        {
            transform.position = new Vector3(
                transform.position.x,
                targetPoint.position.y,
                transform.position.z
            );
            isFalling = false;
            verticalSpeed = 0f;
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            PlayerHealth health = other.GetComponent<PlayerHealth>();
            if (health != null) { }
                health.RegainHealth();
            powerUpSpawner.PowerUpCollected(PowerUps.recovery, index);
            Destroy(gameObject);
        }
        if (other.gameObject.CompareTag("Enemy"))
        {
            EnemyHealth health = other.GetComponent<EnemyHealth>();
            if (health != null) { }
                health.RegainHealth();
            powerUpSpawner.PowerUpCollected(PowerUps.recovery, index);
            Destroy(gameObject);
        }
    }
}
