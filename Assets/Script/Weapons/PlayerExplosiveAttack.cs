using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Causes the gameObject to explode when destroyed.
/// </summary>
public class PlayerExplosiveAttack : PlayerAttack
{
    /// <summary>
    /// What the explosion does to enemies
    /// </summary>
    [SerializeField]
    PlayerAttackData explosionAttackData;

    [SerializeField]
    AudioClip explosionAudio;

    /// <summary>
    /// What the explosion does to players.
    /// </summary>
    [SerializeField]
    EnemyAttackData selfDamageData;

    [SerializeField]
    private float radius;

    [SerializeField]
    private GameObject explosionEffect;

    /// <summary>
    /// Any gameObject the resultant explosion should ignore.
    /// </summary>
    private Destructable destructableIgnored;

    private void OnDestroy()
    {
        WeaponBehavior.SpawnExplosion(transform.position, radius, explosionAttackData, selfDamageData, destructableIgnored);

        AudioSource.PlayClipAtPoint(explosionAudio, transform.position);
        
        Destroy(Instantiate(explosionEffect, transform.position, Quaternion.identity), 2);
    }

    override protected void OnDestructableHit(Destructable destructable, Collider col)
    {
        destructableIgnored = destructable;
        base.OnDestructableHit(destructable, col);
    }

    override public void SetAttackOwner(PlayerController attackOwner)
    {
        base.SetAttackOwner(attackOwner);
        explosionAttackData.owner = attackOwner;
    }
}
