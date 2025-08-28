using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

[RequireComponent(typeof(TankController))]
[RequireComponent(typeof(TankHealth))]
public class TankAgent : Agent
{
    private TankController controller;
    private TankHealth health;
    public Transform enemyTank;
    public Transform[] spawnPoints;

    [Header("AI Training Settings")]
    public float optimalDistance = 8f;

    private int stepsInEpisode = 0;
    private int maxStepsPerEpisode = 5000; // ~50 seconds

    private float timeSinceLastLoS = 0f;

    void Awake()
    {
        controller = GetComponent<TankController>();
        health = GetComponent<TankHealth>();
    }

    public override void OnEpisodeBegin()
    {
        // Reset state
        transform.position = spawnPoints[Random.Range(0, spawnPoints.Length)].position;
        transform.rotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));

        enemyTank.position = spawnPoints[Random.Range(0, spawnPoints.Length)].position;
        enemyTank.rotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));

        stepsInEpisode = 0;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // My position and rotation (normalized to map bounds, assuming ~20 units)
        sensor.AddObservation(transform.localPosition / 20f); // (2)
        sensor.AddObservation(transform.up); // My facing direction (2)

        // Enemy position and rotation
        sensor.AddObservation(enemyTank.localPosition / 20f); // (2)
        sensor.AddObservation(enemyTank.up); // Enemy facing direction (2)

        // Relative information
        Vector2 toEnemy = enemyTank.position - transform.position;
        sensor.AddObservation(toEnemy.normalized); // Direction to enemy (2)
        sensor.AddObservation(toEnemy.magnitude / 20f); // Normalized distance (1)

        float angleToEnemy = Vector2.SignedAngle(transform.up, toEnemy);
        sensor.AddObservation(angleToEnemy / 180f); // Normalized angle (-1 to 1) (1)

        // Velocity information
        sensor.AddObservation(GetComponent<Rigidbody2D>().velocity / 10f); // My velocity (2)
        sensor.AddObservation(enemyTank.GetComponent<Rigidbody2D>().velocity / 10f); // Enemy velocity (2)

        // Tactical information
        sensor.AddObservation(CheckLineOfSight());      // Can see enemy (1)
        sensor.AddObservation(controller.CanShoot());   // Can shoot (1)
        sensor.AddObservation(health.IsDisabled());     // Am I stunned/disabled (1)
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveAction = actions.ContinuousActions[0];   // 0=backward, 1=none, 2=forward
        float turnAction = actions.ContinuousActions[1];   // 0=left, 1=none, 2=right
        int shootAction = actions.DiscreteActions[0];  // 0=no, 1=yes

        bool shoot = (shootAction == 1);

        // --- NEW: PUNISH WASTED SHOT ATTEMPTS ---
        if (shoot && !controller.CanShoot())
        {
            // The agent tried to shoot when it couldn't. This is button mashing.
            AddReward(-0.05f);
        }
        // ----------------------------------------

        controller.Drive(moveAction, turnAction, shoot);

        if (CheckLineOfSight())
        {
            timeSinceLastLoS = 0f; // Reset timer if we can see the enemy
        }
        else
        {
            timeSinceLastLoS += Time.fixedDeltaTime; // Increment timer if we can't
        }

        CalculateRewards(moveAction, turnAction, shoot);

        stepsInEpisode++;
        if (stepsInEpisode >= maxStepsPerEpisode)
        {
            EndEpisode(); // End the "round"
        }
    }

    private void CalculateRewards(float move, float turn, bool shoot)
    {
        // --- A. Small Time Penalty (encourages efficiency) ---
        AddReward(-0.001f);

        // --- B. Positioning and Aiming Analysis ---
        bool hasLineOfSight = CheckLineOfSight();
        Vector3 toEnemy = enemyTank.position - transform.position;
        float distanceToEnemy = toEnemy.magnitude;
        float angleToEnemy = Vector2.Angle(transform.up, toEnemy);

        // --- C. HEAVILY PUNISH HIDING (Increased "Frustration") ---
        if (timeSinceLastLoS > 2.0f) // Shorten the grace period to 2 seconds
        {
            // Drastically increase the penalty. Hiding is now very costly.
            AddReward(-0.1f);
        }

        // --- D. EXPLICITLY REWARD EXPLORATION (The "Anti-Wall-Hugging" Reward) ---
        // Check a small radius around the tank for walls.
        if (Physics2D.OverlapCircle(transform.position, 3f, LayerMask.GetMask("Walls")) == null)
        {
            // If there are no walls nearby, the tank is in an open area. Reward this bravery.
            AddReward(0.05f);
        }

        // --- E. The "Tactical Advantage" Reward Stream (Still important) ---
        if (hasLineOfSight)
        {
            // This logic remains a great way to reward good positioning when engaged.
            AddReward(0.01f); // Base reward for LoS

            float distanceScore = 1.0f - Mathf.Abs(distanceToEnemy - optimalDistance) / optimalDistance;
            AddReward(Mathf.Max(0, distanceScore) * 0.02f);

            float facingScore = 1.0f - (angleToEnemy / 90f);
            AddReward(Mathf.Max(0, facingScore) * 0.03f);
        }

        // --- 5. SHOOTING LOGIC (Unchanged but now better supported) ---
        // Your highly punishing shooting logic is still essential.
        if (shoot && controller.CanShoot())
        {
            AddReward(-0.05f); // Unconditional cost to fire
            if (!hasLineOfSight)
            {
                AddReward(-0.5f); // Punish blind shots
            }
            else if (angleToEnemy < 5f)
            {
                AddReward(0.1f); // Reward well-aimed shots
            }
            else
            {
                AddReward(-0.25f); // Punish poorly-aimed shots
            }
        }

        // --- Wall Avoidance (Backwards) ---
        if (move < 0) // If moving backward
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, -transform.up, 2f, LayerMask.GetMask("Walls"));
            if (hit.collider != null)
            {
                // Penalize moving backward into a nearby wall
                AddReward(-0.1f);
            }
        }
    }

    private bool CheckLineOfSight()
    {
        Vector2 firePointPos = controller.firePoint.position;
        Vector2 direction = ((Vector2)enemyTank.position - firePointPos).normalized;
        float distance = Vector2.Distance(firePointPos, enemyTank.position);
        int layerMask = 1 << gameObject.layer;

        RaycastHit2D hit = Physics2D.Raycast(firePointPos, direction, distance, ~layerMask);
        return hit.collider == null || hit.transform == enemyTank;
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        var discreteActionsOut = actionsOut.DiscreteActions;

        continuousActionsOut[0] = Input.GetAxis("Vertical");   // Forward/back
        continuousActionsOut[1] = Input.GetAxis("Horizontal"); // Turn

        discreteActionsOut[0] = Input.GetKey(KeyCode.Space) ? 1 : 0; // Shoot
    }

    public void RewardForHit() { AddReward(1.0f); }
    public void PenalizeForGettingHit() { AddReward(-1.0f); }
    public void OnMissedShot() { AddReward(-0.4f); } // Increased penalty
}