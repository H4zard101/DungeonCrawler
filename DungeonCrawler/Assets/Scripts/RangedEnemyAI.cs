using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class RangedEnemyAI : MonoBehaviour
{
    public enum EnemyState
    {
        Idle,
        Engaged
    }

    [Header("References")]
    public Transform player;
    public Transform firePoint;
    public GameObject projectilePrefab;

    [Header("Movement")]
    public float moveSpeed = 2f;
    public float preferredRange = 4f;      // distance we like
    public float approachBuffer = 1f;      // how far beyond preferredRange we start moving closer
    public float retreatBuffer = 1.5f;     // how far inside preferredRange we start backing up

    [Header("Aggro")]
    public float detectionRadius = 7f;     // how close player must be to aggro

    [Header("Attack")]
    public float attackInterval = 1.2f;

    [Header("Visuals")]
    public SpriteRenderer spriteRenderer;



    private EnemyState currentState = EnemyState.Idle;
    private Rigidbody2D rb;
    private Collider2D myCollider;
    private Vector2 spawnPosition;
    private float attackTimer;
    private bool facingRight = true;

    [Header("Animation")]
    public Animator animator;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spawnPosition = transform.position;
    }

    private void Start()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null)
                player = p.transform;
        }

        if (firePoint == null)
        {
            firePoint = transform;
        }
    }

    private void Update()
    {
        if (player == null) return;

        float distToPlayer = Vector2.Distance(transform.position, player.position);

        // Attack cooldown timer
        if (attackTimer > 0f)
            attackTimer -= Time.deltaTime;

        switch (currentState)
        {
            case EnemyState.Idle:
                if (distToPlayer <= detectionRadius)
                {
                    currentState = EnemyState.Engaged;
                }
                break;

            case EnemyState.Engaged:
                // If the player runs very far away, forget them and go idle
                if (distToPlayer > detectionRadius * 2f)
                {
                    currentState = EnemyState.Idle;
                }
                else
                {
                    HandleAttacking(distToPlayer);
                }
                break;
        }

        UpdateFacing();
    }

    private void FixedUpdate()
    {
        if (player == null) return;

        Vector2 currentPosition = rb.position;
        Vector2 targetPosition = currentPosition;

        if (currentState == EnemyState.Engaged)
        {
            float distToPlayer = Vector2.Distance(currentPosition, player.position);
            Vector2 dirToPlayer = ((Vector2)player.position - currentPosition).normalized;

            // Decide movement based on distance to player

            // Too far: move closer
            if (distToPlayer > preferredRange + approachBuffer)
            {
                targetPosition = currentPosition + dirToPlayer * moveSpeed * Time.fixedDeltaTime;
            }
            // Too close: retreat a bit
            else if (distToPlayer < preferredRange - retreatBuffer)
            {
                targetPosition = currentPosition - dirToPlayer * moveSpeed * Time.fixedDeltaTime;
            }
            // In a good band around preferredRange: hold position
            else
            {
                targetPosition = currentPosition;
            }
        }
        else
        {
            // Idle: stand still (or you could wander here)
            targetPosition = currentPosition;
        }

        rb.MovePosition(targetPosition);
    }

    private void HandleAttacking(float distToPlayer)
    {
        if (attackTimer > 0f || projectilePrefab == null) return;

        // Don't fire while retreating (too close)
        bool tooClose = distToPlayer < preferredRange - retreatBuffer;

        // Only fire when roughly in a "good" band around preferred range
        bool inGoodRange = distToPlayer > preferredRange - retreatBuffer &&
                           distToPlayer < preferredRange + approachBuffer;

        if (!tooClose && inGoodRange)
        {
            if(animator != null)
            {
                animator.SetTrigger("Attack");
            }
            Shoot();
        }
    }

    private void Shoot()
    {
        Vector2 dir = (player.position - firePoint.position).normalized;

        GameObject projGO = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        Projectile proj = projGO.GetComponent<Projectile>();

        if (proj != null)
        {
            proj.Fire(dir);
        }

        attackTimer = attackInterval;
    }

    private void UpdateFacing()
    {
        if (player == null) return;

        float dx = player.position.x - transform.position.x;

        if (dx > 0.01f)
            SetFacing(true);
        else if (dx < -0.01f)
            SetFacing(false);
    }

    private void SetFacing(bool faceRight)
    {
        if (facingRight == faceRight) return;

        facingRight = faceRight;

        // Flip the whole enemy (body, weapon, firePoint)
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * (facingRight ? 1f : -1f);
        transform.localScale = scale;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, preferredRange);
    }
}