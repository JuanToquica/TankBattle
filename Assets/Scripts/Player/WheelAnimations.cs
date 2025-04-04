using UnityEngine;

public class WheelAnimations : MonoBehaviour
{
    [SerializeField] private Animator trackLeftAnimator;
    [SerializeField] private Animator trackRightAnimator;
    [SerializeField] private Animator LeftWheelsAnimator;
    [SerializeField] private Animator RightWheelsAnimator;
    private PlayerController playerController;

    private void Start()
    {
        playerController = GetComponent<PlayerController>();    
    }
    private void Update()
    {
        Vector2 input = playerController.input;
        if (input.magnitude != 0)
        {
            if (input.x != 0 && input.y == 0)
            {
                trackLeftAnimator.SetFloat("AnimationSpeed", input.x > 0 ? 0.5f : -0.5f);
                trackRightAnimator.SetFloat("AnimationSpeed", input.x > 0 ? -0.5f : 0.5f);
                LeftWheelsAnimator.SetFloat("AnimationSpeed", input.x > 0 ? 0.5f : -0.5f);
                RightWheelsAnimator.SetFloat("AnimationSpeed", input.x > 0 ? -0.5f : 0.5f);
            }
            if (input.y > 0)
            {
                trackLeftAnimator.SetFloat("AnimationSpeed", input.x == 0 || input.x > 0 ? 1f : 0.7f);
                trackRightAnimator.SetFloat("AnimationSpeed", input.x == 0 || input.x < 0 ? 1f : 0.7f);
                LeftWheelsAnimator.SetFloat("AnimationSpeed", input.x == 0 || input.x > 0 ? 1f : 0.7f);
                RightWheelsAnimator.SetFloat("AnimationSpeed", input.x == 0 || input.x < 0 ? 1f : 0.7f);
            }
            else if (input.y < 0)
            {
                trackLeftAnimator.SetFloat("AnimationSpeed", input.x == 0 || input.x < 0 ? -1f : -0.7f);
                trackRightAnimator.SetFloat("AnimationSpeed", input.x == 0 || input.x > 0 ? -1f : -0.7f);
                LeftWheelsAnimator.SetFloat("AnimationSpeed", input.x == 0 || input.x < 0 ? -1f : -0.7f);
                RightWheelsAnimator.SetFloat("AnimationSpeed", input.x == 0 || input.x > 0 ? -1f : -0.7f);
            }
        }
        else 
        {
            trackLeftAnimator.SetFloat("AnimationSpeed", 0);           
            trackRightAnimator.SetFloat("AnimationSpeed", 0);
            LeftWheelsAnimator.SetFloat("AnimationSpeed", 0);
            RightWheelsAnimator.SetFloat("AnimationSpeed", 0);
        }  
    }
}
