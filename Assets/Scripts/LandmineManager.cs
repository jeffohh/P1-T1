using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LandmineManager : MonoBehaviour
{
    [Header("Spawn Settings")]
    public int maxLandmines = 10;
    public float spawnInterval = 2f;
    public GameObject landminePrefab;
    public Tilemap groundTilemap;

    private List<LandmineSpawnZone> zones = new List<LandmineSpawnZone>();
    private int currentMineCount = 0;

    void Awake()
    {
        // Auto-find all child zones
        zones.AddRange(GetComponentsInChildren<LandmineSpawnZone>());
        Debug.Log($"Found {zones.Count} landmine zones.");
    }

    void Start()
    {
        InvokeRepeating(nameof(SpawnLandmine), 0f, spawnInterval);
    }

    void SpawnLandmine()
    {
        if (currentMineCount >= maxLandmines || zones.Count == 0)
            return;

        LandmineSpawnZone zone = zones[Random.Range(0, zones.Count)];
        Vector2? spawnPos = zone.GetValidSpawnPoint(groundTilemap);

        if (spawnPos == null)
        {
            Debug.LogWarning("Could not find a valid spawn point in zone.");
            return;
        }

        GameObject mine = Instantiate(landminePrefab, spawnPos.Value, Quaternion.identity);
        currentMineCount++;

        Landmine mineScript = mine.GetComponent<Landmine>();
        mineScript.onDestroyed += OnLandmineDestroyed;
    }

    void OnLandmineDestroyed()
    {
        currentMineCount--;
    }
}
