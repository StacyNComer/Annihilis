using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarcasterBehavior : RapidWeapon
{
    [SerializeField]
    private GameObject primaryProjectile;

    [SerializeField]
    private GameObject secondaryProjectile;

    protected override void FirePrimary()
    {
        if (player.TryUseAmmo(1, primaryAmmoType))
        {
            fireAudio.Play();
            SpawnProjectile(primaryProjectile);
            fireRateTracker = fireRate;
        }
    }

    public override void FireSecondary()
    {
        if (player.TryUseAmmo(1, AmmoType.Citrine))
        {
            SpawnProjectile(secondaryProjectile);
            secondaryAudio.Play();
        }
        else
        {
            NoAmmoAlert();
        }
    }

    protected override void EndWeaponCharge()
    {
        throw new System.NotImplementedException();
    }
}
