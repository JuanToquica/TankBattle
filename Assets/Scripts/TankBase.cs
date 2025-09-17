using DG.Tweening;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public enum State { accelerating, braking, quiet, constantSpeed }

public abstract class TankBase : MonoBehaviour
{
    protected Rigidbody rb;
    public State _currentState;

    [Header("References")]
    [SerializeField] protected TankConfig tankConfig;
    [SerializeField] protected Transform superStructure;
    [HideInInspector] public BoxCollider tankCollider;
    [SerializeField] protected ParticleSystem smokeVfx;
    public Transform turret;    
    [SerializeField] protected AudioSource engineSource;
    protected TankAudioController tankAudioController;


    [Header("Movement")]
    public Vector3 minInertiaTensor;
    public Vector3 maxInertiaTensor;
    [HideInInspector] public float movement;
    [HideInInspector] public float rotation;  
    protected float turretRotationRef;
    protected float currentRotationSpeed;
    protected bool centeringTurret;
    protected bool hasMomentum;
    protected bool isGrounded;
    protected bool frontalCollision;
    protected bool backCollision;
    protected bool frontalCollisionWithCorner;
    protected bool backCollisionWithCorner;
    protected bool isOnSlope;
    protected float speed;
    protected float tankRotationSpeed;
    [HideInInspector] public float turretRotationSpeed;
    protected float directionOrInput;
    protected float brakingTime;
    protected float movementRef;
    protected float rotationRef;
    protected Vector3 normalGround;
    public bool dying;

    [Header("Suspension")]
    [SerializeField] protected Transform[] suspensionPoints;
    [SerializeField] protected float suspensionLenght;
    [SerializeField] protected float minSpringStrength;
    [SerializeField] protected float minDampSensitivity;
    [SerializeField] protected float maxSpringStrength;
    [SerializeField] protected float maxDampSensitivity;
    protected float[] lastDistances;
    protected float springStrength;
    protected float dampSensitivity;

    protected Sequence suspensionRotationSequence;
    protected Coroutine OnTankOverturnedCoroutine;

    protected float targetPitch;

    public State currentState
    {
        get => _currentState;
        set
        {
            if (_currentState != value)
            {
                _currentState = value;
                if (_currentState == State.accelerating)
                    smokeVfx.Play();
                if (_currentState == State.braking || _currentState == State.accelerating)
                    ApplySuspensionAnimation();
            }
        }
    }

    protected void SetState()
    {
        bool hasSameDirection = Mathf.Sign(directionOrInput) == Mathf.Sign(movement);
        bool hasVelocity = rb.linearVelocity.magnitude > 0.1f;
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

    protected void SetPitch()
    {
        if (speed == tankConfig.speedMinMax.y && (currentState == State.accelerating || currentState == State.constantSpeed)) 
            targetPitch = tankConfig.pitchBoost; //Si tiene powerup de velocidad
        else if (currentState == State.quiet || currentState == State.braking) targetPitch = tankConfig.pitchIdle;
        else if (currentState == State.accelerating || currentState == State.constantSpeed) targetPitch = tankConfig.pitchMoving;

        engineSource.pitch = Mathf.Lerp(engineSource.pitch, targetPitch, tankConfig.pitchAcceleration);
    }

    protected virtual void RotateTank()
    {
        float targetAngularVelocityY = rotation * currentRotationSpeed * Mathf.Deg2Rad;
        rb.angularVelocity = new Vector3(rb.angularVelocity.x, targetAngularVelocityY, rb.angularVelocity.z);
    }

    protected void SetIsOnSlope()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 4f))
        {
            isOnSlope = Vector3.Angle(hit.normal, Vector3.up) > 5f;
        }
        normalGround = hit.normal;
    }

    protected void BrakeTank()
    {
        if (isGrounded && Mathf.Abs(movement) < 0.01f && rb.linearVelocity.magnitude < 0.5f && isOnSlope && 
            ((rb.angularVelocity.magnitude < 0.001 && rotation == 0) || rotation != 0))
        {           
            rb.linearDamping = 50;
        }
        else
        {
            rb.linearDamping = 0.4f;
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
        if (movement > 0 && directionOrInput < 0 && frontalCollision)
            movement = 0;

        if (movement < 0 && directionOrInput > 0 && backCollision)
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
        int contactPointsWithGround = 0;
        for (int i = 0; i < suspensionPoints.Length; i++)
        {
            Transform point = suspensionPoints[i];
            bool ray = Physics.Raycast(point.position, -point.up, out RaycastHit hit, suspensionLenght);
            float angle = Vector3.Angle(Vector3.up, hit.normal);
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

                if (angle < 70)
                    contactPointsWithGround++;
            }
        }
        if (contactPointsWithGround == 9)
            isGrounded = true;       
        else if (contactPointsWithGround >=3 && contactPointsWithGround <= 8 && !frontalCollision && !backCollision)
            isGrounded = true;
        else
            isGrounded = false;
        //Setear AngularDamping
        if (contactPointsWithGround > 4)
            rb.angularDamping = tankConfig.angularDampingInGround;
        else
            rb.angularDamping = tankConfig.angularDampingOutGround;
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
        if (superStructure == null)
        {
            if (suspensionRotationSequence != null && suspensionRotationSequence.IsActive())
                suspensionRotationSequence.Kill();
            return;
        }

        if (suspensionRotationSequence != null && suspensionRotationSequence.IsActive())
            suspensionRotationSequence.Kill();

        suspensionRotationSequence = DOTween.Sequence().SetLink(superStructure.gameObject);

        if (currentState == State.accelerating)
        {
            if (movement > 0 && frontalCollision || movement < 0 && backCollision)
                return;
            suspensionRotationSequence.Append(superStructure.DOLocalRotate(new Vector3(tankConfig.suspensionRotation * Mathf.Sign(movement) * -1, 0, 0),
            tankConfig.accelerationTime).SetEase(Ease.InOutQuad)).SetLink(superStructure.gameObject);

            suspensionRotationSequence.Append(superStructure.DOLocalRotate(Vector3.zero, 2).SetEase(Ease.InSine)).SetLink(superStructure.gameObject);
        }

        if (currentState == State.braking)
        {
            float percentage = MathF.Abs(movement) > 0.9 ? 1 : Mathf.Abs(movement) - 0.3f;

            suspensionRotationSequence.Append(superStructure.DOLocalRotate(new Vector3(tankConfig.suspensionRotation * Mathf.Sign(movement) * percentage, 0, 0),
                tankConfig.accelerationTime).SetEase(Ease.OutQuad)).
                SetLink(superStructure.gameObject);

            suspensionRotationSequence.Append(superStructure.DOLocalRotate(new Vector3(-1.5f * Mathf.Sign(movement), 0, 0), tankConfig.balanceDuration).
                SetEase(Ease.InOutSine)).SetLink(superStructure.gameObject);
            suspensionRotationSequence.Append(superStructure.DOLocalRotate(Vector3.zero, tankConfig.regainDuration).
                SetEase(Ease.InSine)).SetLink(superStructure.gameObject);
        }
    }

    public void OnCollisionStay(Collision collision)
    {
        Vector3 flatForward = Vector3.ProjectOnPlane(transform.forward, normalGround).normalized;

        Vector3 origin1 = tankCollider.ClosestPoint(transform.position + transform.right * 0.25f + (flatForward * 1.5f)) - transform.forward * 0.1f;
        Vector3 origin2 = tankCollider.ClosestPoint(transform.position - transform.right * 0.25f + (flatForward * 1.5f)) - transform.forward * 0.1f;
        Vector3 origin3 = tankCollider.ClosestPoint(transform.position + transform.right * 0.25f - (flatForward * 1.5f)) + transform.forward * 0.1f;
        Vector3 origin4 = tankCollider.ClosestPoint(transform.position - transform.right * 0.25f - (flatForward * 1.5f)) + transform.forward * 0.1f;

        bool rightFrontalCollision = Physics.Raycast(origin1, flatForward, tankConfig.raycastDistance, ~0);
        bool leftFrontalCollision = Physics.Raycast(origin2, flatForward, tankConfig.raycastDistance, ~0);
        bool rightBackCollision = Physics.Raycast(origin3, -flatForward, tankConfig.raycastDistance, ~0);
        bool leftBackCollision = Physics.Raycast(origin4, -flatForward, tankConfig.raycastDistance, ~0);

        frontalCollision = rightFrontalCollision || leftFrontalCollision;
        backCollision = rightBackCollision || leftBackCollision;

        if ((rightFrontalCollision && leftFrontalCollision) || (rightBackCollision && leftBackCollision))
        {
            if (directionOrInput == 0)
                movement = 0;
            return;
        }
        if (!isGrounded)
            currentRotationSpeed = 0;
        else
            currentRotationSpeed = tankRotationSpeed;
    }

    public void OnCollisionExit(Collision collision)
    {
        frontalCollision = false;
        frontalCollisionWithCorner = false;
        backCollision = false;
        backCollisionWithCorner = false;
        currentRotationSpeed = tankRotationSpeed;
    }

    protected IEnumerator OnTankOverturned()
    {
        movement = 0;
        rotation = 0;
        yield return new WaitForSeconds(1);

        if (Vector3.Dot(transform.up, Vector3.up) > 0.4f)
        {
            OnTankOverturnedCoroutine = null;
            yield break;
        }          
        Vector3 oldCenterOfMass = rb.centerOfMass;

        if (!frontalCollision && !backCollision)
        {          
            rb.centerOfMass = new Vector3(0, -0.8f, 0);
            yield return new WaitForSeconds(1.5f);
            if (Vector3.Dot(transform.up, Vector3.up) > 0.4f)
            {
                movement = 0;
                rotation = 0;
                rb.centerOfMass = oldCenterOfMass;
                OnTankOverturnedCoroutine = null;
                yield break;
            }                              
        }
        rb.centerOfMass = oldCenterOfMass;     
        if (transform.CompareTag("Player"))
        {
            PlayerHealth health = GetComponent<PlayerHealth>();
            health.TakeDamage(500);
        }
        else if (transform.CompareTag("Enemy"))
        {
            EnemyHealth health = GetComponent<EnemyHealth>();
            health.TakeDamage(500);
        }
        rb.useGravity = false;
    }

    public virtual void SpeedPowerUp(float duration)
    {
        tankAudioController.PlayPowerUpSound();
        smokeVfx.Play();
        speed = tankConfig.speedMinMax.y;
        tankRotationSpeed = tankConfig.tankRotationSpeedMinMax.y;
        turretRotationSpeed = tankConfig.turretRotationSpeedMinMax.y;
        rb.inertiaTensor = maxInertiaTensor;
        springStrength = maxSpringStrength;
        dampSensitivity = maxDampSensitivity;
        Invoke("RestoreSpeed", duration);
    }

    protected virtual void RestoreSpeed()
    {
        speed = tankConfig.speedMinMax.x;
        tankRotationSpeed = tankConfig.tankRotationSpeedMinMax.x;
        turretRotationSpeed = tankConfig.turretRotationSpeedMinMax.x;
        rb.inertiaTensor = minInertiaTensor;
        springStrength = minSpringStrength;
        dampSensitivity = minDampSensitivity;
    }

    public virtual void OnTankDead()
    {
        movement = 0;
        rotation = 0;
        engineSource.Stop();
        tankCollider.enabled = false;        
        smokeVfx.Stop();
        if (OnTankOverturnedCoroutine != null)
            StopCoroutine(OnTankOverturnedCoroutine);
        dying = true;
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
