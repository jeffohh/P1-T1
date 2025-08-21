using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankMovement : MonoBehaviour
{
    public float speed = 5f; // Speed of the tank
    public float rotationSpeed = 200f; // Speed of the tank's rotation
    public Transform turret; // Reference to the turret transform


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float moveInput = Input.GetAxis("Vertical"); // Get vertical input (W/S or Up/Down arrows)
        float turnInput = Input.GetAxis("Horizontal"); // Get horizontal input (A/D or Left/Right arrows)

        moveInput *= speed * Time.deltaTime; // Calculate movement based on input and speed
        turnInput *= rotationSpeed * Time.deltaTime; // Calculate rotation based on input and rotation speed

        transform.Translate(0, 0, moveInput); // Move the tank forward/backward
        transform.Rotate(0, turnInput, 0); // Rotate the tank left/right

        if (Input.GetKey(KeyCode.Q))
            turret.Rotate(0, rotationSpeed * Time.deltaTime, 0);
        if (Input.GetKey(KeyCode.E))
            turret.Rotate(0, -rotationSpeed * Time.deltaTime, 0);
    }
}
