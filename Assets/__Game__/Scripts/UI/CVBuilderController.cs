using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;

public class CVBuilderController : MonoBehaviour, IResettable
{
    [Header("Window Reference")]
    public DraggableWindow cvBuilderWindow;
    public DraggableWindow jobBoardWindow;

    [Header("Desktop Reference")] 
    public DesktopController desktopController; 

    [Header("Single Drop Zones")]
    public DropZone nameField;
    public DropZone photoField;
    public DropZone coverLetterField;

    [Header("Multi Drop Zones")]
    public MultiDropZone languagesField;
    public MultiDropZone hobbiesField;
    public MultiDropZone volunteerField;

    [Header("UI")]
    public Button submitButton;

    [Header("Submit Button Sprites")]
    [SerializeField] private Sprite submitActiveSprite;
    [SerializeField] private Sprite submitInactiveSprite;

    private void Start()
    {
        submitButton.onClick.AddListener(OnSubmitClicked);
        UpdateSubmitButton();
    }

    private void OnEnable()
    {
        ApplyOptionVisibility();
    }

    private void Update()
    {
        UpdateSubmitButton();
    }

    public void ResetState()
    {
        // Reset single drop zones
        if (nameField != null)
            nameField.ClearDropZone();

        if (photoField != null)
            photoField.ClearDropZone();

        if (coverLetterField != null)
            coverLetterField.ClearDropZone();

        // Reset multi drop zones
        if (languagesField != null)
            languagesField.ClearDropZone();

        if (hobbiesField != null)
            hobbiesField.ClearDropZone();

        if (volunteerField != null)
            volunteerField.ClearDropZone();

        // Update submit button state
        UpdateSubmitButton();
    }

    private void UpdateSubmitButton()
    {
        // Check if all required fields are filled
        bool allFilled = nameField.IsFilled() &&
                         photoField.IsFilled() &&
                         coverLetterField.IsFilled() &&
                         languagesField.IsFilled() &&
                         hobbiesField.IsFilled() &&
                         volunteerField.IsFilled();

        submitButton.interactable = allFilled;

        Image img = submitButton.GetComponent<Image>();
        if (img != null)
        {
            if (allFilled && submitActiveSprite != null)
            {
                img.sprite = submitActiveSprite;
            }
            else if (!allFilled && submitInactiveSprite != null)
            {
                img.sprite = submitInactiveSprite;
            }
        }
    }

    private void OnSubmitClicked()
    {
        Debug.Log("=== CV SUBMITTED ===");

        // Collect all CV choices
        string chosenName = nameField.currentValue;
        string chosenPhoto = photoField.currentValue;
        string chosenCoverLetter = coverLetterField.currentValue;

        List<string> chosenLanguages = languagesField.GetValues();
        List<string> chosenHobbies = hobbiesField.GetValues();
        List<string> chosenVolunteer = volunteerField.GetValues();

        Debug.Log($"Name: {chosenName}");
        Debug.Log($"Photo: {chosenPhoto}");
        Debug.Log($"Cover Letter: {chosenCoverLetter}");
        Debug.Log($"Languages: {string.Join(", ", chosenLanguages)}");
        Debug.Log($"Hobbies: {string.Join(", ", chosenHobbies)}");
        Debug.Log($"Volunteer: {string.Join(", ", chosenVolunteer)}");

        // Create CV choices object
        CVChoices cv = new CVChoices
        {
            chosenName = chosenName,
            chosenPhoto = chosenPhoto,
            chosenCoverLetter = chosenCoverLetter,
            chosenLanguages = chosenLanguages.ToArray(),
            chosenHobbies = chosenHobbies.ToArray(),
            chosenVolunteer = chosenVolunteer.ToArray()
        };

        // Create application using constructor
        ApplicationData application = new ApplicationData(
            GameManager.Instance.currentJobApplyingTo,
            cv,
            GameManager.Instance.currentMonth
        );

        // --- AUTHENTISITET / SELVBILDE ---
        float authenticityCost = CalculateAuthenticityCost(cv);
        float oldAuth = GameManager.Instance.authenticityScore;
        float newAuth = Mathf.Max(0f, oldAuth - authenticityCost);
        GameManager.Instance.authenticityScore = newAuth;
        application.authenticityCost = authenticityCost;  // Store the cost!

        Debug.Log($"[AUTH] CV authenticity cost: {authenticityCost} (score {oldAuth} → {newAuth})");

        // Trigger Aika’s monolog om CV-valget
        if (AikaMonologueManager.Instance != null)
        {
            AikaMonologueManager.Instance.OnCvSubmitted(authenticityCost);
        }

        // --- ENERGI ---
        float oldEnergy = GameManager.Instance.emotionalEnergy;
        GameManager.Instance.emotionalEnergy -= GameManager.Instance.applicationEnergyCost;
        GameManager.Instance.emotionalEnergy = Mathf.Clamp(
            GameManager.Instance.emotionalEnergy,
            0,
            GameManager.Instance.maxEnergy
        );

        Debug.Log($"[ENERGY] Application submitted: {oldEnergy} → {GameManager.Instance.emotionalEnergy} (-{GameManager.Instance.applicationEnergyCost})");

        // Save application
        GameManager.Instance.submittedApplications.Add(application);

        Debug.Log($"Application saved! Total applications: {GameManager.Instance.submittedApplications.Count}");
        Debug.Log($"Applied to: {application.job.companyName}");

        // Close UI + update desktop as before...
        if (cvBuilderWindow != null)
            cvBuilderWindow.CloseWindow();

        if (jobBoardWindow != null)
            jobBoardWindow.CloseWindow();

        if (desktopController != null)
        {
            desktopController.UpdateUI();
            desktopController.UpdateStatusMessage();
            desktopController.SpawnCvIcon();
        }
        else
        {
            Debug.LogWarning("[CV BUILDER] DesktopController reference not assigned!");
        }

        ResetState();
    }

    private void ApplyOptionVisibility()
    {
        // First CV if no applications have been submitted yet
        bool isFirstCv =
            GameManager.Instance == null ||
            GameManager.Instance.submittedApplications.Count == 0;

        // Look at all draggable options in this window
        DraggableOption[] options = GetComponentsInChildren<DraggableOption>(true);

        foreach (var opt in options)
        {
            if (opt.onlyAfterFirstCv)
            {
                // Hide them on the very first CV, show them afterwards
                opt.gameObject.SetActive(!isFirstCv);
            }
        }
    }

    //Here is where the authenticity cost is calculated based on CV choices... NEED TO TWEAK VALUES AND FIGURE OUT HOW THEY WORK!!!
    private float CalculateAuthenticityCost(CVChoices cv)
    {
        float cost = 0f;

        // --- NAME ---
        // Most authentic: Aikerim Mukhan (0)
        if (cv.chosenName == "Aika Mukhan")
        {
            cost += 5f;  // soft tilpasning
        }
        else if (cv.chosenName == "Aika Moen")
        {
            cost += 10f; // full fornorsking
        }

        // --- PHOTO ---
        // Culture/personal photo = 0 cost
        // Profesjonelt foto = litt tilpasning
        if (cv.chosenPhoto.Contains("profesjonell") || cv.chosenPhoto.Contains("business"))
        {
            cost += 3f;
        }

        // Ikke inkludere bilde = skjule seg
        if (cv.chosenPhoto == "Ikke inkluder bilde" || cv.chosenPhoto == "No photo")
        {
            cost += 5f;
        }

        // --- PROFIL / SELF-DESCRIPTION ---
        // Profil 1: strukturerte, nøytrale buzzwords → “CV-norsk”
        if (cv.chosenCoverLetter.StartsWith("Strukturert og lærevillig"))
        {
            cost += 8f;
        }
        // Profil 2: "bakgrunn fra Kina", "Flerspråklig" = mer autentisk (0 ekstra)

        // --- LANGUAGES ---
        bool showsChinese = false;
        foreach (string lang in cv.chosenLanguages)
        {
            if (lang.Contains("Kinesisk") || lang.Contains("Chinese"))
                showsChinese = true;
        }

        // If she *never* shows these, treat it as editing them away
        if (!showsChinese)
            cost += 5f;

        // --- HOBBIES ---
        bool hasPersonalHobby = false;
        bool allVeryNorwegian = true;

        foreach (string hobby in cv.chosenHobbies)
        {
            // “Real/own” interests
            if (hobby.Contains("Illustrasjon") ||
                hobby.Contains("Spilldesign") ||
                hobby.Contains("Matlaging") ||
                hobby.Contains("Keramikk"))
            {
                hasPersonalHobby = true;
            }

            // Very “CV-Norwegian” stuff
            if (!(hobby.Contains("Friluftsliv") ||
                  hobby.Contains("Hytteliv") ||
                  hobby.Contains("Langrenn") ||
                  hobby.Contains("Soppsanking")))
            {
                allVeryNorwegian = false;
            }
        }

        // If she ONLY shows the “correct Norwegian” hobbies and hides the nerdy/creative ones:
        if (!hasPersonalHobby && cv.chosenHobbies.Length > 0 && allVeryNorwegian)
        {
            cost += 7f;
        }

        // --- VERV / VOLUNTEER ---
        bool showsDiversityVolunteer = false;
        foreach (string vol in cv.chosenVolunteer)
        {
            if (vol.Contains("innvandrer") || vol.Contains("Innvandrer") ||
                vol.Contains("minoritet"))
            {
                showsDiversityVolunteer = true;
            }
        }

        // If she *has* verv, but never mentions the innvandrer-mentor-ting:
        if (!showsDiversityVolunteer && cv.chosenVolunteer.Length > 0)
        {
            cost += 6f;
        }

        Debug.Log($"Authenticity cost: {cost}");
        return cost;
    }
}