using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class TankController : MonoBehaviour
{
    [Header("Settings")]
    public float moveSpeed = 3f;
    public float rotationSpeed = 180f;

    public Transform firePoint;
    public GameObject bulletPrefab;
    private GameObject currentBullet; // track active shell

    Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0; // important for top-down
    }

    // Called every frame by input drivers
    public void Drive(float moveInput, float turnInput, bool shoot)
    {
        // Move
        Vector2 forward = moveInput * moveSpeed * transform.up;
        rb.velocity = forward;

        // Rotate
        float newRot = rb.rotation - turnInput * rotationSpeed * Time.fixedDeltaTime;
        rb.MoveRotation(newRot);

        // Shoot
        if (shoot)
        {
            Fire();
        }
    }

    void Fire()
    {
        if (currentBullet != null) return; // only one shell at a time

        currentBullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        var rbBullet = currentBullet.GetComponent<Rigidbody2D>();
        rbBullet.velocity = firePoint.up * 12f;

        Bullet bullet = currentBullet.GetComponent<Bullet>();
        bullet.owner = this;
    }

    public void OnShellDestroyed()
    {
        currentBullet = null;
    }
}
