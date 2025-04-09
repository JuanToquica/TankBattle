using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    private Animator animator;
    [SerializeField] private float cooldown;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform projectilesContainer;
    private float cooldownTimer;

    private void Start()
    {
        animator = GetComponent<Animator>();
        cooldownTimer = cooldown;
    }

    private void Update()
    {
        if (cooldownTimer < cooldown)
        {
            cooldownTimer = Mathf.Clamp(cooldownTimer + Time.deltaTime, 0, cooldown);
        }
    }
    public void Fire()
    {
        if (cooldownTimer == cooldown)
        {
            animator.SetBool("Fire", true);
            GameObject projectile = Instantiate(projectilePrefab, spawnPoint.position, Quaternion.Euler(0, spawnPoint.rotation.eulerAngles.y, 0));
            projectile.transform.SetParent(projectilesContainer);
            projectile.tag = "PlayerProjectile";
            cooldownTimer = 0;
        }        
    }

    public void EndAnimation()
    {
        animator.SetBool("Fire", false);
    }
}
