using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float rotationSpeed = 360f;
    Rigidbody2D rb;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Forward/backward
        float move = Input.GetAxis("Vertical") * moveSpeed;
        // Rotation
        float rotate = -Input.GetAxis("Horizontal") * rotationSpeed;

        // Apply movement
        rb.velocity = transform.up * move;
        rb.MoveRotation(rb.rotation + rotate * Time.fixedDeltaTime);
    }
}
