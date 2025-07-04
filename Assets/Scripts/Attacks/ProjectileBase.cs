using UnityEngine;

public abstract class ProjectileBase : MonoBehaviour
{
    [SerializeField] protected GameObject impactVfx;
    protected float speed;
    protected float maxRange;
    protected int damageAmount;
    protected Vector3 currentPosition;
    protected Vector3 direction;
    protected float travelledDistance;
    protected string launcherTag;
    protected float distanceThisFrame;

    public virtual void Initialize(Vector3 startPos, Vector3 dir, float bulletSpeed, float range, int damage, string tag)
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

    protected RaycastHit LaunchRaycast()
    {
        bool raycast = Physics.Raycast(currentPosition, direction, out RaycastHit hit, distanceThisFrame) && !hit.transform.CompareTag("Flag");
        return hit;
    }

    protected bool OnRaycastImpact(RaycastHit hit)
    {
        transform.position = hit.point;

        if (hit.transform.CompareTag("Enemy") && launcherTag != "EnemyProjectile")
        {
            OnEnemyImpact(hit);
            return true;
        }
        else if (hit.transform.CompareTag("Player") && launcherTag != "PlayerProjectile")
        {
            OnPlayerImpact(hit);
            return true;
        }
        return false;
    }

    protected void OnEnemyImpact(RaycastHit hit)
    {
        EnemyHealth enemyHealth = hit.transform.GetComponent<EnemyHealth>();
        EnemyAI enemyAI = hit.transform.GetComponent<EnemyAI>();
        if (enemyHealth != null)
            enemyHealth.TakeDamage(damageAmount);
        if (enemyAI != null)
            enemyAI.knowsPlayerPosition = true;
    }

    protected void OnPlayerImpact(RaycastHit hit)
    {
        PlayerHealth player = hit.transform.GetComponent<PlayerHealth>();
        if (player != null)
        {
            player.TakeDamage(damageAmount);
        }
    }

    protected virtual void ContinueLaunch()
    {
        currentPosition += direction * distanceThisFrame;
        travelledDistance += distanceThisFrame;
        transform.position = currentPosition;

        if (travelledDistance >= maxRange)
        {
            DestroyBullet();
        }
    }

    protected abstract void InstantiateImpactVfx(RaycastHit hit);
    protected abstract void DestroyBullet();

}
