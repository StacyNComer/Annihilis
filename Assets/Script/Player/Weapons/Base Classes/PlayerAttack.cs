using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A general use class for holding information about player attacks. Use for attacks with invariable statistics.
/// </summary>
[System.Serializable]
public class PlayerAttackData
{
    [HideInInspector]
    public PlayerController owner;

    [SerializeField]
    private float damage;
    [SerializeField]
    private float weakpointMultiplier = 1;
    [SerializeField]
    private float stun;
    [SerializeField]
    private bool canAnnihilate;

    /// <summary>
    /// Whether or not this attack should Annihilate an enemy if it kills them. Annihilating enemies causes them to drop bonus ammo/health.
    /// </summary>
    public bool GetCanAnnihilate()
    {
        return canAnnihilate;
    }

    public float GetDamage(bool hitWeapoint)
    {
        return damage * (hitWeapoint? weakpointMultiplier : 1);
    }

    public float GetStunBuildup()
    {
        return stun;
    }

    /// <summary>
    /// Returns true if the attack's damage is different from hitting an enemy's weakpoint.
    /// </summary>
    /// <returns></returns>
    public bool WeakpointAttack()
    {
        return weakpointMultiplier != 1;
    }
}

/// <summary>
/// A class for anything that should damage a destructable on contact. Make sure to call SetAttackOwner when creating an instance of this class.
/// </summary>
public class PlayerAttack : MonoBehaviour
{
    [SerializeField]
    protected PlayerAttackData attackData;
    [SerializeField]
    private bool pierceWalls = false;

    private void OnTriggerEnter(Collider other)
    {
        ProcessCollision(other);
    }

    private void OnCollisionEnter(Collision collision)
    {
        var col = collision.collider;

        ProcessCollision(col);
    }

    /// <summary>
    /// Runs the logic that should occur when the given collider is hit by this attack. Used in this components collision events.
    /// </summary>
    private void ProcessCollision(Collider col)
    {
        if (col.CompareTag("Destructable"))
        {
            //Try and find the destructable component to damage (sometimes it is in a parent gameObject).
            var destructable = col.GetComponentInParent<Destructable>();

            OnDestructableHit(destructable, col);

        }
        else if (!col.CompareTag("Player") && !pierceWalls && !col.isTrigger)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// What this attack does when it hits a destructable.
    /// </summary>
    /// <param name="destructable"></param>
    /// <param name="col"></param>
    protected virtual void OnDestructableHit(Destructable destructable, Collider col)
    {
        var enemyAI = destructable.GetComponentInParent<EnemyAIBase>();

        //Store whether or not a weakpoint was hit. 
        var weakpointCol = col.name == "weakpoint" && attackData.WeakpointAttack();

        var enemyKilled = destructable.RecieveAttack(attackData, weakpointCol);
        //If the destructable has an AI, have it process the player's attack (so it can be stunned, etc.)
        if (!enemyKilled && enemyAI)
        {
            enemyAI.ApplyPlayerAttack(attackData, weakpointCol);
        }

        Destroy(gameObject);
    }

    /// <summary>
    /// Set's the player responsible for making the attack.
    /// </summary>
    public virtual void SetAttackOwner(PlayerController attackOwner)
    {
        attackData.owner = attackOwner;
    }

}
