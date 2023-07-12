using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The base class for any weapon that should fire continuously while primary fire is held.
/// </summary>
public abstract class RapidWeapon : WeaponBehavior
{
    new void Update()
    {
        base.Update();

        if (isFiring && fireRateTracker <= 0)
        {
            FirePrimary();
        }
    }
}
