using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public TankController owner;
    private TankAgent ownerAgent;
    public float lifeTime = 3f;
    private bool hasHitTarget = false;

    public void Init(TankAgent tankOwner)
    {
        ownerAgent = tankOwner;
    }

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

        // If bullet expired without hitting anything, penalize slightly
        if (!hasHitTarget && ownerAgent != null)
        {
            ownerAgent.OnMissedShot();
        }
    }

    void OnTriggerEnter2D(UnityEngine.Collider2D col)
    {
        hasHitTarget = true; // Mark that we hit something

        // Check if we hit a tank
        TankHealth tank = col.gameObject.GetComponent<TankHealth>();

        if (tank != null && tank.gameObject != owner.gameObject)
        {
            tank.TakeHit();
        }

        TankAgent tankAgent = col.gameObject.GetComponent<TankAgent>();
        if (tankAgent != null && tankAgent != ownerAgent)
        {
            // This shell hit the other AI (good hit!)
            ownerAgent.RewardForHit();
            tankAgent.PenalizeForGettingHit();
        }
        else if (col.CompareTag("Wall") && ownerAgent != null)
        {
            ownerAgent.AddReward(-0.05f); // Increased penalty for hitting walls
        }

        Destroy(gameObject);
    }
}