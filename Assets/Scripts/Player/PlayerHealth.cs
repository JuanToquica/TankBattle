using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private PlayerSpawner spawner;
    [SerializeField] private float maxHealth;
    [SerializeField] private Image healthBar;
    [SerializeField] private GameObject deathVfx;
    private ChangeTankPaint changeTankPaint;
    private PlayerController playerController;
    private PlayerAttack playerAttack;
    private float health;

    private void Start()
    {
        health = maxHealth;
        changeTankPaint = GetComponent<ChangeTankPaint>();
        playerController = GetComponent<PlayerController>();
        playerAttack = GetComponent<PlayerAttack>();
    }
 
    private void Update()
    {
        healthBar.fillAmount = health / maxHealth;
    }

    public void TakeDamage(float damage)
    {
        if (health > 0)
        {
            health -= damage;
            if (health <= 0)
            {               
                Die();
            }
                
        }
    }

    public void RegainHealth()
    {
        health = maxHealth;
    }

    private void Die()
    {
        health = 0;
        if (GameManager.instance.playerHasTheFlag)
        {
            GameManager.instance.OnPlayerDeathWithFlag();
        }
        changeTankPaint.OnTankDead();
        spawner.OnPlayerDead();
        Vector3 directionToCamera = (playerController.cameraController.transform.position - playerController.transform.position).normalized;
        GameObject vfx = Instantiate(deathVfx, transform.position + new Vector3(0, 1.3f, 0) + directionToCamera, transform.rotation);
        vfx.transform.parent = transform;
        playerController.movement = 0;
        playerController.rotation = 0;
        playerController.dying = true;
        playerController.tankCollider.enabled = false;
        if (playerAttack.railgunCoroutine != null)
            StopCoroutine(playerAttack.railgunCoroutine);
        playerAttack.enabled = false;
        InputManager.Instance.playerInput.actions.FindActionMap("Player").Disable();
        foreach (Transform child in transform) //Destruir vfx de powerups que no hayan terminado
        {
            if (child.CompareTag("VFX"))
            {
                Destroy(child.gameObject);
            }
        }
    }
}
