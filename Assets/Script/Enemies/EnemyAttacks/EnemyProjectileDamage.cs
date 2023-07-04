using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyProjectileDamage : MonoBehaviour
{
    [SerializeField]
    private EnemyAttackData attackData;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            other.GetComponent<PlayerController>().ApplyEnemyAttack(attackData, transform.position);
            Destroy(gameObject);
        } else if (!other.CompareTag("Destructable") && !other.isTrigger)
        {
            Destroy(gameObject);
        }
    }
}
