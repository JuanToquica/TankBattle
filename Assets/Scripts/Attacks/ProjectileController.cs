using UnityEngine;
using UnityEngine.SearchService;

public class ProjectileController : MonoBehaviour
{
    [SerializeField] private GameObject impactVfx;
    private float speed;
    private float maxRange;
    private float damageAmount = 3;
    private Vector3 currentPosition;
    private Vector3 direction;
    private float travelledDistance = 0f;
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

        transform.position = currentPosition;
        transform.forward = direction;
    }

    void Update()
    {
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
            }
            if (hit.transform.CompareTag("Player") && launcherTag != "PlayerProjectile")
            {
                PlayerHealth player = hit.transform.GetComponent<PlayerHealth>();
                if (player != null)
                {
                    player.TakeDamage(damageAmount);
                }
            }
            ObjectPoolManager.Instance.GetPooledObject(impactVfx, hit.point + hit.normal * 0.1f, Quaternion.LookRotation(hit.normal));
            ObjectPoolManager.Instance.ReturnPooledObject(gameObject);
        }
        else
        {
            currentPosition += direction * distanceThisFrame;
            travelledDistance += distanceThisFrame;

            transform.position = currentPosition;

            if (travelledDistance >= maxRange)
            {
                ObjectPoolManager.Instance.ReturnPooledObject(gameObject);
            }
        }
    }
}
