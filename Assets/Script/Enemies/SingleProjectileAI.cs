using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleProjectileAI : RangedEnemyAI
{ 
    protected override void Attack()
    {
        SetMovementStopped(false);
        AudioSource.PlayClipAtPoint(attackAudioClip, transform.position);
        SpawnProjectile(projectile, playerT.position);
    }
}
