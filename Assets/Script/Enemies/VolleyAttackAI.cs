using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A ranged AI that fires a volley of projectiles.
/// </summary>
public class VolleyAttackAI : RangedEnemyAI
{
    /// <summary>
    /// How many projectiles to fire each volley.
    /// </summary>
    [SerializeField]
    private int numProjectiles;

    /// <summary>
    /// The delay between each shot in the volley.
    /// </summary>
    [SerializeField]
    private float volleyDelay;

    new protected void Start()
    {
        base.Start();

        //Stop the cooldown from finishing during the volley.
        attackCooldown += volleyDelay * numProjectiles;
    }

    protected override void Attack()
    {
        attackCooldownTracker = attackCooldown;
        StartCoroutine(FireProjectileVolley(projectile, playerT, numProjectiles, volleyDelay, attackAudioClip, 10, () => {
            if(!IsStunned())
            {
                SetMovementStopped(false);
            }
        }));
    }
}
