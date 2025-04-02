using DG.Tweening;
using NUnit.Framework.Constraints;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;
using UnityEngine.Windows.Speech;
using static UnityEngine.GraphicsBuffer;

public enum State{accelerating, braking, quiet, constantSpeed }

public class PlayerController : MonoBehaviour
{
    [HideInInspector] public PlayerInput playerInput;
    public Vector2 input;
    public State _currentState;
    private PlayerAttack playerAttack;
    private Rigidbody rb;
    private float turretRotationInput;

    [Header("References")]
    [SerializeField] private Transform turret;
    [SerializeField] private Transform superStructure;
    [SerializeField] private Transform cameraPivot;
    [SerializeField] private CameraController cameraController;

    [Header("Movement")]
    [SerializeField] private float speed;
    [SerializeField] private float turretRotationSpeed;
    [SerializeField] private float maxTankRotationSpeed;
    [SerializeField] private float accelerationTime;
    public float movement;
    private float movementRef;

    [Header("Suspension")]
    [SerializeField] private float accelerationSuspensionRotation;
    [SerializeField] private float brakingSuspensionRotation;
    [SerializeField] private float balanceDuration;
    [SerializeField] private float regainDuration;
    private Sequence suspensionRotationSequence;
    private Tween turretRotationTween;



    public bool frontalCollision;
    public bool backCollision;
    public bool frontalCollisionWithCorner;
    public bool backCollisionWithCorner;
    private bool centeringTurret;
    public float tankRotationSpeed;


    public State currentState
    {
        get => _currentState;
        set
        {
            if (_currentState != value)
            {
                _currentState = value;
                ManageSuspension();
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
        tankRotationSpeed = maxTankRotationSpeed;
        playerInput.Player.MoveTurretWithMouse.Disable();
        playerInput.Player.Fire.Disable();
    }

    private void Update()
    {
        ReadInputs();
        movement = Mathf.Clamp(Mathf.SmoothDamp(movement, input.y, ref movementRef, accelerationTime), -1, 1);
        if (Mathf.Abs(movement) < 0.01) movement = 0;

        ManipulateMovementInCollision();
        SetState();
        DrawRays();
    }

    private void ReadInputs()
    {
        input = playerInput.Player.Move.ReadValue<Vector2>();
        turretRotationInput = playerInput.Player.MoveTurretWithKeys.ReadValue<float>();
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

        if (input.x != 0)
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

    private void ApplyMovement()
    { 
        Vector3 targetVelocity = transform.forward * movement * speed;
        Vector3 velocityChange = targetVelocity - new Vector3(rb.linearVelocity.x, 0 , rb.linearVelocity.z);
        velocityChange.y = 0;
            
        rb.AddForce(velocityChange, ForceMode.VelocityChange);
    }   
   
    private void ManageSuspension()
    {
        if (currentState == State.accelerating)
        {
            if (movement > 0 && frontalCollision || movement < 0 && backCollision)
                return;
            ApplySuspension(accelerationSuspensionRotation, 1, input.y * -1, Mathf.Abs(movement) > 0.2f? Ease.OutCubic : Ease.InOutQuad);
        }           
        else if (currentState == State.braking)
        {
            float percentage = MathF.Abs(movement) > 0.9 ? 1 : MathF.Abs(movement);
            ApplySuspension(brakingSuspensionRotation, percentage - 0.2f, Mathf.Sign(movement), Ease.OutQuad);
        }
    }

    private void ApplySuspension(float rotation, float percentage, float direction, Ease ease)
    {
        if (suspensionRotationSequence != null && suspensionRotationSequence.IsActive())
            suspensionRotationSequence.Kill();

        suspensionRotationSequence = DOTween.Sequence();
 
        rotation *= percentage;

        suspensionRotationSequence.Append(superStructure.DOLocalRotate(new Vector3(rotation * direction, 0, 0), 
            currentState == State.braking? accelerationTime + 0.1f: accelerationTime).SetEase(ease));

        if (currentState == State.accelerating || currentState == State.constantSpeed)
        {
            suspensionRotationSequence.Append(superStructure.DOLocalRotate(Vector3.zero, 2).SetEase(Ease.InSine));
        }            
        else if (currentState == State.braking)
        {
            suspensionRotationSequence.Append(superStructure.DOLocalRotate(new Vector3(-1.5f * direction, 0, 0), balanceDuration).SetEase(Ease.OutQuad));
            suspensionRotationSequence.Append(superStructure.DOLocalRotate(Vector3.zero, regainDuration).SetEase(Ease.InSine));
        }   
    }

    private void RotateTank() => transform.Rotate(0, tankRotationSpeed * input.x * Time.fixedDeltaTime, 0);

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
            if (Mathf.Abs(angleDifference) > turretRotationSpeed * Time.deltaTime)
                turret.Rotate(0, turretRotationSpeed * direction * Time.fixedDeltaTime, 0);
            else
                turret.rotation = Quaternion.Euler(turret.rotation.eulerAngles.x, cameraPivot.rotation.eulerAngles.y, turret.rotation.eulerAngles.z);
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


    private void OnCollisionStay(Collision collision)
    {
        bool rightFrontalCollision = Physics.Raycast(transform.position + transform.right * 0.3f, transform.forward, 2.3f);
        bool leftFrontalCollision = Physics.Raycast(transform.position + transform.right * -0.3f, transform.forward, 2.3f);
        bool rightBackCollision = Physics.Raycast(transform.position + transform.right * 0.3f, -transform.forward, 1.8f);
        bool leftBackCollision = Physics.Raycast(transform.position + transform.right * -0.3f, -transform.forward, 1.8f);

        foreach (ContactPoint contact in collision.contacts)
        {
            if (contact.thisCollider.GetComponent<BoxCollider>() != null)
            {
                frontalCollision = rightFrontalCollision || leftFrontalCollision;
                backCollision = rightBackCollision || leftBackCollision;               

                if ((rightFrontalCollision && leftFrontalCollision) || (rightBackCollision && leftBackCollision))
                {
                    tankRotationSpeed = 0;
                    transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
                    return;
                }

                Vector3 contactDirection = contact.point - transform.position;
                float dot = Vector3.Dot(contactDirection.normalized, transform.forward);
                if (dot > 0.75f)
                {
                    tankRotationSpeed = 30;
                    frontalCollisionWithCorner = true;                    
                }                                        
                if (dot < -0.75f)
                {
                    tankRotationSpeed = 30;
                    backCollisionWithCorner = true;                    
                }

                transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
            } 
        }
    }
    private void OnCollisionExit(Collision collision)
    {
        frontalCollision = false;
        frontalCollisionWithCorner = false;
        backCollision = false;
        backCollisionWithCorner = false;
        tankRotationSpeed = maxTankRotationSpeed;
    }

    private void DrawRays()
    {
        Debug.DrawRay(transform.position + transform.right * 0.3f, transform.forward * 2.3f, Color.red);
        Debug.DrawRay(transform.position + transform.right * -0.3f, transform.forward * 2.3f, Color.red);
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

