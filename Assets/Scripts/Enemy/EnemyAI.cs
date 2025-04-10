using UnityEngine;
using UnityEngine.AI;
using BehaviorTree;
using System.Collections.Generic;


public class EnemyAI : MonoBehaviour
{
    private Node _root = null;
    private Animator animator;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private Transform projectilesContainer;
    [SerializeField] private float coolDown;
    private float nextShootTime = 0;

    [SerializeField] private Transform turret;
    [SerializeField] private Transform player;
    [SerializeField] private float turretRotationSpeed; 

    private void Start()
    {
        SetUpTree();
        animator = GetComponent<Animator>();
    }
    private void Update()
    {
        if (_root != null)
            _root.Evaluate();
    }

    private void SetUpTree()
    {
        _root = new Sequence(new List<Node> { new TaskAim(player, turret, turretRotationSpeed) ,new TaskAttack(this) });
    }


    public bool CanShoot()
    {
        return Time.time >= nextShootTime;
    }

    public void Shoot()
    {
        if (!CanShoot()) return;

        animator.SetBool("Fire", true);
        GameObject projectile = Instantiate(projectilePrefab, projectileSpawnPoint.position, Quaternion.Euler(0, projectileSpawnPoint.rotation.eulerAngles.y, 0));
        projectile.transform.SetParent(projectilesContainer);
        projectile.tag = "EnemyProjectile";

        nextShootTime = Time.time + coolDown;
    }

    public void EndShootAnimation()
    {
        animator.SetBool("Fire", false);
    }
}