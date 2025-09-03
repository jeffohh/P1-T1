using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalWall : MonoBehaviour
{
    [Header("Portal Link")]
    public PortalWall linkedPortal;

    private HashSet<Collider2D> objectsInPortal = new HashSet<Collider2D>();

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Object entered portal: " + collision.name);
        if (linkedPortal == null) return;

        if (objectsInPortal.Contains(collision)) return;

        Rigidbody2D rb = collision.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            // Teleport the object to the linked portal's position
            Vector3 localPos = transform.InverseTransformPoint(collision.transform.position);
            Vector3 newWorldPos = linkedPortal.transform.TransformPoint(localPos);

            Vector3 portalCenter = linkedPortal.transform.position;

            Vector3 inwardDir = (portalCenter - newWorldPos).normalized;
            float offsetDistance = 0.5f; // tweak this value

            rb.position = newWorldPos + inwardDir * offsetDistance;

            // Adjust object's rotation to match the linked portal's orientation based on the difference between the two portals
            Quaternion portalDeltaRotation = linkedPortal.transform.rotation * Quaternion.Inverse(transform.rotation);
            Vector2 newFacingDir = portalDeltaRotation * collision.transform.right;
            float newRotation = Mathf.Atan2(newFacingDir.y, newFacingDir.x) * Mathf.Rad2Deg;
            rb.rotation = newRotation;

            Vector2 newVelocity = portalDeltaRotation * rb.velocity;
            rb.velocity = newVelocity;

            // Add to the set to prevent immediate re-teleportation
            linkedPortal.objectsInPortal.Add(collision);

            // Start a coroutine to remove the object from the set after a short delay
            if (collision.CompareTag("Bullet"))
                linkedPortal.StartCoroutine(linkedPortal.DelayedRemove(collision));
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (objectsInPortal.Contains(collision))
        {
            objectsInPortal.Remove(collision);
        }
    }

    private IEnumerator DelayedRemove(Collider2D collider, float delay = 0.1f)
    {
        yield return new WaitForSeconds(delay);
        objectsInPortal.Remove(collider);
    }
}
