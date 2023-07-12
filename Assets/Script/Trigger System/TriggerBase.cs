using UnityEngine;

/// <summary>
/// A base class for any script made to activate Triggerable GameObjects.
/// </summary>
public abstract class TriggerBase : MonoBehaviour
{
    /// <summary>
    /// If true, the trigger can only be fired once ever.
    /// </summary>
    [SerializeField]
    private bool onlyTriggerOnce = true;
    /// <summary>
    /// What trigger group should be affected when this GameObject fires its trigger. Only Triggerables under toTrigger are affected if this is left blank.
    /// </summary>
    [SerializeField, Tooltip("Only Triggerables under toTrigger can be affected if this is left blank.")]
    protected string triggerGroupTriggered;
    /// <summary>
    /// Specific Triggerables to be affected when this gameObject fires its trigger. Don't include any gameObject's that already have a matching trigger group or they'll be triggered twice!
    /// </summary>
    [SerializeField, Tooltip("Don't include any gameObject's that already have a matching trigger group or they'll be triggered twice!")]
    protected TriggerableBehavior[] toTrigger;

    //True if the trigger has been fired before.
    private bool triggered;

    /// <summary>
    /// Fires the trigger, activating all GameObjects that either match this one's trigger group or are in toTrigger.
    /// </summary>
    protected virtual void FireTrigger()
    {
        if(onlyTriggerOnce && !triggered)
        {
            triggered = true;

            foreach (TriggerableBehavior triggerable in toTrigger)
            {
                triggerable.Trigger();
            }

            if(triggerGroupTriggered != "")
            {
                TriggerableBehavior.TriggerGroup(triggerGroupTriggered);
            }
        }
    }
}
