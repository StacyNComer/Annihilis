using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPickup : PickupBase
{
    [SerializeField]
    private int amountHealed;
    [SerializeField]
    private bool overheal;

    protected override void OnCollected(PlayerController collector)
    {
        collector.HealPlayer(amountHealed, overheal);
        collector.pickupMsgManager.AddPickupMessage($"+ {amountHealed} Health");
    }

    protected override bool PickupCondition(PlayerController collector)
    {
        return !overheal && collector.GetHitpoints() < 100;
    }
}
