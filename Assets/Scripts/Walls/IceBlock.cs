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
        Vector2 tilt = ReadTiltVector(); // 读倾斜/后备输入

        // 施加推力（冰面感：即使不按也会有惯性，靠碰撞/摩擦慢慢停）
        rb.AddForce(tilt * acceleration, ForceMode2D.Force);

        // 限速
        if (rb.velocity.sqrMagnitude > maxSpeed * maxSpeed)
        {
            rb.velocity = rb.velocity.normalized * maxSpeed;
        }
    }

    // —— 读倾斜或键盘 —— //
    private Vector2 ReadTiltVector()
    {
        Vector2 v = Vector2.zero;

        if (gyroAvailable)
        {
            // 优先用加速度计（横竖屏都适配更简单）
            // iOS/Android：Input.acceleration 是设备重力方向（-1~1）
            Vector3 acc = Input.acceleration; // x:左右, y:上下(竖屏), z:朝外
            // 这里假定竖屏：向右倾斜→acc.x>0；向上倾斜→acc.y>0
            //v = new Vector2(acc.x, acc.y) * tiltSensitivity;

            // 若需要横屏，把 y/x 互换或取反即可：
             v = new Vector2(acc.x, acc.y) * tiltSensitivity; // 例如右手横屏
        }

        if (v.sqrMagnitude < 0.0001f && enableKeyboardFallback)
        {
            // 编辑器/PC 后备：WASD / 方向键
            float h = Input.GetAxisRaw("Horizontal"); // A/D or ←/→
            float k = Input.GetAxisRaw("Vertical");   // W/S or ↑/↓
            v = new Vector2(h, k);
        }

        // 死区与归一化放到力应用处处理（这里直接返回）
        return v;
    }

    // —— 碰撞伤害 —— //
    void OnCollisionEnter2D(Collision2D col)
    {
        TryDamage(col.collider);
    }

    void OnCollisionStay2D(Collision2D col)
    {
        // 连续接触时也可触发（受冷却限制）
        TryDamage(col.collider);
    }

    private void TryDamage(Collider2D other)
    {
        if (rb.velocity.magnitude < hitSpeedThreshold) return;

        // 如果指定了 player/enemy，则只对这两个生效
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

        // 未指定则对任何 TankHealth 生效
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

        // 只有在目标未处于“禁用/死亡动画”时才打（可按需放开）
        if (!target.IsDisabled())
        {
            target.TakeHit();
        }
    }
}
