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

    [SerializeField] private List<infoTrackWheels> trackWheel;
    [SerializeField] private float maxBreakForce;
    [SerializeField] private float maxSpeed; 
    [SerializeField] private float waitTimeToChangeDirection;

    

    private float breakForce;
    private float tankMass;
    private float torque;
    private float inertia;
    private bool needToBrake;
    private WheelCollider referenceWheel;

    private void Awake()
    {
        playerInput = new PlayerInput();
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        referenceWheel = trackWheel[0].backWheel;
        tankMass = rb.mass + (referenceWheel.mass * 4);
        inertia = 0.5f * (tankMass / 4) * Mathf.Pow(referenceWheel.radius, 2f);
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
        float angularVelocity = speedDifference / referenceWheel.radius;
        torque = (inertia * angularVelocity / 0.1f);

        if (Mathf.Abs(rb.linearVelocity.z) > 0.1f && (Mathf.Sign(input.y) != Mathf.Sign(rb.linearVelocity.z))) //Verifica cambios bruscos de direccion
            if (!needToBrake) StartCoroutine(BrakeBeforeChangeDirection());


        AdjustBraking();

        ApplyTorqueAndBrake();
    }


    private void ApplyTorqueAndBrake()
    {
        foreach (infoTrackWheels eje in trackWheel)
        {
            eje.backWheel.motorTorque = torque;
            eje.backWheel.brakeTorque = breakForce;

            eje.frontWheel.motorTorque = torque;
            eje.frontWheel.brakeTorque = breakForce;
        }
    }

    private void AdjustBraking()
    {
        breakForce = (Mathf.Abs(input.y) < 0.1f || needToBrake) ? maxBreakForce : 0;
        if (breakForce > 0) torque = 0;

    }

    private IEnumerator BrakeBeforeChangeDirection()
    {
        needToBrake = true;
        yield return new WaitForSeconds(waitTimeToChangeDirection);
        needToBrake = false;
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
