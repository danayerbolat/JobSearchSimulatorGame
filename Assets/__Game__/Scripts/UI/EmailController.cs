using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;

public class EmailController : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI companyNameText;
    public TextMeshProUGUI resultText;
    public Button closeButton;

    [Header("Email Messages")]
    [TextArea(3, 6)]
    public string callbackMessageNorwegian = "Gratulerer!\n\nVi vil gjerne invitere deg til intervju. Vennligst kontakt oss for å avtale tidspunkt.\n\nMed vennlig hilsen,\n{0}";

    [TextArea(3, 6)]
    public string rejectionMessageNorwegian = "Takk for din søknad.\n\nDessverre må vi meddele at vi har valgt å gå videre med andre kandidater denne gangen.\n\nVi ønsker deg lykke til videre.\n\nMed vennlig hilsen,\n{0}";

    private void Start()
    {
        closeButton.onClick.AddListener(CloseEmail);

        DisplayLatestEmail();
    }

    private void DisplayLatestEmail()
    {
        // Get the most recent unread response
        ApplicationData latestResponse = GetLatestUnreadResponse();

        if (latestResponse != null)
        {
            // Display the email
            ShowEmail(latestResponse);

            // Mark as read (remove from callbacks/rejections lists)
            MarkAsRead(latestResponse);
        }
        else
        {
            // No emails to show
            companyNameText.text = "Ingen nye meldinger";
            resultText.text = "Du har ingen uleste e-poster.";
        }
    }

    private ApplicationData GetLatestUnreadResponse()
    {
        // Check callbacks first
        if (GameManager.Instance.callbacks.Count > 0)
        {
            return GameManager.Instance.callbacks[0];
        }

        // Then check rejections
        if (GameManager.Instance.rejections.Count > 0)
        {
            return GameManager.Instance.rejections[0];
        }

        return null;
    }

    private void ShowEmail(ApplicationData app)
    {
        // Set company name
        companyNameText.text = $"Fra: {app.job.companyName}";

        // Set message based on callback or rejection
        if (app.gotCallback)
        {
            // CALLBACK - Good news!
            resultText.text = string.Format(callbackMessageNorwegian, app.job.companyName);
            resultText.color = new Color(0.0f, 0.5f, 0.0f); // Dark green

            Debug.Log($"📧 Showing CALLBACK email from {app.job.companyName}");
        }
        else
        {
            // REJECTION - Bad news
            resultText.text = string.Format(rejectionMessageNorwegian, app.job.companyName);
            resultText.color = new Color(0.5f, 0.0f, 0.0f); // Dark red

            Debug.Log($"📧 Showing REJECTION email from {app.job.companyName}");
        }
    }

    // NEW METHOD: Apply emotional impact when email is read
    private void ApplyEmotionalImpact(ApplicationData app)
    {
        float oldEnergy = GameManager.Instance.emotionalEnergy;

        if (app.gotCallback)
        {
            // CALLBACK - Energy boost!
            GameManager.Instance.emotionalEnergy += GameManager.Instance.callbackEnergyBoost;
            GameManager.Instance.emotionalEnergy = Mathf.Clamp(
                GameManager.Instance.emotionalEnergy,
                0,
                GameManager.Instance.maxEnergy
            );

            Debug.Log($"[ENERGY] 💚 CALLBACK! Energy: {oldEnergy} → {GameManager.Instance.emotionalEnergy} (+{GameManager.Instance.callbackEnergyBoost})");
        }
        else
        {
            // REJECTION - Energy drain
            GameManager.Instance.emotionalEnergy -= GameManager.Instance.rejectionEnergyDrain;
            GameManager.Instance.emotionalEnergy = Mathf.Clamp(
                GameManager.Instance.emotionalEnergy,
                0,
                GameManager.Instance.maxEnergy
            );

            Debug.Log($"[ENERGY] 💔 REJECTION! Energy: {oldEnergy} → {GameManager.Instance.emotionalEnergy} (-{GameManager.Instance.rejectionEnergyDrain})");
        }

        // Update backgrounds based on new energy level
        GameManager.Instance.UpdateBackgrounds();

        Debug.Log($"[EMAIL] Emotional impact applied. Current energy: {GameManager.Instance.emotionalEnergy}");
    }

    private void MarkAsRead(ApplicationData app)
    {
        // Remove from unread lists
        GameManager.Instance.callbacks.Remove(app);
        GameManager.Instance.rejections.Remove(app);

        Debug.Log($"📧 Marked email from {app.job.companyName} as read");
    }

    private void CloseEmail()
    {
        // Return to Desktop
        SceneManager.LoadScene("2Desktop");
    }

    //private void MarkAsRead(ApplicationData app)
    //{
    //    // Add to read emails list
    //    if (!GameManager.Instance.readEmails.Contains(app))
    //    {
    //        GameManager.Instance.readEmails.Add(app);
    //    }

    //    // Remove from unread lists
    //    GameManager.Instance.callbacks.Remove(app);
    //    GameManager.Instance.rejections.Remove(app);

    //    Debug.Log($"📧 Marked email from {app.job.companyName} as read");
    //}
}