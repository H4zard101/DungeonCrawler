using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;

    [SerializeField]private Rigidbody2D rb;
    private Vector2 movement;


    public float knockbackDuration = 0.15f;
    private bool isKnockedBack;
    private float knockbackTimer;
    private Vector2 knockbackVelocity;

    [Header("Slow Effect")]
    public float minSpeedMultiplier = 0.2f; // don’t let speed go below 20%

    private float currentSpeedMultiplier = 1f;
    private Coroutine slowRoutine;

    [Header("Visuals")]
    public SpriteRenderer spriteRenderer;
    public Transform attackPoint;
    public Transform weapon;

    private bool facingRight = true;

    public bool canMove = true;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    public void SetCanMove(bool value)
    {
        canMove = value;

        if (!canMove)
        {
            // kill any existing motion immediately
            movement = Vector2.zero;
            if (rb != null)
                rb.velocity = Vector2.zero;
        }
    }

    private void Update()
    {
        if(!canMove)
        {           
            return;
        }
        // get Input
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
        movement.Normalize();

        HandleFacing();
    }

    private void FixedUpdate()
    {

        if (!canMove)
        {
            // make sure physics stays stopped
            rb.velocity = Vector2.zero;
            return;
        }

        if (isKnockedBack)
        {
            rb.velocity = knockbackVelocity;
            knockbackTimer -= Time.fixedDeltaTime;
            if (knockbackTimer <= 0f)
            {
                isKnockedBack = false;
            }
            return;
        }

        // Normal movement with slow multiplier
        rb.velocity = movement * moveSpeed * currentSpeedMultiplier;
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

    public void ApplySlow(float multiplier, float duration)
    {
        // Clamp multiplier so we don't end up with negative or absurd values
        multiplier = Mathf.Clamp(multiplier, minSpeedMultiplier, 1f);

        if (slowRoutine != null)
            StopCoroutine(slowRoutine);

        slowRoutine = StartCoroutine(SlowRoutine(multiplier, duration));
    }

    private System.Collections.IEnumerator SlowRoutine(float multiplier, float duration)
    {
        currentSpeedMultiplier = multiplier;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        currentSpeedMultiplier = 1f;
        slowRoutine = null;
    }

    private void HandleFacing()
    {
        if (movement.x > 0.01f)
        {
            SetFacing(true);
        }
        else if(movement.x < -0.01f)
        {
            SetFacing(false);
        }
    }
    private void SetFacing(bool faceRight)
    {
        if(facingRight == faceRight)
        {
            return;
        }
        facingRight = faceRight;

        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * (facingRight ? 1f : -1f);
        transform.localScale = scale;
    }

}
