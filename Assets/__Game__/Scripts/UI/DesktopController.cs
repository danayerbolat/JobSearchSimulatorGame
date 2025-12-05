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

    [Header("Windows")]
    public DraggableWindow jobBoardWindow;
    public DraggableWindow cvBuilderWindow;
    public DraggableWindow emailWindow;

    [Header("Backgrounds")]
    public GameObject fullEnergyBackground;
    public GameObject mediumEnergyBackground;
    public GameObject lowEnergyBackground;

    [Header("Buttons")]
    public Button cvButton;
    public Button jobBoardButton;
    public Button emailButton;
    public Button calendarButton;

    [Header("Email Notification")]
    public GameObject emailNotif;

    [Header("Debug Panel")]
    public GameObject debugPanel;
    public TextMeshProUGUI debugText;

    private void Start()
    {
        AssignBackgroundsToGameManager();
        CloseAllWindows();

        UpdateUI();
        UpdateBackgrounds();
        UpdateStatusMessage();
        UpdateEmailNotification();

        // Add listeners to buttons
        cvButton.onClick.AddListener(OpenCVFolder);
        jobBoardButton.onClick.AddListener(OpenJobBoard);
        emailButton.onClick.AddListener(OpenEmail);
        calendarButton.onClick.AddListener(TimeSkip); //Clicking here advances time

        // Hide debug panel at start
        if (debugPanel != null)
        {
            debugPanel.SetActive(false);
        }
    }

    private void AssignBackgroundsToGameManager()
    {
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

    private void CloseAllWindows()
    {
        if (jobBoardWindow != null)
            jobBoardWindow.gameObject.SetActive(false);
        if (cvBuilderWindow != null)
            cvBuilderWindow.gameObject.SetActive(false);
        if (emailWindow != null)
            emailWindow.gameObject.SetActive(false);
    }

    public void UpdateBackgrounds()
    {
        // Call GameManager's update, which now has references
        GameManager.Instance.UpdateBackgrounds();
    }

    //I dont think UpdateUI is necessary since months and energy is not displayed on desktop anymore
    public void UpdateUI()
    {
        // Update week display
        dateText.text = $"Month {GameManager.Instance.currentMonth}";

        // Update energy display
        energyText.text = $"Energy: {GameManager.Instance.emotionalEnergy:F0}";
    }

    public void UpdateEmailNotification()
    {
        if (emailNotif == null) return;

        // Show notification if there are unread responses
        bool hasUnreadEmails = GameManager.Instance.HasUnreadResponses();
        emailNotif.SetActive(hasUnreadEmails);
    }

    //Where is this used? I have not seen this in the game anywhere...
    public void UpdateStatusMessage()
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
    private void OpenCVFolder() //This one might not be needed at all... 
    {
        ShowDebugMessage("CV Folder clicked! (Scene not built yet)");
        Debug.Log("CV Folder opened");
    }

    private void OpenJobBoard()
    {
        if (jobBoardWindow != null)
        {
            jobBoardWindow.OpenWindow();
        }
    }

    private void OpenEmail()
    {
        if (emailWindow != null)
        {
            emailWindow.OpenWindow();
        }
    }

    private void TimeSkip()
    {
        ShowDebugMessage("Calendar clicked - advancing week!");
        AdvanceTime();
    }

    private void AdvanceTime()
    {
        // Check if the week limit is hit
        if (GameManager.Instance.currentMonth >= GameManager.Instance.maxMonths)
        {
            ShowDebugMessage("You've reached the end! Time's up.");
            return; // Don't advance further
        }

        GameManager.Instance.AdvanceTime();
        UpdateUI();
        UpdateEmailNotification();
        ShowDebugMessage($"Week advanced to {GameManager.Instance.currentMonth}");

        // Check if this was the last week
        if (GameManager.Instance.currentMonth >= GameManager.Instance.maxMonths)
        {
            ShowDebugMessage("Final week! Better find a job soon...");
        }
    }

    // Can remove this when not needed
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
    // Can remove this when not needed
    private void HideDebugPanel()
    {
        if (debugPanel != null)
        {
            debugPanel.SetActive(false);
        }
    }
}
