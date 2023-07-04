using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField]
    private GameObject spawnedObject;

    [SerializeField]
    private float spawnDelay = 1.5f;

    [SerializeField]
    private bool spawnOnStart;

    /// <summary>
    /// An optional death trigger the spawned gameObject should report to. This make the assumption that the gameObject spawned has a Destructable component.
    /// </summary>
    [SerializeField]
    private TriggerOnDeath DeathTriggerToReport;

    private ParticleSystem spawnParticles;

    private float spawnDelayTracker;

    // Start is called before the first frame update
    void Start()
    {
        spawnParticles = GetComponent<ParticleSystem>();
        
        if(spawnedObject.TryGetComponent(out CustomSpawnData spawnColor))
        {
            var psMain = spawnParticles.main;
                
            psMain.startColor = spawnColor.GetSpawnColor();
        }

        if(spawnOnStart)
        {
            SpawnObject();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (spawnDelayTracker > 0)
        {
            spawnDelayTracker -= Time.deltaTime;

            if (!spawnParticles.isPlaying && spawnDelayTracker <= 1.5)
            {
                spawnParticles.Play();
            }
            else if (spawnDelayTracker <= 0)
            {
                for(int i = 0; i < GameManager.enemyMultiplier; i++)
                {
                    var spawned = Instantiate(spawnedObject, transform.position, transform.rotation);

                    if (DeathTriggerToReport)
                    {
                        DeathTriggerToReport.AddDeathAwaited();
                        spawned.GetComponent<Destructable>().SetDeathTrigger(DeathTriggerToReport);
                    }
                }
         
                spawnParticles.Stop();
            }
        }
    }

    public void SpawnObject()
    {
        spawnDelayTracker = spawnDelay;
    }
}
