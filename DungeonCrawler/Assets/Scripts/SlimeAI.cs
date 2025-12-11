using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class SlimeAI : MonoBehaviour
{
    public enum EnemyState
    {
        Idle,
        Chasing,
        Returning
    };

    [Header("References")]
    public Transform player;

    [Header("Movement")]
    public float moveSpeed = 2f;

    [Header("Aggro / Leash")]
    public float detectionRadius = 5f;
    public float attackRange = 1f;
    public float leashRadius = 8f;

    [Header("Idle Wander (Optional)")]
    public bool enableWander = false;
    public float wanderRadius = 1f;
    public float wanderInterval = 2f;

    private EnemyState currentState = EnemyState.Idle;
    private Rigidbody2D rb;
    private Vector2 spawnPosition;
    private Vector2 idleTarget;
    private float wanderTimer;


    [Header("Combat")]
    public int contactDamge = 10;
    public float attackInterval = 0.7f;
    private float attackTimer;

    [Header("Knockback")]
    public float knockbackDuration = 0.1f;
    private bool isKnockedBack;
    private float knockbackTimer;
    private Vector2 knockbackVelocity;

    [Header("Visuals")]
    public SpriteRenderer spriteRenderer;
    private bool facingRight = true;

    [Header("Animation")]
    public Animator animator;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spawnPosition = transform.position;
        idleTarget = spawnPosition;
    }

    private void Start()
    {
       if(player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if(p != null)
            {
                player = p.transform;
            }
        }
    }

    private void Update()
    {
        if (player == null)
        {
            return;
        }

        float distToPlayer = Vector2.Distance(transform.position, player.position);
        float distFromSpawn = Vector2.Distance(transform.position, spawnPosition);

        if (attackTimer > 0f)
        {
            attackTimer -= Time.deltaTime;
        }

        switch (currentState)
        {
            case EnemyState.Idle:

                if(distToPlayer <= detectionRadius)
                {
                    currentState = EnemyState.Chasing;
                }
                else if(enableWander)
                {
                    UpdateIdleWander();
                }
                break;

            case EnemyState.Chasing:
                if(distToPlayer <= attackRange)
                {
                    TryAttack();
                }

                if(distToPlayer > detectionRadius * 1.5f || distFromSpawn > leashRadius)
                {
                    currentState = EnemyState.Returning;
                }
                break;

            case EnemyState.Returning:
                if(distFromSpawn < 0.1f)
                {
                    currentState = EnemyState.Idle;
                    idleTarget = spawnPosition;
                }
                else if(distToPlayer <=detectionRadius)
                {
                    currentState = EnemyState.Chasing;
                }
                break;
        }

        if (attackTimer > 0f)
        {
            attackTimer -= Time.deltaTime;
        }
        UpdateFacing();
    }

    private void FixedUpdate()
    {
        if(player == null)
        {
            return;
        }
        if(isKnockedBack)
        {
            rb.velocity = knockbackVelocity;
            knockbackTimer -= Time.fixedDeltaTime;
            if(knockbackTimer <= 0f)
            {
                isKnockedBack = false;
                rb.velocity = Vector2.zero;
            }
            return;
        }

        Vector2 currentPosition = rb.position;
        Vector2 targetPosition = currentPosition;

        switch (currentState)
        {

            case EnemyState.Idle:
                if (enableWander && idleTarget != (Vector2)transform.position)
                {
                    targetPosition = Vector2.MoveTowards(currentPosition, idleTarget, moveSpeed * Time.fixedDeltaTime);
                }
                else
                {
                    targetPosition = currentPosition; // stand still
                }
                break;

            case EnemyState.Chasing:


                // Only move if we're *outside* attack range
                float distToPlayer = Vector2.Distance(currentPosition, player.position);
                if (distToPlayer > attackRange * 0.9f)
                {
                    targetPosition = Vector2.MoveTowards(
                        currentPosition,
                        player.position,
                        moveSpeed * Time.fixedDeltaTime
                    );
                }
                else
                {
                    targetPosition = currentPosition; // stop at melee range
                }
                break;

            case EnemyState.Returning:
                targetPosition = Vector2.MoveTowards(
                    currentPosition,
                    spawnPosition,
                    moveSpeed * Time.fixedDeltaTime
                );
                break;

        }
       rb.MovePosition( targetPosition );

    }

    private void UpdateIdleWander()
    {
        wanderTimer -= Time.deltaTime;
        if (wanderTimer <= 0f)
        {
            wanderTimer = wanderInterval;

            // Pick a random point around spawn
            Vector2 randomOffset = Random.insideUnitCircle * wanderRadius;
            idleTarget = spawnPosition + randomOffset;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(spawnPosition == Vector2.zero ? (Vector2)transform.position : spawnPosition, leashRadius);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
    private void TryAttack()
    {
        if(attackTimer > 0f)
        {
            return;
        }
        if(player == null)
        {
            return;
        }

        PlayerHealth ph = player.GetComponent<PlayerHealth>();
        if(ph != null)
        {
            ph.TakeDamage(contactDamge);


            PlayerMovement pm = player.GetComponent<PlayerMovement>();
            if(pm != null)
            {
                Vector2 dir = (player.position - transform.position);
                float force = 6f;
                float duration = 0.1f;
                pm.ApplyKnockback(dir, force, duration);
            }

            if (animator != null)
            {
                animator.SetTrigger("Attack");
            }
        }
        attackTimer = attackInterval;
    }

    public void ApplyKnockback(Vector2 direction, float force, float duration)
    {
        if(direction.sqrMagnitude < 0.0001f)
        {
            return;
        }

        isKnockedBack = true;
        knockbackTimer = duration;
        knockbackVelocity = direction.normalized * force;
    }

    private void UpdateFacing()
    {
        if (player == null || spriteRenderer == null) return;

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

        // Flip the WHOLE enemy, moving bow/staff + firePoint to the other side
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * (facingRight ? 1f : -1f);
        transform.localScale = scale;
    }

}
