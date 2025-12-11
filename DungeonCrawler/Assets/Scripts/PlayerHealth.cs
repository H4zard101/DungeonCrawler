using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 100;
    public int CurrentHealth { get; private set; }

    [Header("Invincibility")]
    public float invincibilityDuration = 0.5f;
    public float flashInterval = 0.1f;

    public UnityEvent<int, int> onHealthChanged;
    public UnityEvent onDeath;

    private bool isInvincible;
    private SpriteRenderer sr;
    private Color defaultColor;
    private Coroutine invincibilityRoutine;

    private void Awake()
    {
        CurrentHealth = maxHealth;

        sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            defaultColor = sr.color;  // whatever you see in Inspector becomes "normal"
    }

    private void Start()
    {
        // Notify UI of initial health
        onHealthChanged?.Invoke(CurrentHealth, maxHealth);
    }

    public void TakeDamage(int amount)
    {
        if (isInvincible || CurrentHealth <= 0)
            return;

        CurrentHealth = Mathf.Clamp(CurrentHealth - amount, 0, maxHealth);
        onHealthChanged?.Invoke(CurrentHealth, maxHealth);

        if (CurrentHealth <= 0)
        {
            Die();
            return;
        }

        // Start invincibility+flash
        if (invincibilityRoutine != null)
            StopCoroutine(invincibilityRoutine);

        invincibilityRoutine = StartCoroutine(InvincibilityCoroutine());
    }

    public void Heal(int amount)
    {
        if (CurrentHealth <= 0) return; // dead, can't heal

        CurrentHealth = Mathf.Clamp(CurrentHealth + amount, 0, maxHealth);
        onHealthChanged?.Invoke(CurrentHealth, maxHealth);
    }

    private IEnumerator InvincibilityCoroutine()
    {
        isInvincible = true;

        float elapsed = 0f;
        bool visible = true;

        while (elapsed < invincibilityDuration)
        {
            elapsed += flashInterval;
            visible = !visible;

            if (sr != null)
            {
                Color c = defaultColor;
                c.a = visible ? 1f : 0.3f; // blink by alpha
                sr.color = c;
            }

            yield return new WaitForSeconds(flashInterval);
        }

        isInvincible = false;

        if (sr != null)
            sr.color = defaultColor;   // guarantee reset
    }

    private void Die()
    {
        Debug.Log("Player died");
        if (invincibilityRoutine != null)
            StopCoroutine(invincibilityRoutine);

        isInvincible = false;

        if (sr != null)
            sr.color = defaultColor;

        onDeath?.Invoke();

        // For now:
        gameObject.SetActive(false);
    }
}
