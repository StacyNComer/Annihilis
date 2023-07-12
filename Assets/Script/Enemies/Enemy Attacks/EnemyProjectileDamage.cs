using UnityEngine;

public class EnemyProjectileDamage : MonoBehaviour
{
    [SerializeField]
    private EnemyAttackData attackData;

    private void OnTriggerEnter(Collider other)
    {
        //Destroy this GameObject if it touches something other than a trigger. Damage a player if one was hit.
        if(other.CompareTag("Player"))
        {
            //Damage the player
            other.GetComponent<PlayerController>().ApplyEnemyAttack(attackData, transform.position);

            Destroy(gameObject);

        } else if (!other.CompareTag("Destructable") && !other.isTrigger)
        {
            Destroy(gameObject);
        }
    }
}
