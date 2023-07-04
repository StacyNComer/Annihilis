using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackerTurret : TurretAI
{
    [SerializeField]
    protected float laserFirePeriod;

    /// <summary>
    /// How far ahead of the player to aim, measured in where the player will be in x seconds.
    /// </summary>
    protected float attackTracking;
    protected float laserFirePeriodTracker;
    /// <summary>
    /// True if the laser as been aimed to its starting location and is waiting for the attack delay to end.
    /// </summary>
    protected bool laserCharging;

    new protected void Start()
    {
        base.Start();

        attackTracking = attackDelay + .2f;

        //The attack of this enemy starts on cooldown so it has the chance to track the player
        attackCooldownTracker = attackCooldown;
    }

    // Update is called once per frame
    new protected void Update()
    {
        base.Update();

        if(attackCooldownTracker > 0)
        {
            attackCooldownTracker -= Time.deltaTime;
        }
        else if(laserCharging)
        {
            attackDelayTracker -= Time.deltaTime;

            if(attackDelayTracker <= 0)
            {
                SetLaserFireEffectActive(true);
                laserFirePeriodTracker = laserFirePeriod;
                laserCharging = false;
            }
        } else if (laserFirePeriodTracker > 0)
        {
            AimAtPlayerWithDampening();
            FireLaser();
            
            laserFirePeriodTracker -= Time.deltaTime;

            if(laserFirePeriodTracker <= 0)
            {
                SetLaserFireEffectActive(false);
                attackCooldownTracker = attackCooldown;
            }
        }
        else if(attackCooldownTracker <= 0 && PlayerLOSCheck())
        {
            if(AimAtPlayer(baseTurnSpeed * 10, attackTracking))
            {
                laserCharging = true;
                attackDelayTracker = attackDelay;
            }
        }
    }
}
