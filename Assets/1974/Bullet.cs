using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 10f;
    Rigidbody2D rb;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.velocity = transform.up * speed; // move forward in local "up" direction
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Destroy bullet on impact
        Destroy(gameObject);
    }
}
