using DG.Tweening;
using NUnit.Framework.Constraints;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Windows;
using UnityEngine.Windows.Speech;

public class TankBase : MonoBehaviour
{
    protected float TankSpeed;
    protected float movement;
    protected float rotation;



    protected Rigidbody rb;
    protected bool frontalCollision;
    protected bool backCollision;
    protected bool frontalCollisionWithCorner;
    protected bool backCollisionWithCorner;

    protected float currentRotationSpeed;
    protected float maxTankRotationSpeed;




    protected void ApplyMovement()
    {
        Vector3 targetVelocity = transform.forward * movement * TankSpeed;
        Vector3 velocityChange = targetVelocity - new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        velocityChange.y = 0;

        rb.AddForce(velocityChange, ForceMode.VelocityChange);
    }


    protected void OnCollisionStay(Collision collision)
    {
        bool rightFrontalCollision = Physics.Raycast(transform.position + transform.right * 0.3f, transform.forward, 2.3f);
        bool leftFrontalCollision = Physics.Raycast(transform.position + transform.right * -0.3f, transform.forward, 2.3f);
        bool rightBackCollision = Physics.Raycast(transform.position + transform.right * 0.3f, -transform.forward, 1.8f);
        bool leftBackCollision = Physics.Raycast(transform.position + transform.right * -0.3f, -transform.forward, 1.8f);

        foreach (ContactPoint contact in collision.contacts)
        {
            if (contact.thisCollider.GetComponent<BoxCollider>() != null && (!contact.otherCollider.transform.CompareTag("Floor")))
            {
                frontalCollision = rightFrontalCollision || leftFrontalCollision;
                backCollision = rightBackCollision || leftBackCollision;

                if ((rightFrontalCollision && leftFrontalCollision) || (rightBackCollision && leftBackCollision))
                {
                    currentRotationSpeed = 0;
                    if (contact.otherCollider.transform.CompareTag("Wall")) transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
                    return;
                }

                Vector3 contactDirection = contact.point - transform.position;
                float dot = Vector3.Dot(contactDirection.normalized, transform.forward);
                if (dot > 0.75f)
                {
                    currentRotationSpeed = 30;
                    frontalCollisionWithCorner = true;
                    if (contact.otherCollider.transform.CompareTag("Wall")) transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
                }
                if (dot < -0.75f)
                {
                    currentRotationSpeed = 30;
                    backCollisionWithCorner = true;
                    if (contact.otherCollider.transform.CompareTag("Wall")) transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
                }
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        frontalCollision = false;
        frontalCollisionWithCorner = false;
        backCollision = false;
        backCollisionWithCorner = false;
        currentRotationSpeed = maxTankRotationSpeed;
    }

    void OnDrawGizmos()
    {
        if (rb != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(rb.worldCenterOfMass, 0.1f);
        }
    }
}
