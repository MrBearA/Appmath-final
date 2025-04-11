using UnityEngine;
using TMPro;

public class UIManagerTMP : MonoBehaviour
{
    [Header("TMP UI Elements")]
    public TMP_Text timerText; // Assign via Inspector
    public TMP_Text hpText;    // Assign via Inspector

    private float startTime;
    private PlayerController playerController; // Assumes your PlayerController has a method to get current HP

    void Start()
    {
        startTime = Time.time;
        // Find the player by tag; ensure your player GameObject is tagged "Player"
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerController = player.GetComponent<PlayerController>();
        }
    }

    void Update()
    {
        // Update the timer display
        float elapsedTime = Time.time - startTime;
        timerText.text = "Time: " + elapsedTime.ToString("F2");

        // Update the HP display using a method or property from your PlayerController.
        // For example, if PlayerController has a GetCurrentHP() method:
        if (playerController != null)
        {
            hpText.text = "HP: " + playerController.GetCurrentHP();
        }
    }
}
