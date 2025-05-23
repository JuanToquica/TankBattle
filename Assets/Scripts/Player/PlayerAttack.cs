using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerAttack : MonoBehaviour
{
    [SerializeField] private Transform firePoint;
    [SerializeField] private float cooldown;
    [SerializeField] private float range;
    [SerializeField] private float aimAngle;
    [SerializeField] private float amountOfRaycast;
    private Animator animator;
    public float cooldownWithPowerUp;
    private float currentCooldown;
    private float cooldownTimer;
    private RaycastHit mainHit;

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
        Aim();
    }

    public void Aim()
    {
        float halfAngle = aimAngle / 2f;
        float angleStep = aimAngle / amountOfRaycast;
        int hitEnemyCounter = 0;
        for (int i = 0; i < amountOfRaycast; i++)
        {
            float currentVerticalAngle = -halfAngle + (i * angleStep);
            Quaternion rotation = Quaternion.AngleAxis(currentVerticalAngle, firePoint.right);
            Vector3 rayDirection = rotation * firePoint.forward;

            if (Physics.Raycast(firePoint.position, rayDirection, out RaycastHit hit, range))
            {                   
                Debug.DrawLine(firePoint.position, hit.point, Color.red, 0.2f);
                if (hit.collider.CompareTag("Enemy"))
                {
                    mainHit = hit;
                }
                else
                {
                    hitEnemyCounter++;
                }
            }
        }
        if (hitEnemyCounter == amountOfRaycast)
        {
            Physics.Raycast(firePoint.position, firePoint.forward, out RaycastHit hit, range);
            mainHit = hit;
        }
    }

    public void Fire()
    {
        if (cooldownTimer == currentCooldown)
        {
            animator.SetBool("Fire", true);
            if (mainHit.collider.CompareTag("Enemy"))
            {
                EnemyHealth enemy = mainHit.collider.GetComponent<EnemyHealth>();
                enemy.TakeDamage(3);
            }
            cooldownTimer = 0;
        }        
    }

    public void RecharchingPowerUp(float duration)
    {
        currentCooldown = cooldownWithPowerUp;
        if (cooldownTimer >= currentCooldown)
            cooldownTimer = currentCooldown;
        Invoke("RestoreCooldown", duration);
    }

    private void RestoreCooldown()
    {
        currentCooldown = cooldown;
    }

    public void EndAnimation()
    {
        animator.SetBool("Fire", false);
    }
}
