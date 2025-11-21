using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DesktopController : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI dateText;
    public TextMeshProUGUI energyText;
    public TextMeshProUGUI statusMessageText;

    [Header("Backgrounds - ASSIGN IN INSPECTOR")]
    public GameObject fullEnergyBackground;
    public GameObject mediumEnergyBackground;
    public GameObject lowEnergyBackground;

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
        AssignBackgroundsToGameManager();

        UpdateUI();
        UpdateBackgrounds();
        UpdateStatusMessage();

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

    private void AssignBackgroundsToGameManager()
    {
        Debug.Log("[DESKTOP] Assigning backgrounds to GameManager...");

        if (fullEnergyBackground != null)
        {
            GameManager.Instance.fullEnergyBackground = fullEnergyBackground;
            Debug.Log($"[DESKTOP] Assigned Full background");
        }
        else
        {
            Debug.LogError("[DESKTOP] Full Energy Background is not assigned in Inspector!");
        }

        if (mediumEnergyBackground != null)
        {
            GameManager.Instance.mediumEnergyBackground = mediumEnergyBackground;
            Debug.Log($"[DESKTOP] Assigned Medium background");
        }
        else
        {
            Debug.LogError("[DESKTOP] Medium Energy Background is not assigned in Inspector!");
        }

        if (lowEnergyBackground != null)
        {
            GameManager.Instance.lowEnergyBackground = lowEnergyBackground;
            Debug.Log($"[DESKTOP] Assigned Low background");
        }
        else
        {
            Debug.LogError("[DESKTOP] Low Energy Background is not assigned in Inspector!");
        }

        Debug.Log($"[DESKTOP] Current energy: {GameManager.Instance.emotionalEnergy}");
    }

    private void UpdateBackgrounds()
    {
        // Call GameManager's update, which now has references
        GameManager.Instance.UpdateBackgrounds();
    }

    private void UpdateUI()
    {
        // Update week display
        dateText.text = $"Month {GameManager.Instance.currentMonth}";

        // Update energy display
        energyText.text = $"Energy: {GameManager.Instance.emotionalEnergy:F0}";
    }

    private void UpdateStatusMessage()
    {
        if (statusMessageText == null) return;

        if (GameManager.Instance.HasPendingApplications())
        {
            foreach (ApplicationData app in GameManager.Instance.submittedApplications)
            {
                if (!app.responseReceived)
                {
                    statusMessageText.text = $"Søknad sendt til {app.job.companyName}\n" +
                                           $"Klikk på kalender for å gå videre i tid";
                    return;
                }
            }
        }
        else if (GameManager.Instance.HasUnreadResponses())
        {
            statusMessageText.text = "Du har nye meldinger i innboksen!";
        }
        else
        {
            statusMessageText.text = "";
        }
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
        SceneManager.LoadScene("4JobBoard");
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
        if (GameManager.Instance.currentMonth >= GameManager.Instance.maxMonths)
        {
            ShowDebugMessage("You've reached the end! Time's up.");
            return; // Don't advance further
        }

        GameManager.Instance.AdvanceWeek();
        UpdateUI();
        ShowDebugMessage($"Week advanced to {GameManager.Instance.currentMonth}");

        // Check if this was the last week
        if (GameManager.Instance.currentMonth >= GameManager.Instance.maxMonths)
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
