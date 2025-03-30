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
    private PlayerAttack playerAttack;
    private Rigidbody rb;
    [HideInInspector] public Vector2 input;
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

    [Header("Suspension")]
    [SerializeField] private float accelerationSuspensionRotation;
    [SerializeField] private float brakingSuspensionRotation;
    [SerializeField] private float suspensionDuration;
    [SerializeField] private float balanceDuration;
    [SerializeField] private float regainDuration;

    private float movement;
    private float turretRotationFactor;
    private float movementSpeed;
    private Sequence suspensionSequence;
    private Tween turretRotationTween;
    private State _currentState;
    private bool isCollidingInFront;
    private bool isCollidingBack;
    public bool centeringTurret;
    private float tankRotationSpeed;
    public float ancho;
    public float largo;

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

        playerInput.Player.CenterTurret.started += ctx => CenterTurretAndChangeTurretControl();
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
        input = playerInput.Player.Move.ReadValue<Vector2>();
        turretRotationInput = playerInput.Player.MoveTurretWithKeys.ReadValue<float>();

        movement = Mathf.Clamp(Mathf.SmoothDamp(movement, input.y, ref movementSpeed, accelerationTime), -1, 1);

        if (Mathf.Abs(movement) < 0.01)
            movement = 0;

        SetState();       
        DetectFrontalCollision();
    }

    private void FixedUpdate()
    {
        ApplyMovement();
        RotateTurret();
     
        if (input.x != 0)
            RotateTank();
        if (centeringTurret)
        {
            turret.transform.localRotation = Quaternion.RotateTowards(turret.transform.localRotation, Quaternion.identity, turretRotationSpeed * Time.deltaTime);
            if (Quaternion.Angle(turret.transform.localRotation, Quaternion.identity) < 0.1f)
            {
                turret.transform.localRotation = Quaternion.Euler(0, 0, 0);
                centeringTurret = false;
            }
        }
    }

    private void SetState()
    {
        if (Mathf.Abs(movement) > 0.01f && Mathf.Abs(movement) < 0.9f && input.y != 0)
            currentState = State.accelerating;
        else if (Mathf.Abs(movement) > 0.01f && Mathf.Abs(movement) < 0.99f && input.y == 0)
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
            if (input.y > 0.1f && isCollidingInFront || input.y < 0.1f && isCollidingBack)
                return;
            ApplySuspension(accelerationSuspensionRotation, 1, input.y * -1, Mathf.Abs(movement) > 0.2f ? Ease.OutCubic : Ease.InOutQuad);
        }           
        else if (currentState == State.braking && !isCollidingBack && !isCollidingInFront)
        {
            float percentage = MathF.Abs(movement) > 0.9 ? 1 : MathF.Abs(movement);
            ApplySuspension(brakingSuspensionRotation, percentage - 0.2f, Mathf.Sign(movement), Ease.OutQuad);
        }
    }

    private void ApplySuspension(float rotation, float percentage, float direction, Ease ease)
    {
        if (suspensionSequence != null && suspensionSequence.IsActive())
            suspensionSequence.Kill();

        rotation *= percentage;

        suspensionSequence = DOTween.Sequence();
        suspensionSequence.Append(superStructure.DOLocalRotate(new Vector3(rotation * direction, 0, 0), currentState == State.constantSpeed? suspensionDuration - 0.5f: suspensionDuration).SetEase(ease));
        if (currentState == State.accelerating || currentState == State.constantSpeed)
            suspensionSequence.Append(superStructure.DOLocalRotate(Vector3.zero, 2).SetEase(Ease.InSine));
        else if (currentState == State.braking)
        {
            suspensionSequence.Append(superStructure.DOLocalRotate(new Vector3(-1.5f * direction, 0, 0), balanceDuration).SetEase(Ease.OutQuad));
            suspensionSequence.Append(superStructure.DOLocalRotate(Vector3.zero, regainDuration).SetEase(Ease.InSine));
        }
    }

    public void DetectFrontalCollision()
    {
        if (Physics.Raycast(transform.position + transform.right * ancho, transform.forward, largo) || 
            Physics.Raycast(transform.position + transform.right * -ancho, transform.forward, largo) ||
            Physics.Raycast(transform.position + transform.right * ancho, -transform.forward, largo) ||
            Physics.Raycast(transform.position + transform.right * -ancho, -transform.forward, largo))
        {
            tankRotationSpeed = 0;
            
        }
        else
        {
            tankRotationSpeed = maxTankRotationSpeed;
        }
            
        Debug.DrawRay(transform.position + transform.right * ancho, transform.forward * largo, Color.red);
        Debug.DrawRay(transform.position + transform.right * -ancho, transform.forward * largo, Color.red);
        Debug.DrawRay(transform.position + transform.right * ancho, -transform.forward * largo, Color.red);
        Debug.DrawRay(transform.position + transform.right * -ancho, -transform.forward * largo, Color.red);
    }

    private void RotateTank() =>transform.Rotate(0, tankRotationSpeed * input.x * Time.fixedDeltaTime, 0);
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

    private void CenterTurretAndChangeTurretControl()
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

    private void OnEnable() => playerInput.Enable();
    private void OnDisable() => playerInput.Disable();

    private void OnCollisionStay(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
            Vector3 contactDirection = contact.point - transform.position;
            float dot = Vector3.Dot(contactDirection.normalized, transform.forward);                
            if (dot > 0.75f && tankRotationSpeed != 0)
            {
                isCollidingInFront = true;
                tankRotationSpeed = 30;
            }               
            if (dot < -0.75f && tankRotationSpeed != 0)
            {
                isCollidingBack = true;
                tankRotationSpeed = 30;
            }           
        }
    }
    private void OnCollisionExit(Collision collision)
    {
        isCollidingInFront = false;
        isCollidingBack = false;
        tankRotationSpeed = maxTankRotationSpeed;
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

