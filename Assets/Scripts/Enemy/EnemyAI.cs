using UnityEngine;
using UnityEngine.AI;
using BehaviorTree;
using System.Collections.Generic;


public class EnemyAI : MonoBehaviour
{
    private Node _root = null;
    private Animator animator;

    [Header ("References")]
    public Transform turret;
    public Transform player;
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private Transform projectilesContainer;
    [SerializeField] private GameObject projectilePrefab;

    [Header ("Movement Parameters")]
    public float turretRotationSpeed;

    [Header("AI Parameters")]
    public float distanceToDetectPlayer;
    public float maxAimingTolerance;
    public float coolDown;
    public float timeToForgetPlayer;
    public bool detectingPlayer;
    public bool knowsPlayerPosition;



    private float nextShootTimer = 0;
    private float timerPlayerNotDetected;

    private void Start()
    {
        SetUpTree();
        animator = GetComponent<Animator>();
    }
    private void Update()               
    {
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
    }

    private void SetUpTree()
    {
        _root = new Sequence(new List<Node> {new TaskDetectPlayer(this),new ConditionalHasLineOfSight(this) , new TaskAim(this) ,new TaskAttack(this) });
    }

    private void FixedUpdate()
    {
        if (_root != null)
            _root.Evaluate();
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

    public void EndShootAnimation()
    {
        animator.SetBool("Fire", false);
    }
}