using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.ProBuilder;
using UnityEngine.UI;

public class PlayerAttack : MonoBehaviour
{
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform projectileContainer;
    [SerializeField] private GameObject shotVfx;
    [SerializeField] private float cooldown;
    [SerializeField] private float range;
    [SerializeField] private float aimAngle;
    [SerializeField] private float amountOfRaycast;
    [SerializeField] private float bulletSpeed;
    [SerializeField] private float bulletRange;
    [SerializeField] private float damageAmount;
    [SerializeField] private Image cooldownImage;
    private Animator animator;
    public float cooldownWithPowerUp;
    private float currentCooldown;
    public float cooldownTimer;
    private Outline currentOutlinedEnemy;
    private RaycastHit mainHit;
    private bool _isAimingAtEnemy;

    private void OnEnable()
    {
        currentCooldown = cooldown;
        if (InputManager.Instance != null)
            InputManager.Instance.RegisterPlayerAttack(this);
    }

    private void Start()
    {
        animator = GetComponent<Animator>();
        currentCooldown = cooldown;
        InputManager.Instance.RegisterPlayerAttack(this);
    }

    private void Update()
    {
        if (cooldownTimer < currentCooldown)
        {
            cooldownTimer = Mathf.Clamp(cooldownTimer + Time.deltaTime, 0, currentCooldown);
        }
        Aim();
        cooldownImage.fillAmount = cooldownTimer / currentCooldown;
    }

    public void Aim()
    {
        float halfAngle = aimAngle / 2f;
        float angleStep = aimAngle / amountOfRaycast;

        bool foundEnemyInThisScan = false;
        RaycastHit bestHitInThisScan = new RaycastHit();
        bestHitInThisScan.distance = Mathf.Infinity;

        for (int i = 0; i < amountOfRaycast; i++)
        {
            float currentVerticalAngle = -halfAngle + (i * angleStep);
            Quaternion rotation = Quaternion.AngleAxis(currentVerticalAngle, firePoint.right);
            Vector3 rayDirection = rotation * firePoint.forward;
            bool ray = Physics.Raycast(firePoint.position, rayDirection, out RaycastHit hit, range);
            Debug.DrawRay(firePoint.position, rayDirection * range, Color.red);
            
            if (ray)
            {                                 
                if (hit.transform.CompareTag("Enemy"))
                {
                    if (hit.distance < bestHitInThisScan.distance)
                    {
                        bestHitInThisScan = hit;                      
                    }
                    foundEnemyInThisScan = true;
                }
            }
        }

        if (foundEnemyInThisScan)
        {           
            Outline newEnemyOutline = bestHitInThisScan.transform.GetComponent<Outline>();

            if (newEnemyOutline != null && newEnemyOutline != currentOutlinedEnemy)
            {
                if (currentOutlinedEnemy != null)
                {
                    currentOutlinedEnemy.enabled = false;
                }
                newEnemyOutline.enabled = true;
                currentOutlinedEnemy = newEnemyOutline;
            }
            else if (newEnemyOutline != null && newEnemyOutline == currentOutlinedEnemy)
            {
                currentOutlinedEnemy.enabled = true;
            }
            mainHit = bestHitInThisScan;
            _isAimingAtEnemy = true;
        }
        else
        {
            if (currentOutlinedEnemy != null)
            {
                currentOutlinedEnemy.enabled = false;
                currentOutlinedEnemy = null;
            }
            Physics.Raycast(firePoint.position, firePoint.forward, out RaycastHit hit, 200);
            mainHit = hit;
            _isAimingAtEnemy = false;
        }
        Debug.DrawLine(firePoint.position, mainHit.point, Color.red, 0.2f);
    }

    public void Fire()
    { 
        if (cooldownTimer == currentCooldown)
        {
            animator.SetBool("Fire", true);
            Instantiate(shotVfx, firePoint.position, firePoint.rotation);
            Vector3 startPos = firePoint.position;
            Vector3 fireDirection;

            if (_isAimingAtEnemy)
            {
                fireDirection = (mainHit.point - startPos).normalized;
            }
            else
            {
                fireDirection = firePoint.forward;
            }

            GameObject bulletInstance = Instantiate(projectilePrefab, startPos, Quaternion.LookRotation(fireDirection));
            ProjectileController bulletSim = bulletInstance.GetComponent<ProjectileController>();
            bulletInstance.transform.SetParent(projectileContainer);

            if (bulletSim != null)
            {
                bulletSim.Initialize(startPos, fireDirection, bulletSpeed, bulletRange, damageAmount);
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
