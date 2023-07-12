using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// The base class for GameObjects that can be affected in some form by a trigger.
/// </summary>
public class TriggerableBehavior : MonoBehaviour
{
    /// <summary>
    /// A dictionary made to contain the trigger groups in play. Each list should contain every triggerableBehavior with a triggerGroup matching its key.
    /// </summary>
    private readonly static Dictionary<string, List<TriggerableBehavior>> triggerGroups = new Dictionary<string, List<TriggerableBehavior>>();

    [SerializeField]
    protected UnityEvent onTriggered;
    /// <summary>
    /// Allows this and any other game object in the same trigger group to be triggered all at once.
    /// </summary>
    [SerializeField]
    private string triggerGroupName;

#if UNITY_EDITOR
    [SerializeField, Tooltip("[EDITOR ONLY] Causes a debug message to be logged when this script is triggered.")]
    private string debugMessage;
#endif

    void Awake()
    {
        //If the triggerable is a part of a trigger group, register it in the static triggerGroups dictionary.
        if(triggerGroupName != "")
        {
            //If this is the first triggerable in its trigger group, initialize its relevant list in the triggerGroups dictionary.
            if (!triggerGroups.ContainsKey(triggerGroupName))
            {
                triggerGroups[triggerGroupName] = new List<TriggerableBehavior>();
            }

            triggerGroups[triggerGroupName].Add(this);
        }
        
#if UNITY_EDITOR
        if(debugMessage != "")
        {
            onTriggered.AddListener(() => Debug.Log(debugMessage));
        }
#endif
    }

    /// <summary>
    /// Invokes this Triggerable's onTrigger event.
    /// </summary>
    public void Trigger()
    {
        onTriggered.Invoke();
    }

    /// <summary>
    /// Triggers every Triggerable within the same trigger group. This function assumes at least one actor exists within the trigger group.
    /// </summary>
    /// <param name="name">This should not be blank. The "" trigger group will always be empty.</param>
    public static void TriggerGroup(string name)
    {
        foreach(TriggerableBehavior triggerable in triggerGroups[name])
        {
            triggerable.onTriggered.Invoke();
        }
    }
}
