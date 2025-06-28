using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : HealthBase
{
    [SerializeField] private PlayerSpawner spawner;
    private PlayerMaterialHandler playerMaterialHandler;
    private PlayerController playerController;
    private PlayerAttack playerAttack;

    private void Start()
    {
        health = maxHealth;
        playerMaterialHandler = GetComponent<PlayerMaterialHandler>();
        playerController = GetComponent<PlayerController>();
        playerAttack = GetComponent<PlayerAttack>();
    }
 

    protected override void Die()
    {
        base.Die();      
        playerMaterialHandler.OnTankDead();       

        Vector3 directionToCamera = (playerController.cameraController.transform.position - playerController.transform.position).normalized;
        GameObject vfx = Instantiate(deathVfx, transform.position + new Vector3(0, 1.3f, 0) + directionToCamera, transform.rotation);
        vfx.transform.parent = transform;

        playerController.movement = 0;
        playerController.rotation = 0;
        playerController.OnTankDead();

        playerAttack.enabled = false;

        if (GameManager.instance.playerHasTheFlag)
        {
            GameManager.instance.OnPlayerDeathWithFlag();
        }
        InputManager.Instance.playerInput.actions.FindActionMap("Player").Disable();
        spawner.OnPlayerDead();
    }
}
