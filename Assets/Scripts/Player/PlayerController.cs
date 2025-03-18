using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows.Speech;

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
    private Vector2 input;

    [SerializeField] private List<InfoWheelAxle> trackWheel;
    [SerializeField] private float speed;


    private WheelCollider referenceWheel;

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
    }

    private void FixedUpdate()
    {
        rb.angularVelocity = input.y * speed * transform.forward;

        ApplyTorqueAndBrake();
    }


    private void ApplyTorqueAndBrake()
    {
        foreach (InfoWheelAxle eje in trackWheel)
        {
            //eje.backWheel.motorTorque = ;
            //eje.backWheel.brakeTorque = ;

            //eje.frontWheel.motorTorque = ;
            //eje.frontWheel.brakeTorque = ;
        }
    }

    private void RotateTank()
    {

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
