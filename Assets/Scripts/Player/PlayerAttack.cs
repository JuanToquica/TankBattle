using System.Collections;
using System.IO.Pipes;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.ProBuilder;
using UnityEngine.UI;
using UnityEngine.Windows;

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
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private GameObject rocketPrefab;
    [SerializeField] private GameObject shotVfx;
    [SerializeField] private GameObject machineGunVfx;
    [SerializeField] private Transform machineGunCannon;
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
    [SerializeField] private Vector2 amountOfShotsInOneRound;
    [SerializeField] private float aimAngle;
    [SerializeField] private int defaultAmountOfRaycast;
    [SerializeField] private int railGunAmountOfRaycast;
    [SerializeField] private float projectileSpeed;
    [SerializeField] private float bulletSpeed;
    [SerializeField] private float rocketSpeed;
    [SerializeField] private float bulletRange;
    [SerializeField] private float railgunDelay;
    [SerializeField] private float railgunAmmo;
    [SerializeField] private float machineGunAmmo;
    [SerializeField] private float rocketAmmo;
    [SerializeField] private float timeBetweenShoots;
    [SerializeField] private float machineGunAngularSpeed;
    public float currentCooldown;
    public float currentRange;   
    public float cooldownTimer;
    public float currentAmountOfShotsInOneRound;
    public float machineGunRotation;
    private int mainTurretDamage;
    private int railgunDamage;
    private int machineGunDamage;
    private int rocketDamage;
    private RaycastHit mainHit;
    private Ray mainRay;
    private bool _isAimingAtEnemy;
    public bool rechargingPowerUpActive;
    public int shotsFired;
    private int aimPhase;
    private bool foundEnemyInThisScan;
    private RaycastHit bestHitInThisScan;
    private int currentAmountOfRaycast;
    public bool firing;
    private float lastShoot;

    private void OnEnable()
    {
        if (InputManager.Instance != null)
            InputManager.Instance.RegisterPlayerAttack(this);
        LoadTurretDamage();
        BackToMainTurret();
        aimPhase = 1;
    }

    private void Start()
    {
        mainTurretAnimator = GetComponent<Animator>();
        InputManager.Instance.RegisterPlayerAttack(this);
    }

    private void Update()
    {
        SetCooldown();
        Aim();
        SetMachineGunRotation();
    }

    private void SetMachineGunRotation()
    {
        if (currentWeapon == Weapons.machineGun)
        {
            machineGunCannon.Rotate(0,0,machineGunAngularSpeed *  machineGunRotation * 5);
            if (firing)
            {
                machineGunRotation = Mathf.Clamp(Mathf.MoveTowards(machineGunRotation, 1, machineGunAngularSpeed * Time.deltaTime), 0, 1);
                //if (machineGunRotation > 0.99f) machineGunRotation = 1;

                if (Time.time > lastShoot + timeBetweenShoots && machineGunRotation == 1)
                {
                    FireWithMachineGun();
                    machineGunVfx.SetActive(true);
                }      
            }
            else
            {
                machineGunRotation = Mathf.Clamp(Mathf.MoveTowards(machineGunRotation, 0, (machineGunAngularSpeed / 3) * Time.deltaTime), 0, 1);
                machineGunVfx.SetActive(false);
            }
        }
        else
        {
            if (machineGunVfx.activeInHierarchy == true)
                machineGunVfx.SetActive(false);
        }
    }

    private void SetCooldown()
    {
        if (cooldownTimer < currentCooldown && (!firing || currentWeapon != Weapons.machineGun))
        {
            cooldownTimer = Mathf.Clamp(cooldownTimer + Time.deltaTime, 0, currentCooldown);
        }
        else if (firing && currentWeapon == Weapons.machineGun)
        {
            float decreasingFactor = currentCooldown / (currentAmountOfShotsInOneRound * timeBetweenShoots);
            cooldownTimer = Mathf.Clamp(cooldownTimer - Time.deltaTime * decreasingFactor, 0, currentCooldown);
        }
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
        float angleStep = aimAngle / currentAmountOfRaycast;

        if (aimPhase == 1)
        {
            foundEnemyInThisScan = false;
            bestHitInThisScan = new RaycastHit();
            bestHitInThisScan.distance = Mathf.Infinity;
            for (int i = 0; i < currentAmountOfRaycast / 2; i++)
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
            aimPhase = 2;
        }
        else if (aimPhase == 2)
        {
            for (int i = currentAmountOfRaycast / 2; i < currentAmountOfRaycast; i++)
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
                if (bestHitInThisScan.transform != null)
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
            aimPhase = 1;
        }   
        Debug.DrawLine(mainGunFirePoint.position, mainHit.point, Color.red, 0.2f);
    }

    public void Fire()
    { 
        if (cooldownTimer == currentCooldown && currentWeapon != Weapons.machineGun)
        {
            switch (currentWeapon)
            {
                case Weapons.mainTurret:
                    FireWithMainTurret();
                    break;
                case Weapons.railGun:
                    StartCoroutine(FireWithRailgun());
                    break;
                case Weapons.rocket:
                    FireWithRocket();
                    break;
            }
        }  
        if (cooldownTimer > timeBetweenShoots * 5 && currentWeapon == Weapons.machineGun)
        {
            firing = true;
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
        ProjectileController bulletController = bulletInstance.GetComponent<ProjectileController>();
        bulletInstance.transform.SetParent(projectileContainer);

        if (bulletController != null)
            bulletController.Initialize(startPos, fireDirection, projectileSpeed, bulletRange, mainTurretDamage);

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
        Vector3 startPos = mainGunFirePoint.position;
        Vector3 fireDirection;

        if (_isAimingAtEnemy)
            fireDirection = (mainHit.point - startPos).normalized;
        else
            fireDirection = mainGunFirePoint.forward;

        GameObject bulletInstance = Instantiate(bulletPrefab, startPos, Quaternion.LookRotation(fireDirection));
        ProjectileController bulletController = bulletInstance.GetComponent<ProjectileController>();
        bulletInstance.transform.SetParent(projectileContainer);

        if (bulletController != null)
            bulletController.Initialize(startPos, fireDirection, bulletSpeed, bulletRange, machineGunDamage);
        shotsFired++;

        if (shotsFired >= machineGunAmmo)
        {
            StopFiring();
            BackToMainTurret();
            return;
        }
        lastShoot = Time.time;
        
        if (cooldownTimer < currentCooldown / currentAmountOfShotsInOneRound)
            StopFiring();        
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

    public void StopFiring()
    {
        if (firing && currentWeapon == Weapons.machineGun)
        {
            firing = false;
            machineGunVfx.SetActive(false);
        }
            
    }

    public void OnWeaponPowerUp()
    {
        int random = 2; //Random.Range(1,4);
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
        else if (currentWeapon == Weapons.railGun)
        {
            currentAmountOfRaycast = railGunAmountOfRaycast;
        }
    }

    [ContextMenu ("backtomainturret")]
    private void BackToMainTurret()
    {
        currentWeapon = Weapons.mainTurret;
        turretMesh.mesh = turretMeshes[0];
        currentRange = rangeOfTurrets[0];
        currentAmountOfRaycast = defaultAmountOfRaycast;
        currentAmountOfShotsInOneRound = amountOfShotsInOneRound.x;
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
        currentAmountOfShotsInOneRound = amountOfShotsInOneRound.y;
        rechargingPowerUpActive = true;
        if (cooldownTimer >= currentCooldown)
            cooldownTimer = currentCooldown;
        Invoke("RestoreCooldown", duration);
    }

    private void RestoreCooldown()
    {
        currentCooldown = turretCooldowns[(int) currentWeapon].x;
        currentAmountOfShotsInOneRound = amountOfShotsInOneRound.x;
        if (cooldownTimer > currentCooldown)
            cooldownTimer = currentCooldown;
        rechargingPowerUpActive = false;
    }

    public void EndAnimation()
    {
        mainTurretAnimator.SetBool("Fire", false);
    }
}
