using UnityEngine;

public class PowerUpBase : MonoBehaviour
{
    [SerializeField] private GameObject puffVFX;
    public PowerUpSpawner powerUpSpawner;
    public Transform targetPoint;
    public int index;
    public float gravity;
    public float vfxOffset;
    private bool isFalling = true;
    private float verticalSpeed;
    private bool vfxInstantiated;

    void Update()
    {
        if (!isFalling) return;

        verticalSpeed += gravity * Time.deltaTime;
        transform.position -= new Vector3(0, verticalSpeed * Time.deltaTime, 0);

        if (transform.position.y < targetPoint.position.y + vfxOffset && !vfxInstantiated)
        {
            Instantiate(puffVFX, targetPoint.position - new Vector3(0, 0.6f, 0), targetPoint.rotation);
            vfxInstantiated = true;
        }

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
