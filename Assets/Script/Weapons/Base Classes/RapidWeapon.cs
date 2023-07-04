using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class RapidWeapon : WeaponBehavior
{
    // Update is called once per frame
    new void Update()
    {
        base.Update();

        if (isFiring && fireRateTracker <= 0)
        {
            FirePrimary();
        }
    }
}
