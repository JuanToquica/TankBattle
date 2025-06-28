using UnityEngine;

public class EndAnimations : MonoBehaviour
{
    [SerializeField] private Animator animator;
    public void EndAnimation()
    {
        animator.SetBool("Fire", false);
    }

    public void EndRocketAnimation()
    {
        animator.SetBool("FireWithLeft", false);
        animator.SetBool("FireWithRight", false);
    }
}
