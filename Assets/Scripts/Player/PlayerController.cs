using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows.Speech;

[System.Serializable]
public class infoTrackWheels
{
    public WheelCollider backWheel;
    public WheelCollider frontWheel;
}

public class PlayerController : MonoBehaviour
{
    private PlayerInput playerInput;
    private Rigidbody rb;
    private Vector2 input;

    public List<infoTrackWheels> trackWheel;

    [SerializeField] private float maxBreakForce;
    [SerializeField] private float maxSpeed;
    private float breakForce;
    private float tankMass;
    public float torque;
    private float inertia;
    public Vector3 velocity;

    private void Awake()
    {
        playerInput = new PlayerInput();
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        tankMass = rb.mass + (trackWheel[0].backWheel.mass * 4);
        inertia = 0.5f * (tankMass / 4) * Mathf.Pow(trackWheel[0].backWheel.radius, 2f);
    }

    private void Update()
    {
        input = playerInput.Player.Move.ReadValue<Vector2>();
    }

    private void FixedUpdate()
    {
        float currentSpeed = Vector3.Dot(rb.linearVelocity, transform.forward);
        float targetSpeed = maxSpeed * input.y;
        float speedDifference = targetSpeed - currentSpeed;

        float angularVelocity = speedDifference / trackWheel[0].backWheel.radius;
        torque = (inertia * angularVelocity / 0.1f);

        if (Mathf.Abs(input.y) < 0.1f)
        {
            breakForce = maxBreakForce;
            torque = 0;
        }
        else
        {
            breakForce = 0;
        }

        foreach (infoTrackWheels eje in trackWheel)
        {
            eje.backWheel.motorTorque = torque;
            eje.backWheel.brakeTorque = breakForce;

            eje.frontWheel.motorTorque = torque;
            eje.frontWheel.brakeTorque = breakForce;
            velocity = rb.linearVelocity;
        }


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
