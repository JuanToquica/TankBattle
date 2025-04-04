using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private float maxHealth;
    [SerializeField] private Image healthBar;
    private float health;

    private void Start()
    {
        health = maxHealth;
    }
    private void Update()
    {
        if (health < maxHealth)
        {
            health = Mathf.Clamp(health += 0.1f * Time.deltaTime,0 ,maxHealth);
        }
        healthBar.fillAmount = health / maxHealth;
    }

    private void SubstractHealth(float damage)
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
        Debug.Log("MUERTO");
    }
}
