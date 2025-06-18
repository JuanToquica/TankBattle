using System.Collections;
using System.IO.Pipes;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.ProBuilder;
using UnityEngine.UI;

public enum Weapons
{
    mainTurret,
    railGun,
    machineGun,
    rocket
}

public class PlayerAttack : MonoBehaviour
{
    [Header ("References")]
    [SerializeField] private Transform mainGunFirePoint;
    [SerializeField] private GameObject[] fakeRockets;
    [SerializeField] private Transform projectileContainer;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private GameObject rocketPrefab;
    [SerializeField] private GameObject shotVfx;
    [SerializeField] private Image cooldownImage;
    [SerializeField] private Mesh[] turretMeshes;
    [SerializeField] private MeshFilter turretMesh;
    private Outline currentOutlinedEnemy;
    private Animator mainTurretAnimator;
    private Animator railgunAnimator;
    private Animator machineGunAnimator;
    private Animator rocketAnimator;
    public Weapons currentWeapon;

    [Header ("Fire Specifications")]
    [SerializeField] private Vector2[] turretCooldowns;
    [SerializeField] private float[] rangeOfTurrets;
    [SerializeField] private float aimAngle;
    [SerializeField] private float amountOfRaycast;
    [SerializeField] private float bulletSpeed;
    [SerializeField] private float rocketSpeed;
    [SerializeField] private float bulletRange;
    [SerializeField] private int mainTurretDamage;
    [SerializeField] private int railgunDamage;
    [SerializeField] private int machineGunDamage;
    [SerializeField] private int rocketDamage;
    [SerializeField] private float railgunDelay;
    [SerializeField] private float railgunAmmo;
    [SerializeField] private float machineGunAmmo;
    [SerializeField] private float rocketAmmo;
    public float currentCooldown;
    public float currentRange;   
    public float cooldownTimer;   
    private RaycastHit mainHit;
    private Ray mainRay;
    private bool _isAimingAtEnemy;
    public bool rechargingPowerUpActive;
    public int shotsFired;
    

    private void OnEnable()
    {
        if (InputManager.Instance != null)
            InputManager.Instance.RegisterPlayerAttack(this);
        LoadTurretDamage();
        BackToMainTurret();
    }

    private void Start()
    {
        mainTurretAnimator = GetComponent<Animator>();
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

    private void LoadTurretDamage()
    {
        mainTurretDamage = DataManager.Instance.GetMainTurretDamage();
        railgunDamage = DataManager.Instance.GetRailgunDamage();
        machineGunDamage = DataManager.Instance.GetMachineGunDamage();
        rocketDamage = DataManager.Instance.GetRocketDamage();
    }


    private void Aim()
    {
        float halfAngle = aimAngle / 2f;
        float angleStep = aimAngle / amountOfRaycast;

        bool foundEnemyInThisScan = false;
        RaycastHit bestHitInThisScan = new RaycastHit();
        bestHitInThisScan.distance = Mathf.Infinity;

        for (int i = 0; i < amountOfRaycast; i++)
        {
            float currentVerticalAngle = -halfAngle + (i * angleStep);
            Quaternion rotation = Quaternion.AngleAxis(currentVerticalAngle, mainGunFirePoint.right);
            Vector3 rayDirection = rotation * mainGunFirePoint.forward;
            bool ray = Physics.Raycast(mainGunFirePoint.position, rayDirection, out RaycastHit hit, currentRange);
            Debug.DrawRay(mainGunFirePoint.position, rayDirection * currentRange, Color.red);
            
            if (ray)
            {                                 
                if (hit.transform.CompareTag("Enemy"))
                {
                    if (hit.distance < bestHitInThisScan.distance)
                    {
                        bestHitInThisScan = hit;
                        mainRay = new Ray(mainGunFirePoint.position, rayDirection);
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
            mainRay = new Ray(mainGunFirePoint.position, mainGunFirePoint.forward);
            Physics.Raycast(mainRay, out RaycastHit hit, 200);
            mainHit = hit;
            _isAimingAtEnemy = false;
        }
        Debug.DrawLine(mainGunFirePoint.position, mainHit.point, Color.red, 0.2f);
    }

    public void Fire()
    { 
        if (cooldownTimer == currentCooldown)
        {
            switch (currentWeapon)
            {
                case Weapons.mainTurret:
                    FireWithMainTurret();
                    break;
                case Weapons.railGun:
                    StartCoroutine(FireWithRailgun());
                    break;
                case Weapons.machineGun:
                    FireWithMachineGun();
                    break;
                case Weapons.rocket:
                    FireWithRocket();
                    break;
            }
        }        
    }

    private void FireWithMainTurret()
    {
        mainTurretAnimator.SetBool("Fire", true);
        Instantiate(shotVfx, mainGunFirePoint.position, mainGunFirePoint.rotation);
        Vector3 startPos = mainGunFirePoint.position;
        Vector3 fireDirection;

        if (_isAimingAtEnemy)
            fireDirection = (mainHit.point - startPos).normalized;
        else
            fireDirection = mainGunFirePoint.forward;

        GameObject bulletInstance = Instantiate(projectilePrefab, startPos, Quaternion.LookRotation(fireDirection));
        ProjectileController bulletSim = bulletInstance.GetComponent<ProjectileController>();
        bulletInstance.transform.SetParent(projectileContainer);

        if (bulletSim != null)
            bulletSim.Initialize(startPos, fireDirection, bulletSpeed, bulletRange, mainTurretDamage);

        cooldownTimer = 0;
    }

    private IEnumerator FireWithRailgun()
    {
        cooldownTimer = 0;
        yield return new WaitForSeconds(railgunDelay);

        RaycastHit[] hits = Physics.RaycastAll(mainRay, currentRange);
        if (hits.Length > 0)
        {
            int impactedEnemies = 0;
            foreach (RaycastHit hit in hits)
            {
                if (hit.transform.CompareTag("Enemy"))
                {
                    impactedEnemies++;
                    EnemyHealth enemy = hit.transform.GetComponent<EnemyHealth>();
                    if (enemy != null)
                        enemy.TakeDamage(railgunDamage / impactedEnemies);
                }
            }        
        }
        shotsFired++;
        if (shotsFired == railgunAmmo)
            BackToMainTurret();
    }

    private void FireWithMachineGun()
    {
        Debug.Log("Disparo con machineGun");
        cooldownTimer = 0;
    }

    private void FireWithRocket()
    {
        shotsFired++;
        Vector3 startPos = fakeRockets[shotsFired-1].transform.position;
        Vector3 fireDirection;

        if (_isAimingAtEnemy)
            fireDirection = (mainHit.point - startPos).normalized;
        else
            fireDirection = mainGunFirePoint.forward;

        fakeRockets[shotsFired - 1].SetActive(false);
        GameObject rocket = Instantiate(rocketPrefab, startPos, Quaternion.LookRotation(fireDirection));
        ProjectileController rocketController = rocket.GetComponent<ProjectileController>();
        rocket.transform.SetParent(projectileContainer);

        if (rocketController != null)
            rocketController.Initialize(startPos, fireDirection, rocketSpeed, bulletRange, rocketDamage);
       
        cooldownTimer = 0;
        if (shotsFired == rocketAmmo)
            BackToMainTurret();
    }


    public void OnWeaponPowerUp()
    {
        int random = 1;
        turretMesh.mesh = turretMeshes[random];
        currentWeapon = (Weapons)random;
        currentRange = rangeOfTurrets[random];
        if (rechargingPowerUpActive)
            currentCooldown = turretCooldowns[(int)currentWeapon].y;
        else
            currentCooldown = turretCooldowns[(int)currentWeapon].x;
        if (cooldownTimer > currentCooldown)
            cooldownTimer = currentCooldown;
        shotsFired = 0;
        if (currentWeapon == Weapons.rocket)
        {            
            for (int i = 0; i < 4; i++)
            {
                fakeRockets[i].SetActive(true);
            }
        }
    }

    [ContextMenu ("backtomainturret")]
    private void BackToMainTurret()
    {
        currentWeapon = Weapons.mainTurret;
        turretMesh.mesh = turretMeshes[0];
        currentRange = rangeOfTurrets[0];
        if (rechargingPowerUpActive)
            currentCooldown = turretCooldowns[0].y;
        else
            currentCooldown = turretCooldowns[0].x;
        if (cooldownTimer > currentCooldown)
            cooldownTimer = currentCooldown;
        for (int i = 0; i < 4; i++)
        {
            fakeRockets[i].SetActive(false);
        }
    }

    public void RecharchingPowerUp(float duration)
    {
        currentCooldown = turretCooldowns[(int)currentWeapon].y;
        rechargingPowerUpActive = true;
        if (cooldownTimer >= currentCooldown)
            cooldownTimer = currentCooldown;
        Invoke("RestoreCooldown", duration);
    }

    private void RestoreCooldown()
    {
        currentCooldown = turretCooldowns[(int) currentWeapon].x;
        if (cooldownTimer > currentCooldown)
            cooldownTimer = currentCooldown;
        rechargingPowerUpActive = false;
    }

    public void EndAnimation()
    {
        mainTurretAnimator.SetBool("Fire", false);
    }
}
