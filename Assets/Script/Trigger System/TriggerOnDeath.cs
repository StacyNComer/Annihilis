using UnityEngine;

/// <summary>
/// A script for a Trigger that should react to one or more Destructables being destroyed.
/// </summary>
public class TriggerOnDeath : TriggerBase
{
    /// <summary>
    /// The Destructables that must die before the trigger fires.
    /// </summary>
    private int awaitedDeaths;

    /// <summary>
    /// Increases the death's this trigger expects by one.
    /// </summary>
    public void AddDeathAwaited()
    {
        awaitedDeaths++;
    }

    /// <summary>
    /// Informs the trigger that something tracked by it died.
    /// </summary>
    public void ReportDeath()
    {
        awaitedDeaths--;

        if (awaitedDeaths == 0)
        {
            FireTrigger();
        }
    }
}
