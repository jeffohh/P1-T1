using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class IceBlock : MonoBehaviour
{
    [Header("Targets (optional)")]
    [Tooltip("可选：指定玩家。若为空，则对任意带 TankHealth 的对象造成伤害。")]
    public TankHealth player;
    [Tooltip("可选：指定敌人。若为空，则对任意带 TankHealth 的对象造成伤害。")]
    public TankHealth enemy;

    [Header("Movement")]
    [Tooltip("倾斜→推力大小")]
    public float acceleration = 20f;
    [Tooltip("最大滑行速度")]
    public float maxSpeed = 8f;
    [Tooltip("陀螺仪/加速度计灵敏度缩放")]
    public float tiltSensitivity = 1.2f;

    [Header("Damage")]
    [Tooltip("造成伤害所需的最小速度")]
    public float hitSpeedThreshold = 2.5f;
    [Tooltip("对同一对象的命中冷却（秒）")]
    public float hitCooldown = 0.6f;

    [Header("Debug / PC Fallback")]
    [Tooltip("在无陀螺仪设备或编辑器中，使用键盘 WASD/箭头作为后备")]
    public bool enableKeyboardFallback = true;

    private Rigidbody2D rb;
    private readonly Dictionary<int, float> lastHitTime = new Dictionary<int, float>();
    private bool gyroAvailable;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        // 建议在 Inspector：Body Type = Dynamic, Gravity Scale = 0, Linear Drag ~ 0, Angular Drag ~ 0
        rb.gravityScale = 0f;
        rb.drag = 0f;
        rb.angularDrag = 0.05f;
    }

    void OnEnable()
    {
        // 启用陀螺仪（若设备支持）
        gyroAvailable = SystemInfo.supportsGyroscope || Application.isMobilePlatform;
        if (SystemInfo.supportsGyroscope)
        {
            Input.gyro.enabled = true;
        }
    }

    void Update()
    {
        // 没有任何逻辑写在 Update；留空即可（或用于调试）
    }

    void FixedUpdate()
    {
        Vector2 tilt = ReadTiltVector(); 

        rb.AddForce(tilt * acceleration, ForceMode2D.Force);

        if (rb.velocity.sqrMagnitude > maxSpeed * maxSpeed)
        {
            rb.velocity = rb.velocity.normalized * maxSpeed;
        }
    }

    private Vector2 ReadTiltVector()
    {
        Vector2 v = Vector2.zero;

        if (gyroAvailable)
        {

            Vector3 acc = Input.acceleration;
            // 竖屏
            //v = new Vector2(acc.x, acc.y) * tiltSensitivity;

            // 横屏
             v = new Vector2(acc.x, acc.y) * tiltSensitivity;
        }

        if (v.sqrMagnitude < 0.0001f && enableKeyboardFallback)
        {
            // WASD
            float h = Input.GetAxisRaw("Horizontal"); // A/D or ←/→
            float k = Input.GetAxisRaw("Vertical");   // W/S or ↑/↓
            v = new Vector2(h, k);
        }

        return v;
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        TryDamage(col.collider);
    }

    void OnCollisionStay2D(Collision2D col)
    {
        TryDamage(col.collider);
    }

    private void TryDamage(Collider2D other)
    {
        if (rb.velocity.magnitude < hitSpeedThreshold) return;

        if (player != null || enemy != null)
        {
            TankHealth thTarget = other.GetComponent<TankHealth>();
            if (thTarget == null) return;

            bool isPlayer = (player != null && thTarget == player);
            bool isEnemy = (enemy != null && thTarget == enemy);
            if (!isPlayer && !isEnemy) return;

            DamageOnceWithCooldown(thTarget);
            return;
        }

        var th = other.GetComponent<TankHealth>();
        if (th != null)
        {
            DamageOnceWithCooldown(th);
        }
    }

    private void DamageOnceWithCooldown(TankHealth target)
    {
        int id = target.GetInstanceID();
        float now = Time.time;

        if (lastHitTime.TryGetValue(id, out float last) && now - last < hitCooldown)
            return;

        lastHitTime[id] = now;
        if (!target.IsDisabled())
        {
            target.TakeHit();
        }
    }
}
