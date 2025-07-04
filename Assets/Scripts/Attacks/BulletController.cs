using UnityEngine;

public class BulletController : ProjectileBase
{

    private void Update()
    {
        distanceThisFrame = speed * Time.deltaTime;
        RaycastHit hit = LaunchRaycast();
        if (hit.collider != null)
        {
            OnRaycastImpact(hit);
            InstantiateImpactVfx(hit);
        }
        else
        {
            ContinueLaunch();
        }
    }

    protected override void InstantiateImpactVfx(RaycastHit hit)
    {
        ObjectPoolManager.Instance.GetPooledObject(impactVfx, hit.point + hit.normal * 0.3f, Quaternion.LookRotation(hit.normal));
        ObjectPoolManager.Instance.ReturnPooledObject(gameObject);
    }

    protected override void DestroyBullet()
    {
        ObjectPoolManager.Instance.ReturnPooledObject(gameObject);
    }
}
