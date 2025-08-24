using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public TankController owner;
    private TankAgent ownerAgent;
    public float lifeTime = 3f;

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

        TankAgent tankAgent = col.gameObject.GetComponent<TankAgent>();
        if (tankAgent != null && tankAgent != ownerAgent)
        {
            // This shell hit the other AI (bad for them)
            ownerAgent.RewardForHit();
            tankAgent.PenalizeForGettingHit();
        }
        else if (col.CompareTag("Wall") && ownerAgent != null)
        {
            ownerAgent.AddReward(-0.1f); // wasted shot
            Destroy(gameObject);
        }

    }
}
