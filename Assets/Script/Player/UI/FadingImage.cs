using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Fades an attached UI image out of view when PlayFade() is called.
/// </summary>
public class FadingImage : FadingColor
{
    private Image image;

    new protected void Start()
    {
        image = gameObject.GetComponent<Image>();

        base.Start();
    }

    protected override void SetOpacity(float alpha)
    {
        image.color = new Color(image.color.r, image.color.b, image.color.g, alpha);
    }
}
