using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankFire : MonoBehaviour
{
    public GameObject projoctilePrefab; // Reference to the projectile prefab
    public Transform barrel; // Reference to the turret transform

    void Update()
    {

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Space key pressed, firing projectile.");
            FireProjectile();
        }
    }

    void FireProjectile()
    {
        GameObject projectile = Instantiate(projoctilePrefab, barrel.position, barrel.rotation);
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Debug.Log("Rigidbody found, applying force.");
            rb.AddForce(barrel.forward * 500f); // Adjust the force as needed
        }
    }
}
