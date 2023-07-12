using UnityEngine;

/// <summary>
/// Triggers the given trigger group when touched by the player.
/// </summary>
public class KeyCard : PickupBase
{
    [SerializeField]
    private string keyTriggerGroup;

    protected override void OnCollected(PlayerController collector)
    {
        TriggerableBehavior.TriggerGroup(keyTriggerGroup);

        //Pickup message
        collector.pickupMsgManager.AddPickupMessage($"All {keyTriggerGroup} doors are now open!", 2.5f);
    }
}
