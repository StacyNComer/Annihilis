using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A class for displaying pickup messages to the player and fading them out of view when too many appear at once.
/// </summary>
public class PickupMessageManager : MonoBehaviour
{
    /// <summary>
    /// This should be a GameObject with a TMP Text and FadingText component.
    /// </summary>
    [SerializeField]
    private GameObject pickupMsgPrefab;

    /// <summary>
    /// Prints a message below the player's crosshair that fades away after the given time. If there are 3 messages already displayed, the third oldest begins fading early and any older than that instantly dissappear.
    /// </summary>
    public void AddPickupMessage(string messageText, float fadeDelay = 1)
    {
        //Spawn the message and set its text.
        var message = Instantiate(pickupMsgPrefab, transform);
        message.GetComponentInChildren<TMPro.TMP_Text>().text = messageText;

        //Set the message's fade delay for its text and background.
        message.GetComponentInChildren<FadingText>().SetFadeDelay(fadeDelay);
        message.GetComponentInChildren<FadingImage>().SetFadeDelay(fadeDelay);

        //If there are 4 messages after last one spawns, make the oldest one fade. If there are 5, kill the oldest message.
        if (transform.childCount > 5)
        {
            Destroy(transform.GetChild(0).gameObject);
        } else if (transform.childCount > 4)
        {
            transform.GetChild(0).GetComponentInChildren<FadingText>().SkipFadeDelay();
        }
    }
}
