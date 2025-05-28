using UnityEngine;

public class SpeedPowerUp : MonoBehaviour
{
    [SerializeField] private float powerUpDuration;
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
