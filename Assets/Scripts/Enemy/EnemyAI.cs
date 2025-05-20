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
    public float centeringOffset;

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
        DrawRays();
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
        CalculateCenteredPath(transform.position, waypoints[currentWaypoint].position, 1 << enemyArea, centeringOffset);
        ChangeDesiredMovement();
    }

    public void ChangeArea()
    {
        changingArea = true;
        currentCornerInThePath = 1;
        CalculateCenteredPath(transform.position, waypoints[Random.Range(0, waypoints.Count - 1)].position, NavMesh.AllAreas, 1);
    }


    public void ChangeDesiredMovement()
    {
        directionToTarget = (corners[currentCornerInThePath] - transform.position).normalized;
        directionToTarget.y = 0f;

        Vector3 flatForward = transform.forward;
        flatForward.y = 0; //Para no tener en cuenta las pendientes en el calculo del angulo
        float angle = Vector3.SignedAngle(flatForward, directionToTarget, Vector3.up);

        if (Mathf.Abs(angle) > 90)
        {
            int random = Random.Range(1, 3);
            if (random == 1)
            {
                desiredMovement = Mathf.Abs(angle) > 90f ? -1 : 1;
            }
            else
            {
                desiredMovement = 0;
            }
        }
        else
        {
            desiredMovement = 1;
        }           
    }

    public void CalculateDesiredMovementAndRotation()
    {
        directionToTarget = (corners[currentCornerInThePath] - transform.position).normalized;
        directionToTarget.y = 0f;

        Vector3 flatForward = transform.forward;
        flatForward.y = 0; //Para no tener en cuenta las pendientes en el calculo del angulo
        float angle = Vector3.SignedAngle(flatForward, directionToTarget, Vector3.up);
       
        if (frontalCollision && desiredMovement == 1) //Cambiar direccion al chocar
            desiredMovement = -1;
        else if (backCollision && desiredMovement == -1)
            desiredMovement = 1;
        else if (Mathf.Abs(angle) < 45 && desiredMovement == 0)
        {
            desiredMovement = 1;
        }

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
    
    public List<Vector3> corners;
    public void CalculateCenteredPath(Vector3 startPos, Vector3 endPos, int area, float offset)
    {
        if (NavMesh.CalculatePath(startPos, endPos, area, path))
        {
            List<Vector3> centeredCorners = new List<Vector3>(path.corners);

            if (centeredCorners.Count > 2)
            {
                for (int i = 1; i < centeredCorners.Count - 1; i++)
                {
                    Vector3 previousPoint = path.corners[i - 1];
                    Vector3 currentPoint = path.corners[i];
                    Vector3 nextPoint = path.corners[i + 1];

                    Vector3 prevDir = (currentPoint - previousPoint).normalized;
                    Vector3 nextDir = (currentPoint - nextPoint).normalized;

                    Vector3 displacementDir = (prevDir + nextDir).normalized;

                    centeredCorners[i] += displacementDir * offset;

                    NavMeshHit hit;
                    if (NavMesh.SamplePosition(centeredCorners[i], out hit, offset * 2f, 1 << enemyArea))
                    {
                        centeredCorners[i] = hit.position;
                    }
                    else
                    {
                        centeredCorners[i] = currentPoint;
                    }
                }              
            }
            followingPath = true;
            corners = centeredCorners;
        }      
    }

    public void DrawPath(NavMeshPath path)
    {
        if (path == null || corners.Count < 2) return;

        lineRenderer.positionCount = corners.Count;

        for (int i = 0; i < corners.Count; i++)
        {
            lineRenderer.SetPosition(i, corners[i] + Vector3.up * 0.5f);
        }
    }

    private void DrawRays()
    {
        Vector3 flatForward = Vector3.ProjectOnPlane(transform.forward, normalGround).normalized;

        Vector3 origin1 = tankCollider.ClosestPoint(transform.position + transform.right * 0.3f + (flatForward * 1.5f));
        Vector3 origin2 = tankCollider.ClosestPoint(transform.position - transform.right * 0.3f + (flatForward * 1.5f));
        Vector3 origin3 = tankCollider.ClosestPoint(transform.position + transform.right * 0.3f - (flatForward * 1.5f));
        Vector3 origin4 = tankCollider.ClosestPoint(transform.position - transform.right * 0.3f - (flatForward * 1.5f));

        Debug.DrawRay(origin1, flatForward * raycastDistance, Color.red);
        Debug.DrawRay(origin2, flatForward * raycastDistance, Color.red);
        Debug.DrawRay(origin3, -flatForward * raycastDistance, Color.red);
        Debug.DrawRay(origin4, -flatForward * raycastDistance, Color.red);
    }
}