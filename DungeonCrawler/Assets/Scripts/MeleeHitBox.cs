using System.Collections.Generic;
using UnityEngine;

public class MeleeHitBox : MonoBehaviour
{
    public int damage = 1;

    // Is the current attack swing active?
    private bool attackActive;

    // To avoid hitting the same enemy multiple times in one swing
    private HashSet<Enemy> enemiesHitThisSwing = new HashSet<Enemy>();

    // Called from PlayerCombat at the start of an attack
    public void BeginAttack()
    {
        attackActive = true;
        enemiesHitThisSwing.Clear();
    }

    // Called from PlayerCombat when the attack window ends
    public void EndAttack()
    {
        attackActive = false;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!attackActive) return;

        Enemy enemy = collision.GetComponent<Enemy>();
        if (enemy != null && !enemiesHitThisSwing.Contains(enemy))
        {
            enemiesHitThisSwing.Add(enemy);
            enemy.TakeDamage(damage);
        }
    }
}
