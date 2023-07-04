using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupMessageManager : MonoBehaviour
{
    /// <summary>
    /// This should be a GameObject with a TMP Text and Fading Text component.
    /// </summary>
    [SerializeField]
    private GameObject pickupMsgPrefab;

    /// <summary>
    /// Prints a message below the player's crosshair that fades away after the given time. If there are 3 messages already displayed, the fourth fades no matter the fade time and any older than that instantly dissappear.
    /// </summary>
    public void AddPickupMessage(string messageText, float fadeDelay = 1)
    {
        //Spawn the message an set its text.
        var message = Instantiate(pickupMsgPrefab, transform);
        message.GetComponentInChildren<TMPro.TMP_Text>().text = messageText;

        //The fade delay for pickup messages is already one, so it isn't changed if the default value is used.
        if (fadeDelay != 1)
        {
            message.GetComponentInChildren<FadingText>().SetFadeDelay(fadeDelay);
            message.GetComponentInChildren<FadingImage>().SetFadeDelay(fadeDelay);
        }

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
