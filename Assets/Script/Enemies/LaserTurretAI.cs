using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LaserTurretAI : TurretAI
{
    /// <summary>
    /// The interval at which the player takes damage from being caught in the laser.
    /// </summary>
    [SerializeField, Tooltip("The time this enemy spends firing at the player while in cover.")]
    protected float suppressionTime = 1;

    protected float suppressionTimeTracker;

    // Update is called once per frame
    void Update()
    {
        AimAtPlayerWithDampening();
        
        if(!PlayerLOSCheck())
        {
            //Continue firing the laser until the suppression time ends. 
            if(suppressionTimeTracker > 0)
            {
                FireLaser();

                suppressionTimeTracker -= Time.deltaTime;

                //When the suppression time ends, end the laser attack and set the delay for next time the player is seen.
                if (suppressionTimeTracker <= 0)
                {
                    SetLaserFireEffectActive(false);
                    attackDelayTracker = attackDelay;
                }
            }
        } else //I SEE YOU!
        {
            suppressionTimeTracker = suppressionTime;

            if(attackDelayTracker > 0)
            {
                attackDelayTracker -= Time.deltaTime;

                if(attackDelayTracker <= 0)
                {
                    SetLaserFireEffectActive(true);
                }
            }
            else
            {
                FireLaser();
            }
        }
    }


}
