using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Causes a gameObject to fire a trigger when the player enters an attached trigger volume.
/// </summary>
public class TriggerOnEnter : TriggerBase
{
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            FireTrigger();
        }
    }
}
