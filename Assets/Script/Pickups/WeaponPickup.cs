using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponPickup : PickupBase
{
    [SerializeField]
    private WeaponBehavior weapon;

    protected override void OnCollected(PlayerController collector)
    {
        collector.AddWeapon(weapon);
    }
}
