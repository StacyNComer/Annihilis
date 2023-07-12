using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Adds the given weapon to the player that touches this pickup.
/// </summary>
public class WeaponPickup : PickupBase
{
    [SerializeField]
    private WeaponBehavior weapon;

    protected override void OnCollected(PlayerController collector)
    {
        collector.AddWeapon(weapon);
    }
}
