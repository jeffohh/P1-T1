using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Rigidbody2D))]
public class TankEnemy: MonoBehaviour
{
    public enum State { Patrol, Search, Chase, Attack }

    [Header("Refs")]
    public Transform player;
    public GameObject bulletPrefab;
    public Transform firePoint;       
    public Tilemap obstacleTilemap;         
    public LayerMask obstacleMask;           

    [Header("Movement")]
    public float moveSpeed = 3f;
    public float rotationSpeed = 720f;   

    [Header("Perception & Combat")]
    public float detectionRadius = 8f;        
    public float attackRange = 6f;           
    public float fireRate = 0.5f;
    [Range(0f, 30f)]
    public float fireAimTolerance = 6f;
    public float searchDuration = 2.5f;    
    public float lostSightCooldown = 1.0f;     

    [Header("Pathfinding")]
    public float repathInterval = 0.25f;     
    public List<Transform> patrolPoints;       
    public float waypointReachDist = 0.1f;     

    private Rigidbody2D rb;
    private State state = State.Patrol;
    private List<Vector3> currentPath = new List<Vector3>();
    private int pathIndex = 0;
    private float nextRepathTime = 0f;
    private float nextFireTime = 0f;
    private float searchTimer = 0f;
    private Vector3 lastSeenPlayerPos;
    private int patrolIndex = 0;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        //State switching (perception and decision-making)
        bool hasPlayer = player != null;
        bool inDetect = hasPlayer && Vector2.Distance(transform.position, player.position) <= detectionRadius;
        bool hasLOS = hasPlayer && !Physics2D.Linecast(transform.position, player.position, obstacleMask);

        if (hasPlayer)
        {
            if (inDetect && hasLOS)
            {
                lastSeenPlayerPos = player.position;
                if (Vector2.Distance(transform.position, player.position) <= attackRange)
                    state = State.Attack;
                else
                    state = State.Chase;
                searchTimer = searchDuration;
            }
            else
            {
                // If enemy are still in the search otherwise patrol
                if (searchTimer > 0f)
                {
                    searchTimer -= Time.deltaTime;
                    state = State.Search;
                }
                else
                {
                    state = State.Patrol;
                }
            }
        }
        else
        {
            state = State.Patrol;
        }

        // Execution state behavior
        switch (state)
        {
            case State.Patrol:
                DoPatrol();
                break;
            case State.Search:
                DoSearch();
                break;
            case State.Chase:
                DoChase();
                break;
            case State.Attack:
                DoAttack();
                break;
        }
    }

    void FixedUpdate()
    {
      
        FollowPath();
    }

    

    void DoPatrol()
    {
        Vector3 target;
        if (patrolPoints != null && patrolPoints.Count > 0)
        {
            target = patrolPoints[patrolIndex].position;
            EnsurePathTo(target);

            if (Reached(target))
            {
                patrolIndex = (patrolIndex + 1) % patrolPoints.Count;
            }
        }
        else
        {
            // if no patrolPoints are assigned, randomly walk
            if (currentPath == null || currentPath.Count == 0 || pathIndex >= currentPath.Count)
            {
                Vector3 random = transform.position + (Vector3)(Random.insideUnitCircle * 3f);
                EnsurePathTo(random);
            }
        }
    }

    void DoSearch()
    {
        EnsurePathTo(lastSeenPlayerPos);
        if (Reached(lastSeenPlayerPos) && searchTimer <= Time.deltaTime)
        {
            state = State.Patrol;
        }
    }

    void DoChase()
    {
        if (player == null) return;
        EnsurePathTo(player.position);
    }

    void DoAttack()
    {
        if (player == null) return;


        Vector2 dir = (player.position - firePoint.position).normalized;

        RotateTowards(dir);

        bool hasLOS = !Physics2D.Linecast(firePoint.position, player.position, obstacleMask);

        float angleToTarget = Vector2.Angle(firePoint.up, dir);
        bool aimed = angleToTarget <= fireAimTolerance;
        bool inRange = Vector2.Distance(transform.position, player.position) <= attackRange;

        if (inRange && hasLOS && aimed && Time.time >= nextFireTime)
        {
            if (firePoint != null && bulletPrefab != null)
            {
                Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
                nextFireTime = Time.time + fireRate;
            }
        }

        if (!inRange || !hasLOS)
        {
            searchTimer = Mathf.Max(searchTimer, lostSightCooldown);
            state = State.Chase;
        }

        rb.velocity = Vector2.zero;
    }


    void EnsurePathTo(Vector3 target)
    {
        if (Time.time < nextRepathTime) return;
        nextRepathTime = Time.time + repathInterval;

        if (obstacleTilemap == null)
        {
            // 没有 Tilemap 时，直接朝向目标做简单 steering
            currentPath = new List<Vector3>() { target };
            pathIndex = 0;
            return;
        }

        var path = SimpleA.FindPath(transform.position, target, obstacleTilemap);
        if (path != null && path.Count > 0)
        {
            currentPath = path;
            pathIndex = 0;
        }
        else
        {
            currentPath?.Clear();
        }
    }

    void FollowPath()
    {
        if (currentPath == null || currentPath.Count == 0 || pathIndex >= currentPath.Count)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        Vector3 wp = currentPath[pathIndex];
        Vector2 to = (wp - transform.position);
        Vector2 dir = to.normalized;

        RotateTowards(dir);

        rb.velocity = transform.up * moveSpeed;

        if (to.magnitude <= waypointReachDist)
        {
            pathIndex++;
            if (pathIndex >= currentPath.Count)
            {
                rb.velocity = Vector2.zero;
            }
        }
    }

    void RotateTowards(Vector2 dir)
    {
        float targetAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
        float newAngle = Mathf.MoveTowardsAngle(rb.rotation, targetAngle, rotationSpeed * Time.deltaTime);
        rb.MoveRotation(newAngle);
    }

    bool Reached(Vector3 pos)
    {
        return Vector2.Distance(transform.position, pos) <= waypointReachDist + 0.05f;
    }

    //Visual debugging
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        if (currentPath != null && currentPath.Count > 0)
        {
            Gizmos.color = Color.cyan;
            for (int i = 0; i < currentPath.Count - 1; i++)
                Gizmos.DrawLine(currentPath[i], currentPath[i + 1]);
        }
    }
}
