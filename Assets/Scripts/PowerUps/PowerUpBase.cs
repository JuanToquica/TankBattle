using UnityEngine;

public class PowerUpBase : MonoBehaviour
{
    public PowerUpSpawner powerUpSpawner;
    public Transform targetPoint;
    public int index;
    public float gravity;
    private bool isFalling = true;
    private float verticalSpeed;

    void Update()
    {
        if (!isFalling) return;

        verticalSpeed += gravity * Time.deltaTime;
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
}
