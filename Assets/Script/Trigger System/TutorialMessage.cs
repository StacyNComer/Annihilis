using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Text area's don't work in Unity Events. Neither do functions with more than one parameter... :(
/// </summary>
public class TutorialMessage : TriggerableBehavior
{
    [SerializeField, TextArea]
    private string message;

    private PlayerController player;

    private void Start()
    {
        player = GameManager.Player;

        onTriggered.AddListener(() => player.ShowTutorialMessage(message));
    }
}
