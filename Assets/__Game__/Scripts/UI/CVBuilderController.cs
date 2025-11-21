using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;

public class CVBuilderController : MonoBehaviour
{
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

    private void Start()
    {
        submitButton.onClick.AddListener(OnSubmitClicked);
        UpdateSubmitButton();
    }

    private void Update()
    {
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

        if (allFilled)
        {
            submitButton.GetComponent<Image>().color = new Color(0.3f, 0.69f, 0.31f);
        }
        else
        {
            submitButton.GetComponent<Image>().color = Color.gray;
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

        // Calculate authenticity cost
        float authenticityCost = CalculateAuthenticityCost(cv);
        GameManager.Instance.authenticityScore -= authenticityCost;

        // Drain energy from applying
        Debug.Log($"[ENERGY] Before submission: {GameManager.Instance.emotionalEnergy}");
        GameManager.Instance.emotionalEnergy -= 5f;
        GameManager.Instance.emotionalEnergy = Mathf.Clamp(
            GameManager.Instance.emotionalEnergy,
            0,
            GameManager.Instance.maxEnergy
        );

        // Save application to GameManager
        GameManager.Instance.submittedApplications.Add(application);

        Debug.Log($"✅ Application saved! Total applications: {GameManager.Instance.submittedApplications.Count}");
        Debug.Log($"Applied to: {application.job.companyName}");

        // Update backgrounds
        Debug.Log($"[ENERGY] After submission (-5): {GameManager.Instance.emotionalEnergy}");
        GameManager.Instance.UpdateBackgrounds();

        // Return to Desktop
        SceneManager.LoadScene("2Desktop");
    }

    private float CalculateAuthenticityCost(CVChoices cv)
    {
        float cost = 0f;

        // Using adapted name
        if (cv.chosenName == "Joanna Bolat")
        {
            cost += 10f;
        }

        // Removing photo
        if (cv.chosenPhoto == "No photo" || cv.chosenPhoto == "Ikke inkluder bilde")
        {
            cost += 5f;
        }

        // Norwegian-focused cover letter
        if (cv.chosenCoverLetter.Contains("Norwegian") || cv.chosenCoverLetter.Contains("Norsk"))
        {
            cost += 8f;
        }

        // Hiding non-European languages
        bool hidesMandarin = true;
        bool hidesKazakh = true;
        foreach (string lang in cv.chosenLanguages)
        {
            if (lang.Contains("Mandarin")) hidesMandarin = false;
            if (lang.Contains("Kazakh") || lang.Contains("Kasakhisk")) hidesKazakh = false;
        }
        if (hidesMandarin) cost += 5f;
        if (hidesKazakh) cost += 3f;

        // Only Norwegian hobbies
        bool hasRealHobbies = false;
        foreach (string hobby in cv.chosenHobbies)
        {
            if (hobby.Contains("Crochet") || hobby.Contains("Hekling") ||
                hobby.Contains("Video") || hobby.Contains("Dataspill") ||
                hobby.Contains("Art") || hobby.Contains("Kunst") ||
                hobby.Contains("Reading") || hobby.Contains("Lesing"))
            {
                hasRealHobbies = true;
                break;
            }
        }
        if (!hasRealHobbies && cv.chosenHobbies.Length > 0)
        {
            cost += 7f;
        }

        // Hiding diversity volunteer work
        bool hidesDiversityWork = true;
        foreach (string vol in cv.chosenVolunteer)
        {
            if (vol.Contains("Immigrant") || vol.Contains("Innvandrer") || vol.Contains("minoritet"))
            {
                hidesDiversityWork = false;
                break;
            }
        }
        if (hidesDiversityWork && cv.chosenVolunteer.Length > 0)
        {
            cost += 6f;
        }

        Debug.Log($"Authenticity cost: {cost}");
        return cost;
    }
}