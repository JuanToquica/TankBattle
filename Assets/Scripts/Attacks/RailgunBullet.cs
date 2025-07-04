using UnityEngine;

public class RailgunBullet : ProjectileBase
{   
    [SerializeField] private float destroyTime;    
    private bool isDestroying;

    public override void Initialize(Vector3 startPos, Vector3 dir, float bulletSpeed, float range, int damage, string tag)
    {
        base.Initialize(startPos, dir, bulletSpeed, range, damage, tag);
        isDestroying = false;
    }

    private void Update()
    {
        if (isDestroying) return;
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
        ObjectPoolManager.Instance.GetPooledObject(impactVfx, hit.point + hit.normal * 0.1f, Quaternion.LookRotation(hit.normal));
        isDestroying = true;
        Invoke("Destroy", destroyTime);
    }

    protected override void DestroyBullet()
    {
        isDestroying = true;
        Invoke("Destroy", destroyTime);
    }

    private void Destroy()
    {
        ObjectPoolManager.Instance.ReturnPooledObject(gameObject);
    }
}
