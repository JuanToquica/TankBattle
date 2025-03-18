using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;
using UnityEngine.Windows.Speech;
using static UnityEngine.GraphicsBuffer;

[System.Serializable]
public class InfoWheelAxle
{
    public WheelCollider backWheel;
    public WheelCollider frontWheel;
}

public class PlayerController : MonoBehaviour
{
    private PlayerInput playerInput;
    private Rigidbody rb;
    public Vector2 input;
    private float turretRotationInput;

    [SerializeField] private List<InfoWheelAxle> wheelAxle;
    [SerializeField] private Transform turret;
    [SerializeField] private float speed;
    [SerializeField] private float turretRotationSpeed;
    [SerializeField] private float tankRotationSpeed;
    [SerializeField] private float accelerationTime;


    private WheelCollider referenceWheel;
    private float movement;
    private float movementSpeed;
    

    private void Awake()
    {
        playerInput = new PlayerInput();
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        input = playerInput.Player.Move.ReadValue<Vector2>();
        turretRotationInput = playerInput.Player.MoveTurret.ReadValue<float>();

        movement = Mathf.SmoothDamp(movement, input.y, ref movementSpeed, accelerationTime);

        if (turretRotationInput != 0)
            RotateTurret();
        if (input.x != 0)
            RotateTank();
    }

    private void FixedUpdate()
    {
        Vector3 velocity = rb.linearVelocity;
        velocity.x = movement * speed * transform.forward.x;
        velocity.z = movement * speed * transform.forward.z;
        rb.linearVelocity = velocity;

        //ApplyTorqueAndBrake();
    }


    private void ApplyTorqueAndBrake()
    {
        foreach (InfoWheelAxle eje in wheelAxle)
        {
            //eje.backWheel.motorTorque = ;
            //eje.backWheel.brakeTorque = ;

            //eje.frontWheel.motorTorque = ;
            //eje.frontWheel.brakeTorque = ;
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

    private void OnEnable()
    {
        playerInput.Enable();
    }

    private void OnDisable()
    {
        playerInput.Disable();
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

