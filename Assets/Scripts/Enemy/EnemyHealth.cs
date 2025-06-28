using UnityEngine;
using UnityEngine.UI;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private float maxHealth;
    [SerializeField] private Image healthBar;
    [SerializeField] private Transform healthBarCanvas;
    [SerializeField] private GameObject deathVfx;
    [SerializeField] private float timeOfDeath;
    public EnemyManager enemyManager;
    private BoxCollider boxCollider;
    private EnemyAttack enemyAttack;
    private Outline outline;
    private EnemyAI enemyAI;
    public Transform player;
    private TankMaterialHandlerBase changeTankPaint;
    private float health;

    private void Start()
    {
        health = maxHealth;
        enemyAI = GetComponent<EnemyAI>();
        changeTankPaint = GetComponent<TankMaterialHandlerBase>();
        boxCollider = GetComponent<BoxCollider>();
        enemyAttack = GetComponent<EnemyAttack>();
        outline = GetComponent<Outline>();
    }

    private void Update()
    {
        if (health < maxHealth)
        {
            health = Mathf.Clamp(health += 0.1f * Time.deltaTime, 0, maxHealth);
        }
        healthBar.fillAmount = health / maxHealth;

        LookAtThePlayer();
    }

    private void LookAtThePlayer()
    {
        healthBarCanvas.transform.forward = -Camera.main.transform.forward;
    }

    public void TakeDamage(float damage)
    {
        if (health > 0)
        {
            health -= damage;
            if (health < 0)
                Die();
        }
    }
    
    public void RegainHealth()
    {
        health = maxHealth;
    }

    private void Die()
    {
        int area = enemyAI.enemyArea;
        if (enemyAI.enemyArea == 13)
            area = 7;
        if (enemyAI.enemyArea == 14)
        {
            enemyManager.chasingInArea14 = false;
            area = enemyAI.oldArea;
        }         
        enemyManager.DeadEnemy(area);
        changeTankPaint.OnTankDead();
        GameObject vfx = Instantiate(deathVfx, transform.position + new Vector3(0,1.3f,0) + enemyAI.directionToPlayer, transform.rotation);
        vfx.transform.parent = transform;
        enemyAI.desiredMovement = 0;
        enemyAI.desiredRotation = 0;
        enemyAI.Dying = true;
        outline.enabled = false;
        enemyAttack.DisbleRockets();
        healthBarCanvas.gameObject.SetActive(false);
        enemyAttack.enabled = false;
        boxCollider.enabled = false;
        foreach (Transform child in transform) //Destruir vfx de powerups que no hayan terminado
        {
            if (child.CompareTag("VFX"))
            {
                Destroy(child.gameObject);
            }
        }
        Invoke("DestroyTank", timeOfDeath);
    }

    private void DestroyTank()
    {
        Destroy(gameObject);
    }
}
