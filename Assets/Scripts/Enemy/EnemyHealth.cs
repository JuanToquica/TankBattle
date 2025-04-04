using UnityEngine;
using UnityEngine.UI;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private float maxHealth;
    [SerializeField] private Image healthBar;
    [SerializeField] private Transform healthBarCanvas;    
    [SerializeField] private Transform player;
    private Transform healthBarTransform;
    private float health;

    private void Start()
    {
        health = maxHealth;
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

    public void SubstractHealth(float damage)
    {
        if (health > 0)
        {
            health -= damage;
            if (health < 0)
                Die();
        }
    }

    private void Die()
    {
        Debug.Log("ENEMY MUERTO");
        Destroy(gameObject);
    }
}
