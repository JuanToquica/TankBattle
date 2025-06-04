using UnityEngine;
using UnityEngine.UI;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private float maxHealth;
    [SerializeField] private Image healthBar;
    [SerializeField] private Transform healthBarCanvas;
    public EnemyManager enemyManager;
    private EnemyAI enemyAI;
    public Transform player;
    private Transform healthBarTransform;
    private float health;

    private void Start()
    {
        health = maxHealth;
        enemyAI = GetComponent<EnemyAI>();
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
        Debug.Log("ENEMY MUERTO");
        int area = enemyAI.enemyArea;
        if (enemyAI.enemyArea == 13)
            area = 7;
        if (enemyAI.enemyArea == 14)
        {
            enemyManager.chasingInArea14 = false;
            area = enemyAI.oldArea;
        }         
        enemyManager.DeadEnemy(area);    
        Destroy(gameObject);
    }
}
