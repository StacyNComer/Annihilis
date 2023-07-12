using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Used for Mortifire, the player's starting weapon.
/// </summary>
public class ChargedWeaponBehavior : WeaponBehavior
{
    [Header("Attack Prefabs")]
    [SerializeField]
    private GameObject unchargedProjectile;
    [SerializeField]
    private GameObject lowChargedProjectile;
    [SerializeField]
    private GameObject medChargedProjectile;
    [SerializeField]
    private GameObject hiChargedProjectile;
    [SerializeField]
    private GameObject secondaryProjectile;

    [Header("Audio Sources")]
    [SerializeField]
    private AudioSource fireChargedAudio;
    [SerializeField]
    private AudioSource chargeIncrementAudio;
    [SerializeField]
    private AudioSource chargeMaxAudio;

    private float totalChargeTime = 0;
    /// <summary>
    /// The seconds it takes for the weapon to increment a charge level.
    /// </summary>
    private float chargeIncrementTime = .5f;
    private int chargeLevel = 0;
    /// <summary>
    /// How much ammo each level of charge, from low to hi, uses.
    /// </summary>
    private int[] chargeAmmoCosts = new int[] { 2, 3, 4 };

    new protected void Update()
    {
        base.Update();

        //Charge the weapon while fire is held down. If fire is not being pressed and this weapon has been charged, fire the weapon.
        if (isFiring && fireRateTracker <= 0)
        {
            totalChargeTime += Time.deltaTime;

            //TODO: Smooth out how charging is coded if I decide to keep it.
            //Prevent the player from charging to a level they don't have enough ammo for.
            if (!player.TestAmmo(chargeAmmoCosts[0], primaryAmmoType))
            {
                totalChargeTime = Mathf.Min(.4f, totalChargeTime);
            }
            else if(!player.TestAmmo(chargeAmmoCosts[1], primaryAmmoType))
            {
                totalChargeTime = Mathf.Min(.9f, totalChargeTime);
            }
            else if (!player.TestAmmo(chargeAmmoCosts[2], primaryAmmoType))
            {
                totalChargeTime = Mathf.Min(1.4f, totalChargeTime);
            }

            //Increment the charge level every chargeIncrementTime seconds while playing the appropriate audio.
            if(totalChargeTime >= chargeIncrementTime * (1 + chargeLevel))
            {
                //Play the charge audio. A different sound is played if the weapon is charged to its maximum value.
                if(chargeLevel < 2)
                {
                    chargeIncrementAudio.Play();
                } else if(chargeLevel == 2)
                {
                    chargeMaxAudio.Play();
                }

                chargeLevel++;
            }
        }
        else if (totalChargeTime > 0)
        {
            FirePrimary();
        }
    }

    override protected void FirePrimary()
    {
        //Fire the weapon with a different projectile based on the amount the weapon is charged.
        //The player having enough ammo is checked as the weapon charges, so it does not need to be checked here unless the player didn't charge their weapon.
        if (totalChargeTime < 0.5f)
        {
            if(player.TryUseAmmo(1, primaryAmmoType))
            {
                SpawnProjectile(unchargedProjectile);
                fireAudio.Play();
            }
            else
            {
                NoAmmoAlert();
            }
        }
        else if (totalChargeTime < 1)
        {
            SpawnProjectile(lowChargedProjectile);
            player.ExpendAmmo(chargeAmmoCosts[0], GetPrimaryAmmoType());
            fireChargedAudio.Play();
        }
        else if (totalChargeTime < 1.5f)
        {
            SpawnProjectile(medChargedProjectile);
            player.ExpendAmmo(chargeAmmoCosts[1], GetPrimaryAmmoType());
            fireChargedAudio.Play();
        }
        else
        {
            SpawnProjectile(hiChargedProjectile);
            player.ExpendAmmo(chargeAmmoCosts[2], GetPrimaryAmmoType());
            fireChargedAudio.Play();
        }

        fireRateTracker = fireRate;
        totalChargeTime = 0;
        chargeLevel = 0;
    }

    override public void FireSecondary()
    {
        if(player.TryUseAmmo(1, AmmoType.Citrine))
        {
            SpawnProjectile(secondaryProjectile);
            secondaryAudio.Play();
        } else
        {
            NoAmmoAlert();
        }
    }

    protected override void EndWeaponCharge()
    {
        totalChargeTime = 0;
    }
}
