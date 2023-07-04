using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A class meant to hold custom data for spawn FX, such as the color of particles that should be used.
/// </summary>
public class CustomSpawnData : MonoBehaviour
{
    [SerializeField]
    private Color spawnColor;
    
    public Color GetSpawnColor()
    {
        return spawnColor;
    }
}
