using UnityEngine;
using UnityEngine.AI;
using BehaviorTree;
using System.Collections.Generic;
using UnityEngine.Windows;
using static UnityEngine.GridBrushBase;


public class EnemyAI : TankBase
{
    private Node _root = null;
    private Animator animator;
    private WheelAnimations wheelAnimations;
    public NavMeshPath path;
    public Transform[] waypoints;
    [SerializeField] private LineRenderer lineRenderer;

    [Header ("References")]
    public Transform turret;
    public Transform player;
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private Transform projectilesContainer;
    [SerializeField] private GameObject projectilePrefab;

    [Header ("Movement")]
    [SerializeField] public float turretRotationSpeed;
    [SerializeField] private float accelerationTime;
    [SerializeField] private float brakingTime;
    [SerializeField] private float angularAccelerationTime;
    private float movementRef;
    private float rotationRef;
    public int movementDirection;   
    public float rotationDirection;   
    private Vector3 directionToTarget;
    public float adjustedAngleToTarget;

    [Header("AI Parameters")]
    public float distanceToDetectPlayer;
    public float stoppingDistance;
    public float maxAimingTolerance;
    public float coolDown;
    public float timeToForgetPlayer;
    public bool detectingPlayer;
    public bool knowsPlayerPosition;
    public int enemyArea;
    public int currentWaypoint = 0;
    public int currentCornerInThePath = 1;
    public bool followingPath;

    private float nextShootTimer = 0;
    private float timerPlayerNotDetected;

    private void Start()
    {
        SetUpTree();
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        wheelAnimations = GetComponent<WheelAnimations>();
        path = new NavMeshPath();

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
        if (_root != null)
            _root.Evaluate();
        
        SetKnowsPlayerPosition();
        DrawPath(path);
        
        if (followingPath)
            CalculateDirectionOfMovementAndRotation();
        else
        {
            movementDirection = 0;
            rotationDirection = 0;
        }
        InterpolateMovementAndRotation();

        nextShootTimer = Mathf.Clamp(nextShootTimer + Time.deltaTime, 0, coolDown);
        wheelAnimations.SetParameters(movement, rotation, movementDirection, rotationDirection);
    }

    private void InterpolateMovementAndRotation()
    {
        if (movementDirection != 0)
            movement = Mathf.Clamp(Mathf.SmoothDamp(movement, movementDirection, ref movementRef, accelerationTime), -1, 1);
        else
            movement = Mathf.Clamp(Mathf.SmoothDamp(movement, movementDirection, ref movementRef, brakingTime), -1, 1);
        if (Mathf.Abs(movement) < 0.01) movement = 0;

        rotation = Mathf.Clamp(Mathf.SmoothDamp(rotation, rotationDirection, ref rotationRef, angularAccelerationTime), -1, 1);
        if (Mathf.Abs(rotation) < 0.01) rotation = 0;
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

    private void FixedUpdate()
    {       
        ApplyMovement();
        RotateTank();
    }

    protected override void SetState()
    {
        throw new System.NotImplementedException();
    }
    public void CalculatePath()
    {
        currentCornerInThePath = 1;
        NavMesh.CalculatePath(transform.position, waypoints[currentWaypoint].position, 1 << enemyArea, path);
    }

    public void CalculateDirectionOfMovementAndRotation()
    {
        directionToTarget = (path.corners[currentCornerInThePath] - transform.position).normalized;
        directionToTarget.y = 0f;

        Vector3 flatForward = transform.forward;
        flatForward.y = 0; //Para no tener en cuenta las pendientes en el calculo del angulo
        float angle = Vector3.SignedAngle(flatForward, directionToTarget, Vector3.up);

        movementDirection = Mathf.Abs(angle) > 90f ? -1 : 1;

        adjustedAngleToTarget = (movementDirection == -1) ? Vector3.SignedAngle(-flatForward, directionToTarget, Vector3.up) : angle;
        rotationDirection = Mathf.Sign(adjustedAngleToTarget);
    }

    protected override void RotateTank()
    {       
        if (Mathf.Abs(adjustedAngleToTarget) > tankRotationSpeed * Time.fixedDeltaTime)
            transform.Rotate(0, rotation * tankRotationSpeed * Time.fixedDeltaTime, 0);
    }

    protected override void RotateTurret()
    {
        Vector3 directionToPlayer = (player.position - turret.position).normalized;
        float angle = Vector3.SignedAngle(turret.forward, directionToPlayer, Vector3.up);
        turret.Rotate(0, turretRotationSpeed * Mathf.Sign(angle) * Time.fixedDeltaTime, 0);
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