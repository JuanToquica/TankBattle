using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform playerTurret;
    [SerializeField] private Transform player;
    [SerializeField] private Transform mainCamera;
    [SerializeField] private Transform raycastOrigin;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private Vector3 maxOffset;
    [SerializeField] private Vector3 minOffset;
    [SerializeField] private float maxRotation;
    [SerializeField] private float minRotation;
    [SerializeField] private float smoothingInCollision;
    [SerializeField] private float rotationSmoothingWithKeys;
    [SerializeField] private float rotationSmoothingWithMouse;
    public float originOffset;
    public float sensitivity;
    private float currentT = 1;
    public float horizontalRotation;
    private float rotationRef;
    LayerMask combinedLayers;
    public float distanceA;
    public float distanceB;

    private void Start()
    {
        combinedLayers = (1 << 6) | (1 << 2);
    }
    private void Update()
    {
        if (InputManager.Instance.playerInput.actions["MoveTurretWithKeys"].enabled)
        {
            Quaternion targetRotation = Quaternion.Euler(transform.rotation.eulerAngles.x, playerTurret.rotation.eulerAngles.y, 0);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSmoothingWithKeys * Time.deltaTime);
            horizontalRotation = transform.eulerAngles.y;
        }
        else if (InputManager.Instance.playerInput.actions["MoveTurretWithMouse"].enabled)
        {
            horizontalRotation += InputManager.Instance.mouseInput * sensitivity * Time.deltaTime;
            Quaternion targetRotation = Quaternion.Euler(transform.rotation.eulerAngles.x, horizontalRotation, 0);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSmoothingWithMouse * Time.deltaTime);
        }

        
       
        
        Vector3 directionToCamera = (mainCamera.position - raycastOrigin.position).normalized;
        Vector3 origin = raycastOrigin.position + directionToCamera * distanceA;
        float distance = Mathf.Abs(maxOffset.z) + distanceB;

        Debug.DrawRay(origin, directionToCamera * distance);

        float targetT = Physics.Raycast(origin, directionToCamera, out RaycastHit hit, distance, combinedLayers) ?
            Mathf.Clamp01((hit.distance / distance) - originOffset) : 1;

        currentT = Mathf.Lerp(currentT, targetT, Time.deltaTime * smoothingInCollision);
        mainCamera.localPosition = Vector3.Lerp(minOffset, maxOffset, currentT);
        mainCamera.localEulerAngles = new Vector3(Mathf.Lerp(minRotation, maxRotation, currentT), 0, 0);
        playerController.cameraPivotRotation = transform.eulerAngles.y;
    }
}
