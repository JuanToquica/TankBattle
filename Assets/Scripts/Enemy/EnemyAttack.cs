using UnityEngine;

public class EnemyAttack : AttackBase
{
    private EnemyAI enemy;

    private void OnEnable()
    {
        LoadTurretDamage();
        BackToMainTurret();
        aimPhase = 1;
    }

    private void Start()
    {
        mainTurretAnimator = GetComponent<Animator>();
        enemy = GetComponent<EnemyAI>();
        fireDirection = mainGunFirePoint.forward;
    }

    private void Update()
    {
        SetCooldown();
        SetMachineGunRotation();
        if (enemy != null && !enemy.detectingPlayer && currentWeapon == Weapons.machineGun && firing)
            StopFiring();
        if (firing)
            Aim();
    }

    protected override void LoadTurretDamage()
    {
        int playerArmorStrenght = DataManager.Instance.GetArmorStrengthDamage();
        mainTurretDamage = (int)(DataManager.Instance.GetMainTurretDamage() / (playerArmorStrenght / 100f));
        railgunDamage = (int)(DataManager.Instance.GetRailgunDamage() / (playerArmorStrenght / 100f));
        machineGunDamage = (int)(DataManager.Instance.GetMachineGunDamage() / (playerArmorStrenght / 100f));
        rocketDamage = (int)(DataManager.Instance.GetRocketDamage() / (playerArmorStrenght / 100f));
    }

    public void Shoot()
    {
        if (!CanShoot()) return;
        Aim();
        Fire();     
    }

    protected override void Aim()
    {
        base.Aim();
    }

    public bool CanShoot()
    {
        return cooldownTimer == currentCooldown;
    }


    public void EndAnimation()
    {
        mainTurretAnimator.SetBool("Fire", false);
    }
}
