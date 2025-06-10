using System;
using UnityEngine;
using UnityEngine.Windows;

public class GarageTankController : MonoBehaviour
{
    [Header("Rotation Settings")]
    [SerializeField] private Transform turret;
    [SerializeField] private float turretRotationSpeed;
    [SerializeField] private float maxTankRotationSpeed;
    [SerializeField] private float tankConstantRotationSpeed;
    [SerializeField] private float rotationSmoothing;
    private float rotationVelocity;
    private bool centeringTurret;
    private bool canRotate;

    [Header("Mesh Renderer And Material References")]
    [SerializeField] private Material[] redPaint;
    [SerializeField] private Material[] bluePaint;
    [SerializeField] private Material[] purplePaint;
    [SerializeField] private Material[] yellowPaint;
    [SerializeField] private Material[] greenPaint;
    [SerializeField] private MeshRenderer turretMeshRenderer;
    [SerializeField] private MeshRenderer cannonMeshRenderer;
    [SerializeField] private MeshRenderer tankMeshRenderer;
    [SerializeField] private MeshRenderer SmallWheelsRightMeshRenderer;
    [SerializeField] private MeshRenderer WheelFrontRightMeshRenderer;
    [SerializeField] private MeshRenderer WheelBackRightMeshRenderer;
    [SerializeField] private MeshRenderer SmallWheelsLeftMeshRenderer;
    [SerializeField] private MeshRenderer WheelFrontLeftMeshRenderer;
    [SerializeField] private MeshRenderer WheelBackLeftMeshRenderer;
    [SerializeField] private SkinnedMeshRenderer TrackRightMeshRenderer;
    [SerializeField] private SkinnedMeshRenderer TrackLeftMeshRenderer;
    private Material[][] materials;

    private void OnEnable()
    {
        if (InputManager.Instance != null)
            InputManager.Instance.garageTankController = this;
        rotationVelocity = tankConstantRotationSpeed;
        if (materials == null)
        {
            materials = new Material[][]{
                new Material[] { redPaint[0], redPaint[1] },
                new Material[] { bluePaint[0], bluePaint[1] },
                new Material[] { purplePaint[0], purplePaint[1] },
                new Material[] { yellowPaint[0], yellowPaint[1] },
                new Material[] { greenPaint[0], greenPaint[1] }
            };
        }

    }
    private void Update()
    {
        SetRotationVelocity();
        RotateTank();
        if (InputManager.Instance.turretInput != 0)
            RotateTurret();
        if (centeringTurret)
            CenterTurret();
    }

    private void SetRotationVelocity()
    {
        if (canRotate)
        {
            rotationVelocity = maxTankRotationSpeed * InputManager.Instance.mouseInput;
        }
        else
        {
            rotationVelocity = Mathf.Lerp(rotationVelocity, tankConstantRotationSpeed, Time.deltaTime * rotationSmoothing);
            if (rotationVelocity < tankConstantRotationSpeed && rotationVelocity > tankConstantRotationSpeed - 0.1)
                rotationVelocity = tankConstantRotationSpeed;
        }
    }
    private void RotateTank()
    {
        transform.Rotate(0, rotationVelocity * Time.deltaTime, 0);
    }

    private void RotateTurret()
    {
        centeringTurret = false;
        turret.Rotate(0, turretRotationSpeed * InputManager.Instance.turretInput * Time.deltaTime, 0);
    }

    public void StartTurretCentering()
    {
        centeringTurret = true;
    }

    private void CenterTurret()
    {
        turret.transform.localRotation = Quaternion.RotateTowards(turret.transform.localRotation, Quaternion.identity, turretRotationSpeed * Time.deltaTime);
        if (Quaternion.Angle(turret.transform.localRotation, Quaternion.identity) < 0.1f)
        {
            turret.transform.localRotation = Quaternion.Euler(0, 0, 0);
            centeringTurret = false;
        }
    }

    public void SetCanRotateTank(bool value)
    {
        canRotate = value;
    }

    public void ChangeTankMaterial(int index)
    {
        tankMeshRenderer.materials = materials[index];
        turretMeshRenderer.materials = new Material[] { materials[index][1], materials[index][0] };
        cannonMeshRenderer.materials = new Material[] { materials[index][1] };
        SmallWheelsRightMeshRenderer.materials = new Material[] { materials[index][0] };
        WheelFrontRightMeshRenderer.materials = new Material[] {materials[index][0]};
        WheelBackRightMeshRenderer.materials = new Material[] {materials[index][0] };
        SmallWheelsLeftMeshRenderer.materials = new Material[] {materials[index][0]};
        WheelFrontLeftMeshRenderer.materials = new Material[] {materials[index][0]};
        WheelBackLeftMeshRenderer.materials = new Material[] {materials[index][0]};
        TrackRightMeshRenderer.materials = new Material[] {materials[index][0]};
        TrackLeftMeshRenderer.materials = new Material[] {materials[index][0]};
    }
}
