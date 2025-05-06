using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Windows;

public enum State { accelerating, braking, quiet, constantSpeed }

public abstract class TankBase : MonoBehaviour
{
    protected Rigidbody rb;
    public State _currentState;    

    [Header("References")]
    [SerializeField] protected Transform superStructure;
    public Transform turret;
    

    [Header("Movement")]
    [SerializeField] protected float speed;
    [SerializeField] protected float tankRotationSpeed;
    [SerializeField] protected float turretRotationSpeed;
    [SerializeField] protected float accelerationTime;
    [SerializeField] protected float angularAccelerationTime;
    [SerializeField] protected float angularDampingInGround;
    [SerializeField] protected float angularDampingOutGround;
    protected float directionOrInput;
    protected float brakingTime;
    public float movement;
    protected float rotation;
    protected float movementRef;
    protected float rotationRef;
    protected float currentRotationSpeed;
    protected bool centeringTurret;
    public bool hasMomentum;
    public bool isGrounded;
    public bool frontalCollision;
    public bool backCollision;
    public bool frontalCollisionWithCorner;
    public bool backCollisionWithCorner;
    public bool isOnSlope;

    [Header("Suspension")]
    [SerializeField] protected Transform[] suspensionPoints;
    [SerializeField] protected float suspensionLenght;
    [SerializeField] protected float springStrength;
    [SerializeField] protected float dampSensitivity;
    protected float[] lastDistances;

    [Header("Suspension Animation")]
    [SerializeField] protected float suspensionRotation;
    [SerializeField] protected float balanceDuration;
    [SerializeField] protected float regainDuration;
    protected Sequence suspensionRotationSequence;

    public State currentState
    {
        get => _currentState;
        set
        {
            if (_currentState != value)
            {
                _currentState = value;
                if (_currentState == State.braking || _currentState == State.accelerating)
                    ApplySuspensionAnimation();
            }
        }
    }

    protected void SetState()
    {
        bool hasSameDirection = Mathf.Sign(directionOrInput) == Mathf.Sign(movement);
        bool hasVelocity = rb.linearVelocity.magnitude > 0.05f;
        float absMovement = Mathf.Abs(movement);

        if (absMovement > 0.01f && absMovement < 0.9f && directionOrInput != 0 && hasSameDirection)
            currentState = State.accelerating;
        else if (absMovement > 0.01f && absMovement < 0.99f && hasVelocity && (directionOrInput == 0 || !hasSameDirection))
            currentState = State.braking;
        else if (absMovement > 0.9)
            currentState = State.constantSpeed;
        else
            currentState = State.quiet;
    }
    protected void SetIsGrounded()
    {
        bool ray = Physics.Raycast(transform.position, -transform.up, out RaycastHit hit, 0.95f, 1 << 6);
        bool ray2 = Physics.Raycast(transform.position + transform.forward * 1, -transform.up, out RaycastHit hit2, 0.95f, 1 << 6);
        bool ray3 = Physics.Raycast(transform.position - transform.forward * 1, -transform.up, out RaycastHit hit3, 0.95f, 1 << 6);
        Debug.DrawRay(transform.position, -transform.up * 0.95f, Color.red);
        Debug.DrawRay(transform.position + transform.forward * 1, -transform.up * 0.95f, Color.red);
        Debug.DrawRay(transform.position - transform.forward * 1, -transform.up * 0.95f, Color.red);

        if ((ray && hit.collider.transform.CompareTag("Floor")) || (ray2 && hit2.collider.transform.CompareTag("Floor")) || (ray3 && hit3.collider.transform.CompareTag("Floor")))
            isGrounded = true;
        else
            isGrounded = false;
        //Setear AngularDamping
        if (isGrounded && (ray2 && hit2.collider.transform.CompareTag("Floor")) && (ray3 && hit3.collider.transform.CompareTag("Floor")))
            rb.angularDamping = angularDampingInGround;
        else
            rb.angularDamping = angularDampingOutGround;

    }

    public abstract void RotateTurret();
    protected virtual void RotateTank()
    {
        transform.Rotate(0, rotation * currentRotationSpeed * Time.fixedDeltaTime, 0);
    }

    protected void SetIsOnSlope()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 4f))
        {
            isOnSlope = Vector3.Angle(hit.normal, Vector3.up) > 5f;
        }
    }
    protected void BrakeTank()
    {
        if (isGrounded && Mathf.Abs(movement) < 0.01f && rb.linearVelocity.magnitude < 0.5f && isOnSlope)
        {
            rb.linearDamping = 50;
        }
        else
        {
            rb.linearDamping = 0.1f;
        }
    }

    protected void SetMomentum()
    {
        if ((Mathf.Abs(movement) > 0.7f && directionOrInput != 0) || Mathf.Abs(transform.rotation.eulerAngles.x) > 20)
            hasMomentum = true;
        else if (Mathf.Abs(movement) < 0.1f || directionOrInput == 0)
            hasMomentum = false;
    }

    protected void ManipulateMovementInCollision()
    {
        bool hasVelocity = rb.linearVelocity.magnitude > 0.1f;
        if (movement > 0 && directionOrInput < 0 && (frontalCollision || frontalCollisionWithCorner) && !hasVelocity)
            movement = 0;

        if (movement < 0 && directionOrInput > 0 && (backCollision || backCollisionWithCorner) && !hasVelocity)
            movement = 0;
    }

    protected void ApplyMovement()
    {
        Vector3 targetVelocity = transform.forward * movement * speed;
        Vector3 velocityChange = targetVelocity - new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        velocityChange.y = 0;

        rb.AddForce(velocityChange, ForceMode.VelocityChange);
    }

    protected void ApplySuspension()
    {
        for (int i = 0; i < suspensionPoints.Length; i++)
        {
            Transform point = suspensionPoints[i];
            bool ray = Physics.Raycast(point.position, -point.up, out RaycastHit hit, suspensionLenght);
            Debug.DrawRay(point.position, -point.up * suspensionLenght, Color.red);
            if (ray)
            {
                float compression = suspensionLenght - hit.distance;
                float springForce = compression * springStrength;
                float relativeVelocity = (lastDistances[i] - hit.distance) / Time.fixedDeltaTime;
                float dampingForce = relativeVelocity * dampSensitivity;
                float totalForce = springForce + dampingForce;

                rb.AddForceAtPosition(point.up * totalForce, point.position);
                lastDistances[i] = hit.distance;
            }
        }
    }
    protected void CenterTurret()
    {
        turret.transform.localRotation = Quaternion.RotateTowards(turret.transform.localRotation, Quaternion.identity, turretRotationSpeed * Time.deltaTime);
        if (Quaternion.Angle(turret.transform.localRotation, Quaternion.identity) < 0.1f)
        {
            turret.transform.localRotation = Quaternion.Euler(0, 0, 0);
            centeringTurret = false;
        }
    }

    protected void ApplySuspensionAnimation()
    {
        if (suspensionRotationSequence != null && suspensionRotationSequence.IsActive())
            suspensionRotationSequence.Kill();

        suspensionRotationSequence = DOTween.Sequence();

        if (currentState == State.accelerating)
        {
            if (movement > 0 && frontalCollision || movement < 0 && backCollision)
                return;
            suspensionRotationSequence.Append(superStructure.DOLocalRotate(new Vector3(suspensionRotation * Mathf.Sign(movement) * -1, 0, 0),
            accelerationTime).SetEase(Ease.InOutQuad));

            suspensionRotationSequence.Append(superStructure.DOLocalRotate(Vector3.zero, 2).SetEase(Ease.InSine));
        }

        if (currentState == State.braking)
        {
            float percentage = MathF.Abs(movement) > 0.9 ? 1 : Mathf.Abs(movement) - 0.3f;

            suspensionRotationSequence.Append(superStructure.DOLocalRotate(new Vector3(suspensionRotation * Mathf.Sign(movement) * percentage, 0, 0),
                accelerationTime).SetEase(Ease.OutQuad));
            suspensionRotationSequence.Append(superStructure.DOLocalRotate(new Vector3(-1.5f * Mathf.Sign(movement), 0, 0), balanceDuration).SetEase(Ease.InOutSine));
            suspensionRotationSequence.Append(superStructure.DOLocalRotate(Vector3.zero, regainDuration).SetEase(Ease.InSine));
        }
    }

    protected void OnCollisionStay(Collision collision)
    {
        bool rightFrontalCollision = Physics.Raycast(transform.position + transform.right * 0.3f, transform.forward, 2.3f);
        bool leftFrontalCollision = Physics.Raycast(transform.position + transform.right * -0.3f, transform.forward, 2.3f);
        bool rightBackCollision = Physics.Raycast(transform.position + transform.right * 0.3f, -transform.forward, 1.8f);
        bool leftBackCollision = Physics.Raycast(transform.position + transform.right * -0.3f, -transform.forward, 1.8f);

        frontalCollision = rightFrontalCollision || leftFrontalCollision;
        backCollision = rightBackCollision || leftBackCollision;

        if ((rightFrontalCollision && leftFrontalCollision) || (rightBackCollision && leftBackCollision))
        {
            currentRotationSpeed = 0;
            if (directionOrInput == 0)
                movement = 0;
            return;
        }
        foreach (ContactPoint contact in collision.contacts)
        {
            Vector3 contactDirection = contact.point - transform.position;
            float dot = Vector3.Dot(contactDirection.normalized, transform.forward);

            if (dot > 0.5f)
            {
                currentRotationSpeed = 30;
                frontalCollisionWithCorner = true;
            }
            if (dot < -0.5f)
            {
                currentRotationSpeed = 30;
                backCollisionWithCorner = true;
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        frontalCollision = false;
        frontalCollisionWithCorner = false;
        backCollision = false;
        backCollisionWithCorner = false;
        currentRotationSpeed = tankRotationSpeed;
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
