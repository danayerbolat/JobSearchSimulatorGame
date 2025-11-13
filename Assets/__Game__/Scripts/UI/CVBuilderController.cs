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
        // Collect single choices
        string chosenName = nameField.currentValue;
        string chosenPhoto = photoField.currentValue;
        string chosenCoverLetter = coverLetterField.currentValue;

        // Collect multiple choices
        List<string> chosenLanguages = languagesField.GetValues();
        List<string> chosenHobbies = hobbiesField.GetValues();
        List<string> chosenVolunteer = volunteerField.GetValues();

        Debug.Log("=== CV SUBMITTED ===");
        Debug.Log($"Name: {chosenName}");
        Debug.Log($"Photo: {chosenPhoto}");
        Debug.Log($"Cover Letter: {chosenCoverLetter}");
        Debug.Log($"Languages: {string.Join(", ", chosenLanguages)}");
        Debug.Log($"Hobbies: {string.Join(", ", chosenHobbies)}");
        Debug.Log($"Volunteer: {string.Join(", ", chosenVolunteer)}");

        // TODO: Save to GameManager

        SceneManager.LoadScene("2Desktop");
    }
}