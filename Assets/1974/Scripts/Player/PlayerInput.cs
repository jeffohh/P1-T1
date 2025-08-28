using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TankController))]
public class PlayerInput : MonoBehaviour
{
    TankController controller;

    [Header("Controller")]
    public Joystick joystick; 
    void Awake()
    {
        controller = GetComponent<TankController>();
    }

    void FixedUpdate()
    {
        //float move = Input.GetKey(KeyCode.W) ? 1f : Input.GetKey(KeyCode.S) ? -1f : 0f;
        //float turn = 0f;
        //if (Input.GetKey(KeyCode.A)) turn = -1f;
        //if (Input.GetKey(KeyCode.D)) turn = 1f;

        //bool shoot = Input.GetKey(KeyCode.Space);

        //controller.Drive(move, turn, shoot);


        // joystick input
        float h = joystick.Horizontal;
        float v = joystick.Vertical;
        float move = v;  
        float turn = -h;  
        bool shoot = false;
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Input.mousePosition;
            if (mousePos.x < Screen.width / 2)
            {
                shoot = true;
            }
        }
    }
}
