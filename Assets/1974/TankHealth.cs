using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankHealth : MonoBehaviour
{
    [Header("Hit Settings")]
    public float disableTime = 1.5f;   // seconds tank is disabled
    private bool isDisabled = false;

    private TankController controller;
    private SpriteRenderer spriteRenderer;
    private float disableTimer;

    private Rigidbody2D rb;

    void Awake()
    {
        controller = GetComponent<TankController>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
    }

    public void TakeHit()
    {
        if (isDisabled) return; // already disabled

        isDisabled = true;
        disableTimer = disableTime;

        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;

        controller.enabled = false; // stop movement + shooting
        StartCoroutine(FlashRoutine());
    }

    void Update()
    {
        if (isDisabled)
        {
            disableTimer -= Time.deltaTime;
            if (disableTimer <= 0f)
            {
                isDisabled = false;
                controller.enabled = true; // restore control
                spriteRenderer.enabled = true; // ensure visible
            }
        }
    }

    private System.Collections.IEnumerator FlashRoutine()
    {
        while (isDisabled)
        {
            spriteRenderer.enabled = !spriteRenderer.enabled;
            yield return new WaitForSeconds(0.125f);
        }
    }

    public bool IsDisabled()
    {
        return isDisabled;
    }
}
