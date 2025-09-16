using DG.Tweening;
using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.HID;

public class PlayerController : TankBase
{
    private WheelAnimations wheelAnimations;   
    private Vector2 input;
    [HideInInspector] public float turretRotationInput;
    [HideInInspector] public float cameraPivotRotation;
    [SerializeField] private HUD hud;

    [Header ("Camera")]
    [SerializeField] private Transform cameraPivot;

    private void Awake()
    {
        tankCollider = GetComponent<BoxCollider>();
    }

    private void OnEnable()
    {
        dying = false;
        if (rb != null && rb.useGravity == false)
            rb.useGravity = true;
        OnTankOverturnedCoroutine = null;
        currentRotationSpeed = tankRotationSpeed;
        movement = 0;
        rotation = 0;
        lastDistances = new float[suspensionPoints.Length];
        springStrength = minSpringStrength;
        dampSensitivity = minDampSensitivity;
        turret.localRotation = Quaternion.identity;
        tankCollider.enabled = true;
        if (rb != null)
        {
            RestoreSpeed();
            rb.inertiaTensor = minInertiaTensor;
        }
        if (InputManager.Instance != null)
            InputManager.Instance.RegisterPlayerController(this);
        engineSource = GetComponent<AudioSource>();
        engineSource.Play();
        engineSource.pitch = pitchIdleLow;
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        wheelAnimations = GetComponent<WheelAnimations>();   
        InputManager.Instance.playerInput.actions["MoveTurretWithMouse"].Disable();
        InputManager.Instance.playerInput.actions["Shoot1"].Disable();
        InputManager.Instance.RegisterPlayerController(this);

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
        if (!dying)
            ReadAndInterpolateInputs();       
        ManipulateMovementInCollision();
        SetState();
        SetPitch();
        DrawRays();       
        wheelAnimations.SetParameters(movement, rotation, input.y, input.x);
        if (Vector3.Dot(transform.up, Vector3.up) < 0.4f && OnTankOverturnedCoroutine == null)
        {
            OnTankOverturnedCoroutine = StartCoroutine(OnTankOverturned());
        }
            
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
        input = InputManager.Instance.moveInput;
        directionOrInput = input.y;
        turretRotationInput = InputManager.Instance.turretInput;

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
    public void RotateTurret()
    {
        if (InputManager.Instance.playerInput.actions["MoveTurretWithKeys"].enabled)
        {
            if (turretRotationInput != 0)
            {
                centeringTurret = false;
                turret.Rotate(0, turretRotationSpeed * turretRotationInput * Time.deltaTime, 0);               
            }
        }
        else if (InputManager.Instance.playerInput.actions["MoveTurretWithMouse"].enabled && turret.rotation.eulerAngles.y != cameraPivotRotation)
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
        centeringTurret = true;
        if (InputManager.Instance.playerInput.actions["MoveTurretWithMouse"].enabled)
        {
            InputManager.Instance.playerInput.actions["Shoot1"].Disable();
            InputManager.Instance.playerInput.actions["MoveTurretWithMouse"].Disable();
            InputManager.Instance.playerInput.actions["SwitchTurretControlToMouse"].Enable();
            InputManager.Instance.playerInput.actions["MoveTurretWithKeys"].Enable();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
    public void SwitchTurretControlToMouse()
    {
        if (InputManager.Instance.playerInput.actions["MoveTurretWithKeys"].enabled)
        {
            InputManager.Instance.playerInput.actions["MoveTurretWithKeys"].Disable();
            InputManager.Instance.playerInput.actions["Shoot1"].Enable();
            InputManager.Instance.playerInput.actions["SwitchTurretControlToMouse"].Disable();
            InputManager.Instance.playerInput.actions["MoveTurretWithMouse"].Enable();
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

    public override void SpeedPowerUp(float duration)
    {
        base.SpeedPowerUp(duration);
        hud.OnSpeedPowerUp(duration);
    }

    protected override void RestoreSpeed()
    {
        base.RestoreSpeed();
        hud.OnSpeedPowerUpDeactivated();
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

