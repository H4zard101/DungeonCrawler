using System.Collections;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("Attack Settings")]
    public float attackCoolDown = 0.3f;
    public float attackDelay = 0.05f;
    public int damage = 1;

    [Header("Attack Area")]
    public Transform attackPoint;
    public float attackRange = 0.5f;
    public LayerMask enemyLayers;

    public KeyCode attackKey = KeyCode.Mouse0;

    public float lastAttackTime;

    [Header("Animation")]
    public Animator animator;

    public bool canAttack = true;
    private void Update()
    {

        if(Input.GetKeyDown(attackKey))
        {
            TryAttack();
        }
    }

    private void TryAttack()
    {
        if (!canAttack)
        {
            return;
        }

        if (Time.time < lastAttackTime + attackCoolDown)
        {
            return;
        }
        if(animator != null)
        {
            animator.SetTrigger("Attack");
        }
        lastAttackTime = Time.time;
        StartCoroutine(AttackRoutine());
    }

    private IEnumerator AttackRoutine()
    {
        if (attackDelay > 0f)
        {
            yield return new WaitForSeconds(attackDelay);
        }
        DoDamage();
    }

    private void DoDamage()
    {
        if(attackPoint == null)
        {
            Debug.LogWarning("PlayerCombat: attackPoint is not assigned.");
            return;
        }

        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);

        foreach (Collider2D hit in hits)
        {
            Enemy enemy = hit.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);

                SlimeAI slime = hit.GetComponent<SlimeAI>();
                if(slime != null)
                {
                    Vector2 dir = (hit.transform.position - transform.position);
                    float force = 8f;
                    float duration = 0.1f;
                    slime.ApplyKnockback(dir, force, duration);
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if(attackPoint == null)
        {
            return;
        }
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}
