using UnityEngine;
using UnityEngine.Rendering;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform playerTurret;
    [SerializeField] private Transform mainCamera;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private Vector3 maxOffset;
    [SerializeField] private Vector3 minOffset;
    [SerializeField] private float maxRotation;
    [SerializeField] private float minRotation;
    [SerializeField] private float smoothingInCollision;
    [SerializeField] private float rotationSmoothingWithKeys;
    [SerializeField] private float rotationSmoothingWithMouse;
    public float sensitivity;
    private float currentT = 1;
    public float horizontalRotation;
    private float rotationRef;

    private void Update()
    {
        if (playerController.playerInput.Player.MoveTurretWithKeys.enabled)
        {
            Quaternion targetRotation = Quaternion.Euler(transform.rotation.eulerAngles.x, playerTurret.rotation.eulerAngles.y, 0);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSmoothingWithKeys * Time.deltaTime);
            horizontalRotation = transform.eulerAngles.y;
        }
        else if (playerController.playerInput.Player.MoveTurretWithMouse.enabled)
        {
            horizontalRotation += playerController.mouseInput * sensitivity * Time.deltaTime;
            Quaternion targetRotation = Quaternion.Euler(transform.rotation.eulerAngles.x, horizontalRotation, 0);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSmoothingWithMouse * Time.deltaTime);
        }

        float distance = Mathf.Abs(maxOffset.z) + 1;
        Vector3 directionToCamera = ((mainCamera.position) - playerTurret.position).normalized;
        Debug.DrawRay(playerTurret.position - playerTurret.forward * 0.85f, directionToCamera * distance);
        float targetT = Physics.Raycast(playerTurret.position - playerTurret.forward * 0.85f, directionToCamera, out RaycastHit hit, distance, 1 << 6) ?
            Mathf.Clamp01(hit.distance / distance) : 1;

        currentT = Mathf.Lerp(currentT, targetT, Time.deltaTime * smoothingInCollision);
        mainCamera.localPosition = Vector3.Lerp(minOffset, maxOffset, currentT);
        mainCamera.localEulerAngles = new Vector3(Mathf.Lerp(minRotation, maxRotation, currentT), 0, 0);
        playerController.cameraPivotRotation = transform.eulerAngles.y;
    }
}
