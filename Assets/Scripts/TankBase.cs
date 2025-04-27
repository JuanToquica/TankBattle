using DG.Tweening;
using System;
using UnityEngine;
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
    protected float brakingTime;
    protected float movement;
    protected float rotation;
    protected float movementRef;
    protected float rotationRef;
    protected float currentRotationSpeed;
    protected bool centeringTurret;
    public bool hasMomentum;
    protected bool frontalCollision;
    protected bool backCollision;
    protected bool frontalCollisionWithCorner;
    protected bool backCollisionWithCorner;

    [Header("Suspension")]
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
                    ApplySuspension();
            }
        }
    }

    protected void SetState(float directionOrInput)
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

    public abstract void RotateTurret();
    protected virtual void RotateTank()
    {
        transform.Rotate(0, rotation * currentRotationSpeed * Time.fixedDeltaTime, 0);
    }

    protected void SetMomentum(float directionOrInput)
    {
        if (Mathf.Abs(movement) > 0.7f && directionOrInput != 0)
            hasMomentum = true;
        else if (Mathf.Abs(movement) < 0.1f || directionOrInput == 0)
            hasMomentum = false;
    }

    protected void ManipulateMovementInCollision(float directionOrInput)
    {
        if (movement > 0 && directionOrInput < 0 && (frontalCollision || frontalCollisionWithCorner))
            movement = 0;

        if (movement < 0 && directionOrInput > 0 && (backCollision || backCollisionWithCorner))
            movement = 0;
    }

    protected void ApplyMovement()
    {
        Vector3 targetVelocity = transform.forward * movement * speed;
        Vector3 velocityChange = targetVelocity - new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        velocityChange.y = 0;

        rb.AddForce(velocityChange, ForceMode.VelocityChange);
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

    protected void ApplySuspension()
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
            float percentage = MathF.Abs(movement) > 0.9 ? 1 : MathF.Abs(movement) - 0.3f;

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

        foreach (ContactPoint contact in collision.contacts)
        {
            if (contact.thisCollider.GetComponent<BoxCollider>() != null && (!contact.otherCollider.transform.CompareTag("Floor")))
            {
                frontalCollision = rightFrontalCollision || leftFrontalCollision;
                backCollision = rightBackCollision || leftBackCollision;

                if ((rightFrontalCollision && leftFrontalCollision) || (rightBackCollision && leftBackCollision))
                {
                    currentRotationSpeed = 0;
                    if (contact.otherCollider.transform.CompareTag("Wall")) transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
                    return;
                }

                Vector3 contactDirection = contact.point - transform.position;
                float dot = Vector3.Dot(contactDirection.normalized, transform.forward);
                if (dot > 0.75f)
                {
                    currentRotationSpeed = 30;
                    frontalCollisionWithCorner = true;
                    if (contact.otherCollider.transform.CompareTag("Wall")) transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
                }
                if (dot < -0.75f)
                {
                    currentRotationSpeed = 30;
                    backCollisionWithCorner = true;
                    if (contact.otherCollider.transform.CompareTag("Wall")) transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
                }
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
