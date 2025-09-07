using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class Landmine : MonoBehaviour
{
    public float dropDuration = 1.5f;
    public Color armedColor = Color.red;
    public Action onDestroyed;

    private SpriteRenderer sr;
    private Collider2D col;
    private bool isArmed = false;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();

        // Initial state
        col.enabled = false;
        transform.localScale = Vector3.one * 2f; // Big size
        SetAlpha(0f); // Fully transparent
    }

    void Start()
    {
        StartCoroutine(DeploySequence());
    }

    IEnumerator DeploySequence()
    {
        float elapsed = 0f;

        Vector3 startScale = transform.localScale;
        Vector3 endScale = Vector3.one;
        float startAlpha = 0f;
        float endAlpha = 1f;

        while (elapsed < dropDuration)
        {
            float t = elapsed / dropDuration;

            transform.localScale = Vector3.Lerp(startScale, endScale, t);
            SetAlpha(Mathf.Lerp(startAlpha, endAlpha, t));

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localScale = endScale;
        SetAlpha(endAlpha);

        ArmLandmine();
    }

    void ArmLandmine()
    {
        isArmed = true;
        sr.color = armedColor;
        col.enabled = true;
        // Add any other "armed" effects here (sound, particle, etc.)
    }

    void SetAlpha(float alpha)
    {
        Color c = sr.color;
        c.a = alpha;
        sr.color = c;
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (!isArmed) return;

        TankHealth tankHealth = col.gameObject.GetComponent<TankHealth>();
       
        // Check if we hit an enemy tank
        if (tankHealth != null)
        {
            // Apply the hit and make the tank disabled regardless
            tankHealth.TakeHit();

            // Destroy the landmine
            onDestroyed?.Invoke();
            Destroy(gameObject);
        }

        
    }
}
