using UnityEngine;
using UnityEngine.SearchService;

public class ProjectileController : MonoBehaviour
{
    private Rigidbody rb;
    [SerializeField] private float speed;
    [SerializeField] private float timeToDestroy;


    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.linearVelocity = transform.forward * speed;
        Destroy(gameObject, timeToDestroy);
    }


    private void OnTriggerEnter(Collider other)
    {      
        if (this.CompareTag("PlayerProjectile"))
        {
            if (other.CompareTag("Enemy"))
            {
                EnemyHealth enemy = other.GetComponent<EnemyHealth>();
                if (enemy != null)
                    enemy.TakeDamage(10);
                Destroy(gameObject);
            }
            if (!other.CompareTag("Player"))
                Destroy(gameObject);
        }
        else if (this.CompareTag("EnemyProjectile"))
        {
            if (other.CompareTag("Player"))
            {
                PlayerHealth player = other.GetComponent<PlayerHealth>();
                if (player != null)
                    player.TakeDamage(3);
                Destroy(gameObject);
            }
            if (!other.CompareTag("Enemy"))
                Destroy(gameObject);
        }

    }
}
