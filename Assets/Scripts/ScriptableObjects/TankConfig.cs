using UnityEngine;

[CreateAssetMenu(fileName = "NewTankConfig", menuName = "ScriptableObject/TankConfig")]
public class TankConfig : ScriptableObject
{
    [Header("Movement")]
    public Vector2 speedMinMax;
    public Vector2 tankRotationSpeedMinMax;
    public Vector2 turretRotationSpeedMinMax;
    public float accelerationTime;
    public float angularAccelerationTime;
    public float angularDampingInGround;
    public float angularDampingOutGround;
    public float raycastDistance;


    [Header("Suspension Animation")]
    public float suspensionRotation;
    public float balanceDuration;
    public float regainDuration;

    [Header("Engine Sound")]
    public float pitchIdleLow;
    public float pitchIdle;
    public float pitchMoving;
    public float pitchBoost;
    public float pitchAcceleration;
}
