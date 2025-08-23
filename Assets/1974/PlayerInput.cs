using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TankController))]
public class PlayerInput : MonoBehaviour
{
    TankController controller;
    void Awake()
    {
        controller = GetComponent<TankController>();
    }

    void FixedUpdate()
    {
        float move = Input.GetKey(KeyCode.W) ? 1f : Input.GetKey(KeyCode.S) ? -1f : 0f;
        float turn = 0f;
        if (Input.GetKey(KeyCode.A)) turn = -1f;
        if (Input.GetKey(KeyCode.D)) turn = 1f;

        bool shoot = Input.GetKey(KeyCode.Space);

        controller.Drive(move, turn, shoot);
    }
}
