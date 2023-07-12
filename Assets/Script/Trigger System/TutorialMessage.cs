using UnityEngine;

/// <summary>
/// Showa a message at the top of the player's screen when triggered.
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

    //Text area's don't work in Unity Events. Neither do functions with more than one parameter... :(
}
