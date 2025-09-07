using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class TankController : MonoBehaviour
{
    [Header("Settings")]
    public float moveSpeed = 3f;
    public float rotationSpeed = 180f;
    public float damping = 0.99f;

    public Transform firePoint;
    public GameObject bulletPrefab;
    private GameObject currentBullet;

    [Header("Refs")]
    public GameObject barrel;
    private Animator barrelAnimator;

    private Vector2 externalForces;

    Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        barrelAnimator = barrel.GetComponent<Animator>();
    }

    public void Drive(float moveInput, float turnInput, bool shoot)
    {
        if (enabled == false) return;

        // Move
        Vector2 forward = moveInput * moveSpeed * transform.up;
        rb.velocity = forward;
        rb.velocity += externalForces;

        // Rotate
        float newRot = rb.rotation - turnInput * rotationSpeed * Time.fixedDeltaTime;
        rb.MoveRotation(newRot);

        // Shoot with cooldown
        if (shoot && CanShoot())
        {
            Fire();
        }

        externalForces = Vector2.Lerp(externalForces, Vector2.zero, Time.fixedDeltaTime * damping);
    }

    public bool CanShoot()
    {
        return currentBullet == null;
    }

    void Fire()
    {
        if (!CanShoot()) return;

        barrelAnimator.SetTrigger("isFire");

        currentBullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        var rbBullet = currentBullet.GetComponent<Rigidbody2D>();
        rbBullet.velocity = firePoint.up * 12f;

        Bullet bullet = currentBullet.GetComponent<Bullet>();
        bullet.owner = this;

        TankAgent agent = GetComponent<TankAgent>();
        if (agent != null)
        {
            currentBullet.GetComponent<Bullet>().Init(agent);
        }
    }

    public void SetVelocity(Vector2 dir)
    {
        rb.velocity = dir * moveSpeed;
        rb.velocity += externalForces;
    }

    public void SetRotation(float angle)
    {
        rb.rotation = angle;
    }
    public void HandleFire(bool shoot)
    {
        if (shoot && CanShoot())
        {
            Fire();
        }
    }

    public void OnShellDestroyed()
    {
        currentBullet = null;
    }

    public void AddExternalForce(Vector2 force)
    {
        externalForces += force;
    }

    public void ClearExternalForces()
    {
        externalForces = Vector2.zero;
    }
}