using UnityEngine;

/// <summary>
/// An enemy that fires a single projectile everytime it attacks.
/// </summary>
public class SingleProjectileAI : RangedEnemyAIBase
{ 
    protected override void Attack()
    {
        SetMovementStopped(false);
        AudioSource.PlayClipAtPoint(attackAudioClip, transform.position);
        SpawnProjectile(projectile, playerT.position);
    }
}
