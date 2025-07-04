using UnityEngine;
using UnityEngine.Animations;

public class RocketController : ProjectileBase
{
    [SerializeField] private GameObject smokeVFX;
    [SerializeField] private float explosionRadius;    
    private SmokeTrail smokeTrail;

    public override void Initialize(Vector3 startPos, Vector3 dir, float bulletSpeed, float range, int damage, string tag)
    {
        base.Initialize(startPos, dir, bulletSpeed, range, damage, tag);

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


    private void Update()
    {
        distanceThisFrame = speed * Time.deltaTime;
        RaycastHit hit = LaunchRaycast();
        if (hit.collider != null && !hit.transform.CompareTag(launcherTag))
        {
            if (!OnRaycastImpact(hit))
            {
                Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius);
                foreach (var hitCollider in hitColliders)
                {
                    if (hitCollider.transform.CompareTag("Enemy"))
                    {
                        EnemyHealth enemy = hitCollider.transform.GetComponent<EnemyHealth>();
                        if (enemy != null)
                        {
                            enemy.TakeDamage((int)CalculateDamage(hitCollider));
                        }
                    }
                    else if (hitCollider.transform.CompareTag("Player"))
                    {
                        PlayerHealth player = hitCollider.transform.GetComponent<PlayerHealth>();
                        if (player != null)
                        {
                            player.TakeDamage((int)CalculateDamage(hitCollider));
                        }
                    }
                }
            }
            InstantiateImpactVfx(hit);           
        }
        else
        {
            ContinueLaunch();
        }
    }

    private float CalculateDamage(Collider hitCollider)
    {
        float distanceFromCenter = Vector3.Distance(transform.position, hitCollider.bounds.center);
        float t = Mathf.Clamp01(distanceFromCenter / explosionRadius);
        float finalDamage = Mathf.Lerp(damageAmount * 2, 0, t);
        return finalDamage;
    }

    protected override void InstantiateImpactVfx(RaycastHit hit)
    {
        ObjectPoolManager.Instance.GetPooledObject(impactVfx, hit.point + hit.normal * 0.7f, Quaternion.LookRotation(hit.normal));
        smokeTrail.OnRocketCollision();
        ObjectPoolManager.Instance.ReturnPooledObject(gameObject);
    }

    protected override void DestroyBullet()
    {
        smokeTrail.OnRocketCollision();
        ObjectPoolManager.Instance.ReturnPooledObject(gameObject);
    }
}
