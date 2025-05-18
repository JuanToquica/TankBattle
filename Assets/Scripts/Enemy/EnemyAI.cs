using UnityEngine;
using UnityEngine.AI;
using BehaviorTree;
using System.Collections.Generic;
using UnityEngine.Windows;
using UnityEngine.UIElements;
using System.Collections;
using Unity.Burst.Intrinsics;

public class EnemyAI : TankBase
{    
    private WheelAnimations wheelAnimations;     
    public LineRenderer lineRenderer;
    public NavMeshPath path;
    public EnemyAttack enemyAttack;
    private Node _rootOfMovement = null;
    private Node _rootOfTurret = null;

    [Header("Enemy")]       
    public Transform player;   
    public float desiredMovement;
    public float desiredRotation;
    private float adjustedAngleToTarget;
    private Vector3 directionToTarget;


    [Header("AI Parameters")]

    [SerializeField] private float timeToLeaveSpawn;
    [SerializeField] private float timeToForgetPlayer;
    public float distanceToDetectPlayer;
    public float farDistance;
    public float nearDistance;
    public float maxAimingTolerance;
    public bool detectingPlayer;
    public bool knowsPlayerPosition;
    public int enemyArea;
    public List<Transform> waypoints;
    public int currentWaypoint;
    public int currentCornerInThePath;
    public bool followingPath;  
    private float timerPlayerNotDetected;
    public Vector3 directionToPlayer;
    public float distanceToPlayer;
    public float angleToPlayer;
    public bool changingArea;
    public bool isAwakening;
    public bool patrolWait;

    private void Start()
    {
        EnemyAttack enemyAttack = GetComponent<EnemyAttack>();
        wheelAnimations = GetComponent<WheelAnimations>();
        tankCollider = GetComponent<BoxCollider>();       
        rb = GetComponent<Rigidbody>();
        path = new NavMeshPath();       

        RestoreSpeed();
        currentRotationSpeed = tankRotationSpeed;
        lastDistances = new float[suspensionPoints.Length];

        SetUpTrees();
        changingArea = true;
        StartCoroutine(InitialDelay());
    }

    IEnumerator InitialDelay()
    {
        isAwakening = true;
        yield return new WaitForSeconds(timeToLeaveSpawn);
        isAwakening = false;
    }

    private void SetUpTrees()
    {
        TaskDetectPlayer detectPlayer = new TaskDetectPlayer(this);
        TaskAim aim = new TaskAim(this);
        TaskAttack attack = new TaskAttack(enemyAttack);
        TaskChasePlayer chasePlayer = new TaskChasePlayer(this);
        TaskPatrol patrol = new TaskPatrol(this);
        TaskPausePatrol pausePatrol = new TaskPausePatrol(this);
        TaskWatch watch = new TaskWatch(this);
        TaskSearchPlayer searchPlayer = new TaskSearchPlayer(this);
        TaskAvoidPlayer avoidPlayer = new TaskAvoidPlayer(this);
        TaskDodgeAttacks dodgeAttacks = new TaskDodgeAttacks(this);
        TaskChangeArea changeArea = new TaskChangeArea(this);
        ConditionIsPlayerFar playerFar = new ConditionIsPlayerFar(this);
        ConditionIsPlayerNearby playerNearby = new ConditionIsPlayerNearby(this);
        ConditionalHasLineOfSight hasLineOfSight = new ConditionalHasLineOfSight(this);

        Sequence attackSequence = new Sequence(new List<Node> { hasLineOfSight, aim, attack });
        Selector chaseOrNotSelector = new Selector(new List<Node> { playerFar, new Inverter(hasLineOfSight) });
        Sequence chasePlayerSequence = new Sequence(new List<Node> { chaseOrNotSelector, chasePlayer });
        Sequence avoidPlayerSequence = new Sequence(new List<Node> { playerNearby, avoidPlayer});

        _rootOfMovement = new Selector(new List<Node> { changeArea, new Sequence(new List<Node> { detectPlayer, new Selector(new List<Node> { chasePlayerSequence, avoidPlayerSequence, dodgeAttacks }) }), new Selector(new List<Node> { pausePatrol, patrol}) });
        _rootOfTurret = new Selector(new List<Node> { new Sequence(new List<Node> { detectPlayer, new Selector(new List<Node> { attackSequence, searchPlayer}) }), watch});
    }

    private void Update()               
    {
        UpdatePlayerInfo();
        if (_rootOfMovement != null && !isAwakening)
            _rootOfMovement.Evaluate();
        if (_rootOfTurret != null && !isAwakening)
            _rootOfTurret.Evaluate();

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
        followingPath = true;
    }

    public void ChangeArea()
    {
        changingArea = true;
        currentCornerInThePath = 1;
        NavMesh.CalculatePath(transform.position, waypoints[Random.Range(0, waypoints.Count-1)].position, NavMesh.AllAreas, path);
        followingPath = true;
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
        if (Mathf.Abs(adjustedAngleToTarget) > currentRotationSpeed * Time.fixedDeltaTime * 3)
            desiredRotation = Mathf.Sign(adjustedAngleToTarget);
        else
            desiredRotation = 0;
    }

    protected override void RotateTank()
    {
        base.RotateTank();
    }

    public override void RotateTurret()
    {
        turret.Rotate(0, turretRotationSpeed * Mathf.Sign(angleToPlayer) * Time.fixedDeltaTime, 0);
    }
  
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