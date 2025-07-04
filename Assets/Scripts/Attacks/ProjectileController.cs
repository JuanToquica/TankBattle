using UnityEngine;
using UnityEngine.SearchService;

public class ProjectileController : ProjectileBase
{
    [SerializeField] private GameObject bounceVfx;
    [SerializeField] private float BounceAngle;    
    private int Bounces;

    public override void Initialize(Vector3 startPos, Vector3 dir, float bulletSpeed, float range, int damage, string tag)
    {
        base.Initialize(startPos, dir, bulletSpeed, range, damage, tag);
        Bounces = 0;
    }

    private void Update()
    {
        distanceThisFrame = speed * Time.deltaTime;
        RaycastHit hit = LaunchRaycast();
        if (hit.collider != null)
        {
            if (!OnRaycastImpact(hit))
            {
                if (hit.transform.CompareTag("Environment") || hit.transform.CompareTag("Floor") && Bounces < 1)
                {
                    if (Vector3.Angle(transform.forward, -hit.normal) > BounceAngle)
                    {
                        ObjectPoolManager.Instance.GetPooledObject(bounceVfx, hit.point + hit.normal * 0.3f, Quaternion.LookRotation(hit.normal));
                        direction = Vector3.Reflect(direction, hit.normal);
                        transform.forward = direction;
                        Bounces++;
                        return;
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

    protected override void InstantiateImpactVfx(RaycastHit hit)
    {
        ObjectPoolManager.Instance.GetPooledObject(impactVfx, hit.point + hit.normal * 0.4f, Quaternion.LookRotation(hit.normal));
        ObjectPoolManager.Instance.ReturnPooledObject(gameObject);
    }

    protected override void DestroyBullet()
    {
        ObjectPoolManager.Instance.ReturnPooledObject(gameObject);
    }
}
