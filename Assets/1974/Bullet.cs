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
    // In Bullet.cs

    void OnTriggerEnter2D(UnityEngine.Collider2D col)
    {
        hasHitTarget = true;

        TankHealth tankHealth = col.gameObject.GetComponent<TankHealth>();
        TankAgent tankAgent = col.gameObject.GetComponent<TankAgent>();

        // Check if we hit an enemy tank
        if (tankHealth != null && tankHealth.gameObject != owner.gameObject)
        {
            // Check the tank's state BEFORE applying the hit
            bool wasAlreadyDisabled = tankHealth.IsDisabled();

            // Apply the hit and make the tank disabled regardless
            tankHealth.TakeHit();

            // --- REVISED REWARD LOGIC ---
            // Only give the owner agent a reward if the tank was NOT already disabled
            if (ownerAgent != null && !wasAlreadyDisabled)
            {
                ownerAgent.RewardForHit();
            }

            // The agent that got hit should always be penalized
            if (tankAgent != null && tankAgent != ownerAgent)
            {
                tankAgent.PenalizeForGettingHit();
            }
        }
        else if (col.CompareTag("Wall") && ownerAgent != null)
        {
            // This penalty is for bullets hitting walls, not the tank itself.
            // It's a different kind of "miss". It's fine to keep it.
            ownerAgent.AddReward(-0.2f);
        }

        Destroy(gameObject);
    }

}