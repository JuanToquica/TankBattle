using UnityEngine;
using UnityEngine.AI;
using BehaviorTree;
using System.Collections.Generic;
using UnityEngine.Windows;
using static UnityEngine.GridBrushBase;


public class EnemyAI : MonoBehaviour
{
    private Node _root = null;
    private Animator animator;
    private Rigidbody rb;
    public NavMeshPath path;
    public Transform[] waypoints;

    [Header ("References")]
    public Transform turret;
    public Transform player;
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private Transform projectilesContainer;
    [SerializeField] private GameObject projectilePrefab;

    [Header ("Movement Parameters")]
    public float turretRotationSpeed;
    public float tankRotationSpeed;
    public float speed;
    public float accelerationTime;
    public int movementDirection;
    private float movement;
    private float movementRef;
    private float rotationDirection;
    private float rotation;
    private float rotationRef;
    public float angularAccelerationTime;
    private Vector3 directionToTarget;
    private float adjustedAngleToTarget;

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
        nextShootTimer = Mathf.Clamp(nextShootTimer + Time.deltaTime, 0, coolDown);

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
        DrawPath(path);

        movement = Mathf.Clamp(Mathf.SmoothDamp(movement, movementDirection, ref movementRef, accelerationTime), -1, 1);
        if (Mathf.Abs(movement) < 0.01) movement = 0;

        rotation = Mathf.Clamp(Mathf.SmoothDamp(rotation, rotationDirection, ref rotationRef, angularAccelerationTime), -1, 1);
        if (Mathf.Abs(rotation) < 0.01) rotation = 0;
    }

    private void FixedUpdate()
    {
        if (followingPath)
            CalculateDirectionOfMovementAndRotation();
        Move();
        RotateTank();
    }

    public void CalculatePath()
    {
        currentCornerInThePath = 1;
        NavMesh.CalculatePath(transform.position, waypoints[currentWaypoint].position, 1 << enemyArea, path);
    }

    
    public bool CanShoot()
    {
        return nextShootTimer == coolDown;
    }

    public void CalculateDirectionOfMovementAndRotation()
    {
        directionToTarget = (path.corners[currentCornerInThePath] - transform.position).normalized;
        directionToTarget.y = 0f;
        float angle = Vector3.SignedAngle(transform.forward, directionToTarget, Vector3.up);

        movementDirection = Mathf.Abs(angle) > 90f ? -1 : 1;

        adjustedAngleToTarget = (movementDirection == -1) ? Vector3.SignedAngle(-transform.forward, directionToTarget, Vector3.up) : angle;
        rotationDirection = Mathf.Sign(adjustedAngleToTarget);
    }

    private void Move()
    {
        Vector3 targetVelocity = transform.forward *  movement * speed;
        Vector3 velocityChange = targetVelocity - new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        velocityChange.y = 0;

        rb.AddForce(velocityChange, ForceMode.VelocityChange);
    }

    private void RotateTank()
    {       
        if (Mathf.Abs(adjustedAngleToTarget) > tankRotationSpeed * Time.fixedDeltaTime)
            transform.Rotate(0, rotation * tankRotationSpeed * Time.fixedDeltaTime, 0);
        else
        {
            Vector3 lookDir = (movementDirection == 1) ? directionToTarget : -directionToTarget;
            transform.rotation = Quaternion.LookRotation(lookDir);
        }
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
    

    [SerializeField] private LineRenderer lineRenderer;
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