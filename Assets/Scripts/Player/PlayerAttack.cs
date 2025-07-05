using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerAttack : AttackBase
{
    [Header("Player")]
    [SerializeField] private Image cooldownImage;
    [SerializeField] private HUD hud;
    private Outline currentOutlinedEnemy;    
    private bool _isAimingAtEnemy;    

    private void OnEnable()
    {
        currentCooldown = turretCooldowns[0].x;
        if (InputManager.Instance != null)
            InputManager.Instance.RegisterPlayerAttack(this);
        LoadTurretDamage();
        BackToMainTurret(0);
        aimPhase = 1;
    }

    private void Start()
    {
        InputManager.Instance.RegisterPlayerAttack(this);
        fireDirection = mainGunFirePoint.forward;
    }

    private void Update()
    {
        SetCooldown();
        Aim();
        SetMachineGunRotation();
    }

    protected override void LoadTurretDamage()
    {
        mainTurretDamage = DataManager.Instance.GetMainTurretDamage();
        railgunDamage = DataManager.Instance.GetRailgunDamage();
        machineGunDamage = DataManager.Instance.GetMachineGunDamage();
        rocketDamage = DataManager.Instance.GetRocketDamage();
    }

    protected override void SetCooldown()
    {
        base.SetCooldown();
        cooldownImage.fillAmount = cooldownTimer / currentCooldown;
    }


    protected override void Aim()
    {
        base.Aim();
        if (aimPhase == 1)
        {
            if (foundTankInThisScan)
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
                }
            }
            else
            {
                if (currentOutlinedEnemy != null)
                {
                    currentOutlinedEnemy.enabled = false;
                    currentOutlinedEnemy = null;
                }               
            }           
        }   
    }

    public override void RecharchingPowerUp(float duration)
    {
        base.RecharchingPowerUp(duration);
        hud.OnRechargingPowerUp(duration);
    }

    protected override void RestoreCooldown()
    {
        base.RestoreCooldown();
        hud.OnRechargingPowerUpDeactivated();
    }
}
