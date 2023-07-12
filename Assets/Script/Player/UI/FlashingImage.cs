using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A script that allows the attached image to be flashed a given color.
/// </summary>
[RequireComponent(typeof(Image))]
public class FlashingImage : MonoBehaviour
{
    private Color baseColor;

    private Color flashColor;

    private Image image;

    private float flashTime;
    private float flashTracker;

    void Start()
    {
        image = GetComponent<Image>();
        baseColor = image.color;
    }

    void Update()
    {
        if(flashTracker > 0)
        {
            float alpha = flashTracker / flashTime;

            image.color = Color.Lerp(baseColor, flashColor, alpha);

            flashTracker -= Time.deltaTime;
        }
    }

    /// <summary>
    /// Causes the attached image to flash the given color. The image is lerped back to its base color over the given time.
    /// </summary>
    public void FlashImage(Color color, float time)
    {
        flashColor = color;
        flashTracker =  flashTime = time;
    }

    /// <summary>
    /// Causes the attached image to flash the given color and lerp back to newBaseColor over the given time.
    /// </summary>
    public void FlashImage(Color color, Color newBaseColor, float time)
    {
        baseColor = newBaseColor;

        FlashImage(color, time);
    }

    /// <summary>
    /// Sets the base color of the image. If the image is not currently flashing, the image's color is changed to match. Otherwise, the image will lerp to its new base color as the flashing ends instead of its old one.
    /// </summary>
    /// <param name="color"></param>
    public void SetBaseColor(Color color)
    {
        baseColor = color;
        
        if(flashTracker <= 0)
        {
            image.color = baseColor;
        }
    }
}
