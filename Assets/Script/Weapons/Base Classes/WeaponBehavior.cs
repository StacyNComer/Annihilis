using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The base class for all weapons. Handles what happens what happens when a weapon is fired, but not ammo expendadure.
/// </summary>
public abstract class WeaponBehavior : MonoBehaviour
{
    [SerializeField]
    protected float fireRate = .25f;
    protected float fireRateTracker = 0;

    [SerializeField]
    private string weaponName;

    [SerializeField]
    protected PlayerController player;

    [SerializeField]
    protected AmmoType primaryAmmoType;
    [SerializeField]
    protected AmmoType secondaryAmmoType;

    [SerializeField]
    private int weaponSlot;

    [Header("Audio Sources")]
    [SerializeField]
    protected AudioSource fireAudio;
    [SerializeField]
    protected AudioSource secondaryAudio;
    [SerializeField]
    private AudioSource noAmmoAudio;

    protected Transform attackSpawnPoint;

    protected bool isFiring;

    private void Start()
    {
        attackSpawnPoint = player.GetAttackSpawnPoint();
    }

    // Update is called once per frame
    protected void Update()
    {
        if (fireRateTracker > 0)
        {
            fireRateTracker -= Time.deltaTime;
        } 
    }

    public static void SpawnExplosion(Vector3 pos, float radius, PlayerAttackData playerAttackData, EnemyAttackData selfDamageData, Destructable ignoredDestructable)
    {
        //Get the overlapped gameObjects
        var collisions = Physics.OverlapSphere(pos, radius);

        //Tracks which destructables have already been hit by the explosion. Most enemies have multiple colliders that could be caught in the sphere.
        var destructablesIgnored = new List<Destructable>();
        //Add the ignoredDestructable, if it isn't null.
        if(ignoredDestructable != null)
        {
            destructablesIgnored.Add(ignoredDestructable);
        }

        //Tracks whether or not the player was hit so that they are only damaged once (The player has 2 colliders).
        var hitPlayer = false;
        
        //Damage any overlapped 
        foreach (Collider col in collisions)
        {
            if (col.CompareTag("Destructable"))
            {
                var destructableHit = col.GetComponentInParent<Destructable>();

                if(!destructablesIgnored.Contains(destructableHit))
                {
                    //NOTE: in the future, destructables should just check for enemy AI :P.
                    var enemyKilled = col.GetComponentInParent<Destructable>().RecieveAttack(playerAttackData, false);

                    if (!enemyKilled)
                    {
                        var enemyAI = col.GetComponentInParent<EnemyAIBase>();
                        
                        if(enemyAI)
                            enemyAI.ApplyPlayerAttack(playerAttackData, false);
                    }

                    destructablesIgnored.Add(destructableHit);
                }
            }
            else if (!hitPlayer && col.CompareTag("Player"))
            {
                col.GetComponent<PlayerController>().ExplosiveDamage(selfDamageData, pos, radius);
                hitPlayer = true;
            }    
        }
    }

    protected abstract void FirePrimary();
    public abstract void FireSecondary();

    //Called when the weapon stops firing or is unequipped. Use to reset certain weapon traits, such as if it was being charged.
    protected abstract void EndWeaponCharge();

    /// <summary>
    /// Plays a sound and displays a message telling the player that they are out of precious ammo.
    /// </summary>
    protected void NoAmmoAlert()
    {
        noAmmoAudio.Play();
        player.pickupMsgManager.AddPickupMessage("No Ammo!");
    }

    /// <summary>
    /// Spawns a projectile at the position and rotation of the weapon's projectile spawn point.
    /// </summary>
    protected void SpawnProjectile(GameObject projectile)
    {
        var attack = Instantiate(projectile, attackSpawnPoint.position, attackSpawnPoint.rotation).GetComponent<PlayerAttack>();
        attack.SetAttackOwner(player);
    }

    public string GetWeaponName()
    {
        return weaponName;
    }

    public int GetWeaponSlot()
    {
        return weaponSlot;
    }

    public AmmoType GetPrimaryAmmoType()
    {
        return primaryAmmoType;
    }

    public void StartFiringPrimary()
    {
        isFiring = true;
    }

    public void StopFiringPrimary()
    {
        isFiring = false;
    }
}
