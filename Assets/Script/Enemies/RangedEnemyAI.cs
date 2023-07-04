using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// AI for a simple ranged attacker. The AI approaches the player until it is both within range and has LOS. It then starts an attack animation when if its attack is off cooldown.
/// </summary>
public abstract class RangedEnemyAI : EnemyAIBase
{
    [SerializeField]
    protected float attackRange;

    [SerializeField]
    protected GameObject projectile;

    [SerializeField]
    protected AudioClip attackAudioClip;

    // Update is called once per frame
    void Update()
    {
        ProcessCooldowns();

        if (!IsStunned())
        {
            FacePlayer();

            if (Vector3.Distance(transform.position, playerT.position) <= attackRange && PlayerLOSCheck())
            {
                if (CanAttack() && !animator.GetBool(animAttackingVar))
                {
                    navAgent.ResetPath();
                    SetMovementStopped(true);
                    animator.SetBool(animAttackingVar, true);
                }
            }
            else
            {
                ChasePlayer();
            }
        }
    }

    /// <summary>
    /// A function meant to be called at the end of a attack animation. Calls Attack()
    /// </summary>
    private void OnAttack()
    {
        Attack();
        attackCooldownTracker = attackCooldown;
        animator.SetBool(animAttackingVar, false);
    }

    /// <summary>
    /// Called by OnAttack before the enemy's cooldown is set. Override this to control what a ranged enemy does when they attack.
    /// </summary>
    protected abstract void Attack();

    public override void Stun()
    {
        base.Stun();

        //Cancel this enemy's attack if it is stunned.
        animator.SetBool(animAttackingVar, false);
    }
}
