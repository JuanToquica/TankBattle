using UnityEngine;
using UnityEngine.Animations;

public class RocketController : MonoBehaviour
{
    [SerializeField] private GameObject impactVfx;
    [SerializeField] private GameObject smokeVFX;
    [SerializeField] private float explosionRadius;
    private float speed;
    private float maxRange;
    private int damageAmount;
    private Vector3 currentPosition;
    private Vector3 direction;
    private float travelledDistance;
    private SmokeTrail smokeTrail;
    private GameObject launcher;
    private string launcherTag;


    public void Initialize(Vector3 startPos, Vector3 dir, float bulletSpeed, float range, int damage, GameObject launcher, string tag)
    {
        currentPosition = startPos;
        direction = dir;
        speed = bulletSpeed;
        maxRange = range;
        damageAmount = damage;

        travelledDistance = 0f;

        transform.position = currentPosition;
        transform.forward = direction;
        this.launcher = launcher;
        this.launcherTag = tag;
        smokeTrail = ObjectPoolManager.Instance.GetPooledObject(smokeVFX, transform.position, transform.rotation).GetComponent<SmokeTrail>();
        ParentConstraint smokeTrailConstraint = smokeTrail.GetComponent<ParentConstraint>();
        ConstraintSource newSource = new ConstraintSource
        {
            sourceTransform = transform,
            weight = 1.0f
        };
        int sourceIndex = smokeTrailConstraint.AddSource(newSource);
        Vector3 offset = new Vector3(0, 0, -0.4f);
        smokeTrailConstraint.SetTranslationOffset(sourceIndex, offset);
        smokeTrailConstraint.constraintActive = true;
    }

    void Update()
    {
        float distanceThisFrame = speed * Time.deltaTime;

        if (Physics.Raycast(currentPosition, direction, out RaycastHit hit, distanceThisFrame) && hit.collider.gameObject != launcher && !hit.transform.CompareTag("Flag"))
        {
            transform.position = hit.point;        

            if (hit.transform.CompareTag("Enemy") && launcherTag != "EnemyProjectile")
            {
                EnemyHealth enemyHealth = hit.transform.GetComponent<EnemyHealth>();
                EnemyAI enemyAI = hit.transform.GetComponent<EnemyAI>();
                if (enemyHealth != null)
                    enemyHealth.TakeDamage(damageAmount);
                if (enemyAI != null)
                    enemyAI.knowsPlayerPosition = true;
            }
            else if (hit.transform.CompareTag("Player") && launcherTag != "PlayerProjectile")
            {
                PlayerHealth player = hit.transform.GetComponent<PlayerHealth>();
                if (player != null)
                {
                    player.TakeDamage(damageAmount);
                }
            }
            else
            {
                Debug.Log("generando esphera");
                Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius);
                foreach (var hitCollider in hitColliders)
                {
                    if (hitCollider.transform.CompareTag("Enemy"))
                    {
                        Debug.Log("detectando enemigo con esphera");
                        EnemyHealth enemy = hitCollider.transform.GetComponent<EnemyHealth>();
                        if (enemy != null)
                        {
                            float distanceFromCenter = Vector3.Distance(transform.position, hitCollider.bounds.center);
                            float t = Mathf.Clamp01(distanceFromCenter/ explosionRadius);
                            float finalDamage = Mathf.Lerp(damageAmount * 2, 0, t);
                            Debug.Log(Vector3.Distance(transform.position, hitCollider.bounds.center));
                            Debug.Log("daño final" + finalDamage);
                            enemy.TakeDamage((int)finalDamage);
                        }
                    }
                    else if (hitCollider.transform.CompareTag("Player"))
                    {
                        PlayerHealth player = hitCollider.transform.GetComponent<PlayerHealth>();
                        if (player != null)
                        {
                            float distanceFromCenter = Vector3.Distance(transform.position, hitCollider.bounds.center);
                            float t = Mathf.Clamp01(distanceFromCenter / explosionRadius);
                            float finalDamage = Mathf.Lerp(damageAmount * 2, 0, t);
                            Debug.Log(Vector3.Distance(transform.position, hitCollider.bounds.center));
                            player.TakeDamage((int)finalDamage);
                        }
                    }                     
                }
            }
            ObjectPoolManager.Instance.GetPooledObject(impactVfx, hit.point + hit.normal * 0.7f, Quaternion.LookRotation(hit.normal));
            smokeTrail.OnRocketCollision();
            ObjectPoolManager.Instance.ReturnPooledObject(gameObject);
        }
        else
        {
            currentPosition += direction * distanceThisFrame;
            travelledDistance += distanceThisFrame;

            transform.position = currentPosition;

            if (travelledDistance >= maxRange)
            {
                smokeTrail.OnRocketCollision();
                ObjectPoolManager.Instance.ReturnPooledObject(gameObject);
            }
        }
    }
}
