using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Destroy(gameObject, 5f); // Destroy the projectile after 5 seconds
    }

    private void OnCollisionEnter(Collision collision)
    {
        Destroy(gameObject);
    }
}
