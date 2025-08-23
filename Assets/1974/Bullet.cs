using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public TankController owner;
    public float lifeTime = 3f;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void OnDestroy()
    {
        if (owner != null)
        {
            owner.OnShellDestroyed();
        }
    }

    void OnTriggerEnter2D(UnityEngine.Collider2D col)
    {
        // Check if we hit a tank
        TankHealth tank = col.gameObject.GetComponent<TankHealth>();
        if (tank != null && tank.gameObject != owner.gameObject)
        {
            tank.TakeHit();
        }

        Destroy(gameObject); // remove shell
    }
}
