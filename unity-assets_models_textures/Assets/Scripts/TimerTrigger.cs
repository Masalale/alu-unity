using UnityEngine;

/// <summary>
/// Start line for the run clock: arms the player's Timer the first time the
/// player walks out through this trigger.
/// </summary>
public class TimerTrigger : MonoBehaviour
{
    /// <summary>Player whose Timer component is enabled when the start line is crossed.</summary>
    public GameObject player;

    // True once the trigger has armed the clock, so walking back over the start
    // line can neither reset nor re-trigger the timer.
    private bool hasStarted;

    // Enables the player's Timer component the first time the player leaves the trigger.
    private void OnTriggerExit(Collider other)
    {
        if (hasStarted || other.gameObject != player)
        {
            return;
        }

        hasStarted = true;
        player.GetComponent<Timer>().enabled = true;
    }
}
