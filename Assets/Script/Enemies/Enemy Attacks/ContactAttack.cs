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

        //Cache an enemyAI class, if one is found, and set "hasAI"
        hasAI = TryGetComponent(out enemyAI);
    }

    private void Update()
    {
        if(cooldownTracker > 0)
        {
            cooldownTracker -= Time.deltaTime;

            //If the enemy's movement was stopped due to contact damage being dealt, restart the enemy's movement. TODO: Make it harder for this to stop movement at inappropriate times.
            if(stopMovementDuringCooldown && cooldownTracker <= 0 && !AIstunned())
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
        //Damage a player that touches this GameObject.
        if(collision.gameObject.CompareTag("Player") && cooldownTracker <= 0 && !AIstunned())
        {
            //Plays an animation, if an animation controller is present.
            if (animator)
            {
                animator.SetTrigger("ContactAttack");
            }

            //Damage the player
            collision.gameObject.GetComponent<PlayerController>().ApplyEnemyAttack(attackData, transform.position);

            cooldownTracker = cooldown;

            //Halt an attached AI's movement, if this class is set to do so.
            if (stopMovementDuringCooldown)
            {
                enemyAI.SetMovementStopped(true);
            }
        }
    }
}
