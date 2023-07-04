using UnityEngine;


public class ChaserAI : EnemyAIBase
{
    // Update is called once per frame
    void Update()
    {
        ProcessCooldowns();

        if (!IsStunned())
        {
            ChasePlayer();
        }
    }
}
