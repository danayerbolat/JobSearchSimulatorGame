using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EmailController : MonoBehaviour, IResettable
{
    [Header("Window Reference")]
    public DraggableWindow emailWindow;

    [Header("Desktop Reference")]
    public DesktopController desktopController;

    [Header("UI References - Email List (Left Panel)")]
    public Transform emailListContainer; // Container for email items
    public TextMeshProUGUI inboxHeaderText; // "INNBOKS\n3 uleste"

    [Header("UI References - Email Display (Right Panel)")]
    public GameObject emailDisplayPanel;
    public GameObject emptySelectionPanel;
    public TextMeshProUGUI companyNameText;
    public TextMeshProUGUI jobTitleText;
    public TextMeshProUGUI dateText;
    public TextMeshProUGUI resultText;
    public TextMeshProUGUI messageText;

    [Header("Email Messages")]
    [TextArea(3, 6)]
    public string callbackMessageNorwegian = "Gratulerer!\n\nVi vil gjerne invitere deg til intervju for stillingen som {0}.\n\nVi kontakter deg for å avtale tidspunkt.\n\nMed vennlig hilsen,\n{1}";

    [TextArea(3, 6)]
    public string rejectionMessageNorwegian = "Takk for din søknad til stillingen som {0}.\n\nDessverre må vi meddele at vi har valgt å gå videre med andre kandidater denne gangen.\n\nVi ønsker deg lykke til videre i jobbsøket.\n\nMed vennlig hilsen,\n{1}";

    [Header("Buttons")]
    public Button closeButton;

    private ApplicationData currentlySelectedEmail = null;
    private List<GameObject> emailListItems = new List<GameObject>();

    [Header("Email List Styling")]
    public TMP_FontAsset listFont;                           // assign LTKai-1 SDF here
    public Color listTextColor = new Color32(0x37, 0x27, 0x27, 0xFF); // #372727
    public Color listBackgroundColor = new Color(0.98f, 0.98f, 0.98f); // set in Inspector

    [Header("Monologue Settings")]
    public float monologueDelay = 0.4f; // adjust to taste

    private void Start()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(CloseEmail);
    }

    public void ResetState()
    {
        Debug.Log("[EMAIL] Refreshing email client...");
        RefreshEmailList();
        ShowEmptySelection();
    }

    private void RefreshEmailList()
    {
        // Clear existing items
        foreach (GameObject item in emailListItems)
        {
            if (item != null)
                Destroy(item);
        }
        emailListItems.Clear();

        // Get all emails (newest first)
        List<ApplicationData> allEmails = new List<ApplicationData>(GameManager.Instance.allEmails);
        allEmails.Reverse();

        // Count unread
        int unreadCount = GameManager.Instance.callbacks.Count + GameManager.Instance.rejections.Count;

        // Update header
        if (inboxHeaderText != null)
        {
            if (unreadCount > 0)
            {
                inboxHeaderText.text = $"INNBOKS\n{unreadCount} uleste";
            }
            else
            {
                inboxHeaderText.text = "INNBOKS\nAlle lest";
            }
        }

        if (allEmails.Count == 0)
        {
            CreateEmptyMessage();
        }
        else
        {
            foreach (ApplicationData email in allEmails)
            {
                CreateEmailListItem(email);
            }

            // Auto-select first unread
            ApplicationData firstUnread = allEmails.FirstOrDefault(e =>
                GameManager.Instance.callbacks.Contains(e) ||
                GameManager.Instance.rejections.Contains(e));

            if (firstUnread != null)
            {
                // Auto-show it, but don't apply impact/monologue yet
                SelectEmail(firstUnread, false);
            }
        }
    }

    private void CreateEmptyMessage()
    {
        GameObject item = new GameObject("EmptyMessage");
        item.transform.SetParent(emailListContainer, false);

        TextMeshProUGUI text = item.AddComponent<TextMeshProUGUI>();
        text.text = "Ingen e-poster ennå.\n\nSøk på jobber!";
        text.alignment = TextAlignmentOptions.Left;
        text.fontSize = 24;
        text.color = Color.gray; //change color for empty message

        RectTransform rt = item.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(280, 100);

        emailListItems.Add(item);
    }

    private void CreateEmailListItem(ApplicationData email)
    {
        // Create container
        GameObject item = new GameObject($"Email_{email.job.companyName}");
        item.transform.SetParent(emailListContainer, false);

        RectTransform rt = item.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(280, 100);

        // Background
        Image bg = item.AddComponent<Image>();

        // We still compute isUnread (for the dot / future logic)
        bool isUnread = GameManager.Instance.callbacks.Contains(email) ||
                        GameManager.Instance.rejections.Contains(email);

        // Same background for all items, from Inspector
        if (bg != null)
        {
            bg.color = listBackgroundColor;
        }

        // Button
        Button btn = item.AddComponent<Button>();
        ColorBlock colors = btn.colors;
        colors.highlightedColor = new Color(0.9f, 0.95f, 1.0f);
        colors.pressedColor = new Color(0.8f, 0.9f, 1.0f);
        colors.selectedColor = new Color(0.85f, 0.92f, 1.0f);
        btn.colors = colors;
        btn.onClick.AddListener(() => SelectEmail(email, true));

        // Layout
        VerticalLayoutGroup layout = item.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(12, 12, 10, 10);
        layout.spacing = 0;
        layout.childAlignment = TextAnchor.UpperLeft;
        layout.childForceExpandWidth = true;
        layout.childControlWidth = true;
        layout.childControlHeight = true;

        // Company name
        GameObject companyObj = new GameObject("CompanyName");
        companyObj.transform.SetParent(item.transform, false);
        TextMeshProUGUI companyText = companyObj.AddComponent<TextMeshProUGUI>();
        companyText.text = (isUnread ? "● " : "") + email.job.companyName; // keep dot for unread
        companyText.fontSize = 32;
        companyText.fontStyle = isUnread ? FontStyles.Bold : FontStyles.Normal;

        if (listFont != null)
            companyText.font = listFont;
        companyText.color = listTextColor;

        // Result
        GameObject resultObj = new GameObject("Result");
        resultObj.transform.SetParent(item.transform, false);
        TextMeshProUGUI resultText = resultObj.AddComponent<TextMeshProUGUI>();

        if (email.gotCallback)
        {
            resultText.text = "Intervju Innkalling";
        }
        else
        {
            resultText.text = "Takk for søknaden din";
        }
        resultText.fontSize = 28;

        if (listFont != null)
            resultText.font = listFont;
        resultText.color = listTextColor;

        // Date
        GameObject dateObj = new GameObject("Date");
        dateObj.transform.SetParent(item.transform, false);
        TextMeshProUGUI dateText = dateObj.AddComponent<TextMeshProUGUI>();
        dateText.text = $"Måned {email.submittedMonth + 2}";
        dateText.fontSize = 20;

        if (listFont != null)
            dateText.font = listFont;
        dateText.color = listTextColor;

        emailListItems.Add(item);
    }


    public void SelectEmail(ApplicationData email, bool triggeredByClick = false)
    {
        if (email == null) return;

        currentlySelectedEmail = email;
        ShowEmailContent(email);

        // Was this email unread before this selection?
        bool wasUnread = GameManager.Instance.callbacks.Contains(email) ||
                         GameManager.Instance.rejections.Contains(email);

        // Only apply impact + monologue if:
        //  - it WAS unread, and
        //  - this selection came from an actual click
        if (triggeredByClick && wasUnread)
        {
            ApplyEmotionalImpact(email);
            MarkAsRead(email);
            RefreshEmailList();

            // Re-select the same email after refresh, but this time
            // as a non-clicked selection (just to show it, no impact)
            SelectEmail(email, false);

            if (desktopController != null)
            {
                desktopController.UpdateEmailNotification();
            }
        }
    }

    private void ShowEmailContent(ApplicationData email)
    {
        if (emptySelectionPanel != null)
            emptySelectionPanel.SetActive(false);

        if (emailDisplayPanel != null)
            emailDisplayPanel.SetActive(true);

        if (companyNameText != null)
            companyNameText.text = $"Fra: {email.job.companyName}";

        if (jobTitleText != null)
            jobTitleText.text = $"Ang din søknad på stillingen: {email.job.jobTitle}";

        if (dateText != null)
            dateText.text = $"Måned {email.submittedMonth + 2}"; //Might need to change how date is displayed later

        if (resultText != null)
        {
            if (email.gotCallback)
            {
                resultText.text = "Vi ønsker å invitere deg på intervju";
                resultText.color = new Color(0.0f, 0.0f, 0.0f); //Change color for callback text here if needed
            }
            else
            {
                resultText.text = "Takk for søknaden din";
                resultText.color = new Color(0.0f, 0.0f, 0.0f); //Change color for rejection text here if needed
            }
        }

        if (messageText != null)
        {
            if (email.gotCallback)
            {
                messageText.text = string.Format(callbackMessageNorwegian,
                    email.job.jobTitle, email.job.companyName);
            }
            else
            {
                messageText.text = string.Format(rejectionMessageNorwegian,
                    email.job.jobTitle, email.job.companyName);
            }
        }
    }

    private void ShowEmptySelection()
    {
        if (emailDisplayPanel != null)
            emailDisplayPanel.SetActive(false);

        if (emptySelectionPanel != null)
            emptySelectionPanel.SetActive(true);
    }

    private void ApplyEmotionalImpact(ApplicationData app)
    {
        float oldEnergy = GameManager.Instance.emotionalEnergy;

        if (app.gotCallback)
        {
            GameManager.Instance.emotionalEnergy += GameManager.Instance.callbackEnergyBoost;
            GameManager.Instance.emotionalEnergy = Mathf.Clamp(
                GameManager.Instance.emotionalEnergy, 0, GameManager.Instance.maxEnergy);

            Debug.Log($"[ENERGY] CALLBACK! Energy: {oldEnergy} → {GameManager.Instance.emotionalEnergy}");
        }
        else
        {
            GameManager.Instance.emotionalEnergy -= GameManager.Instance.rejectionEnergyDrain;
            GameManager.Instance.emotionalEnergy = Mathf.Clamp(
                GameManager.Instance.emotionalEnergy, 0, GameManager.Instance.maxEnergy);

            Debug.Log($"[ENERGY] REJECTION! Energy: {oldEnergy} → {GameManager.Instance.emotionalEnergy}");
        }

        float newEnergy = GameManager.Instance.emotionalEnergy;
        GameManager.Instance.UpdateBackgrounds();

        // 🔹 Show Aika's monologue after a short delay
        if (AikaMonologueManager.Instance != null)
        {
            StartCoroutine(DelayedMonologue(app, oldEnergy, newEnergy));
        }
    }

    private IEnumerator DelayedMonologue(ApplicationData app, float oldEnergy, float newEnergy)
    {
        // Wait a bit so the player can see the email first
        yield return new WaitForSeconds(monologueDelay);

        if (AikaMonologueManager.Instance != null)
        {
            AikaMonologueManager.Instance.OnEmailOpened(
                app,
                oldEnergy,
                newEnergy,
                app.authenticityCost
            );
        }
    }

    private void MarkAsRead(ApplicationData app)
    {
        if (!GameManager.Instance.readEmails.Contains(app))
        {
            GameManager.Instance.readEmails.Add(app);
        }

        GameManager.Instance.callbacks.Remove(app);
        GameManager.Instance.rejections.Remove(app);
    }

    private void CloseEmail()
    {
        if (emailWindow != null)
        {
            emailWindow.CloseWindow();
        }

        if (desktopController != null)
        {
            desktopController.UpdateUI();
            desktopController.UpdateEmailNotification();
        }
    }
}
