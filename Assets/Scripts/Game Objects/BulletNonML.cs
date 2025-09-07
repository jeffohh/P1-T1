using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class BulletNonML : MonoBehaviour
{
    public TankController owner;  
    public float speed = 12f;       
    public float lifeTime = 3f;

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
    }

    void Start()
    {

        rb.velocity = transform.up * speed;

        Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (owner != null && col.gameObject == owner.gameObject)
            return;

        TankHealth tankHealth = col.GetComponent<TankHealth>();
        if (tankHealth != null)
        {
            tankHealth.TakeHit(); 
                                 
        }

        Destroy(gameObject);
    }


    void OnDestroy()
    {
        if (owner != null)
        {
            owner.OnShellDestroyed();
        }
    }
}
