using UnityEngine;

/// <summary>
/// Fades an attached TMP Text component when PlayFade() is called.
/// </summary>
public class FadingText : FadingColor
{
    private TMPro.TMP_Text text;

    new protected void Start()
    {
        text = gameObject.GetComponent<TMPro.TMP_Text>();

        base.Start();
    }

    protected override void SetOpacity(float alpha)
    {
        text.color = new Color(text.color.r, text.color.b, text.color.g, alpha);
    }
}
