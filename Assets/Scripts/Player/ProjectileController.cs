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

    public void Initialize(Vector3 startPos, Vector3 dir, float bulletSpeed, float range, float damage)
    {
        currentPosition = startPos;
        direction = dir;
        speed = bulletSpeed;
        maxRange = range;
        damageAmount = damage;

        travelledDistance = 0f;

        transform.position = currentPosition;
        transform.forward = direction;
    }

    void Update()
    {
        float distanceThisFrame = speed * Time.deltaTime;

        if (Physics.Raycast(currentPosition, direction, out RaycastHit hit, distanceThisFrame))
        {
            transform.position = hit.point;

            if (hit.transform.CompareTag("Enemy"))
            {
                EnemyHealth enemy = hit.transform.GetComponent<EnemyHealth>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damageAmount);
                }
            }
            if (hit.transform.CompareTag("Player"))
            {
                PlayerHealth player = hit.transform.GetComponent<PlayerHealth>();
                if (player != null)
                {
                    player.TakeDamage(damageAmount);
                }
            }
            Instantiate(impactVfx, hit.point -direction * 0.3f, Quaternion.identity);
            Destroy(gameObject);
        }
        else
        {
            currentPosition += direction * distanceThisFrame;
            travelledDistance += distanceThisFrame;

            transform.position = currentPosition;

            if (travelledDistance >= maxRange)
            {
                Destroy(gameObject);
            }
        }
    }

    void OnDrawGizmos()
    {
        if (Application.isPlaying && direction != Vector3.zero)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(currentPosition, direction * speed * Time.deltaTime);
        }
    }
}
