using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Motion")]
    public float speed = 8f;
    public float lifetime = 3f;

    [Header("Damage")]
    public int damage = 10;

    [Header("Knockback")]
    public bool applyKnockback = false;
    public float knockbackForce = 6f;
    public float knockbackDuration = 0.1f;

    [Header("Status Effects")]
    public bool applySlow = false;
    [Range(0.1f, 1f)]
    public float slowMultiplier = 0.5f;
    public float slowDuration = 1.5f;

    private Vector2 direction;

    private Rigidbody2D rb;
    private Collider2D ownerCollider;


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Fire(Vector2 dir)
    {
        direction = dir.normalized;

        if (rb != null)
            rb.velocity = direction * speed;

        if (direction.sqrMagnitude > 0.0001f)
            transform.up = direction;

        Collider2D myCol = GetComponent<Collider2D>();
        if (myCol != null && ownerCollider != null)
        {
            Physics2D.IgnoreCollision(myCol, ownerCollider);
        }



        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        transform.position += (Vector3)(direction * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Ignore hitting the owner
        if (collision == ownerCollider)
            return;

        // Ignore other enemies / their weapons so projectiles don't blow up on the mage/knife guy
        if (collision.CompareTag("Enemy") || collision.CompareTag("EnemyWeapon"))
            return;
        if (collision.CompareTag("Player"))
        {
            PlayerHealth ph = collision.GetComponent<PlayerHealth>();
            if (ph != null)
            {
                ph.TakeDamage(damage);

                PlayerMovement pm = collision.GetComponent<PlayerMovement>();
                if (pm != null)
                {
                    if (applyKnockback)
                    {
                        Vector2 knockDir = (collision.transform.position - transform.position);
                        pm.ApplyKnockback(knockDir, knockbackForce, knockbackDuration);
                    }

                    if (applySlow)
                    {
                        pm.ApplySlow(slowMultiplier, slowDuration);
                    }
                }

                Destroy(gameObject);
                return;
            }
        }
        Destroy(gameObject);
    }
}