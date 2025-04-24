using DG.Tweening;
using NUnit.Framework.Constraints;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Windows;
using UnityEngine.Windows.Speech;

public enum State{accelerating, braking, quiet, constantSpeed }

public class PlayerController : TankBase
{
    [HideInInspector] public PlayerInput playerInput;
    public Vector2 input;
    public State _currentState;
    private PlayerAttack playerAttack;
    private float turretRotationInput;

    [Header("References")]
    [SerializeField] private Transform turret;
    [SerializeField] private Transform superStructure;
    [SerializeField] private Transform cameraPivot;
    [SerializeField] private CameraController cameraController;

    [Header("Movement")]
    [SerializeField] private float speed;
    [SerializeField] private float tankRotationSpeed;
    [SerializeField] private float turretRotationSpeed;  
    [SerializeField] private float accelerationTime;
    [SerializeField] private float angularAccelerationTime;
    private float movementRef;
    private float tankRotationRef;
    private bool centeringTurret;

    [Header("Suspension")]
    [SerializeField] private float accelerationSuspensionRotation;
    [SerializeField] private float brakingSuspensionRotation;
    [SerializeField] private float balanceDuration;
    [SerializeField] private float regainDuration;
    private Sequence suspensionRotationSequence;
    

    public State currentState
    {
        get => _currentState;
        set
        {
            if (_currentState != value)
            {
                _currentState = value;
                if (_currentState == State.braking || _currentState == State.accelerating)
                    ApplySuspension();
            }
        }
    }
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
        playerInput.Player.MoveTurretWithMouse.Disable();
        playerInput.Player.Fire.Disable();

        base.maxTankRotationSpeed = tankRotationSpeed;
        base.currentRotationSpeed = tankRotationSpeed;
        base.TankSpeed = speed;
    }

    private void Update()
    {
        ReadAndInterpolateInputs();       
        ManipulateMovementInCollision();
        SetState();
        DrawRays();
    }

    private void ReadAndInterpolateInputs()
    {
        input = playerInput.Player.Move.ReadValue<Vector2>();
        turretRotationInput = playerInput.Player.MoveTurretWithKeys.ReadValue<float>();

        movement = Mathf.Clamp(Mathf.SmoothDamp(movement, input.y, ref movementRef, accelerationTime), -1, 1);
        if (Mathf.Abs(movement) < 0.01) movement = 0;

        rotation = Mathf.Clamp(Mathf.SmoothDamp(rotation, input.x, ref tankRotationRef, angularAccelerationTime), -1, 1);
        if (Mathf.Abs(rotation) < 0.01) rotation = 0;
    }

    private void ManipulateMovementInCollision()
    {
        if (movement > 0 && input.y < 0 && (frontalCollision || frontalCollisionWithCorner))
            movement = 0;

        if (movement < 0 && input.y > 0 && (backCollision || backCollisionWithCorner))
            movement = 0;     
    }

    private void FixedUpdate()
    {
        ApplyMovement();
        RotateTurret();
        RotateTank();
    
        if (centeringTurret)
            CenterTurret();
    }

    private void SetState()
    {
        if (Mathf.Abs(movement) > 0.01f && Mathf.Abs(movement) < 0.9f && input.y != 0)
            currentState = State.accelerating;
        else if (Mathf.Abs(movement) > 0.01f && Mathf.Abs(movement) < 0.99f && input.y == 0 && rb.linearVelocity.magnitude > 0.05f)
            currentState = State.braking;
        else if (Mathf.Abs(movement) > 0.9)
            currentState = State.constantSpeed;
        else
            currentState = State.quiet;
    }


    private void ApplySuspension()
    {
        if (suspensionRotationSequence != null && suspensionRotationSequence.IsActive())
            suspensionRotationSequence.Kill();

        suspensionRotationSequence = DOTween.Sequence();     

        if (currentState == State.accelerating)
        {
            if (movement > 0 && frontalCollision || movement < 0 && backCollision)
                return;
            suspensionRotationSequence.Append(superStructure.DOLocalRotate(new Vector3(accelerationSuspensionRotation * input.y * -1, 0, 0),
            accelerationTime).SetEase(Mathf.Abs(movement) > 0.2f ? Ease.OutCubic : Ease.InOutQuad));

            suspensionRotationSequence.Append(superStructure.DOLocalRotate(Vector3.zero, 2).SetEase(Ease.InSine));
        }

        if (currentState == State.braking)
        {
            float percentage = MathF.Abs(movement) > 0.9 ? 1 : MathF.Abs(movement);
            suspensionRotationSequence.Append(superStructure.DOLocalRotate(new Vector3(brakingSuspensionRotation * Mathf.Sign(movement) * (percentage - 0.2f), 0, 0), 
                accelerationTime + 0.1f).SetEase(Ease.OutQuad));

            suspensionRotationSequence.Append(superStructure.DOLocalRotate(new Vector3(-1.5f * Mathf.Sign(movement), 0, 0), balanceDuration).SetEase(Ease.OutQuad));
            suspensionRotationSequence.Append(superStructure.DOLocalRotate(Vector3.zero, regainDuration).SetEase(Ease.InSine));
        }
    }

    private void RotateTank() => transform.Rotate(0, currentRotationSpeed * rotation * Time.fixedDeltaTime, 0);

    private void RotateTurret()
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

    private void CenterTurret()
    {
        turret.transform.localRotation = Quaternion.RotateTowards(turret.transform.localRotation, Quaternion.identity, turretRotationSpeed * Time.deltaTime);
        if (Quaternion.Angle(turret.transform.localRotation, Quaternion.identity) < 0.1f)
        {
            turret.transform.localRotation = Quaternion.Euler(0, 0, 0);
            centeringTurret = false;
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

