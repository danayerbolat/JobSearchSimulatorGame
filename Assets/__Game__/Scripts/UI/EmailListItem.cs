using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EmailListItem : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI companyNameText;
    public TextMeshProUGUI resultText;
    public TextMeshProUGUI dateText;
    public GameObject unreadDot;
    public Image backgroundImage;
    public Button button;

    private ApplicationData emailData;
    private EmailController emailController;
    private bool isSelected = false;

    public ApplicationData EmailData => emailData;

    public void Setup(ApplicationData email, EmailController controller)
    {
        emailData = email;
        emailController = controller;

        // Auto-find components if not assigned
        if (companyNameText == null)
            companyNameText = transform.Find("TopRow/CompanyName")?.GetComponent<TextMeshProUGUI>();
        if (resultText == null)
            resultText = transform.Find("Result")?.GetComponent<TextMeshProUGUI>();
        if (dateText == null)
            dateText = transform.Find("Date")?.GetComponent<TextMeshProUGUI>();
        if (unreadDot == null)
            unreadDot = transform.Find("TopRow/UnreadDot")?.gameObject;
        if (backgroundImage == null)
            backgroundImage = GetComponent<Image>();
        if (button == null)
            button = GetComponent<Button>();

        // Set company name
        if (companyNameText != null)
        {
            companyNameText.text = email.job.companyName;
        }

        // Set result
        if (resultText != null)
        {
            if (email.gotCallback)
            {
                resultText.text = "Intervju Innkalling";
                resultText.color = new Color(0.0f, 0.0f, 0.0f);
            }
            else
            {
                resultText.text = "Takk for din søknad";
                resultText.color = new Color(0.0f, 0.0f, 0.0f);
            }
        }

        // Set date
        if (dateText != null)
        {
            dateText.text = $"Måned {email.submittedMonth + 2}";
        }

        // Check if unread
        bool isUnread = GameManager.Instance.callbacks.Contains(email) ||
                        GameManager.Instance.rejections.Contains(email);

        // Show/hide unread dot
        if (unreadDot != null)
        {
            unreadDot.SetActive(isUnread);
        }

        // Bold company name if unread
        if (companyNameText != null && isUnread)
        {
            companyNameText.fontStyle = FontStyles.Bold;
        }

        // Setup button
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClick);
        }
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;

        if (backgroundImage != null)
        {
            if (selected)
            {
                backgroundImage.color = new Color(0.85f, 0.92f, 1.0f); // Selected blue
            }
            else
            {
                // Check if unread for color
                bool isUnread = GameManager.Instance.callbacks.Contains(emailData) ||
                                GameManager.Instance.rejections.Contains(emailData);

                if (isUnread)
                {
                    backgroundImage.color = new Color(0.95f, 0.97f, 1.0f); // Unread light blue
                }
                else
                {
                    backgroundImage.color = new Color(0.98f, 0.98f, 0.98f); // Read light gray
                }
            }
        }
    }

    private void OnClick()
    {
        if (emailController != null && emailData != null)
        {
            emailController.SelectEmail(emailData);
        }
    }
}