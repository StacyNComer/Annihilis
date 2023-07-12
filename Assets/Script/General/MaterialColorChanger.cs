using UnityEngine;

/// <summary>
/// Changes the color property of the given material each update.
/// </summary>
public class MaterialColorChanger : MonoBehaviour
{
    [SerializeField]
    new private Renderer renderer;
    [SerializeField]
    private string colorPropertyName = "_Color";
    [SerializeField]
    private int materialIndex;

    private MaterialPropertyBlock colorOverride;

    /// <summary>
    /// The last color assigned to the material.
    /// </summary>
    private Color currentColor;

    /// <summary>
    /// If this is different from currentColor, it is assigned to the material and set as the current color.
    /// </summary>
    public Color color;

    private void Start()
    {
        colorOverride = new MaterialPropertyBlock();
        UpdateColor();
    }

    // Update is called once per frame
    void Update()
    {
        if(color != currentColor)
        {
            UpdateColor();
        }
    }

    private void UpdateColor()
    {
        currentColor = color;
        colorOverride.SetColor(colorPropertyName, currentColor);
        renderer.SetPropertyBlock(colorOverride, materialIndex);
    }
}
