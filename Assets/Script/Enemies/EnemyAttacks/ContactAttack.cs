using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Causes an enemy to damage the player on contact, with an optional cooldown. If this component finds an EnemyAI, contact damage will not be applied while the AI is stunned.
/// If an animation for a contact attack is made, it should be activated with a "ContactAttack" trigger.
/// </summary>
public class ContactAttack : MonoBehaviour
{
    [SerializeField]
    private float cooldown;
    [SerializeField]
    private EnemyAttackData attackData;
    /// <summary>
    /// If the enemy should wait on the attack's cooldown to end before it tries to move again. Assumes the enemy has an EnemyAIBase derived component when true.
    /// </summary>
    [SerializeField]
    private bool stopMovementDuringCooldown;

    private Animator animator;
    private EnemyAIBase enemyAI;

    private float cooldownTracker = 0;
    private bool hasAI;

    private void Start()
    {
        animator = GetComponent<Animator>();

        hasAI = TryGetComponent(out enemyAI);
    }

    private void Update()
    {
        if(cooldownTracker > 0)
        {
            cooldownTracker -= Time.deltaTime;

            if(stopMovementDuringCooldown && cooldownTracker <= 0)
            {
                enemyAI.SetMovementStopped(false);
            }
        }
    }

    //Returns true if the EnemyAI component on this gameObject is stunned. Returns false if the AI either isn't stunned or was never present to begin with.
    private bool AIstunned()
    {
        return hasAI && enemyAI.IsStunned();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("Player") && cooldownTracker <= 0 && !AIstunned())
        {
            if (animator)
            {
                animator.SetTrigger("ContactAttack");
            }

            collision.gameObject.GetComponent<PlayerController>().ApplyEnemyAttack(attackData, transform.position);
            cooldownTracker = cooldown;

            if (stopMovementDuringCooldown)
            {
                enemyAI.SetMovementStopped(true);
            }
        }
    }
}
