using DG.Tweening;
using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : TankBase
{
    public PlayerInput playerInput;
    private WheelAnimations wheelAnimations;
    private PlayerAttack playerAttack;
    private Vector2 input;
    public float turretRotationInput;
    public float mouseInput;
    public float cameraPivotRotation;

    [Header ("Camera")]
    [SerializeField] private Transform cameraPivot;
    [SerializeField] private CameraController cameraController;

    private void Awake()
    {
        playerInput = new PlayerInput();

        playerInput.Player.CenterTurret.started += ctx => ActivateTurretCenteringAndChangeTurretControlToKeys();
        playerInput.Player.Fire.started += ctx => playerAttack.Fire();
        playerInput.Player.FireKeyOnly.started += ctx => playerAttack.Fire();
        playerInput.Player.SwitchTurretControlToMouse.started += ctx => SwitchTurretControlToMouse();
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerAttack = GetComponent<PlayerAttack>();
        wheelAnimations = GetComponent<WheelAnimations>();
        tankCollider = GetComponent<BoxCollider>();
        playerInput.Player.MoveTurretWithMouse.Disable();
        playerInput.Player.Fire.Disable();

        RestoreSpeed();
        currentRotationSpeed = tankRotationSpeed;
        lastDistances = new float[suspensionPoints.Length];
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
        input = playerInput.Player.Move.ReadValue<Vector2>();
        directionOrInput = input.y;
        mouseInput = playerInput.Player.MoveTurretWithMouse.ReadValue<float>();
        turretRotationInput = playerInput.Player.MoveTurretWithKeys.ReadValue<float>();

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
        if (playerInput.Player.MoveTurretWithKeys.enabled)
        {
            if (turretRotationInput != 0)
            {
                centeringTurret = false;
                turret.Rotate(0, turretRotationSpeed * turretRotationInput * Time.deltaTime, 0);               
            }
        }
        else if (playerInput.Player.MoveTurretWithMouse.enabled && turret.rotation.eulerAngles.y != cameraPivotRotation)
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

    private void ActivateTurretCenteringAndChangeTurretControlToKeys()
    {
        centeringTurret = true;
        if (playerInput.Player.MoveTurretWithMouse.enabled)
        {
            playerInput.Player.Fire.Disable();
            playerInput.Player.MoveTurretWithMouse.Disable();
            playerInput.Player.FireKeyOnly.Enable();            
            playerInput.Player.SwitchTurretControlToMouse.Enable();
            playerInput.Player.MoveTurretWithKeys.Enable();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
    private void SwitchTurretControlToMouse()
    {
        if (playerInput.Player.MoveTurretWithKeys.enabled)
        {
            playerInput.Player.FireKeyOnly.Disable();
            playerInput.Player.MoveTurretWithKeys.Disable();
            playerInput.Player.Fire.Enable();
            playerInput.Player.SwitchTurretControlToMouse.Disable();           
            playerInput.Player.MoveTurretWithMouse.Enable();
            centeringTurret = false;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;           
        }
    }

    private void DrawRays()
    {       
        Vector3 flatForward = Vector3.ProjectOnPlane(transform.forward, normalGround).normalized;

        Vector3 origin1 = tankCollider.ClosestPoint(transform.position + transform.right * 0.3f + (flatForward * 1.5f));
        Vector3 origin2 = tankCollider.ClosestPoint(transform.position - transform.right * 0.3f + (flatForward * 1.5f));
        Vector3 origin3 = tankCollider.ClosestPoint(transform.position + transform.right * 0.3f - (flatForward * 1.5f));
        Vector3 origin4 = tankCollider.ClosestPoint(transform.position - transform.right * 0.3f - (flatForward * 1.5f));

        Debug.DrawRay(origin1, flatForward * raycastDistance, Color.red);
        Debug.DrawRay(origin2, flatForward * raycastDistance, Color.red);
        Debug.DrawRay(origin3, -flatForward * raycastDistance, Color.red);
        Debug.DrawRay(origin4, -flatForward * raycastDistance, Color.red);
    }

    private void OnEnable() => playerInput.Enable();
    private void OnDisable() => playerInput.Disable();
    void OnDrawGizmos()
    {
        if (rb != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(rb.worldCenterOfMass, 0.1f);
        }
    }
}

