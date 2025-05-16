using System.Collections;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    private Animator animator;       
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform projectilesContainer;
    [SerializeField] private float cooldown;
    public float cooldownWithPowerUp;
    private float currentCooldown;
    private float cooldownTimer;
    private Coroutine restoreCoroutine;

    private void Start()
    {
        animator = GetComponent<Animator>();
        currentCooldown = cooldown;
        cooldownTimer = currentCooldown;
    }

    private void Update()
    {
        if (cooldownTimer < currentCooldown)
        {
            cooldownTimer = Mathf.Clamp(cooldownTimer + Time.deltaTime, 0, currentCooldown);
        }
    }
    public void Fire()
    {
        if (cooldownTimer == currentCooldown)
        {
            animator.SetBool("Fire", true);
            GameObject projectile = Instantiate(projectilePrefab, spawnPoint.position, Quaternion.Euler(0, spawnPoint.rotation.eulerAngles.y, 0));
            projectile.transform.SetParent(projectilesContainer);
            projectile.tag = "PlayerProjectile";
            cooldownTimer = 0;
        }        
    }

    public void RecharchingPowerUp(float duration)
    {
        if (restoreCoroutine != null)
            StopCoroutine(restoreCoroutine);
        currentCooldown = cooldownWithPowerUp;
        if (cooldownTimer >= currentCooldown)
            cooldownTimer = currentCooldown;
        restoreCoroutine = StartCoroutine(RestoreCooldown(duration));
    }

    private IEnumerator RestoreCooldown(float duration)
    {
        yield return new WaitForSeconds(duration);
        currentCooldown = cooldown;
    }

    public void EndAnimation()
    {
        animator.SetBool("Fire", false);
    }
}
