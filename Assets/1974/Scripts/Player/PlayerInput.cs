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
        bool shoot = Input.GetKey(KeyCode.Space);
        controller.Drive(Input.GetAxisRaw("Vertical"), Input.GetAxisRaw("Horizontal"), shoot);
    }
}
