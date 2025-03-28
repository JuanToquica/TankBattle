using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    private Animator animator;
    [SerializeField] private float cooldown;
    public float cooldownTimer;

    private void Start()
    {
        animator = GetComponent<Animator>();
        cooldownTimer = cooldown;
    }

    private void Update()
    {
        if (cooldownTimer < cooldown)
        {
            cooldownTimer = Mathf.Clamp(cooldownTimer + Time.deltaTime, 0, cooldown);
        }
    }
    public void Fire()
    {
        if (cooldownTimer == cooldown)
        {
            animator.SetBool("Fire", true);
            cooldownTimer = 0;
        }        
    }

    public void EndAnimation()
    {
        animator.SetBool("Fire", false);
    }
}
