using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;
using UnityEngine.Windows.Speech;
using static UnityEngine.GraphicsBuffer;


public class PlayerController : MonoBehaviour
{
    private PlayerInput playerInput;
    private Rigidbody rb;
    public Vector2 input;
    private float turretRotationInput;

    [SerializeField] private List <WheelCollider> frontAxle;
    [SerializeField] private List <WheelCollider> backAxel;
    [SerializeField] private List<Array> suspensionOrder;
    [SerializeField] private Transform turret;
    [SerializeField] private float speed;
    [SerializeField] private float turretRotationSpeed;
    [SerializeField] private float tankRotationSpeed;
    [SerializeField] private float accelerationTime;
    [SerializeField] private float stabilizationTime;


    private WheelCollider referenceWheel;
    public float movement;
    private float movementSpeed;
    public bool accelerating;
    public bool constantSpeed;
    public bool stabilizedSuspension;
    public bool quiet;
    public bool braking;


    private void Awake()
    {
        playerInput = new PlayerInput();
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        //suspensionOrder.Add(frontAxle); 
        //suspensionOrder.Add(backAxel);
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
        foreach (WheelCollider collider in backAxel)
        {
            collider.brakeTorque = 100000000;
        }
    }

    private void FixedUpdate()
    {
        ApplyMovement();

        accelerating = Mathf.Abs(movement) > 0.01f && Mathf.Abs(movement) < 0.99f && input.y != 0;
        braking = Mathf.Abs(movement) > 0.01f && Mathf.Abs(movement) < 0.99f && input.y == 0;
        constantSpeed = Mathf.Abs(movement) > 0.99;       
        quiet = !accelerating && !constantSpeed && !braking;
        stabilizedSuspension = backAxel[0].suspensionSpring.targetPosition != 0 && frontAxle[0].suspensionSpring.targetPosition != 0;

        if (accelerating)
        {
            if (movement > 0)
                ApplySuspension(backAxel, frontAxle);
            else
                ApplySuspension(frontAxle, backAxel);
        }
        if (braking)
        {
            if (movement > 0)
                ApplySuspension(frontAxle, backAxel);
            else
                ApplySuspension(backAxel, frontAxle);
        }
        if ((constantSpeed || quiet) && !stabilizedSuspension && !braking)
        {
            if (frontAxle[0].suspensionSpring.targetPosition != 0)
                StartCoroutine(BalanceSuspension(frontAxle));
            if (backAxel[0].suspensionSpring.targetPosition !=0)
                StartCoroutine(BalanceSuspension(backAxel));
            
        }
    }

    private void ApplyMovement()
    {
        Vector3 velocity = rb.linearVelocity;
        velocity.x = movement * speed * transform.forward.x;
        velocity.z = movement * speed * transform.forward.z;
        rb.linearVelocity = velocity;
    }

    private void ApplySuspension(List<WheelCollider> wheelColliders1, List<WheelCollider> wheelColliders2)
    {
        foreach (WheelCollider collider in wheelColliders1)
        {
            JointSpring suspension = collider.suspensionSpring;
            suspension.targetPosition = 1;
            collider.suspensionSpring = suspension;
        }
        foreach (WheelCollider collider in wheelColliders2)
        {
            JointSpring suspension = collider.suspensionSpring;
            suspension.targetPosition = 0;
            collider.suspensionSpring = suspension;
        }  

    }

    private IEnumerator BalanceSuspension(List<WheelCollider> wheelColliders)
    {
        float startTime = Time.time;
        float initialTargetPosition = wheelColliders[0].suspensionSpring.targetPosition;
        Debug.Log("balanciando suspension");
        while ((Time.time - startTime) < stabilizationTime && (constantSpeed || quiet))
        {
            float t = Mathf.Clamp01((Time.time - startTime) / stabilizationTime);
            foreach (WheelCollider collider in wheelColliders)
            {
                JointSpring suspension = collider.suspensionSpring;
                suspension.targetPosition = Mathf.Lerp(initialTargetPosition, 0, t);                               
                collider.suspensionSpring = suspension;               
            }         
            yield return null;
        }
        foreach (WheelCollider collider in wheelColliders)
        {
            JointSpring suspension = collider.suspensionSpring;
            suspension.targetPosition = 0;
            collider.suspensionSpring = suspension;
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

