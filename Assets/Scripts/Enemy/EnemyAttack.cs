using Unity.VisualScripting;
using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    private Animator animator;
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float coolDown;
    [HideInInspector] public Transform projectilesContainer;    
    private float nextShootTimer = 0;

    private void Start()
    {
        animator = GetComponent<Animator>();
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
        GameObject projectile = Instantiate(projectilePrefab, projectileSpawnPoint.position, Quaternion.Euler(0, projectileSpawnPoint.rotation.eulerAngles.y, 0));
        projectile.transform.SetParent(projectilesContainer);
        projectile.tag = "EnemyProjectile";

        nextShootTimer = 0;
    }

    public void EndShootAnimation() => animator.SetBool("Fire", false);
}
