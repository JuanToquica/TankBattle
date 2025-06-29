using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform playerTurret;
    [SerializeField] private Transform player;
    [SerializeField] private Transform mainCamera;
    [SerializeField] private Transform raycastOrigin;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private EnemyManager enemyManager;
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
    public float additionalDistance;
    public bool playerAlive;
    private ParentConstraint parentConstraint;
    private Transform enemyTurret;

    private void Awake()
    {
        parentConstraint = GetComponent<ParentConstraint>();
    }
    private void Start()
    {
        combinedLayers = (1 << 6) | (1 << 2);
    }
    private void Update()
    {
        if (playerAlive)
        {
            if (InputManager.Instance.playerInput.actions["MoveTurretWithKeys"].enabled)
            {
                Quaternion targetRotation = Quaternion.Euler(transform.rotation.eulerAngles.x, playerTurret.rotation.eulerAngles.y, 0);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSmoothingWithKeys * Time.deltaTime);
                horizontalRotation = transform.eulerAngles.y;
            }
            else if (InputManager.Instance.playerInput.actions["MoveTurretWithMouse"].enabled)
            {
                horizontalRotation += InputManager.Instance.mouseInput * (sensitivity * 10) * Time.deltaTime;
                Quaternion targetRotation = Quaternion.Euler(transform.rotation.eulerAngles.x, horizontalRotation, 0);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSmoothingWithMouse * Time.deltaTime);
            }
        }       
        else
        {
            if (enemyTurret != null)
            {
                Quaternion targetRotation = Quaternion.Euler(transform.rotation.eulerAngles.x, enemyTurret.rotation.eulerAngles.y, 0);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSmoothingWithKeys * Time.deltaTime);
                horizontalRotation = transform.eulerAngles.y;
            }      
        }
        
        Vector3 directionToCamera = (mainCamera.position - raycastOrigin.position).normalized;
        Vector3 origin = raycastOrigin.position + directionToCamera;
        float distance = Mathf.Abs(maxOffset.z) + additionalDistance;
        float targetT;
        Debug.DrawRay(origin, directionToCamera * distance);

        if (Physics.Raycast(origin, directionToCamera, out RaycastHit hit, distance, combinedLayers) && !hit.transform.CompareTag("Railing"))
        {
            if (Mathf.Abs(Vector3.Dot(hit.normal, Vector3.up)) > 0.98f)
            {
                targetT = 1;                   
            }
            else if (hit.transform.CompareTag("Floor") && hit.distance > (distance - additionalDistance))
            {
                targetT = 1;
            }
            else
            {
                targetT = Mathf.Clamp01((hit.distance / distance) - originOffset);
            }
        }
        else
        {
            targetT = 1;
        }      

        currentT = Mathf.Lerp(currentT, targetT, Time.deltaTime * smoothingInCollision);
        mainCamera.localPosition = Vector3.Lerp(minOffset, maxOffset, currentT);
        mainCamera.localEulerAngles = new Vector3(Mathf.Lerp(minRotation, maxRotation, currentT), 0, 0);
        playerController.cameraPivotRotation = transform.eulerAngles.y;
    }

    public void OnPlayerRevived()
    {
        horizontalRotation = 0;
        transform.rotation = Quaternion.identity;
        parentConstraint.constraintActive = false;
        parentConstraint.SetSources(new List<ConstraintSource>());
        ConstraintSource newSource = new ConstraintSource
        {
            sourceTransform = player,
            weight = 1.0f
        };
        parentConstraint.AddSource(newSource);
        parentConstraint.constraintActive = true;
    }

    public void OnPlayerDead()
    {
        playerAlive = false;
        parentConstraint.constraintActive = false;
        parentConstraint.SetSources(new List<ConstraintSource>());
        EnemyAI enemy = enemyManager.GetEnemyThatKilledThePlayer();
        ConstraintSource newSource = new ConstraintSource
        {
            sourceTransform = enemy.transform,
            weight = 1.0f
        };
        enemyTurret = enemy.turret;
        parentConstraint.AddSource(newSource);
        parentConstraint.constraintActive = true;
    }
}
