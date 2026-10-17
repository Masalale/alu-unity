using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Run clock for the player: counts up from 0:00.00 and drives the on-screen label.
/// </summary>
public class Timer : MonoBehaviour
{
    /// <summary>Label that displays the elapsed time formatted as m:ss.ff.</summary>
    public Text timerText;

    // Total elapsed seconds. Accumulated in place so the value survives a respawn.
    private float elapsedTime;

    // True while the clock is counting; cleared by StopTimer when the player wins.
    private bool isRunning = true;

    /// <summary>True while the clock is still counting.</summary>
    public bool IsRunning
    {
        get { return isRunning; }
    }

    // Shows the starting reading of 0:00.00 before the first counted frame.
    private void Start()
    {
        timerText.text = FormatTime(elapsedTime);
    }

    // Accumulates elapsed time and refreshes the label while counting.
    private void Update()
    {
        if (!enabled || !isRunning)
        {
            return;
        }

        elapsedTime += Time.deltaTime;
        timerText.text = FormatTime(elapsedTime);
    }

    /// <summary>Freezes the clock at its current reading; used when the player wins.</summary>
    public void StopTimer()
    {
        isRunning = false;
    }

    // Formats seconds as m:ss.ff by working in whole hundredths of a second, so the
    // seconds field can never round up into 60 and wrap onto the minute.
    private static string FormatTime(float seconds)
    {
        int totalHundredths = Mathf.FloorToInt(seconds * 100f);
        int minutes = totalHundredths / 6000;
        int wholeSeconds = totalHundredths / 100 % 60;
        int hundredths = totalHundredths % 100;
        return $"{minutes}:{wholeSeconds:00}.{hundredths:00}";
    }
}
