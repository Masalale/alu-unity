using UnityEngine;

/// <summary>
/// Goal trigger: stops the run clock the first time the player reaches the flag and
/// switches the timer label to a large green reading.
/// </summary>
public class WinTrigger : MonoBehaviour
{
    /// <summary>Player carrying the Timer component to stop.</summary>
    public GameObject player;

    // Timer component on the player, cached once on Awake.
    private Timer timer;

    // True once the player has won, so staying inside the goal does nothing further.
    private bool hasWon;

    // Label size used for the winning reading, up from the 48 used while running.
    private const int winFontSize = 60;

    // Caches the player's Timer component before the first trigger message.
    private void Awake()
    {
        timer = player.GetComponent<Timer>();
    }

    // Stops the clock and restyles the label the first time the player enters the goal.
    private void OnTriggerEnter(Collider other)
    {
        if (hasWon || other.gameObject != player || !timer.IsRunning)
        {
            return;
        }

        hasWon = true;
        timer.StopTimer();
        timer.timerText.fontSize = winFontSize;
        timer.timerText.color = Color.green;
    }
}
