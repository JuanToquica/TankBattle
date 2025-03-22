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
    public Vector2 input;
    private float turretRotationInput;
    public event Action<float> StateUpdated;


    [SerializeField] private Transform turret;
    [SerializeField] private Transform superStructure;
    [SerializeField] private float speed;
    [SerializeField] private float turretRotationSpeed;
    [SerializeField] private float tankRotationSpeed;
    [SerializeField] private float accelerationTime;
    [SerializeField] private float suspensionRotation;

    [SerializeField] private float suspensionDuration;
    [SerializeField] private float balanceDuration;
    [SerializeField] private Ease AccelerationAndBrakingEaseType;
    [SerializeField] private Ease balanceEaseType;


    public float movement;
    private float movementSpeed;
    private Tween suspensionTween;
    [SerializeField] private State _currentState;

    public State currentState
    {
        get => _currentState;
        set
        {
            if (_currentState != value) 
            {
                _currentState = value;
                if (currentState == State.accelerating)
                    StateUpdated?.Invoke(input.y * 1);
                else if (currentState == State.braking)
                    StateUpdated?.Invoke(Mathf.Sign(movement) * -1);
            }           
        }
    }



    private void Awake() => playerInput = new PlayerInput();

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        StateUpdated += ApplySuspension;
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
    }

    private void FixedUpdate()
    {
        ApplyMovement();

        SetState();

    }

    private void SetState()
    {
        if (Mathf.Abs(movement) > 0.01f && Mathf.Abs(movement) < 0.99f && input.y != 0)
            currentState = State.accelerating;
        else if (Mathf.Abs(movement) > 0.01f && Mathf.Abs(movement) < 0.99f && input.y == 0)
            currentState = State.braking;
        else if (Mathf.Abs(movement) > 0.99)
            currentState = State.constantSpeed;
        else
            currentState = State.quiet;
    }

    private void ApplyMovement()
    {
        Vector3 velocity = rb.linearVelocity;
        velocity.x = movement * speed * transform.forward.x;
        velocity.z = movement * speed * transform.forward.z;
        rb.linearVelocity = velocity;
    }   
   
    private void ApplySuspension(float direction)
    {
        suspensionTween.Kill();
        suspensionTween = superStructure.DOLocalRotate(new Vector3(suspensionRotation * direction, 0, 0), suspensionDuration).SetEase(AccelerationAndBrakingEaseType).OnComplete(BalanceSuspension);
    }

    private void BalanceSuspension()
    {
        suspensionTween = superStructure.DOLocalRotate(new Vector3(0, 0, 0), balanceDuration).SetEase(balanceEaseType);
    }



    private void RotateTank()
    {
        transform.Rotate(0, tankRotationSpeed * input.x * Time.deltaTime, 0);
    }

    private void RotateTurret()
    {
        turret.Rotate(0, turretRotationSpeed * turretRotationInput * Time.deltaTime, 0);
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

