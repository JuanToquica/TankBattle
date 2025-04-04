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
        if (other.CompareTag("Enemy"))
        {
            EnemyHealth enemy = other.GetComponent<EnemyHealth>();
            if (enemy != null)
                enemy.SubstractHealth(3);
            Destroy(gameObject);
        }
        if (!other.CompareTag("Player"))
            Destroy(gameObject);
    }
}
