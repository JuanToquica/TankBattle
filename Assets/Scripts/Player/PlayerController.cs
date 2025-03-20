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

    [SerializeField] private WheelCollider[] frontAxle;
    [SerializeField] private WheelCollider[] backAxle;
    [SerializeField] private List<WheelCollider[]> suspensionOrder;
    [SerializeField] private Transform turret;
    [SerializeField] private float speed;
    [SerializeField] private float turretRotationSpeed;
    [SerializeField] private float tankRotationSpeed;
    [SerializeField] private float accelerationTime;
    [SerializeField] private float stabilizationTime;


    private WheelCollider referenceWheel;
    public float movement;
    private float movementSpeed;
    public bool stabilizedSuspension;
    public State currentState;
    public string ListPosition;

    public bool movingForward;
    public bool brakingForward;
    public bool stabilizeFirstFrontalAxisAtRest;



    private void Awake()
    {
        playerInput = new PlayerInput();
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        suspensionOrder = new List<WheelCollider[]>();
        suspensionOrder.Add(frontAxle); 
        suspensionOrder.Add(backAxle);
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

        Vector3 rotation = transform.eulerAngles;
        rotation.z = 0;
        transform.eulerAngles = rotation;

        foreach (WheelCollider collider in frontAxle)
        {
            collider.brakeTorque = 100000000;
        }
        foreach (WheelCollider collider in backAxle)
        {
            collider.brakeTorque = 100000000;
        }
        if (suspensionOrder[0] == frontAxle)
            ListPosition = "front, back";
        else
            ListPosition = "back, front";

    }

    private void FixedUpdate()
    {
        ApplyMovement();

        stabilizedSuspension = backAxle[0].suspensionSpring.targetPosition != 0 && frontAxle[0].suspensionSpring.targetPosition != 0;

        SetState();
        AdministerSuspensionOrder();
        switch (currentState)
        {
            case State.accelerating:
            case State.braking:
                ApplySuspension();
                break;
            case State.constantSpeed:
            case State.quiet:
                if(!stabilizedSuspension)
                    StartCoroutine(BalanceSuspension());
                break;
        }
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

    private void AdministerSuspensionOrder()
    {
        movingForward = movement > 0 && (currentState == State.accelerating || currentState == State.constantSpeed);
        brakingForward = currentState == State.braking && movement > 0;
        stabilizeFirstFrontalAxisAtRest = currentState == State.quiet && frontAxle[0].suspensionSpring.targetPosition > backAxle[0].suspensionSpring.targetPosition;
        if ((!movingForward && currentState != State.braking && currentState != State.quiet) || brakingForward || stabilizeFirstFrontalAxisAtRest )
        {
            if (suspensionOrder[0] != frontAxle)
                suspensionOrder.Reverse();  
        }
        else
        {
            if (suspensionOrder[0] != backAxle)
                suspensionOrder.Reverse();
        }
    }
    private void ApplySuspension()
    {
        int i = 1;
        foreach (WheelCollider[] array in suspensionOrder)
        {
            foreach (WheelCollider collider in array)
            {
                JointSpring suspension = collider.suspensionSpring;
                suspension.targetPosition = i;
                collider.suspensionSpring = suspension;
            }
            i--;
        }
    }

    private IEnumerator BalanceSuspension()
    {
        State initialState = currentState;
        foreach (WheelCollider[] array in suspensionOrder)
        {
            float startTime = Time.time;
            float initialTargetPosition = array[0].suspensionSpring.targetPosition;
            if (initialTargetPosition == 0)
                continue;

            while (array[0].suspensionSpring.targetPosition > 0 && (currentState == State.constantSpeed || currentState == State.quiet))
            {
                if (currentState != initialState)
                    break;
                float t = Mathf.Clamp01((Time.time - startTime) / stabilizationTime);
                if (t > 0.98f)
                    t = 1.0f;
                Debug.Log(t);
                foreach (WheelCollider collider in array)
                {
                    JointSpring suspension = collider.suspensionSpring;
                    suspension.targetPosition = Mathf.Lerp(initialTargetPosition, 0, t);
                    collider.suspensionSpring = suspension;
                    
                }
                yield return null;
            }
            if (currentState != initialState)
                break;
        }      
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

