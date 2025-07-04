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
    public EnemyManager enemyManager;
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
    public int centeringOffset;
    private Vector3 flatForward;
    public float angleToCorner;
    public bool dodgingAttacks;
    public int oldArea = 0;

    private void Start()
    {
        EnemyAttack enemyAttack = GetComponent<EnemyAttack>();
        wheelAnimations = GetComponent<WheelAnimations>();
        tankCollider = GetComponent<BoxCollider>();       
        rb = GetComponent<Rigidbody>();
        path = new NavMeshPath();

        springStrength = minSpringStrength;
        dampSensitivity = minDampSensitivity;
        rb.inertiaTensor = minInertiaTensor;

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
        TaskAvoidPlayer avoidPlayer = new TaskAvoidPlayer(this);
        TaskDodgeAttacks dodgeAttacks = new TaskDodgeAttacks(this);
        TaskChangeArea changeArea = new TaskChangeArea(this);
        ConditionIsPlayerFar playerFar = new ConditionIsPlayerFar(this, enemyManager);
        ConditionIsPlayerNearby playerNearby = new ConditionIsPlayerNearby(this);
        ConditionalHasLineOfSight hasLineOfSight = new ConditionalHasLineOfSight(this);

        Sequence attackSequence = new Sequence(new List<Node> { hasLineOfSight, aim, attack });
        Selector chaseOrNotSelector = new Selector(new List<Node> { playerFar, new Inverter(hasLineOfSight) });
        Sequence chasePlayerSequence = new Sequence(new List<Node> { chaseOrNotSelector, chasePlayer });
        Sequence avoidPlayerSequence = new Sequence(new List<Node> { playerNearby, avoidPlayer});

        _rootOfMovement = new Selector(new List<Node> { changeArea, 
            new Sequence(new List<Node> { detectPlayer, 
                new Selector(new List<Node> { chasePlayerSequence, avoidPlayerSequence, 
                    new Selector(new List<Node> { pausePatrol, dodgeAttacks })}) }), 
            new Selector(new List<Node> { pausePatrol, patrol}) });
        
        _rootOfTurret = new Selector(new List<Node> { 
            new Sequence(new List<Node> { detectPlayer, 
                new Selector(new List<Node> { attackSequence, watch}) }), watch});
    }

    private void Update()               
    {
        InterpolateMovementAndRotation();
        if (dying) return;
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
        ManipulateMovementInCollision();
        SetState();      
        wheelAnimations.SetParameters(movement, rotation, desiredMovement, desiredRotation);
        DrawPath(path);
        DrawRays();
        if (Vector3.Dot(transform.up, Vector3.up) < 0.3f && OnTankOverturnedCoroutine == null)
            OnTankOverturnedCoroutine = StartCoroutine(OnTankOverturned());
    }
    private void FixedUpdate()
    {
        ApplySuspension();
        RotateTank();
        BrakeTank();
        if (isGrounded)
            ApplyMovement();  
    }

    private void UpdatePlayerInfo()
    {
        directionToPlayer = (player.position - turret.position).normalized;
        distanceToPlayer = (player.position - turret.position).magnitude;

        Vector3 flatTurretForward = turret.forward;
        flatTurretForward.y = 0;
        angleToPlayer = Vector3.SignedAngle(flatTurretForward, directionToPlayer, Vector3.up);
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
        if (player.gameObject.activeSelf)
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
        else
        {
            detectingPlayer = false;
            knowsPlayerPosition = false;
        }
    }

    public void CalculatePath(Vector3 newPosition)
    {
        currentCornerInThePath = 1;
        CalculateCenteredPath(transform.position, newPosition, 1 << enemyArea, centeringOffset);
        ChangeDesiredMovement();
    }

    public void ChangeArea()
    {
        changingArea = true;
        currentCornerInThePath = 1;
        int waypoint = 0;
        if (enemyArea == 13)
        {
            if ((player.position - waypoints[1].position).magnitude < (player.position - waypoints[0].position).magnitude)
            {
                waypoint = 1;
            }
        }
        else if (enemyArea == 14)
        {
            Debug.Log(oldArea);
            if (oldArea == 10)
            {
                if ((player.position - waypoints[1].position).magnitude < (player.position - waypoints[3].position).magnitude)
                    waypoint = 1;
                else
                    waypoint = 3;
            }
            else if (oldArea == 5)
            {
                if ((player.position - waypoints[2].position).magnitude < (player.position - waypoints[0].position).magnitude)
                    waypoint = 2;
            }

        }
        CalculateCenteredPath(transform.position, waypoints[waypoint].position, NavMesh.AllAreas, 2);
        ChangeDesiredMovement();
    }


    public void ChangeDesiredMovement()
    {
        CalculateDesiredMovementAndRotation();

        if (Mathf.Abs(angleToCorner) > 90)
        {
            int random = Random.Range(1, 3);
            if ((random == 1 && !frontalCollisionWithCorner && !backCollisionWithCorner && !changingArea) || dodgingAttacks)
            {
                desiredMovement = Mathf.Abs(angleToCorner) > 90f ? -1 : 1;               
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

        flatForward = transform.forward;
        flatForward.y = 0; //Para no tener en cuenta las pendientes en el calculo del angulo
        angleToCorner = Vector3.SignedAngle(flatForward, directionToTarget, Vector3.up);
       
        if (frontalCollision && desiredMovement == 1) //Cambiar direccion al chocar
            desiredMovement = -1;
        else if (backCollision && desiredMovement == -1)
            desiredMovement = 1;
        else if ((frontalCollision || backCollision) && desiredMovement == 0)
            desiredMovement = Random.Range(-1, 2);
        else if (Mathf.Abs(angleToCorner) < 40 && desiredMovement == 0)
            desiredMovement = 1;

        adjustedAngleToTarget = (desiredMovement == -1) ? Vector3.SignedAngle(-flatForward, directionToTarget, Vector3.up) : angleToCorner;
        if (Mathf.Abs(adjustedAngleToTarget) > currentRotationSpeed * Time.fixedDeltaTime * 3)
            desiredRotation = Mathf.Sign(adjustedAngleToTarget);
        else
            desiredRotation = 0;
    }

    protected override void RotateTank()
    {
        base.RotateTank();
    }


    public void RotateTurret(float speed)
    {
        turret.Rotate(0, speed * Mathf.Sign(angleToPlayer) * Time.deltaTime, 0);
    }

    public void RotatoTurretToWatch(float angle)
    {
        turret.Rotate(0, turretRotationSpeed* Mathf.Sign(angle) * Time.deltaTime, 0);
    }
    
    public List<Vector3> corners;
    public void CalculateCenteredPath(Vector3 startPos, Vector3 endPos, int area, int offset)
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

                    int randomOffset;
                    if (area == 1 << 4 || area == 1 << 9 && !changingArea)
                    {
                        randomOffset = Random.Range(offset - 1, offset + 1);
                        centeredCorners[i] += displacementDir * randomOffset;
                    }
                    else if (area == 1 << 7 || area == 1 << 12 && !changingArea)
                    {
                        randomOffset = Random.Range(offset - 1, offset + 2);
                        centeredCorners[i] += displacementDir * randomOffset;                       
                    }
                    else if (changingArea && i == 1) // Mover el primer punto luego de spawnear hacia adelante
                    {
                        if (enemyArea == 6 || enemyArea == 7 || enemyArea == 11 || enemyArea == 12)
                            centeredCorners[i] += prevDir * offset * 3;
                        else
                            centeredCorners[i] += prevDir * offset;
                    }
                    else
                    {
                        centeredCorners[i] += displacementDir * offset;
                    }

                    NavMeshHit hit;
                    if (!changingArea && NavMesh.SamplePosition(centeredCorners[i], out hit, offset * 2f, 1 << enemyArea))
                    {
                        centeredCorners[i] = hit.position;
                    }
                    else if (changingArea && NavMesh.SamplePosition(centeredCorners[i], out hit, offset * 2f,NavMesh.AllAreas))
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

    public void EvaluateAreaChange()
    {
        if (enemyArea == 7 && (NavMesh.SamplePosition(player.position, out NavMeshHit hit, 2f, 1 << 13) || player.position.z < -45))
        {
            enemyManager.ChangeAreaToChase(enemyArea);
        }
        else if (!enemyManager.chasingInArea14 && (enemyArea == 5 || enemyArea == 10) && (NavMesh.SamplePosition(player.position, out NavMeshHit hit2, 2f, 1 << 14) || player.position.z < -45))
        {
            enemyManager.ChangeAreaToChase(enemyArea);
        }       
    }

    public bool EvaluateBackToOriginalArea()
    {
        if (enemyArea == 13)
        {
            int allAreasExcept13 = ~(1 << 13);
            if (NavMesh.SamplePosition(player.position, out NavMeshHit hit3, 2f, allAreasExcept13) && (player.position.z > -25 || !knowsPlayerPosition))
            {
                enemyManager.BackToOriginalArea(enemyArea, oldArea);
                return true;
            }
                
        }
        else if (enemyManager.chasingInArea14 && enemyArea == 14)
        {
            int allAreasExcept14 = ~(1 << 14);
            if (NavMesh.SamplePosition(player.position, out NavMeshHit hit3, 2f, allAreasExcept14) && (player.position.z > -25 || !knowsPlayerPosition))
            {
                enemyManager.BackToOriginalArea(enemyArea, oldArea);
                return true;
            }              
        }
        return false;
    }


    private void DrawRays()
    {
        Vector3 flatForward = Vector3.ProjectOnPlane(transform.forward, normalGround).normalized;

        Vector3 origin1 = tankCollider.ClosestPoint(transform.position + transform.right * 0.3f + (flatForward * 1.5f)) - transform.forward * 0.1f;
        Vector3 origin2 = tankCollider.ClosestPoint(transform.position - transform.right * 0.3f + (flatForward * 1.5f)) - transform.forward * 0.1f;
        Vector3 origin3 = tankCollider.ClosestPoint(transform.position + transform.right * 0.3f - (flatForward * 1.5f)) + transform.forward * 0.1f;
        Vector3 origin4 = tankCollider.ClosestPoint(transform.position - transform.right * 0.3f - (flatForward * 1.5f)) + transform.forward * 0.1f;

        Debug.DrawRay(origin1, flatForward * raycastDistance, Color.red);
        Debug.DrawRay(origin2, flatForward * raycastDistance, Color.red);
        Debug.DrawRay(origin3, -flatForward * raycastDistance, Color.red);
        Debug.DrawRay(origin4, -flatForward * raycastDistance, Color.red);
    }
}