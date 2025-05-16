using UnityEngine;
using UnityEngine.AI;
using BehaviorTree;
using System.Collections.Generic;
using UnityEngine.Windows;


public class EnemyAI : TankBase
{
    private Node _root = null;
    private Animator animator;
    private WheelAnimations wheelAnimations;
    public NavMeshPath path;
    public Transform[] waypoints;
    public LineRenderer lineRenderer;

    [Header("Enemy")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private Transform projectilesContainer;
    [SerializeField] private GameObject projectilePrefab;
    private float desiredMovement;
    private float desiredRotation;
    private float adjustedAngleToTarget;
    private Vector3 directionToTarget;
    

    [Header("AI Parameters")]
    [SerializeField] private float coolDown;
    [SerializeField] private float timeToForgetPlayer;
    public float distanceToDetectPlayer;
    public float stoppingDistance;
    public float maxAimingTolerance;
    public bool detectingPlayer;
    public bool knowsPlayerPosition;
    public int enemyArea;
    public int currentWaypoint;
    public int currentCornerInThePath;
    public bool followingPath;
    private float nextShootTimer = 0;
    private float timerPlayerNotDetected;
    public Vector3 directionToPlayer;
    public float distanceToPlayer;
    public float angleToPlayer;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        wheelAnimations = GetComponent<WheelAnimations>();
        tankCollider = GetComponent<BoxCollider>();
        path = new NavMeshPath();
        

        RestoreSpeed();
        currentRotationSpeed = tankRotationSpeed;
        lastDistances = new float[suspensionPoints.Length];

        SetUpTree();
    }

    private void SetUpTree()
    {
        TaskDetectPlayer detectPlayer = new TaskDetectPlayer(this);
        TaskAim aim = new TaskAim(this);
        TaskAttack attack = new TaskAttack(this);
        TaskChasePlayer chasePlayer = new TaskChasePlayer(this);
        TaskPatrol patrol = new TaskPatrol(this);
        ConditionIsPlayerFar playerFar = new ConditionIsPlayerFar(this);
        ConditionalHasLineOfSight hasLineOfSight = new ConditionalHasLineOfSight(this);

        Sequence attackSequence = new Sequence(new List<Node> { hasLineOfSight, aim, attack });
        Selector chaseOrNotSelector = new Selector(new List<Node> { playerFar, new Inverter(hasLineOfSight) });
        Sequence chaseAndAttackSequence = new Sequence(new List<Node> { chaseOrNotSelector, new Parallel(new List<Node> { chasePlayer, attackSequence }) });

        _root = new Selector(new List<Node> { new Sequence(new List<Node> { detectPlayer, new Selector(new List<Node> { chaseAndAttackSequence, attackSequence }) }), patrol });
    }

    private void Update()               
    {
        UpdatePlayerInfo();
        if (_root != null)
            _root.Evaluate();

        SetKnowsPlayerPosition(); 
        if (followingPath)
        {
            CalculateDesiredMovementAndRotation();
        }  
        else
        {
            desiredMovement = 0;
            desiredRotation = 0;
        }
        SetIsOnSlope();
        InterpolateMovementAndRotation();
        ManipulateMovementInCollision();
        SetState();
        nextShootTimer = Mathf.Clamp(nextShootTimer + Time.deltaTime, 0, coolDown);
        wheelAnimations.SetParameters(movement, rotation, desiredMovement, desiredRotation);
        DrawPath(path);
    }
    private void FixedUpdate()
    {
        RotateTank();
        BrakeTank();
        if (isGrounded)
            ApplyMovement();
        ApplySuspension();
    }

    private void UpdatePlayerInfo()
    {
        directionToPlayer = (player.position - turret.position).normalized;
        distanceToPlayer = (player.position - turret.position).magnitude;
        angleToPlayer = Vector3.SignedAngle(turret.forward, directionToPlayer, Vector3.up);
    }
    private void InterpolateMovementAndRotation()
    {
        rotation = Mathf.Clamp(Mathf.SmoothDamp(rotation, desiredRotation, ref rotationRef, angularAccelerationTime), -1, 1);
        if (Mathf.Abs(rotation) < 0.01) rotation = 0;

        SetMomentum();
        if (isGrounded)
        {
            float smoothTime = desiredMovement != 0 ? accelerationTime : brakingTime;
            if (desiredMovement != 0 && Mathf.Sign(desiredMovement) != Mathf.Sign(movement) && hasMomentum)
                smoothTime = 1;

            movement = Mathf.Clamp(Mathf.SmoothDamp(movement, desiredMovement, ref movementRef, smoothTime), -1, 1);
            brakingTime = Mathf.Lerp(0.2f, 0.4f, Mathf.Abs(movement));
            if (Mathf.Abs(movement) < 0.01f)
                movement = 0;
            if (Mathf.Abs(movement) > 0.99f && desiredMovement != 0 && Mathf.Sign(desiredMovement) == Mathf.Sign(movement))
                movement = 1 * desiredMovement;
        }
        directionOrInput = desiredMovement;
    }

    private void SetKnowsPlayerPosition()
    {
        if (detectingPlayer)
        {
            knowsPlayerPosition = true;
            if (timerPlayerNotDetected > 0) timerPlayerNotDetected = 0;
        }
        else
        {
            timerPlayerNotDetected = Mathf.Clamp(timerPlayerNotDetected + Time.deltaTime, 0, timeToForgetPlayer);
            if (timerPlayerNotDetected == timeToForgetPlayer) knowsPlayerPosition = false;
        }
    }

    public void CalculatePath()
    {
        currentCornerInThePath = 1;
        NavMesh.CalculatePath(transform.position, waypoints[currentWaypoint].position, 1 << enemyArea, path);
    }

    public void CalculateDesiredMovementAndRotation()
    {
        directionToTarget = (path.corners[currentCornerInThePath] - transform.position).normalized;
        directionToTarget.y = 0f;

        Vector3 flatForward = transform.forward;
        flatForward.y = 0; //Para no tener en cuenta las pendientes en el calculo del angulo
        float angle = Vector3.SignedAngle(flatForward, directionToTarget, Vector3.up);

        desiredMovement = Mathf.Abs(angle) > 90f ? -1 : 1;

        adjustedAngleToTarget = (desiredMovement == -1) ? Vector3.SignedAngle(-flatForward, directionToTarget, Vector3.up) : angle;
        desiredRotation = Mathf.Sign(adjustedAngleToTarget);
    }

    protected override void RotateTank()
    {
        if (Mathf.Abs(adjustedAngleToTarget) > currentRotationSpeed * Time.fixedDeltaTime)
            base.RotateTank();
    }

    public override void RotateTurret()
    {
        turret.Rotate(0, turretRotationSpeed * Mathf.Sign(angleToPlayer) * Time.fixedDeltaTime, 0);
    }

    public bool CanShoot()
    {
        return nextShootTimer == coolDown;
    }

    public void Shoot()
    {
        if (!CanShoot()) return;

        animator.SetBool("Fire", true);
        GameObject projectile = Instantiate(projectilePrefab, projectileSpawnPoint.position, Quaternion.Euler(0, projectileSpawnPoint.rotation.eulerAngles.y, 0));
        projectile.transform.SetParent(projectilesContainer);
        projectile.tag = "EnemyProjectile";

        nextShootTimer = 0;
    }

    public void EndShootAnimation() => animator.SetBool("Fire", false);
    
    public void DrawPath(NavMeshPath path)
    {
        if (path == null || path.corners.Length < 2) return;

        lineRenderer.positionCount = path.corners.Length;

        for (int i = 0; i < path.corners.Length; i++)
        {
            lineRenderer.SetPosition(i, path.corners[i] + Vector3.up * 0.5f);
        }
    }
}