using UnityEngine;

/// <summary>
/// Gives the player health when touched by the player.
/// </summary>
public class HealthPickup : PickupBase
{
    [SerializeField]
    private int amountHealed;
    [SerializeField]
    private bool overheal;

    protected override void OnCollected(PlayerController collector)
    {
        //Heal the player
        collector.HealPlayer(amountHealed, overheal);

        //Pickup message
        collector.pickupMsgManager.AddPickupMessage("+ " + amountHealed + " " + (overheal? "Overheal" : "Health"));
    }

    protected override bool PickupCondition(PlayerController collector)
    {
        //Prevent a player with full health from grabbing a health pickup.
        return overheal || collector.GetHitpoints() < 100;
    }
}
