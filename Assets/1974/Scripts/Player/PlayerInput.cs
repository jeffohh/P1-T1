using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(TankController))]
public class PlayerInput : MonoBehaviour
{
    private TankController controller;

    [Header("Controller")]
    public Joystick joystick;
    [Range(0f, 1f)] public float deadZone = 0.1f;

    private float cachedMove;
    private float cachedTurn;
    private bool cachedShoot;
    private bool useJoystick;

    void Awake()
    {
        controller = GetComponent<TankController>();
    }

    void Update()
    {
        useJoystick = false; 

        // ---- Joystick input ----
        float jx = 0f, jy = 0f;
        if (joystick != null)
        {
            jx = joystick.Horizontal;
            jy = joystick.Vertical;
        }
        Vector2 inputDir = new Vector2(jx, jy);

        if (inputDir.sqrMagnitude > deadZone * deadZone)
        {
            inputDir.Normalize();

      
            controller.SetVelocity(inputDir);

            float angle = Mathf.Atan2(inputDir.y, inputDir.x) * Mathf.Rad2Deg - 90f;
            controller.SetRotation(angle);

            useJoystick = true;
        }
        else
        {
           
            controller.SetVelocity(Vector2.zero);
        }

        // keyboard input
        if (!useJoystick)
        {
            float h = (Input.GetKey(KeyCode.D) ? 1f : 0f) + (Input.GetKey(KeyCode.A) ? -1f : 0f);
            float v = (Input.GetKey(KeyCode.W) ? 1f : 0f) + (Input.GetKey(KeyCode.S) ? -1f : 0f);
            cachedMove = v;
            cachedTurn = h;
        }

        // keyboard fire
        if (Input.GetKeyDown(KeyCode.Space))
            cachedShoot = true;

        // touch fire
        for (int i = 0; i < Input.touchCount; i++)
        {
            var t = Input.GetTouch(i);
            if (t.phase == TouchPhase.Began)
            {
                bool onRight = t.position.x >= Screen.width * 0.5f;
                bool overUI = false;
                if (EventSystem.current != null)
                {
                    var ped = new PointerEventData(EventSystem.current) { position = t.position };
                    var results = new List<RaycastResult>();
                    EventSystem.current.RaycastAll(ped, results);
                    overUI = results.Count > 0;
                }
                if (onRight && !overUI)
                    cachedShoot = true;
            }
        }
    }

    void FixedUpdate()
    {
        controller.HandleFire(cachedShoot);

        if (!useJoystick)
        {
            controller.Drive(cachedMove, cachedTurn, false);
        }

        cachedShoot = false;
    }
}
