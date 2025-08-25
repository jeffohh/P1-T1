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

    // --- Action Smoothing ---
    private int lastMoveAction = 1; // 1 = none
    private int lastTurnAction = 1; // 1 = none

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

        lastMoveAction = 1;
        lastTurnAction = 1;

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
        int moveAction = actions.DiscreteActions[0];   // 0=backward, 1=none, 2=forward
        int turnAction = actions.DiscreteActions[1];   // 0=left, 1=none, 2=right
        int shootAction = actions.DiscreteActions[2];  // 0=no, 1=yes

        // --- Apply Action Smoothing Penalty ---
        // Penalize switching from forward to backward (or vice versa) directly.
        if ((lastMoveAction == 0 && moveAction == 2) || (lastMoveAction == 2 && moveAction == 0))
        {
            AddReward(-0.02f);
        }
        // Penalize switching from left to right (or vice versa) directly.
        if ((lastTurnAction == 0 && turnAction == 2) || (lastTurnAction == 2 && turnAction == 0))
        {
            AddReward(-0.02f);
        }

        lastMoveAction = moveAction;
        lastTurnAction = turnAction;

        // --- Control the tank ---
        float move = (moveAction == 0) ? -1f : (moveAction == 2) ? 1f : 0f;
        float turn = (turnAction == 0) ? -1f : (turnAction == 2) ? 1f : 0f;
        bool shoot = (shootAction == 1);

        controller.Drive(move, turn, shoot);

        if (CheckLineOfSight())
        {
            timeSinceLastLoS = 0f; // Reset timer if we can see the enemy
        }
        else
        {
            timeSinceLastLoS += Time.fixedDeltaTime; // Increment timer if we can't
        }

        CalculateRewards(move, turn, shoot);

        stepsInEpisode++;
        if (stepsInEpisode >= maxStepsPerEpisode)
        {
            EndEpisode(); // End the "round"
        }
    }

    private void CalculateRewards(float move, float turn, bool shoot)
    {
        // Small penalty to encourage action over inaction
        AddReward(-0.001f);

        // --- REVISED AND FINAL Shooting Rewards ---
        bool hasLineOfSight = CheckLineOfSight();
        Vector3 toEnemy = enemyTank.position - transform.position;
        float angleToEnemy = Vector2.Angle(transform.up, toEnemy);

        if (shoot && controller.CanShoot())
        {
            // Increase the unconditional cost. Firing is a commitment.
            AddReward(-0.05f);

            if (!hasLineOfSight)
            {
                // Make shooting at walls extremely punishing.
                AddReward(-0.5f);
            }
            else if (angleToEnemy < 5f) // Stricter angle for a "good shot"
            {
                // The reward for a perfect shot is still there.
                AddReward(0.1f);
            }
            else
            {
                // Significantly increase the penalty for a badly aimed but clear shot.
                AddReward(-0.25f);
            }
        }

        if (timeSinceLastLoS > 3.0f) // If it's been over 3 seconds since seeing the enemy
        {
            AddReward(-0.005f); // Apply a small, continuous penalty
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
        var actions = actionsOut.DiscreteActions;
        actions[0] = 1; actions[1] = 1; actions[2] = 0;
        if (Input.GetKey(KeyCode.W)) actions[0] = 2; else if (Input.GetKey(KeyCode.S)) actions[0] = 0;
        if (Input.GetKey(KeyCode.A)) actions[1] = 0; else if (Input.GetKey(KeyCode.D)) actions[1] = 2;
        if (Input.GetKey(KeyCode.Space)) actions[2] = 1;
    }

    public void RewardForHit() { AddReward(1.0f); }
    public void PenalizeForGettingHit() { AddReward(-1.0f); }
    public void OnMissedShot() { AddReward(-0.4f); } // Increased penalty
}