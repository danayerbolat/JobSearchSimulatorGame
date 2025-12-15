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
    public Button jobBoardButton;
    public Button emailButton;
    public Button calendarButton;

    [Header("Email Notification")]
    public GameObject emailNotif;

    [Header("CV Icons")]
    public Transform cvIconContainer;   // e.g. a RectTransform on the desktop canvas
    public GameObject cvIconPrefab;     // prefab with icon + TMP label

    private int cvVersionCounter = 0;

    [Header("CV Icon Layout")]
    public int iconsPerRow = 4;
    public Vector2 firstIconOffset = new Vector2(40f, -40f);  // start position inside container
    public Vector2 iconSpacing   = new Vector2(140f, -150f);  // horizontal / vertical spacing
    public float jitterRange = 10f; // try 5–15

    [Header("Debug Panel")]
    public GameObject debugPanel;
    public TextMeshProUGUI debugText;

    [Header("Tutorial")]
    public GameObject tutorialOverlay;

    [Header("Month Display")]
    public GameObject monthDisplayPanel;
    public TextMeshProUGUI monthDisplayText;
    private Coroutine hideMonthRoutine;

    private void Start()
    {
        AssignBackgroundsToGameManager();
        CloseAllWindows();

        UpdateUI();
        UpdateBackgrounds();
        UpdateStatusMessage();
        UpdateEmailNotification();

        // Add listeners to buttons
        jobBoardButton.onClick.AddListener(OpenJobBoard);
        emailButton.onClick.AddListener(OpenEmail);
        calendarButton.onClick.AddListener(TimeSkip); //Clicking here advances time

        // Hide debug panel at start
        if (debugPanel != null)
        {
            debugPanel.SetActive(false);
        }

        // Setup tutorial overlay
        SetupTutorialOverlay();

        // Hide month display at start
        if (monthDisplayPanel != null)
        {
            monthDisplayPanel.SetActive(false);
        }
    }

    private void SetupTutorialOverlay()
    {
        if (tutorialOverlay != null)
        {
            tutorialOverlay.SetActive(true);

            // Add listener for any click to dismiss
            Button overlayButton = tutorialOverlay.GetComponent<Button>();
            if (overlayButton != null)
            {
                overlayButton.onClick.AddListener(DismissTutorial);
            }
            else
            {
                // If there's no button, add a click event directly to the panel
                Image overlayImage = tutorialOverlay.GetComponent<Image>();
                if (overlayImage != null)
                {
                    Button btn = tutorialOverlay.AddComponent<Button>();
                    btn.onClick.AddListener(DismissTutorial);
                }
            }
        }
    }

    private void DismissTutorial()
    {
        if (tutorialOverlay != null)
        {
            tutorialOverlay.SetActive(false);
            Debug.Log("[TUTORIAL] Tutorial dismissed");
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
        if (GameManager.Instance == null) return;

        // Example: "Måned 3 / 10"
        if (dateText != null)
        {
            int m = GameManager.Instance.currentMonth;
            int max = GameManager.Instance.maxMonths;
            dateText.text = $"Måned {m} / {max}";
        }
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
        ShowMonthDisplay();
        AdvanceTime();
    }

    private void ShowMonthDisplay()
    {
        if (monthDisplayPanel == null || monthDisplayText == null)
            return;

        // Stop any existing hide routine
        if (hideMonthRoutine != null)
        {
            StopCoroutine(hideMonthRoutine);
        }

        // Update text with current month
        monthDisplayText.text = $"Måned {GameManager.Instance.currentMonth + 1}";

        // Show panel
        monthDisplayPanel.SetActive(true);

        // Hide after 3 seconds
        hideMonthRoutine = StartCoroutine(HideMonthDisplayAfterDelay());
    }

    private System.Collections.IEnumerator HideMonthDisplayAfterDelay()
    {
        yield return new WaitForSeconds(2f);

        if (monthDisplayPanel != null)
        {
            monthDisplayPanel.SetActive(false);
        }

        hideMonthRoutine = null;
    }

    private void AdvanceTime()
    {
        // Check if the week limit is hit
        if (GameManager.Instance.currentMonth >= GameManager.Instance.maxMonths)
        {
            return; // Don't advance further
        }

        GameManager.Instance.AdvanceTime();
        UpdateUI();
        UpdateEmailNotification();
        //ShowDebugMessage($"Week advanced to {GameManager.Instance.currentMonth}");
    }

    private void HideDebugPanel()
    {
        if (debugPanel != null)
        {
            debugPanel.SetActive(false);
        }
    }

    public void SpawnCvIcon()
    {
        Debug.Log($"[DESKTOP] SpawnCvIcon called on {name}");
        if (cvIconPrefab == null || cvIconContainer == null)
        {
            Debug.LogWarning("[DESKTOP] Missing cvIconPrefab or cvIconContainer.");
            return;
        }

        cvVersionCounter++;

        GameObject icon = Instantiate(cvIconPrefab, cvIconContainer);

        // Set label
        TextMeshProUGUI label = icon.GetComponentInChildren<TextMeshProUGUI>();
        if (label != null)
        {
            label.text = $"CV_versjon{cvVersionCounter}.docx";
        }

        RectTransform rt = icon.GetComponent<RectTransform>();
        if (rt != null)
        {
            int index = cvVersionCounter - 1; // 0-based
            int col = index % iconsPerRow;
            int row = index / iconsPerRow;

            // Base grid position
            Vector2 pos = firstIconOffset + new Vector2(
                col * iconSpacing.x,
                row * iconSpacing.y
            );

            // Small random jitter around the grid slot
            Vector2 jitter = new Vector2(
                Random.Range(-jitterRange, jitterRange),
                Random.Range(-jitterRange, jitterRange)
            );

            rt.anchoredPosition = pos + jitter;
        }
    }
}
