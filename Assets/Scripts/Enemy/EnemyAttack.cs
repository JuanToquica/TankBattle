using UnityEngine;

public class EnemyAttack : AttackBase
{
    private EnemyAI enemy;

    protected override void Start()
    {
        base.Start();
        enemy = GetComponent<EnemyAI>();      
    }

    protected override void Update()
    {
        base.Update();
        if (enemy != null && !enemy.detectingPlayer && currentWeapon == Weapons.machineGun && firing)
            StopFiring();
        if (firing)
            Aim();
    }

    protected override void LoadTurretDamage()
    {
        int playerArmorStrenght = DataManager.Instance.GetArmorStrengthDamage();
        mainTurretDamage = (int)((DataManager.Instance.GetMainTurretDamage() * 0.7f) / (playerArmorStrenght / 100f));
        railgunDamage = (int)((DataManager.Instance.GetRailgunDamage() * 0.7f) / (playerArmorStrenght / 100f));
        machineGunDamage = (int)((DataManager.Instance.GetMachineGunDamage() * 0.7f) / (playerArmorStrenght / 100f));
        rocketDamage = (int)((DataManager.Instance.GetRocketDamage() * 0.7f) / (playerArmorStrenght / 100f));
    }

    public void Shoot()
    {
        if (!CanShoot()) return;
        Aim();
        Fire();     
    }

    public bool CanShoot() { return cooldownTimer == currentCooldown; }
}
