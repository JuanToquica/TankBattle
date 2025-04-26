using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : TankBase
{
    public PlayerInput playerInput;
    private WheelAnimations wheelAnimations;
    private PlayerAttack playerAttack;
    private Vector2 input;
    private float turretRotationInput;

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
        playerInput.Player.MoveTurretWithMouse.Disable();
        playerInput.Player.Fire.Disable();

        currentRotationSpeed = tankRotationSpeed;      
    }

    private void Update()
    {
        ReadAndInterpolateInputs();       
        ManipulateMovementInCollision(input.y);
        SetState();
        DrawRays();
        wheelAnimations.SetParameters(movement, rotation, input.y, input.x);
    }
    private void FixedUpdate()
    {
        ApplyMovement();
        RotateTurret();
        RotateTank();
        if (centeringTurret)
            CenterTurret();
    }

    private void ReadAndInterpolateInputs()
    {
        input = playerInput.Player.Move.ReadValue<Vector2>();
        turretRotationInput = playerInput.Player.MoveTurretWithKeys.ReadValue<float>();

        rotation = Mathf.Clamp(Mathf.SmoothDamp(rotation, input.x, ref tankRotationRef, angularAccelerationTime), -1, 1);
        if (Mathf.Abs(rotation) < 0.01) rotation = 0;

        SetMomentum(input.y);

        float smoothTime = input.y != 0 ? accelerationTime : brakingTime;
        if (input.y != 0 && Mathf.Sign(input.y) != Mathf.Sign(movement) && hasMomentum)
            smoothTime = 1;
        
        movement = Mathf.Clamp(Mathf.SmoothDamp(movement, input.y, ref movementRef, smoothTime), -1f, 1f);
        brakingTime = Mathf.Lerp(0.2f, 0.4f, Mathf.Abs(movement));
        if (Mathf.Abs(movement) < 0.01f) 
            movement = 0;
        if (Mathf.Abs(movement) > 0.99f && input.y != 0 && Mathf.Sign(input.y) == Mathf.Sign(movement)) 
            movement = 1 * input.y;
    }

    protected override void SetState()
    {
        bool hasSameDirection = Mathf.Sign(input.y) == Mathf.Sign(movement);
        bool hasVelocity = rb.linearVelocity.magnitude > 0.05f;
        float absMovement = Mathf.Abs(movement);

        if (absMovement > 0.01f && absMovement < 0.9f && input.y != 0 && hasSameDirection)
            currentState = State.accelerating;
        else if (absMovement > 0.01f && absMovement < 0.99f && hasVelocity && (input.y == 0 || !hasSameDirection))
            currentState = State.braking;
        else if (absMovement > 0.9)
            currentState = State.constantSpeed;
        else
            currentState = State.quiet;
    }

    protected override void RotateTank() => transform.Rotate(0, currentRotationSpeed * rotation * Time.fixedDeltaTime, 0);
    protected override void RotateTurret()
    {        
        if (playerInput.Player.MoveTurretWithKeys.enabled)  
        {
            if (turretRotationInput != 0)
            {
                centeringTurret = false;
                turret.Rotate(0, turretRotationSpeed * turretRotationInput * Time.fixedDeltaTime, 0);
            }
        }
        else if (playerInput.Player.MoveTurretWithMouse.enabled && turret.rotation.eulerAngles.y != cameraPivot.rotation.eulerAngles.y)
        {
            float angleDifference = Mathf.DeltaAngle(turret.rotation.eulerAngles.y, cameraPivot.rotation.eulerAngles.y);
            float direction = Mathf.Sign(angleDifference);
            if (Mathf.Abs(angleDifference) > turretRotationSpeed * Time.fixedDeltaTime)
                turret.Rotate(0, turretRotationSpeed * direction * Time.fixedDeltaTime, 0);
            else
            {
                turret.rotation = Quaternion.Euler(turret.rotation.eulerAngles.x, cameraPivot.eulerAngles.y, turret.rotation.eulerAngles.x);
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
        Debug.DrawRay(transform.position + transform.right * 0.3f, transform.forward * 2.1f, Color.red);
        Debug.DrawRay(transform.position + transform.right * -0.3f, transform.forward * 2.1f, Color.red);
        Debug.DrawRay(transform.position + transform.right * 0.3f, -transform.forward * 1.8f, Color.red);
        Debug.DrawRay(transform.position + transform.right * -0.3f, -transform.forward * 1.8f, Color.red);
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

