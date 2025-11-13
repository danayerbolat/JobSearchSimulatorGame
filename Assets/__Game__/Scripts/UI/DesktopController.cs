using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DesktopController : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI weekText;
    public TextMeshProUGUI energyText;

    [Header("Buttons")]
    public Button cvButton;
    public Button jobBoardButton;
    public Button emailButton;
    public Button calendarButton;

    [Header("Debug Panel")]
    public GameObject debugPanel;
    public TextMeshProUGUI debugText;

    private void Start()
    {
        UpdateUI();

        // Add listeners to buttons
        cvButton.onClick.AddListener(OpenCVFolder);
        jobBoardButton.onClick.AddListener(OpenJobBoard);
        emailButton.onClick.AddListener(OpenEmail);
        calendarButton.onClick.AddListener(OpenCalendar);

        // Hide debug panel at start
        if (debugPanel != null)
        {
            debugPanel.SetActive(false);
        }
    }

    private void UpdateUI()
    {
        // Update week display
        weekText.text = $"Week {GameManager.Instance.currentWeek}";

        // Update energy display
        energyText.text = $"Energy: {GameManager.Instance.emotionalEnergy:F0}";
    }

    // Button functions
    private void OpenCVFolder()
    {
        ShowDebugMessage("CV Folder clicked! (Scene not built yet)");
        Debug.Log("CV Folder opened");
    }

    private void OpenJobBoard()
    {
        ShowDebugMessage("Job Board clicked! (Scene not built yet)");
        Debug.Log("Job Board opened");
    }

    private void OpenEmail()
    {
        ShowDebugMessage("Email clicked! (Scene not built yet)");
        Debug.Log("Email opened");
    }

    private void OpenCalendar()
    {
        ShowDebugMessage("Calendar clicked - advancing week!");
        AdvanceWeek();
    }

    private void AdvanceWeek()
    {
        // Check if the week limit is hit
        if (GameManager.Instance.currentWeek >= GameManager.Instance.maxWeeks)
        {
            ShowDebugMessage("You've reached the end! Time's up.");
            return; // Don't advance further
        }

        GameManager.Instance.AdvanceWeek();
        UpdateUI();
        ShowDebugMessage($"Week advanced to {GameManager.Instance.currentWeek}");

        // Check if this was the last week
        if (GameManager.Instance.currentWeek >= GameManager.Instance.maxWeeks)
        {
            ShowDebugMessage("Final week! Better find a job soon...");
        }
    }

    private void ShowDebugMessage(string message)
    {
        if (debugPanel != null && debugText != null)
        {
            debugPanel.SetActive(true);
            debugText.text = message;

            // Hide after 3 seconds
            Invoke("HideDebugPanel", 3f);
        }
    }

    private void HideDebugPanel()
    {
        if (debugPanel != null)
        {
            debugPanel.SetActive(false);
        }
    }
}
