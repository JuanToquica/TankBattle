using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform playerTurret;
    [SerializeField] private Transform mainCamera;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private Vector3 maxOffset;
    [SerializeField] private Vector3 minOffset;
    [SerializeField] private float rotationSmoothingWithKeys;
    [SerializeField] private float rotationSmoothingWithMouse;
    [SerializeField] private float sensitivity;
    [HideInInspector] public float mouseInput;
    private float horizontalRotation;

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
        float distance = Mathf.Abs(maxOffset.z);
        bool ray = Physics.Raycast(playerTurret.position, -playerTurret.forward, out RaycastHit hit, distance, 1 << 6);
        Debug.DrawRay(playerTurret.position, -playerTurret.forward * distance);
        if (ray && hit.transform.CompareTag("Wall"))
        {
            float t = Mathf.Clamp01(hit.distance / distance);
            mainCamera.localPosition = Vector3.Lerp(minOffset, maxOffset, t);
        }
        else 
        {
            mainCamera.localPosition = maxOffset;
        }
    }
}
