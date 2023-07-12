using UnityEngine;

/// <summary>
/// A class meant to hold custom data for spawn FX, such as the color of particles that should be used.
/// </summary>
public class CustomSpawnData : MonoBehaviour
{
    [SerializeField, Tooltip("The color the spawning particles will be for this GameObject.")]
    private Color spawnColor;
    
    /// <summary>
    /// Returns the color that the spawning particles for this GameObject should have.
    /// </summary>
    /// <returns></returns>
    public Color GetSpawnColor()
    {
        return spawnColor;
    }
}
