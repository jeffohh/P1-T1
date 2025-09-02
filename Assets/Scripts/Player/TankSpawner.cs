using UnityEngine;

public class TankSpawner : MonoBehaviour
{
    public GameObject spawnPointsParent; // Assign the parent object with spawn points

    // Moves the given tank to a random spawn point
    public void MoveTankToRandomSpawn(GameObject tank)
    {
        if (spawnPointsParent == null)
        {
            // Debug.LogError("Spawn Points Parent is not assigned!");
            return;
        }

        int count = spawnPointsParent.transform.childCount;

        if (count == 0)
        {
            Debug.LogWarning("No spawn points found!");
            return;
        }

        // Pick a random child
        int randomIndex = Random.Range(0, count);
        Transform spawnPoint = spawnPointsParent.transform.GetChild(randomIndex);

        // Move the tank to that spawn point
        tank.transform.position = spawnPoint.position;
        tank.transform.rotation = spawnPoint.rotation;

        // Optional: Reset velocity or other state
        Rigidbody rb = tank.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        Debug.Log("Tank moved to spawn point: " + spawnPoint.name);
    }
}