using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(BoxCollider2D))]
public class LandmineSpawnZone : MonoBehaviour
{
    private Collider2D spawnArea;
    public LayerMask wallLayerMask;
    private Bounds cachedBounds;

    void Awake()
    {
        spawnArea = GetComponent<Collider2D>();
        spawnArea.isTrigger = true;
        cachedBounds = spawnArea.bounds;
        spawnArea.enabled = false;
    }

    public Vector2? GetValidSpawnPoint(Tilemap tilemap)
    {
        const int maxAttempts = 10;

        for (int i = 0; i < maxAttempts; i++)
        {
            float x = Random.Range(cachedBounds.min.x, cachedBounds.max.x);
            float y = Random.Range(cachedBounds.min.y, cachedBounds.max.y);
            Vector2 worldPos = new Vector2(x, y);

            // Optional: Check for wall colliders
            if (Physics2D.OverlapPoint(worldPos, wallLayerMask) == null)
            {
                return worldPos;
            }
        }

        return null;
    }

    void OnDrawGizmosSelected()
    {
        if (spawnArea == null)
            spawnArea = GetComponent<Collider2D>();

        if (spawnArea == null) return;

        Gizmos.color = new Color(1f, 0.8f, 0f, 0.2f);
        Gizmos.DrawCube(spawnArea.bounds.center, spawnArea.bounds.size);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(spawnArea.bounds.center, spawnArea.bounds.size);
    }
}
