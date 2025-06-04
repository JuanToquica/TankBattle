using DG.Tweening;
using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : TankBase
{
    [SerializeField] private InputManager inputManager;
    private WheelAnimations wheelAnimations;   
    private Vector2 input;
    public float turretRotationInput;
    public float mouseInput;
    public float cameraPivotRotation;

    [Header ("Camera")]
    [SerializeField] private Transform cameraPivot;
    [SerializeField] private CameraController cameraController;

    private void OnEnable()
    {        
        currentRotationSpeed = tankRotationSpeed;
        movement = 0;
        rotation = 0;
        lastDistances = new float[suspensionPoints.Length];
        springStrength = minSpringStrength;
        dampSensitivity = minDampSensitivity;
        if (rb != null)
        {
            RestoreSpeed();
            rb.inertiaTensor = minInertiaTensor;
        }      
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        wheelAnimations = GetComponent<WheelAnimations>();
        tankCollider = GetComponent<BoxCollider>();
        inputManager.playerInput.Player.MoveTurretWithMouse.Disable();
        inputManager.playerInput.Player.Fire.Disable();

        RestoreSpeed();
        currentRotationSpeed = tankRotationSpeed;
        lastDistances = new float[suspensionPoints.Length];
        springStrength = minSpringStrength;
        dampSensitivity = minDampSensitivity;
        rb.inertiaTensor = minInertiaTensor;
    }

    private void Update()
    {
        SetIsOnSlope();
        ReadAndInterpolateInputs();       
        ManipulateMovementInCollision();
        SetState();
        DrawRays();       
        wheelAnimations.SetParameters(movement, rotation, input.y, input.x);
    }
    private void FixedUpdate()
    {
        RotateTank();       
        BrakeTank();
        if (isGrounded)
            ApplyMovement();       
        ApplySuspension();
    }
    private void LateUpdate()
    {
        RotateTurret();
        if (centeringTurret)
            CenterTurret();
    }
    private void ReadAndInterpolateInputs()
    {
        input = inputManager.playerInput.Player.Move.ReadValue<Vector2>();
        directionOrInput = input.y;
        mouseInput = inputManager.playerInput.Player.MoveTurretWithMouse.ReadValue<float>();
        turretRotationInput = inputManager.playerInput.Player.MoveTurretWithKeys.ReadValue<float>();

        SetMomentum();
        if (isGrounded)
        {
            rotation = Mathf.Clamp(Mathf.SmoothDamp(rotation, input.x, ref rotationRef, angularAccelerationTime), -1, 1);
            if (Mathf.Abs(rotation) < 0.01f) rotation = 0;
            
            float smoothTime = input.y != 0 ? accelerationTime : brakingTime;
            if (input.y != 0 && movement != 0 && Mathf.Sign(input.y) != Mathf.Sign(movement) && hasMomentum)
                smoothTime = 1;

            movement = Mathf.Clamp(Mathf.SmoothDamp(movement, input.y, ref movementRef, smoothTime), -1f, 1f);
            if (Mathf.Abs(movement) > 0.99f && input.y != 0 && Mathf.Sign(input.y) == Mathf.Sign(movement))
                movement = 1 * input.y;
        }
        else
        {
            rotation = Mathf.Clamp(Mathf.SmoothDamp(rotation, 0, ref rotationRef, angularAccelerationTime * 3), -1, 1);        
        }
        brakingTime = Mathf.Lerp(0.2f, 0.4f, Mathf.Abs(movement));
        if (Mathf.Abs(movement) < 0.01f)
            movement = 0;
    }
    public override void RotateTurret()
    {
        if (inputManager.playerInput.Player.MoveTurretWithKeys.enabled)
        {
            if (turretRotationInput != 0)
            {
                centeringTurret = false;
                turret.Rotate(0, turretRotationSpeed * turretRotationInput * Time.deltaTime, 0);               
            }
        }
        else if (inputManager.playerInput.Player.MoveTurretWithMouse.enabled && turret.rotation.eulerAngles.y != cameraPivotRotation)
        {
            float angleDifference = Mathf.DeltaAngle(turret.rotation.eulerAngles.y, cameraPivotRotation);
            float direction = Mathf.Sign(angleDifference);
            
            if (Mathf.Abs(angleDifference) > turretRotationSpeed * Time.deltaTime)
            {
                turret.Rotate(0, turretRotationSpeed * direction * Time.deltaTime, 0);
            }               
            else
            {
                turret.rotation = Quaternion.Euler(turret.rotation.eulerAngles.x, cameraPivotRotation, turret.rotation.eulerAngles.x);
                turret.localRotation = Quaternion.Euler(0, turret.localRotation.eulerAngles.y, 0);
            }
        }
    }

    public void ActivateTurretCenteringAndChangeTurretControlToKeys()
    {
        if (GameManager.instance.isTheGamePaused) return;
        centeringTurret = true;
        if (inputManager.playerInput.Player.MoveTurretWithMouse.enabled)
        {
            inputManager.playerInput.Player.Fire.Disable();
            inputManager.playerInput.Player.MoveTurretWithMouse.Disable();
            inputManager.playerInput.Player.FireKeyOnly.Enable();
            inputManager.playerInput.Player.SwitchTurretControlToMouse.Enable();
            inputManager.playerInput.Player.MoveTurretWithKeys.Enable();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
    public void SwitchTurretControlToMouse()
    {
        if (GameManager.instance.isTheGamePaused) return;
        if (inputManager.playerInput.Player.MoveTurretWithKeys.enabled)
        {
            inputManager.playerInput.Player.FireKeyOnly.Disable();
            inputManager.playerInput.Player.MoveTurretWithKeys.Disable();
            inputManager.playerInput.Player.Fire.Enable();
            inputManager.playerInput.Player.SwitchTurretControlToMouse.Disable();
            inputManager.playerInput.Player.MoveTurretWithMouse.Enable();
            centeringTurret = false;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;           
        }
    }

    private void DrawRays()
    {       
        Vector3 flatForward = Vector3.ProjectOnPlane(transform.forward, normalGround).normalized;

        Vector3 origin1 = tankCollider.ClosestPoint(transform.position + transform.right * 0.25f + (flatForward * 1.5f)) - transform.forward * 0.1f;
        Vector3 origin2 = tankCollider.ClosestPoint(transform.position - transform.right * 0.25f + (flatForward * 1.5f)) - transform.forward * 0.1f;
        Vector3 origin3 = tankCollider.ClosestPoint(transform.position + transform.right * 0.25f - (flatForward * 1.5f)) + transform.forward * 0.1f;
        Vector3 origin4 = tankCollider.ClosestPoint(transform.position - transform.right * 0.25f - (flatForward * 1.5f)) + transform.forward * 0.1f;

        Debug.DrawRay(origin1, flatForward * raycastDistance, Color.red);
        Debug.DrawRay(origin2, flatForward * raycastDistance, Color.red);
        Debug.DrawRay(origin3, -flatForward * raycastDistance, Color.red);
        Debug.DrawRay(origin4, -flatForward * raycastDistance, Color.red);
    }

    void OnDrawGizmos()
    {
        if (rb != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(rb.worldCenterOfMass, 0.1f);
        }
    }
}

