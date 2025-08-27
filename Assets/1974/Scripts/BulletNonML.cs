using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class BulletNonML : MonoBehaviour
{
    public TankController owner;    // 谁发射的
    public float speed = 12f;       // 子弹速度
    public float lifeTime = 3f;     // 生存时间

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0; // 确保子弹不受重力影响
    }

    void Start()
    {
        // 子弹生成时朝着 firePoint 的 up 方向飞
        rb.velocity = transform.up * speed;

        // 一定时间后销毁
        Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        // 防止打到自己
        if (owner != null && col.gameObject == owner.gameObject)
            return;

        // 如果打到有 TankHealth 的对象（比如玩家）
        TankHealth tankHealth = col.GetComponent<TankHealth>();
        if (tankHealth != null)
        {
            //tankHealth.TakeHit();
        }

        // 无论击中什么（墙、玩家等）都销毁
        Destroy(gameObject);
    }

    void OnDestroy()
    {
        if (owner != null)
        {
            owner.OnShellDestroyed();
        }
    }
}
