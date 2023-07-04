using System.Collections;
using System.Collections.Generic;
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
    /// What trigger group should be affected when this gameObject fires its trigger. No group is trigger if this is left blank.
    /// </summary>
    [SerializeField]
    protected string triggerGroupTriggered;
    /// <summary>
    /// Specific Triggerables to be affected when this gameObject fires its trigger. Don't include any gameObject's that already have a matching trigger group or it'll be triggered twice!
    /// </summary>
    [SerializeField]
    protected TriggerableBehavior[] toTrigger;

    //True if the trigger has been fired before.
    private bool triggered;

    /// <summary>
    /// Fires the trigger, activating all gameObjects that either match this one's trigger group or are in toTrigger.
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
