using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : HealthBase
{
    public static event System.Action OnPlayerDead;
    [SerializeField] private PlayerSpawner spawner;
    [SerializeField] private Image healthBar;
    private PlayerMaterialHandler playerMaterialHandler;
    private PlayerController playerController;
    private PlayerAttack playerAttack;

    private void OnEnable()
    {
        maxHealth = DataManager.Instance.GetArmorStrengthDamage();
        health = maxHealth;
    }

    private void Start()
    {
        health = maxHealth;
        tankAudioController = GetComponent<TankAudioController>();
        playerMaterialHandler = GetComponent<PlayerMaterialHandler>();
        playerController = GetComponent<PlayerController>();
        playerAttack = GetComponent<PlayerAttack>();
    }

    private void Update()
    {
        healthBar.fillAmount = health / maxHealth;
    }

    protected override void Die()
    {
        base.Die();
        OnPlayerDead?.Invoke();
        playerMaterialHandler.OnTankDead();       

        Vector3 directionToCamera = (Camera.main.transform.position - playerController.transform.position).normalized;
        GameObject vfx = ObjectPoolManager.Instance.GetPooledObject(deathVfx, transform.position + new Vector3(0, 1.3f, 0) + directionToCamera, transform.rotation);
        vfx.transform.parent = transform;
       
        playerController.OnTankDead();

        playerAttack.OnTankDead();
        playerAttack.enabled = false;

        if (GameManager.instance.playerHasTheFlag)
        {
            GameManager.instance.OnPlayerDeathWithFlag();
        }
        InputManager.Instance.playerInput.actions.FindActionMap("Player").Disable();
        spawner.OnPlayerDead();
    }
}
