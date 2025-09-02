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
    private Animator animator;

    private Rigidbody2D rb;

    private TankSpawner spawner;

    void Awake()
    {
        controller = GetComponent<TankController>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spawner = GetComponent<TankSpawner>();
    }

    public void TakeHit()
    {
        if (isDisabled) return; // already disabled
        isDisabled = true;
        disableTimer = disableTime;

        rb.bodyType = RigidbodyType2D.Static;

        controller.enabled = false; // stop movement + shooting

        if (animator != null)
            animator.SetTrigger("isDead");

        StartCoroutine(FlashRoutine());

        // Update Score
        if (gameManager != null)
        {
            bool isAgent = GetComponent<TankAgent>() != null;
            GameManager gameManager = this.gameManager.GetComponent<GameManager>();
            gameManager.IncrementScore(isAgent ? 1 : 2);
        }
    }

    void Update()
    {
        if (isDisabled)
        {
            disableTimer -= Time.deltaTime;
            if (disableTimer <= 0f)
            {
                spawner.MoveTankToRandomSpawn(this.gameObject);

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
