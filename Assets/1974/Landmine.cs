using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Landmine : MonoBehaviour
{
    void OnTriggerEnter2D(UnityEngine.Collider2D col)
    {
       
        TankHealth tankHealth = col.gameObject.GetComponent<TankHealth>();
       
        // Check if we hit an enemy tank
        if (tankHealth != null)
        {
            // Apply the hit and make the tank disabled regardless
            tankHealth.TakeHit();
            Destroy(gameObject);
        }

        
    }
}
