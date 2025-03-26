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
    private PlayerInput playerInput;
    private Rigidbody rb;
    private Vector2 input;
    private float turretRotationInput;

    [Header("References")]
    [SerializeField] private Transform turret;
    [SerializeField] private Transform superStructure;

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
    private float movementSpeed;
    private Sequence suspensionSequence;
    private Tween turretRotationTween;
    private State _currentState;
    private bool isCollidingInFront;
    private bool isCollidingBack;
    public bool centeringTurret;
    private float tankRotationSpeed;

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
        playerInput.Player.CenterTurret.started += ctx => centeringTurret = true;
    }
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        tankRotationSpeed = maxTankRotationSpeed;
    }

    private void Update()
    {
        input = playerInput.Player.Move.ReadValue<Vector2>();
        turretRotationInput = playerInput.Player.MoveTurret.ReadValue<float>();

        movement = Mathf.Clamp(Mathf.SmoothDamp(movement, input.y, ref movementSpeed, accelerationTime), -1, 1);
        if (Mathf.Abs(movement) < 0.01)
            movement = 0;

        if (turretRotationInput != 0)
            RotateTurret();
        if (input.x != 0)
            RotateTank();
        SetState();

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

    private void FixedUpdate()
    {
        ApplyMovement();
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

    private void RotateTank() =>transform.Rotate(0, tankRotationSpeed * input.x * Time.deltaTime, 0);
    private void RotateTurret()
    {
        centeringTurret = false;
        turret.Rotate(0, turretRotationSpeed * turretRotationInput * Time.deltaTime, 0);
    }
        
    private void OnEnable() => playerInput.Enable();
    private void OnDisable() => playerInput.Disable();

    private void OnCollisionStay(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
            Vector3 contactDirection = contact.point - transform.position;
            float dot = Vector3.Dot(contactDirection.normalized, transform.forward);
            if (MathF.Abs(dot) > 0.9f)
            {
                tankRotationSpeed = 0;
                return;
            }                
            if (dot > 0.7f)
            {
                isCollidingInFront = true;
                tankRotationSpeed = 30;
            }               
            if (dot < -0.7f)
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

