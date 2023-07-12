using UnityEngine;

/// <summary>
/// The base class for GameObjects that can be picked up by the player.
/// </summary>
public abstract class PickupBase : MonoBehaviour
{
    [SerializeField]
    private AudioClip pickupAudio;

    /// <summary>
    /// Used to prevent the player's multiple colliders from triggering the pickup more than once.
    /// </summary>
    private bool grabbed;

    /// <summary>
    /// What occurs when the object is touched by the player.
    /// </summary>
    protected abstract void OnCollected(PlayerController collector);

    /// <summary>
    /// The conditions that must be met for the player to collect the pickup. The default implementation simply returns true.
    /// </summary>
    protected virtual bool PickupCondition(PlayerController collector)
    {
        return true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var collector = other.GetComponent<PlayerController>();

            //If the pickup was not already grabbed and the touching player meets the pickup condition.
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
