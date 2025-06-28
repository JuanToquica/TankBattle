using UnityEngine;
using UnityEngine.UI;

public class HealthBase : MonoBehaviour
{
    [SerializeField] protected float maxHealth;
    [SerializeField] protected Image healthBar;
    [SerializeField] protected GameObject deathVfx;
    protected float health;

    protected virtual void Update()
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

    protected virtual void Die()
    {
        health = 0;
        foreach (Transform child in transform) //Destruir vfx de powerups que no hayan terminado
        {
            if (child.CompareTag("VFX"))
            {
                Destroy(child.gameObject);
            }
        }
    }
}
