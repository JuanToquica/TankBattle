using UnityEngine;

public class PowerUpBase : MonoBehaviour
{
    [SerializeField] protected MeshRenderer meshRenderer;
    [SerializeField] protected GameObject puffVFX;
    [SerializeField] protected float duration;
    [SerializeField] protected float vfxOffset;
    public PowerUpSpawner powerUpSpawner;
    public Transform targetPoint;
    public float gravity;
    public int index;
    protected bool isFalling = true;
    protected float verticalSpeed;
    protected bool vfxInstantiated;
    protected bool isDissolving;
    protected float _startTime;
    protected float currentValue;

    void Update()
    {
        if (isDissolving)
        {
            float elapsedTime = Time.time - _startTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            currentValue = Mathf.Lerp(-1f, 1f, t);

            meshRenderer.material.SetFloat("_DissolveFactor", currentValue);
            if (currentValue >= 1)
                Destroy(gameObject);
        }

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
