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
            rb.position = newWorldPos;

            // Adjust the object's velocity to match the linked portal's orientation, make it mirror the velocity relative to the portal's facing direction
            Vector3 localVelocity = transform.InverseTransformDirection(rb.velocity);
            Vector2 reflectedLocalVelocity = Vector2.Reflect(localVelocity, Vector2.up);
            Vector2 newWorldVelocity = linkedPortal.transform.TransformDirection(reflectedLocalVelocity);
            rb.velocity = newWorldVelocity;


            // Adjust object's rotation to match the linked portal's orientation based on the difference between the two portals
            Vector2 facingDir = collision.transform.right; // tank's local right = "forward"
            Vector2 reflectedFacingDir = Vector2.Reflect(facingDir, transform.up); // mirror over portal's surface
            float newRotation = Mathf.Atan2(reflectedFacingDir.y, reflectedFacingDir.x) * Mathf.Rad2Deg - 90f;
            rb.rotation = newRotation;

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
