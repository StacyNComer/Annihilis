using UnityEngine;

/// <summary>
/// A script for spawning GameObjects with an accompanying particle effect at the position of the attached gameObject. The spawn particles can be controlled by attaching a "CustomSpawnData" script to the GameObject being spawned.
/// </summary>
public class Spawner : MonoBehaviour
{
    /// <summary>
    /// The spawning particles appear when the spawn delay has this many seconds left.
    /// </summary>
    private const float spawnEffectTime = 1.5f;

    [SerializeField]
    private GameObject spawnedObject;

    [SerializeField]
    private float spawnDelay = 1.5f;

    [SerializeField]
    private bool spawnOnStart;

    /// <summary>
    /// An optional death trigger the spawned gameObject should report to. This makes the assumption that the gameObject spawned has a Destructable component.
    /// </summary>
    [SerializeField, Tooltip("An optional death trigger the spawned gameObject should report to. This makes the assumption that the gameObject spawned has a Destructable component.")]
    private TriggerOnDeath DeathTriggerToReport;

    private ParticleSystem spawnParticles;

    /// <summary>
    /// While this is above zero, this script is in the process of spawning.
    /// </summary>
    private float spawnDelayTracker;

    void Start()
    {
        //Caching
        spawnParticles = GetComponent<ParticleSystem>();
        
        //If the spawnedObject provides customSpawnData, apply it to the spawner.
        if(spawnedObject.TryGetComponent(out CustomSpawnData spawnEffectData))
        {
            var psMain = spawnParticles.main;
               
            //Set the spawn particle color.
            psMain.startColor = spawnEffectData.GetSpawnColor();
        }

        //Start the spawning process if spawnOnStart is set.
        if(spawnOnStart)
        {
            SpawnObject();
        }
    }

    void Update()
    {
        //The spawning process.
        if (IsSpawning())
        {
            spawnDelayTracker -= Time.deltaTime;

            //Turn on the spawning FX when the spawn delay has spawnEffectTime seconds remaining. Spawn the GameObject when the spawn delay.
            if (!spawnParticles.isPlaying && spawnDelayTracker <= spawnEffectTime)
            {
                spawnParticles.Play();
            }
            else if (spawnDelayTracker <= 0)
            {
                //Spawns this spawners GameObject an amount of times equal to the enemyMultiplier.
                for(int i = 0; i < GameManager.enemyMultiplier; i++)
                {
                    var spawned = Instantiate(spawnedObject, transform.position, transform.rotation);

                    //Adds the spawned enemy to a Death Trigger if one is given.
                    if (DeathTriggerToReport)
                    {
                        DeathTriggerToReport.AddDeathAwaited();
                        spawned.GetComponent<Destructable>().SetDeathTrigger(DeathTriggerToReport);
                    }
                }
         
                //End the spawning FX.
                spawnParticles.Stop();
            }
        }
    }

    /// <summary>
    /// Returns true if this object is in the process of spawning (i.e. It's spawnDelayTracker is > 0).
    /// </summary>
    /// <returns></returns>
    public bool IsSpawning()
    {
        return spawnDelayTracker > 0;
    }

    /// <summary>
    /// Start the spawning process for this script.
    /// </summary>
    public void SpawnObject()
    {
        spawnDelayTracker = spawnDelay;
    }
}
