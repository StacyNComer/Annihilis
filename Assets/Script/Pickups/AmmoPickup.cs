using UnityEngine;

/// <summary>
/// Gives the player ammo when collided with.
/// </summary>
public class AmmoPickup : PickupBase
{
    [SerializeField]
    private AmmoType ammo;
    [SerializeField]
    private int amount;

    protected override bool PickupCondition(PlayerController collector)
    {
        //Prevent the pickup from being collected if the user already has max ammo. 
        return !collector.AmmoMaxed(ammo);
    }


    override protected void OnCollected(PlayerController collector)
    {
        collector.GetComponent<PlayerController>().AddAmmo(amount, ammo);
    }
}
