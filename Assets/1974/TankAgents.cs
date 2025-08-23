using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;

[RequireComponent(typeof(TankController))]
public class TankAgents : Agent
{
    TankController controller;
    void Awake()
    {
        controller = GetComponent<TankController>();
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // Discrete actions: [turn, move, shoot]
        int turn = actions.DiscreteActions[0];   // 0=left,1=none,2=right
        int move = actions.DiscreteActions[1];   // 0=off,1=forward
        int shoot = actions.DiscreteActions[2];  // 0=no,1=yes

        float moveInput = (move == 1) ? 1f : 0f;
        float turnInput = (turn == 0) ? 1f : (turn == 2 ? -1f : 0f);
        bool shootInput = (shoot == 1);

        controller.Drive(moveInput, turnInput, shootInput);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var d = actionsOut.DiscreteActions;
        d[0] = Input.GetKey(KeyCode.A) ? 0 : (Input.GetKey(KeyCode.D) ? 2 : 1);
        d[1] = Input.GetKey(KeyCode.W) ? 1 : 0;
        d[2] = Input.GetKey(KeyCode.Space) ? 1 : 0;
    }
}
