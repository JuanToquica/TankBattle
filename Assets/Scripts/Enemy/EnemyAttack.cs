using System.IO.Pipes;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    private Animator animator;
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float coolDown;
    [HideInInspector] public Transform projectileContainer;
    [SerializeField] private GameObject shotVfx;
    [SerializeField] private float bulletSpeed;
    [SerializeField] private float bulletRange;
    [SerializeField] private float damageAmount;
    public float aimAngle;
    public float amountOfRaycast;
    public float range;
    private EnemyAI enemy;
    private float nextShootTimer = 0;

    private void Start()
    {
        animator = GetComponent<Animator>();
        enemy = GetComponent<EnemyAI>();
    }

    private void Update()
    {
        nextShootTimer = Mathf.Clamp(nextShootTimer + Time.deltaTime, 0, coolDown);
    }
    public bool CanShoot()
    {
        return nextShootTimer == coolDown;
    }

    public void Shoot()
    {
        if (!CanShoot()) return;

        animator.SetBool("Fire", true);
        Instantiate(shotVfx, firePoint.position, firePoint.rotation);

        float halfAngle = aimAngle / 2f;
        float angleStep = aimAngle / amountOfRaycast;

        bool foundPlayer = false;
        RaycastHit bestHitInThisScan = new RaycastHit();
        bestHitInThisScan.distance = Mathf.Infinity;
        Vector3 fireDirection;
        for (int i = 0; i < amountOfRaycast; i++)
        {
            float currentVerticalAngle = -halfAngle + (i * angleStep);
            Quaternion rotation = Quaternion.AngleAxis(currentVerticalAngle, firePoint.right);
            Vector3 rayDirection = rotation * firePoint.forward;
            bool ray = Physics.Raycast(firePoint.position, rayDirection, out RaycastHit hit, range);
            Debug.DrawRay(firePoint.position, rayDirection * range, Color.red);

            if (ray)
            {
                if (hit.transform.CompareTag("Player"))
                {
                    if (hit.distance < bestHitInThisScan.distance)
                    {
                        bestHitInThisScan = hit;
                    }
                    foundPlayer = true;
                }
            }
        }
        if (foundPlayer)
        {
            fireDirection = (bestHitInThisScan.point - firePoint.position).normalized;
        }
        else
        {
            fireDirection = firePoint.forward;
        }


        GameObject bulletInstance = Instantiate(projectilePrefab, firePoint.position, Quaternion.LookRotation(fireDirection.normalized));
        ProjectileController bulletSim = bulletInstance.GetComponent<ProjectileController>();
        bulletInstance.transform.SetParent(projectileContainer);

        if (bulletSim != null)
        {
            bulletSim.Initialize(firePoint.position, fireDirection.normalized, bulletSpeed, bulletRange, damageAmount);
        }

        nextShootTimer = 0;
    }

    public void EndShootAnimation() => animator.SetBool("Fire", false);
}
