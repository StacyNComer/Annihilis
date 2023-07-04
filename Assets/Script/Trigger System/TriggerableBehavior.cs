using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public class TriggerableBehavior : MonoBehaviour
{
    private readonly static Dictionary<string, List<TriggerableBehavior>> triggerGroups = new Dictionary<string, List<TriggerableBehavior>>();

    [SerializeField]
    protected UnityEvent onTriggered;
    /// <summary>
    /// If present, allows this and any other Triggerable of the same name to be triggered at once.
    /// </summary>
    [SerializeField]
    private string triggerGroupName;

#if UNITY_EDITOR
    [SerializeField]
    private string debugMessage;
#endif

    // Start is called before the first frame update
    void Awake()
    {
        if(triggerGroupName != "")
        {
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
