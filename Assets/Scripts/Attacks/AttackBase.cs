using UnityEngine;
using System.Collections;
using Unity.VisualScripting;


public enum Weapons
{
    mainTurret,
    railGun,
    machineGun,
    rocket
}

public abstract class AttackBase : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private WeaponsSettings weaponsSettings;
    [SerializeField] protected Transform mainGunFirePoint;
    [SerializeField] protected Transform railgunFirePoint;
    [SerializeField] protected Transform machineGunFirePoint;
    [SerializeField] protected GameObject[] fakeRockets;
    [SerializeField] private GameObject machineGunVfx;
    [SerializeField] private GameObject backToMainTurretVfx;
    [SerializeField] protected Transform machineGunCannon;
    [SerializeField] protected GameObject[] turrets;
    [SerializeField] protected Animator mainTurretAnimator;
    [SerializeField] protected Animator railgunAnimator;
    [SerializeField] protected Animator rocketAnimator;
    public Weapons currentWeapon;

    [Header("Fire Specifications")]
    [SerializeField] protected string targetTag;
    [SerializeField] protected Vector2[] turretCooldowns;
    [SerializeField] private float turretChangeDelay;
    public float currentCooldown;
    public float currentRange;
    public float cooldownTimer;
    public float currentAmountOfShotsInOneRound;
    public float machineGunRotation;
    public int mainTurretDamage;
    public int railgunDamage;
    public int machineGunDamage;
    public int rocketDamage;
    protected RaycastHit mainHit;
    public bool rechargingPowerUpActive;
    public int shotsFired;
    public int aimPhase;
    protected RaycastHit bestHitInThisScan;
    protected int currentAmountOfRaycast;
    public bool firing;
    protected float lastShoot;
    protected bool foundTankInThisScan;
    protected Vector3 fireDirection;
    protected Coroutine railgunCoroutine;
    protected Coroutine weaponPowerUpCoroutine;
    protected Coroutine backToMainTurretCoroutine;


    protected abstract void LoadTurretDamage();

    protected void SetMachineGunRotation()
    {
        if (currentWeapon == Weapons.machineGun)
        {
            machineGunCannon.Rotate(0, 0, weaponsSettings.machineGunAngularSpeed * machineGunRotation * 5);
            if (firing)
            {
                machineGunRotation = Mathf.Clamp(Mathf.MoveTowards(machineGunRotation, 1, weaponsSettings.machineGunAngularSpeed * Time.deltaTime), 0, 1);
                if (Time.time > lastShoot + weaponsSettings.timeBetweenShoots && machineGunRotation == 1)
                {
                    FireWithMachineGun();
                    machineGunVfx.SetActive(true);
                }
            }
            else
            {
                machineGunRotation = Mathf.Clamp(Mathf.MoveTowards(machineGunRotation, 0, (weaponsSettings.machineGunAngularSpeed / 3) * Time.deltaTime), 0, 1);
                machineGunVfx.SetActive(false);
            }
        }
        else
        {
            if (machineGunVfx.activeInHierarchy == true)
                machineGunVfx.SetActive(false);
        }
    }

    protected virtual void SetCooldown()
    {
        if (cooldownTimer < currentCooldown && (!firing || currentWeapon != Weapons.machineGun))
        {
            cooldownTimer = Mathf.Clamp(cooldownTimer + Time.deltaTime, 0, currentCooldown);
        }
        else if (firing && currentWeapon == Weapons.machineGun)
        {
            float decreasingFactor = currentCooldown / (currentAmountOfShotsInOneRound * weaponsSettings.timeBetweenShoots);
            cooldownTimer = Mathf.Clamp(cooldownTimer - Time.deltaTime * decreasingFactor, 0, currentCooldown);
        }       
    }


    protected virtual void Aim()
    {
        float halfAngle = weaponsSettings.aimAngle / 2f;
        float angleStep = weaponsSettings.aimAngle / currentAmountOfRaycast;

        if (aimPhase == 1)
        {
            foundTankInThisScan = false;
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
                    if (hit.transform.CompareTag(targetTag))
                    {
                        if (hit.distance < bestHitInThisScan.distance)
                        {
                            bestHitInThisScan = hit;
                        }
                        foundTankInThisScan = true;
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
                    if (hit.transform.CompareTag(targetTag))
                    {
                        if (hit.distance < bestHitInThisScan.distance)
                        {
                            bestHitInThisScan = hit;
                        }
                        foundTankInThisScan = true;
                    }
                }
            }
            if (foundTankInThisScan)
            {
                mainHit = bestHitInThisScan;
                fireDirection = (bestHitInThisScan.point - mainGunFirePoint.position).normalized;
            }
            else
            {
                Physics.Raycast(mainGunFirePoint.position, mainGunFirePoint.forward, out RaycastHit hit, 200);
                mainHit = hit;
                fireDirection = mainGunFirePoint.forward;
            }
            aimPhase = 1;
        }
        Debug.DrawLine(mainGunFirePoint.position, mainHit.point, Color.red, 0.2f);
    }


    public void Fire()
    {
        if (!gameObject.activeSelf)
            return;
        if (cooldownTimer == currentCooldown && currentWeapon != Weapons.machineGun)
        {
            switch (currentWeapon)
            {
                case Weapons.mainTurret:
                    FireWithMainTurret();
                    break;
                case Weapons.railGun:
                    if (shotsFired == weaponsSettings.railgunAmmo) return;
                    railgunCoroutine = StartCoroutine(FireWithRailgun());
                    break;
                case Weapons.rocket:
                    FireWithRocket();
                    break;
            }
        }
        if (cooldownTimer > weaponsSettings.timeBetweenShoots * 5 && currentWeapon == Weapons.machineGun)
        {
            firing = true;
        }
    }

    protected void FireWithMainTurret()
    {
        mainTurretAnimator.SetBool("Fire", true);
        ObjectPoolManager.Instance.GetPooledObject(weaponsSettings.shotVfx, mainGunFirePoint.position, mainGunFirePoint.rotation);
        Vector3 startPos = mainGunFirePoint.position;

        GameObject bulletInstance = ObjectPoolManager.Instance.GetPooledObject(weaponsSettings.projectilePrefab, startPos, Quaternion.LookRotation(fireDirection));
        ProjectileController bulletController = bulletInstance.GetComponent<ProjectileController>();

        if (bulletController != null)
            bulletController.Initialize(startPos, fireDirection, weaponsSettings.projectileSpeed, weaponsSettings.bulletRange, mainTurretDamage);

        cooldownTimer = 0;
    }

    protected IEnumerator FireWithRailgun()
    {        
        cooldownTimer = 0;
        GameObject vfx = ObjectPoolManager.Instance.GetPooledObject(weaponsSettings.railgunVfx, railgunFirePoint.position + railgunFirePoint.forward * 1.6f, railgunFirePoint.rotation);
        vfx.transform.SetParent(railgunFirePoint);
        yield return new WaitForSeconds(weaponsSettings.railgunDelay);
        Aim();
        railgunAnimator.SetBool("Fire", true);
        Vector3 startPos = railgunFirePoint.position;

        GameObject bulletInstance = ObjectPoolManager.Instance.GetPooledObject(weaponsSettings.railgunBulletPrefab, startPos, Quaternion.LookRotation(fireDirection));
        RailgunBullet bulletController = bulletInstance.GetComponent<RailgunBullet>();

        if (bulletController != null)
            bulletController.Initialize(startPos, fireDirection, weaponsSettings.railgunBulletSpeed, weaponsSettings.bulletRange, railgunDamage);

        shotsFired++;
        if (shotsFired == weaponsSettings.railgunAmmo)
            BackToMainTurret(turretChangeDelay);
    }

    protected void FireWithMachineGun()
    {
        if (shotsFired >= weaponsSettings.machineGunAmmo) return;
        Vector3 startPos = machineGunFirePoint.position;

        GameObject bulletInstance = ObjectPoolManager.Instance.GetPooledObject(weaponsSettings.bulletPrefab, startPos, Quaternion.LookRotation(fireDirection));
        ProjectileController bulletController = bulletInstance.GetComponent<ProjectileController>();

        if (bulletController != null)
            bulletController.Initialize(startPos, fireDirection, weaponsSettings.bulletSpeed, weaponsSettings.bulletRange, machineGunDamage);
        shotsFired++;

        if (shotsFired >= weaponsSettings.machineGunAmmo)
        {
            StopFiring();
            BackToMainTurret(turretChangeDelay);
            return;
        }
        lastShoot = Time.time;

        if (cooldownTimer < currentCooldown / currentAmountOfShotsInOneRound)
            StopFiring();
    }

    protected void FireWithRocket()
    {
        if (shotsFired == weaponsSettings.rocketAmmo) return;
            shotsFired++;

        if (shotsFired - 1 == 0 || shotsFired - 1 == 2)
            rocketAnimator.SetBool("FireWithRight", true);
        else
            rocketAnimator.SetBool("FireWithLeft", true);

        Vector3 startPos = fakeRockets[shotsFired - 1].transform.position;
        Vector3 direction;
        if (fireDirection == mainGunFirePoint.forward)
            direction = fireDirection;
        else
            direction = (mainHit.point - startPos).normalized;

        fakeRockets[shotsFired - 1].SetActive(false);
        GameObject rocket = ObjectPoolManager.Instance.GetPooledObject(weaponsSettings.rocketPrefab, startPos, Quaternion.LookRotation(direction));
        RocketController rocketController = rocket.GetComponent<RocketController>();

        if (rocketController != null)
            rocketController.Initialize(startPos, direction, weaponsSettings.rocketSpeed, weaponsSettings.bulletRange, rocketDamage, gameObject);
        
        cooldownTimer = 0;
        if (shotsFired == weaponsSettings.rocketAmmo)
            BackToMainTurret(turretChangeDelay);
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
        weaponPowerUpCoroutine = StartCoroutine(OnWeaponPowerUpCoroutine());
    }

    protected IEnumerator OnWeaponPowerUpCoroutine()
    {
        yield return new WaitForSeconds(turretChangeDelay);
        int random = Random.Range(1, 4);
        ChangeTurret(random);
        currentWeapon = (Weapons)random;
        currentRange = weaponsSettings.rangeOfTurrets[random];
        if (rechargingPowerUpActive)
            currentCooldown = turretCooldowns[(int)currentWeapon].y;
        else
            currentCooldown = turretCooldowns[(int)currentWeapon].x;
        cooldownTimer = currentCooldown;

        shotsFired = 0;
        if (currentWeapon == Weapons.rocket)
        {
            for (int i = 0; i < 4; i++)
            {
                fakeRockets[i].SetActive(true);
            }
        }
        else
        {
            DisableRockets();
        }
        if (currentWeapon == Weapons.railGun)
        {
            currentAmountOfRaycast = weaponsSettings.railGunAmountOfRaycast;
        }
    }

    protected void BackToMainTurret(float delay)
    {
        backToMainTurretCoroutine = StartCoroutine(BackToMainTurretCoroutine(delay));
    }

    protected IEnumerator BackToMainTurretCoroutine(float delay)
    {
        if (currentWeapon != Weapons.mainTurret)
        {
            GameObject vfx = Instantiate(backToMainTurretVfx, transform.position, transform.rotation);
            vfx.transform.parent = transform;
        }
            
        yield return new WaitForSeconds(delay);
        currentWeapon = Weapons.mainTurret;
        ChangeTurret(0);
        currentRange = weaponsSettings.rangeOfTurrets[0];
        currentAmountOfRaycast = weaponsSettings.defaultAmountOfRaycast;
        currentAmountOfShotsInOneRound = weaponsSettings.amountOfShotsInOneRound.x;
        if (rechargingPowerUpActive)
            currentCooldown = turretCooldowns[0].y;
        else
            currentCooldown = turretCooldowns[0].x;
        if (cooldownTimer > currentCooldown)
            cooldownTimer = currentCooldown;
        DisableRockets();
    }

    protected void ChangeTurret(int index)
    {
        for (int i = 0; i < turrets.Length; i++)
        {
            if (i == index)
            {
                turrets[i].SetActive(true);
                continue;
            }           
            turrets[i].SetActive(false);
        }
    }

    public void DisableRockets()
    {
        for (int i = 0; i < 4; i++)
        {
            fakeRockets[i].SetActive(false);
        }
    }

    public void OnTankDead()
    {
        DisableRockets();
        machineGunVfx.SetActive(false);
        if (railgunCoroutine != null)
            StopCoroutine(railgunCoroutine);
        if (weaponPowerUpCoroutine != null)
            StopCoroutine(weaponPowerUpCoroutine);
        if (backToMainTurretCoroutine != null)
            StopCoroutine(backToMainTurretCoroutine);
    }

    public void RecharchingPowerUp(float duration)
    {
        currentCooldown = turretCooldowns[(int)currentWeapon].y;
        currentAmountOfShotsInOneRound = weaponsSettings.amountOfShotsInOneRound.y;
        rechargingPowerUpActive = true;
        if (cooldownTimer >= currentCooldown)
            cooldownTimer = currentCooldown;
        Invoke("RestoreCooldown", duration);
    }


    protected void RestoreCooldown()
    {
        currentCooldown = turretCooldowns[(int)currentWeapon].x;
        currentAmountOfShotsInOneRound = weaponsSettings.amountOfShotsInOneRound.x;
        if (cooldownTimer > currentCooldown)
            cooldownTimer = currentCooldown;
        rechargingPowerUpActive = false;
    }
}
