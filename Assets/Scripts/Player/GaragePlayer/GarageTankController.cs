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
    

    private void OnEnable()
    {
        if (InputManager.Instance != null)
            InputManager.Instance.RegisterGarageTankController(this);
        rotationVelocity = tankConstantRotationSpeed;       
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
            rotationVelocity = maxTankRotationSpeed * InputManager.Instance.mouseInput * -1;
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
}
