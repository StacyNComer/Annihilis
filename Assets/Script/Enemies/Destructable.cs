using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Allows a Game Object to be destroyed by player attacks. Note that anything that effects the gameObject's AI is instead handled in EnemyAIBase or a derived class.
/// </summary>
public class Destructable : MonoBehaviour
{
    [SerializeField]
    private float health;

    [Header("Loot Drops")]
    [SerializeField]
    private EnemyLootTable baseLootTable;
    [SerializeField]
    private EnemyLootTable annihilatedLootTable;

    [Header("FX")]
    [SerializeField]
    private AudioClip damageAudio;
    [SerializeField]
    private AudioClip weakpointAudio;
    [SerializeField]
    private AudioClip deathAudio;
    /// <summary>
    /// A gameObject that is spawned upon this actor's death. 
    /// </summary>
    [SerializeField]
    private GameObject deathEffect;

    /// <summary>
    /// The death trigger this script should notify when this gameObject is destroyed.
    /// </summary>
    private TriggerOnDeath deathTrigger;

    private void OnDestroy()
    {
        //Report this gameObjects destruction to its deathTrigger, if one exists. This code is placed here instead of in the Die method to protect against softlocks.
        if (deathTrigger)
        {
            deathTrigger.ReportDeath();
        }
    }

    /// <summary>
    /// Substract the given value from the destructable's health. This function should not be used for healing, as the value will not be clamped.
    /// </summary>
    /// <param name="damage"></param>
    /// <returns>True if the damage killed the destructable.</returns>
    private bool Damage(float damage)
    {
        if (health > 0)
        {
            health -= damage;

            //AudioSource.PlayClipAtPoint(damageAudio, transform.position);
            AudioSource.PlayClipAtPoint(damageAudio, transform.position);

            if (health <= 0)
            {
                if (deathEffect)
                {
                    Instantiate(deathEffect, transform.position, transform.rotation);
                }
                Die();
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        { 
            return false;
        }
    }

    /// <summary>
    /// Kills the destructable. Violently. (Yes this plays its death effects).
    /// </summary>
    public void Die()
    {
        AudioSource.PlayClipAtPoint(deathAudio, transform.position);



        Destroy(gameObject);
    }

    /// <summary>
    /// Process the effects of a player attack on a destructable. Also gives the player this destuctable's loot.
    /// </summary>
    /// <returns>True if the damage killed the destructable.</returns>
    public bool RecieveAttack(PlayerAttackData attack, bool hitWeakpoint, float damageMod = 1)
    {
        var attackKilled = Damage(attack.GetDamage(hitWeakpoint) * damageMod);

        if (hitWeakpoint)
        {
            //The weakpoint hit sound is louder if the attack killed the enemy.
            AudioSource.PlayClipAtPoint(weakpointAudio, transform.position, attackKilled ? 1.6f : 1.25f);
        }

        if(attackKilled)
        {
            var playerLoot = attack.GetCanAnnihilate() ? annihilatedLootTable : baseLootTable;
            playerLoot.GiveLoot(attack.owner);
        }

        return attackKilled;
    }

    /// <summary>
    /// Sets the DeathTrigger this script should report its death to.
    /// </summary>
    public void SetDeathTrigger(TriggerOnDeath newDeathTrigger)
    {
        deathTrigger = newDeathTrigger;
    }
}
