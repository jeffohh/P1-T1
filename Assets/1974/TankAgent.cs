using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

[RequireComponent(typeof(TankController))]
public class TankAgent : Agent
{
    private TankController controller;
    public Transform enemyTank;
    public Transform[] spawnPoints;

    [Header("AI Training Settings")]
    public float rewardForFacing = 0.005f;
    public float rewardForClosing = 0.002f;
    public float penaltyForDistance = 0.001f;

    private Vector3 lastPosition;
    private float lastDistanceToEnemy;
    private bool hasLineOfSight;
    private float timeSinceLastShot;

    void Awake()
    {
        controller = GetComponent<TankController>();
    }

    public override void OnEpisodeBegin()
    {
        // Reset state
        timeSinceLastShot = 0f;
        hasLineOfSight = false;

        // Pick random spawn point
        int i = Random.Range(0, spawnPoints.Length);
        transform.position = spawnPoints[i].position;
        transform.rotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));

        lastPosition = transform.position;
        lastDistanceToEnemy = Vector3.Distance(transform.position, enemyTank.position);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // My position and rotation (normalized to map bounds)
        sensor.AddObservation(transform.position / 20f); // my position (2)
        sensor.AddObservation(transform.up); // my facing direction (2)

        // Enemy position and rotation
        sensor.AddObservation(enemyTank.position / 20f); // enemy position (2)
        sensor.AddObservation(enemyTank.up); // enemy facing direction (2)

        // Relative information (still useful for direct decision making)
        Vector2 toEnemy = enemyTank.position - transform.position;
        sensor.AddObservation(toEnemy.normalized); // direction to enemy (2)
        sensor.AddObservation(toEnemy.magnitude / 20f); // normalized distance (1)

        // Angular information for aiming
        float angleToEnemy = Vector2.SignedAngle(transform.up, toEnemy);
        sensor.AddObservation(angleToEnemy / 180f); // normalized angle (-1 to 1) (1)

        // Velocity information
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        sensor.AddObservation(rb.velocity / 10f); // my velocity (2)

        // Enemy velocity
        Rigidbody2D enemyRb = enemyTank.GetComponent<Rigidbody2D>();
        if (enemyRb != null)
        {
            sensor.AddObservation(enemyRb.velocity / 10f); // enemy velocity (2)
        }
        else
        {
            sensor.AddObservation(Vector2.zero); // (2)
        }

        // Tactical information
        sensor.AddObservation(hasLineOfSight ? 1f : 0f); // can see enemy (1)
        sensor.AddObservation(controller.CanShoot() ? 1f : 0f); // can shoot (1)
        sensor.AddObservation(Mathf.Min(timeSinceLastShot / 3f, 1f)); // time since shot (1)

        // Predictive aiming helper - where enemy will be in 1 second
        Vector3 predictedEnemyPos = enemyTank.position + (enemyRb != null ? (Vector3)enemyRb.velocity : Vector3.zero);
        Vector2 toPredictedEnemy = predictedEnemyPos - transform.position;
        sensor.AddObservation(toPredictedEnemy.normalized); // direction to predicted position (2)
        float angleToPredicted = Vector2.SignedAngle(transform.up, toPredictedEnemy);
        sensor.AddObservation(angleToPredicted / 180f); // angle to predicted position (1)

        // Total: 24 observations
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        int moveAction = actions.DiscreteActions[0];   // 0=backward, 1=none, 2=forward
        int turnAction = actions.DiscreteActions[1];   // 0=left, 1=none, 2=right
        int shootAction = actions.DiscreteActions[2];  // 0=no, 1=yes

        float move = (moveAction == 0) ? -1f : (moveAction == 2) ? 1f : 0f;
        float turn = (turnAction == 0) ? -1f : (turnAction == 2) ? 1f : 0f;
        bool shoot = (shootAction == 1);

        // Track shooting attempts
        if (shoot && controller.CanShoot())
        {
            timeSinceLastShot = 0f;
        }

        controller.Drive(move, turn, shoot);

        // Calculate rewards
        CalculateRewards(move, turn, shoot);

        timeSinceLastShot += Time.fixedDeltaTime;
    }

    private void CalculateRewards(float move, float turn, bool shoot)
    {
        Vector3 toEnemy = enemyTank.position - transform.position;
        float currentDistance = toEnemy.magnitude;

        // 1. Reward for facing the enemy
        float angleToEnemy = Vector2.Angle(transform.up, toEnemy);
        float facingReward = Mathf.Lerp(rewardForFacing, -rewardForFacing / 2f, angleToEnemy / 180f);
        AddReward(facingReward);

        // 2. Reward for getting closer when far, maintaining distance when close
        float optimalDistance = 8f; // Optimal engagement distance
        if (currentDistance > optimalDistance && currentDistance < lastDistanceToEnemy)
        {
            AddReward(rewardForClosing); // Reward for closing distance when far
        }
        else if (currentDistance < optimalDistance && currentDistance > lastDistanceToEnemy)
        {
            AddReward(rewardForClosing * 0.5f); // Small reward for backing up when too close
        }

        // 3. Distance-based penalty (encourages finding good engagement range)
        float distancePenalty = Mathf.Abs(currentDistance - optimalDistance) * penaltyForDistance / 20f;
        AddReward(-distancePenalty);

        // 4. Line of sight detection and reward
        hasLineOfSight = CheckLineOfSight();
        if (hasLineOfSight)
        {
            AddReward(0.015f); // Increased reward for seeing the enemy

            // Extra reward for good aiming with line of sight
            if (angleToEnemy < 10f) // within 10 degrees
            {
                AddReward(0.03f);
            }
            else if (angleToEnemy < 30f) // within 30 degrees  
            {
                AddReward(0.01f);
            }
        }

        // 5. Predictive aiming reward - reward for aiming where enemy will be
        Rigidbody2D enemyRb = enemyTank.GetComponent<Rigidbody2D>();
        if (enemyRb != null && enemyRb.velocity.magnitude > 0.1f)
        {
            Vector3 predictedPos = enemyTank.position + (Vector3)enemyRb.velocity * 0.8f; // Predict ~0.8s ahead
            Vector3 toPredicted = predictedPos - transform.position;
            float angleToPredicted = Vector2.Angle(transform.up, toPredicted);

            if (angleToPredicted < 15f && hasLineOfSight)
            {
                AddReward(0.02f); // Reward predictive aiming
            }
        }

        // 6. Shooting behavior rewards/penalties
        if (shoot && controller.CanShoot())
        {
            if (!hasLineOfSight)
            {
                AddReward(-0.08f); // Higher penalty for blind shooting
            }
            else if (angleToEnemy > 45f)
            {
                AddReward(-0.05f); // Penalty for poorly aimed shots
            }
            else if (angleToEnemy < 15f)
            {
                AddReward(0.05f); // Reward for well-aimed shots
            }
        }

        // 7. Tactical positioning - reward for good positioning relative to enemy facing
        Vector3 enemyForward = enemyTank.up;
        Vector3 toMe = transform.position - enemyTank.position;
        float enemyAimingAtMe = Vector2.Angle(enemyForward, toMe);

        // Reward for being out of enemy's direct aim
        if (enemyAimingAtMe > 45f)
        {
            AddReward(0.008f);
        }

        // 8. Movement rewards
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (currentDistance > optimalDistance * 1.5f && rb.velocity.magnitude > 0.1f)
        {
            AddReward(0.002f); // Reward movement when far from optimal range
        }

        // 9. Small time penalty to encourage decisive action
        AddReward(-0.0008f);

        // Update tracking variables
        lastDistanceToEnemy = currentDistance;
        lastPosition = transform.position;
    }

    private bool CheckLineOfSight()
    {
        Vector2 direction = (enemyTank.position - transform.position).normalized;
        float distance = Vector2.Distance(transform.position, enemyTank.position);

        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, distance);

        // If we hit the enemy tank directly, or if we hit nothing (clear path)
        if (hit.collider == null || hit.collider.CompareTag("Tank"))
        {
            return true;
        }

        return false;
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var actions = actionsOut.DiscreteActions;

        // Move (W/S)
        if (Input.GetKey(KeyCode.W)) actions[0] = 2;
        else if (Input.GetKey(KeyCode.S)) actions[0] = 0;
        else actions[0] = 1;

        // Turn (A/D)
        if (Input.GetKey(KeyCode.A)) actions[1] = 0;
        else if (Input.GetKey(KeyCode.D)) actions[1] = 2;
        else actions[1] = 1;

        // Shoot (Space)
        actions[2] = Input.GetKey(KeyCode.Space) ? 1 : 0;
    }

    public void RewardForHit()
    {
        AddReward(+2f); // Increased reward for successful hit
        EndEpisode();
    }

    public void PenalizeForGettingHit()
    {
        AddReward(-2f); // Increased penalty for getting hit
        EndEpisode();
    }

    // Called when agent shoots but misses
    public void OnMissedShot()
    {
        AddReward(-0.1f);
    }
}