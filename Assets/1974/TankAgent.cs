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
    public Transform enemyTank; // assign player tank in Inspector

    public Transform[] spawnPoints;


    void Awake()
    {
        controller = GetComponent<TankController>();
    }

    public override void OnEpisodeBegin()
    {
        // pick random spawn point
        int i = Random.Range(0, spawnPoints.Length);
        transform.position = spawnPoints[i].position;
        transform.rotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Vector from me to enemy
        Vector2 toEnemy = enemyTank.position - transform.position;

        sensor.AddObservation(toEnemy.normalized); // direction
        sensor.AddObservation(toEnemy.magnitude);  // distance
        sensor.AddObservation(transform.up);       // my facing
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        int moveAction = actions.DiscreteActions[0];   // 0=backward, 1=none, 2=forward
        int turnAction = actions.DiscreteActions[1];   // 0=left, 1=none, 2=right
        int shootAction = actions.DiscreteActions[2];  // 0=no, 1=yes

        float move = (moveAction == 0) ? -1f : (moveAction == 2) ? 1f : 0f;
        float turn = (turnAction == 0) ? -1f : (turnAction == 2) ? 1f : 0f;
        bool shoot = (shootAction == 1);

        controller.Drive(move, turn, shoot);

        AddReward(-0.001f); // time penalty to prevent idling

        // Encourage moving into line of sight
        RaycastHit2D hit = Physics2D.Raycast(transform.position, (enemyTank.position - transform.position).normalized);
        if (hit.collider != null && hit.collider.CompareTag("Tank"))
        {
            AddReward(0.01f); // reward for seeing the enemy
        }
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

    // Reward helpers, called by Shell.cs
    public void RewardForHit()
    {
        AddReward(+1f);
        EndEpisode();
    }

    public void PenalizeForGettingHit()
    {
        AddReward(-1f);
        EndEpisode();
    }
}