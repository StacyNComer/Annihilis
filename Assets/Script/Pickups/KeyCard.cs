using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyCard : PickupBase
{
    [SerializeField]
    private string keyTrigger;

    protected override void OnCollected(PlayerController collector)
    {
        TriggerableBehavior.TriggerGroup(keyTrigger);
        collector.pickupMsgManager.AddPickupMessage($"All {keyTrigger} doors are now open!", 2.5f);
    }
}
