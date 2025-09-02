using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(BoxCollider2D))]
public class KinematicForceEmitter2D : MonoBehaviour
{
    public float forceMultiplier = 10f;
    public LayerMask hitLayers;

    private Rigidbody2D rb;
    private BoxCollider2D col;

    private Vector2 lastPosition;
    private float lastRotationZ;

    private ContactFilter2D contactFilter;
    private RaycastHit2D[] castResults = new RaycastHit2D[10];

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<BoxCollider2D>();
        lastPosition = transform.position;
        lastRotationZ = transform.eulerAngles.z;

        contactFilter = new ContactFilter2D();
        contactFilter.SetLayerMask(hitLayers);
        contactFilter.useTriggers = false;
    }

    void FixedUpdate()
    {
        Vector2 currentPosition = transform.position;
        float currentRotationZ = transform.eulerAngles.z;

        Vector2 linearVelocity = (currentPosition - lastPosition) / Time.fixedDeltaTime;
        float angularVelocityRad = Mathf.DeltaAngle(lastRotationZ, currentRotationZ) / Time.fixedDeltaTime * Mathf.Deg2Rad;

        // Always try a cast — even if position didn't change
        Vector2 move = currentPosition - lastPosition;
        int hitCount = col.Cast(move.normalized, contactFilter, castResults, move.magnitude + 0.01f);

        for (int i = 0; i < hitCount; i++)
        {
            var hit = castResults[i];
            if (hit.rigidbody != null && hit.rigidbody.gameObject != gameObject)
            {
                Vector2 r = hit.point - (Vector2)transform.position;
                Vector2 tangentialVelocity = Vector2.Perpendicular(r.normalized) * angularVelocityRad * r.magnitude;

                Vector2 totalVelocity = linearVelocity + tangentialVelocity;

                float alignment = Vector2.Dot(totalVelocity.normalized, (hit.rigidbody.position - (Vector2)transform.position).normalized);
                float strength = totalVelocity.magnitude * Mathf.Clamp01(alignment) * forceMultiplier;

                hit.rigidbody.WakeUp();
                hit.rigidbody.AddForceAtPosition(totalVelocity.normalized * strength, hit.point, ForceMode2D.Impulse);

                Debug.DrawRay(hit.point, totalVelocity.normalized * 0.3f, Color.red, 0.2f);
            }
        }

        lastPosition = currentPosition;
        lastRotationZ = currentRotationZ;
    }
}
