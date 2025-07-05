using UnityEngine;
using UnityEngine.Windows;

public class WheelAnimations : MonoBehaviour
{
    [SerializeField] private Animator trackLeftAnimator;
    [SerializeField] private Animator trackRightAnimator;
    [SerializeField] private Animator LeftWheelsAnimator;
    [SerializeField] private Animator RightWheelsAnimator;
    private float movement;
    private float rotation;
    private float wantsToMove;
    private float wantsToRotate;


    public void SetParameters(float movement, float rotation, float wantsToMove, float wantsToRotate)
    {
        this.movement = movement;
        this.rotation = rotation;
        this.wantsToMove = wantsToMove;
        this.wantsToRotate = wantsToRotate;
    }

    private void Update()
    {
        if (movement != 0 || rotation != 0)
        {
            if (rotation != 0 && wantsToMove == 0)
            {
                trackLeftAnimator.SetFloat("AnimationSpeed", rotation > 0 ? rotation * 0.8f : rotation * 0.8f);
                trackRightAnimator.SetFloat("AnimationSpeed", rotation > 0 ? rotation * -0.8f : rotation * -0.8f);
                LeftWheelsAnimator.SetFloat("AnimationSpeed", rotation > 0 ? rotation * 0.8f : rotation * 0.8f);
                RightWheelsAnimator.SetFloat("AnimationSpeed", rotation > 0 ? rotation * -0.8f : rotation * -0.8f);
            }
            if (movement > 0)
            {
                trackLeftAnimator.SetFloat("AnimationSpeed", rotation == 0 || rotation > 0 ? movement * 1.5f : movement * 0.8f);
                trackRightAnimator.SetFloat("AnimationSpeed", rotation == 0 || rotation < 0 ? movement * 1.5f : movement * 0.8f);
                LeftWheelsAnimator.SetFloat("AnimationSpeed", rotation == 0 || rotation > 0 ? movement * 1.5f : movement * 0.8f);
                RightWheelsAnimator.SetFloat("AnimationSpeed", rotation == 0 || rotation < 0 ? movement * 1.5f : movement * 0.8f);
            }
            else if (movement < 0)
            {
                trackLeftAnimator.SetFloat("AnimationSpeed", rotation == 0 || rotation < 0 ? movement * 1.5f : movement * 0.8f);
                trackRightAnimator.SetFloat("AnimationSpeed", rotation == 0 || rotation > 0 ? movement * 1.5f : movement * 0.8f);
                LeftWheelsAnimator.SetFloat("AnimationSpeed", rotation == 0 || rotation < 0 ? movement * 1.5f : movement *  0.8f);
                RightWheelsAnimator.SetFloat("AnimationSpeed", rotation == 0 || rotation > 0 ? movement * 1.5f : movement * 0.8f);
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
