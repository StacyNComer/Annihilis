using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[System.Serializable]
public class EnemyAttackData
{
    [SerializeField]
    private int damage;

    /// <summary>
    /// If this is 0, the attack won't stagger the player.
    /// </summary>
    [SerializeField]
    private float staggerTime;

    [SerializeField]
    private float staggerForce;

    public int GetDamage()
    {
        return damage;
    }

    public float GetStaggerForce()
    {
        return staggerForce;
    }

    public float GetStaggerTime()
    {
        return staggerTime;
    }
}

[System.Serializable]
public struct EnemyAmmoDrop
{
    [SerializeField]
    private AmmoType ammoType;

    [SerializeField]
    private int amountDropped;

    /// <summary>
    /// Gives the ammo from this drop to the given player character.
    /// </summary>
    public void GiveAmmoDrop(PlayerController player)
    {
        player.AddAmmo(amountDropped, ammoType);
    }
}

[System.Serializable]
public struct EnemyLootTable
{
    [SerializeField]
    private EnemyAmmoDrop[] ammoDrops;

    [SerializeField]
    private int enemyHealthDropped;

    [SerializeField]
    private bool healthDropOverheals;

    public void GiveLoot(PlayerController player)
    {
        //Negative healing will not be tolerated!
        if(enemyHealthDropped > 0)
        {
            player.HealPlayer(enemyHealthDropped, healthDropOverheals);
            player.pickupMsgManager.AddPickupMessage($"+{enemyHealthDropped} {(healthDropOverheals ? "Overheal" : "Health")}");
        }
        

        foreach (EnemyAmmoDrop ammoDrop in ammoDrops)
        {
            ammoDrop.GiveAmmoDrop(player);
        }
    }
}

/// <summary>
/// The base class for controlling enemy movement and any attack more complicated than simply running into the player. Also holds functions for basic attacks that are shared between derived classes.
/// </summary>
//[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAIBase : MonoBehaviour
{
    #region Constants
    protected const string animAttackingVar = "Attacking";
    protected const string animStunnedVar = "Stunned";
    #endregion

    [Header("Enemy Stats")]
    [SerializeField]
    protected float attackCooldown;   
    [SerializeField]
    protected Transform attackSpawnPoint;
    /// <summary>
    /// How much stun buildup must be applied to the enemy before it is stunned.
    /// </summary>
    [SerializeField]
    private float stunThreshold;
    [SerializeField]
    private float stunTime;

    [Header("FX")]
    [SerializeField]
    private ParticleSystem stunParticleSystem;

    protected Animator animator;
    protected float attackCooldownTracker = 0;
    /// <summary>
    /// The current stun buildup this enemy has.
    /// </summary>
    private float currentStun;
    private bool imbued;
    /// <summary>
    /// If this reaches 1, the enemy becomes imbued.
    /// </summary>
    private float imbueBuildup;
    protected NavMeshAgent navAgent;
    protected PlayerController player;
    protected Transform playerT;

    /// <summary>
    /// While this is greater than 0, the enemy is stunned.
    /// </summary>
    private float stunTimeTracker;

    // Start is called before the first frame update
    protected void Start()
    {
        animator = GetComponent<Animator>();
        navAgent = GetComponent<NavMeshAgent>();

        player = GameManager.Player;
        playerT = player.transform;
    }

    public static int GetEnemyAttackLayerMask()
    {
        return ~LayerMask.GetMask("Ignore Raycast", "Projectile", "Enemy");
    }

    #region Protected Methods
    /// <summary>
    /// Rotates the given transform toward the player at "aimSpeed" degrees a second. Returns true if the player was successfully aimed at.
    /// </summary>
    /// <param name="aimSpeed">The degrees/s at which the enemy rotates.</param>
    /// <param name="aimLeading">Modifies the position aimed at to where the player will be in this many seconds.</param>
    protected bool AimAtPlayer(Transform aimedTransform, float aimSpeed, float aimLeading = 0)
    {
        var aimPos = playerT.position + player.GetClampedVelocity()*aimLeading;
        var targetRot = Quaternion.LookRotation(aimPos - aimedTransform.position);
        aimedTransform.rotation = Quaternion.RotateTowards(aimedTransform.rotation, targetRot, aimSpeed * Time.deltaTime);

        return aimedTransform.rotation == targetRot;
    }

    /// <summary>
    /// Rotates the attached transform toward the player at "aimSpeed" degrees a second. Returns true if the player was successfully aimed at.
    /// </summary>
    /// <param name="aimSpeed">The degrees/s at which the enemy rotates.</param>
    /// <param name="aimLeading">Modifies the position aimed at to where the player will be in this many seconds.</param>
    protected bool AimAtPlayer(float aimSpeed, float aimLeading = 0)
    {
        return AimAtPlayer(transform, aimSpeed, aimLeading);
    }

    protected bool CanAttack()
    {
        return attackCooldownTracker <= 0 && stunTimeTracker <= 0;
    }

    protected void ChasePlayer()
    {
        navAgent.SetDestination(player.transform.position);
    }

    /// <summary>
    /// Rotates the enemy to face the player. This function does not pitch the enemy up or down.
    /// </summary>
    protected void FacePlayer()
    {
        var playerPosFlattened = new Vector3(playerT.position.x, transform.position.y, playerT.position.z);
        transform.rotation = Quaternion.LookRotation(playerPosFlattened - transform.position);
    }

    /// <summary>
    /// For use in coroutines. Fires numShots projectiles at the target Transform every volleyDelay seconds. Will end prematurely if the enemy is stunned.
    /// </summary>
    /// <param name="onVolleyCompleted">This is allowed to be null.</param>
    /// <returns></returns>
    protected IEnumerator FireProjectileVolley(GameObject projectile, Transform targetT, int numShots, float volleyDelay, AudioClip fireAudioClip, float spread = 0, System.Action onVolleyCompleted = null)
    {
        float volleyDelayTracker = 0;

        for(int i = 0; i < numShots;)
        {
            //End the volley if the enemy is stunned.
            if(IsStunned())
            {
                break;
            }

            if(volleyDelayTracker <= 0)
            {
                AudioSource.PlayClipAtPoint(fireAudioClip, transform.position);
                SpawnProjectile(projectile, targetT.position, spread);
                volleyDelayTracker = volleyDelay;
                i++;
            } else
            {
                volleyDelayTracker -= Time.deltaTime;
            }

            yield return null;
        }

        if(onVolleyCompleted != null)
        {
            onVolleyCompleted();
        } 
    }

    protected bool PlayerLOSCheck()
    {
        if(Physics.Raycast(transform.position, Vector3.Normalize(playerT.position - transform.position), out RaycastHit hit, Mathf.Infinity, GameManager.GetRaycastLayerMask(), QueryTriggerInteraction.Ignore) && hit.collider.CompareTag("Player"))
        {  
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Processes any cooldowns as well as any other values that every enemy must set over time. This should be placed in the update function of any derived class.
    /// </summary>
    protected void ProcessCooldowns()
    {
        if(attackCooldownTracker > 0)
        {
            attackCooldownTracker -= Time.deltaTime;
        }
        
        if(IsStunned())
        {
            stunTimeTracker -= Time.deltaTime;

            if(stunTimeTracker <= 0)
            {
                navAgent.isStopped = false;

                //Clear the stun particles
                stunParticleSystem.Stop();
                stunParticleSystem.Clear();

                //Exit stun animation
                animator.SetBool(animStunnedVar, false);
            }
        }
    }

    /// <summary>
    /// Spawns the given projectile facing targetPos. Can also accept a spread value
    /// </summary>
    /// <param name="projectile"></param>
    /// <param name="targetPos"></param>
    /// <param name="spread"></param>
    protected void SpawnProjectile(GameObject projectile, Vector3 targetPos, float spread = 0)
    {
        var spawned = Instantiate(projectile, attackSpawnPoint.position, Quaternion.identity);
        spawned.transform.LookAt(targetPos);

        if(spread > 0)
        {
            var deviation = Random.Range(-spread, spread);
            spawned.transform.Rotate(0, deviation, 0);
        }
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Applies any effects from a player attack that are handled by the AI, such as Stun or Imbuing. The "Destructable" component handles actual damage dealt to the the enemy, if it is vulnerable to such a thing.
    /// </summary>
    /// <param name="attackData"></param>
    /// <param name="weakpointHit"></param>
    public void ApplyPlayerAttack(PlayerAttackData attackData, bool weakpointHit)
    {
        AddStun(attackData.GetStun());
    }

    /// <summary>
    /// Adds stun buildup to the enemy. If this stuns the enemy, the currentStun is reset. Enemies cannot have stun buildup added while they are stunned.
    /// </summary>
    public void AddStun(float amount)
    {
        if(!IsStunned())
        {
            currentStun += amount;

            if (currentStun >= stunThreshold)
            {
                Stun();
                currentStun = 0;
            }
        } 
    }

    /// <summary>
    /// True if the enemy is stunned for any reason.
    /// </summary>
    public bool IsStunned()
    {
        return stunTimeTracker > 0;
    }

    /// <summary>
    /// Stops/resumes the navAgent movement of this enemy.
    /// </summary>
    /// <param name="isStopped"></param>
    public void SetMovementStopped(bool isStopped)
    {
       if(navAgent.isOnNavMesh)
       {
            navAgent.isStopped = isStopped;
       }
        
    }

    /// <summary>
    /// Stuns the enemy. Can be overriden in derived classes for additional effects.
    /// </summary>
    public virtual void Stun()
    {
        stunTimeTracker = stunTime;

        if(navAgent && navAgent.isOnNavMesh)
        {
            navAgent.isStopped = true;
        }

        stunParticleSystem.Play();

        animator.SetBool(animStunnedVar, true);
    }
    #endregion
}
