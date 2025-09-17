using UnityEngine;
using UnityEngine.UI;

public class HealthBase : MonoBehaviour
{
    [SerializeField] protected float maxHealth;
    [SerializeField] protected GameObject deathVfx;
    protected float health;
    protected TankAudioController tankAudioController;

    public virtual void TakeDamage(int damage)
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
        tankAudioController.PlayPowerUpSound();
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
