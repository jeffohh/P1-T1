using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankHealth : MonoBehaviour
{
    [Header("Hit Settings")]
    public float disableTime = 1.5f;   // seconds tank is disabled
    private bool isDisabled = false;

    public GameObject gameManager;

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

        bool isAgent = GetComponent<TankAgent>() != null;
        if (isAgent)
        {
            GameManager gameManager = this.gameManager.GetComponent<GameManager>();
            gameManager.IncrementScore(1);
        }

        isDisabled = true;
        disableTimer = disableTime;

        rb.bodyType = RigidbodyType2D.Static;

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
                rb.bodyType = RigidbodyType2D.Dynamic;
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
