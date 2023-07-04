using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A weapon with 3 stages of charge for its primary.
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

    private float chargeTime = 0;
    private int chargeLevel = 0;
    private int[] chargeAmmoCosts = new int[] { 2, 3, 4 };

    new protected void Update()
    {
        base.Update();

        if (isFiring && fireRateTracker <= 0)
        {
            chargeTime += Time.deltaTime;

            //TODO: Smoothout how charging is coded if I decide to keep it.
            if (!player.TestAmmo(chargeAmmoCosts[0], primaryAmmoType))
            {
                chargeTime = Mathf.Min(.4f, chargeTime);
            }
            else if(!player.TestAmmo(chargeAmmoCosts[1], primaryAmmoType))
            {
                chargeTime = Mathf.Min(.9f, chargeTime);
            }
            else if (!player.TestAmmo(chargeAmmoCosts[2], primaryAmmoType))
            {
                chargeTime = Mathf.Min(1.4f, chargeTime);
            }

            if(chargeTime >= 0.5f * (1 + chargeLevel))
            {
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
        else if (chargeTime > 0)
        {
            FirePrimary();
        }
    }

    override protected void FirePrimary()
    {
        //NOTE: The player  having enough ammo is checked as the weapon charges, so it does not need to be checked here unless the player didn't charge their weapon.
        if (chargeTime < 0.5f)
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
        else if (chargeTime < 1)
        {
            SpawnProjectile(lowChargedProjectile);
            player.ExpendAmmo(chargeAmmoCosts[0], GetPrimaryAmmoType());
            fireChargedAudio.Play();
        }
        else if (chargeTime < 1.5f)
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
        chargeTime = 0;
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
        chargeTime = 0;
    }
}
