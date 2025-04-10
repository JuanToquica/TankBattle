using UnityEngine;
using BehaviorTree;

public class TaskAim : Node
{
    private Transform player;
    private Transform turret;
    private float tolerance;
    private float turretRotationSpeed;
    public TaskAim(Transform player, Transform turret, float turretRotationSpeed, float tolerance = 5f)
    {
        this.player = player;
        this.turret = turret;
        this.turretRotationSpeed = turretRotationSpeed;
        this.tolerance = tolerance;       
    }

    private void RotateTurret(float angle)
    {
        turret.Rotate(0, turretRotationSpeed * Mathf.Sign(angle) * Time.fixedDeltaTime, 0);
    }

    public override NodeState Evaluate()
    {
        Vector3 directionToPlayer = (player.position - turret.position).normalized;
        float angle = Vector3.SignedAngle(turret.forward, directionToPlayer, Vector3.up);

        if (Mathf.Abs(angle) < tolerance) return NodeState.SUCCESS;
        else
        {
            RotateTurret(angle);
            return NodeState.RUNNING;
        }
            
    }
}
