using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
