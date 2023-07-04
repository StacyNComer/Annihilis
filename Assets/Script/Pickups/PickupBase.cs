using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PickupBase : MonoBehaviour
{
    [SerializeField]
    private AudioClip pickupAudio;

    //Used to prevent the player's multiple colliders from triggering the pickup more than once.
    private bool grabbed;

    protected abstract void OnCollected(PlayerController collector);

    /// <summary>
    /// The conditions that must be met for the player to collect the pickup. The default implementation simply returns true.
    /// </summary>
    /// <param name="collector"></param>
    protected virtual bool PickupCondition(PlayerController collector)
    {
        return true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var collector = other.GetComponent<PlayerController>();

            if (!grabbed && PickupCondition(collector))
            {
                AudioSource.PlayClipAtPoint(pickupAudio, transform.position, .5f);
                OnCollected(collector);
                grabbed = true;

                Destroy(gameObject);
            }
        }
    }
}
