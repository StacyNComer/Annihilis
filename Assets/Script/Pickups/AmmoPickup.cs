using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmoPickup : PickupBase
{
    [SerializeField]
    private AmmoType ammo;
    [SerializeField]
    private int amount;

    /// <summary>
    /// Prevent the pickup from being collected if the user already has max ammo. 
    /// </summary>
    protected override bool PickupCondition(PlayerController collector)
    {
        return !collector.AmmoMaxed(ammo);
    }


    override protected void OnCollected(PlayerController collector)
    {
        collector.GetComponent<PlayerController>().AddAmmo(amount, ammo);
    }
}
