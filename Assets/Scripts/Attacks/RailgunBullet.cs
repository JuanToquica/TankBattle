using UnityEngine;

public class RailgunBullet : MonoBehaviour
{
    [SerializeField] private GameObject impactVfx;
    [SerializeField] private float destroyTime;
    private float speed;
    private float maxRange;
    private float damageAmount;
    private Vector3 currentPosition;
    private Vector3 direction;
    private float travelledDistance;
    private bool isDestroying;
    private string launcherTag;

    public void Initialize(Vector3 startPos, Vector3 dir, float bulletSpeed, float range, float damage, string tag)
    {
        currentPosition = startPos;
        direction = dir;
        speed = bulletSpeed;
        maxRange = range;
        damageAmount = damage;

        launcherTag = tag;
        travelledDistance = 0f;
        isDestroying = false;
        transform.position = currentPosition;
        transform.forward = direction;
    }

    void Update()
    {
        if (isDestroying) return;
        float distanceThisFrame = speed * Time.deltaTime;

        if (Physics.Raycast(currentPosition, direction, out RaycastHit hit, distanceThisFrame) && !hit.transform.CompareTag("Flag"))
        {
            transform.position = hit.point;

            if (hit.transform.CompareTag("Enemy") && launcherTag != "EnemyProjectile")
            {
                EnemyHealth enemy = hit.transform.GetComponent<EnemyHealth>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damageAmount);
                }
                ObjectPoolManager.Instance.GetPooledObject(impactVfx, hit.point + hit.normal * 0.1f, Quaternion.LookRotation(hit.normal));
                isDestroying = true;
                Invoke("Destroy", destroyTime);
            }
            else if (hit.transform.CompareTag("Player") && launcherTag != "PlayerProjectile")
            {
                PlayerHealth player = hit.transform.GetComponent<PlayerHealth>();
                if (player != null)
                {
                    player.TakeDamage(damageAmount);
                }
                ObjectPoolManager.Instance.GetPooledObject(impactVfx, hit.point + hit.normal * 0.1f, Quaternion.LookRotation(hit.normal));
                isDestroying = true;
                Invoke("Destroy", destroyTime);
            }
            else
            {
                ObjectPoolManager.Instance.GetPooledObject(impactVfx, hit.point + hit.normal * 0.1f, Quaternion.LookRotation(hit.normal));
                isDestroying = true;
                Invoke("Destroy", destroyTime);
            }
        }
        else
        {
            currentPosition += direction * distanceThisFrame;
            travelledDistance += distanceThisFrame;

            transform.position = currentPosition;

            if (travelledDistance >= maxRange)
            {
                isDestroying = true;
                Invoke("Destroy", destroyTime);
            }
        }
    }

    private void Destroy()
    {
        ObjectPoolManager.Instance.ReturnPooledObject(gameObject);
    }
}
