using UnityEngine;

/// <summary>
/// An AI that simply chases the player. The GameObject will need a Contact Damage component to damage the player.
/// </summary>
public class ChaserAI : EnemyAIBase
{
    void Update()
    {
        ProcessCooldowns();

        if (!IsStunned())
        {
            ChasePlayer();
        }
    }
}
