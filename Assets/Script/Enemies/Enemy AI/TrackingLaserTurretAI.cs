using UnityEngine;

/// <summary>
/// A laser turret that aims ahead of the player before sweeping a laser toward them.
/// </summary>
public class TrackingLaserTurretAI : LaserTurretAIBase
{
    [SerializeField]
    protected float laserFireTime;
    /// <summary>
    /// The time the enemy spends warning the player of its attack. Should be set using "SetAttackDelay" so that the laser's tracking works properly.
    /// </summary>
    [SerializeField]
    private float attackDelay = 1.25f;
    [SerializeField, Tooltip("Use this value increase how much the laser leads its targeting ahead of the player. The laser targets where the player will be in attackDelay + attackTrackingOffset seconds.")]
    private float attackLeadOffset = .2f;

    protected float attackDelayTracker;
    /// <summary>
    /// How far ahead of the player to aim the laser, measured in where the player will be in the given amount of seconds.
    /// </summary>
    protected float attackLeading;
    protected float laserFirePeriodTracker;
    /// <summary>
    /// True if the laser as been aimed to its starting location and is waiting for the attack delay to end.
    /// </summary>
    protected bool laserCharging;
    

    new protected void Start()
    {
        base.Start();

        SetLaserRenderingMode(LaserRenderingMode.Disabled);

        attackDelayTracker = attackDelay;

        //Initialize attackLeading
        CalculateAttackTracking();

        //The attack of this enemy starts on cooldown so it has the chance to track the player
        attackCooldownTracker = attackCooldown;
    }

    // Update is called once per frame
    new protected void Update()
    {
        base.Update();

        if(attackCooldownTracker > 0) 
        {
            //Decrement laser cooldown
            attackCooldownTracker -= Time.deltaTime;

            if(attackCooldownTracker <= 0)
            {
                SetLaserRenderingMode(LaserRenderingMode.Targeting);
            }
        }
        else if(laserCharging)
        {
            attackDelayTracker -= Time.deltaTime;

            if(attackDelayTracker <= 0)
            {
                SetLaserRenderingMode(LaserRenderingMode.Firing);
                laserFirePeriodTracker = laserFireTime;
                laserCharging = false;
            }
        } else if (LaserFiring())
        {
            AimAtPlayerWithDampening();
            
            laserFirePeriodTracker -= Time.deltaTime;

            if(laserFirePeriodTracker <= 0)
            {
                SetLaserRenderingMode(LaserRenderingMode.Disabled);
                attackCooldownTracker = attackCooldown;
            }
        }
        else if(attackCooldownTracker <= 0 && PlayerLOSCheck())
        {
            if(AimAtPlayer(baseTurnSpeed * 30, attackLeading))
            {
                laserCharging = true;
                attackDelayTracker = attackDelay;
            }
        }

        UpdateLaser(LaserFiring());
    }

    private bool LaserFiring()
    {
        return laserFirePeriodTracker > 0;
    }

    /// <summary>
    /// Calculates the laser's 
    /// </summary>
    private void CalculateAttackTracking()
    {
        attackLeading = attackDelay + attackLeadOffset;
    }

    /// <summary>
    /// Sets the attackDelay and calls CalculateAttackTracking() to update the attackLeading.
    /// </summary>
    /// <param name="value">The new attackDelay value.</param>
    private void SetAttackDelay(float value)
    {
        attackDelay = value;

        CalculateAttackTracking();
    }
}
