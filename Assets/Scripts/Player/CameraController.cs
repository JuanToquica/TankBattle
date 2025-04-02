using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform playerTurret;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private float rotationSmoothingWithKeys;
    [SerializeField] private float rotationSmoothingWithMouse;
    [SerializeField] private float sensitivity;
    [HideInInspector] public float mouseInput;
    private float horizontalRotation;
    private Vector3 velocity = Vector3.zero;
    
    private void FixedUpdate()
    {       
        if (playerController.playerInput.Player.MoveTurretWithKeys.enabled)
        {
            Quaternion targetRotation = Quaternion.Euler(transform.rotation.eulerAngles.x, playerTurret.rotation.eulerAngles.y, 0);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSmoothingWithKeys * Time.fixedDeltaTime);
            horizontalRotation = transform.eulerAngles.y;
        }      
        else if (playerController.playerInput.Player.MoveTurretWithMouse.enabled)
        {
            mouseInput = playerController.playerInput.Player.MoveTurretWithMouse.ReadValue<float>();            
            horizontalRotation += mouseInput * sensitivity * Time.fixedDeltaTime;
            Quaternion targetRotation = Quaternion.Euler(transform.rotation.eulerAngles.x, horizontalRotation, 0);

            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSmoothingWithMouse * Time.fixedDeltaTime);
        }
    }
}
