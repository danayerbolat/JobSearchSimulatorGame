using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Game State")]
    public int currentWeek = 1;
    public int maxWeeks = 12; // Player has 12 weeks to find job

    [Header("Player Stats")]
    public float emotionalEnergy = 100f;
    public float maxEnergy = 100f;
    public float authenticityScore = 100f; // Hidden from player

    //Maybe a way to track each element of authenticity separately?
    [Header("CV Tracking")]
    public int realCVUsed = 0;
    public int safeCVUsed = 0;
    public int diverseCVUsed = 0;

    [Header("Applications")]
    public List<ApplicationData> submittedApplications = new List<ApplicationData>();
    public List<ApplicationData> callbacks = new List<ApplicationData>();
    public List<ApplicationData> rejections = new List<ApplicationData>();

    private void Awake()
    {
        // Singleton pattern - only one GameManager exists
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AdvanceWeek()
    {
        // Don't advance past max weeks
        if (currentWeek >= maxWeeks)
        {
            Debug.Log("Already at max weeks!");
            return;
        }

        currentWeek++;
        emotionalEnergy -= 5f; // Time passing drains energy

        // Clamp energy, what does this mean?
        emotionalEnergy = Mathf.Clamp(emotionalEnergy, 0, maxEnergy);

        ProcessApplications();
    }

    private void ProcessApplications()
    {
        // Check applications for responses
        foreach (ApplicationData app in submittedApplications)
        {
            if (app.responseReceived) continue; // Already processed

            int weeksSinceSubmission = currentWeek - app.submittedWeek;

            if (weeksSinceSubmission >= 2)
            {
                // Time to get response
                app.responseReceived = true;

                // Roll for success based on CV type
                if (Random.value <= app.successChance)
                {
                    callbacks.Add(app);
                    emotionalEnergy += 10f; // Callback feels good!
                }
                else
                {
                    rejections.Add(app);
                    emotionalEnergy -= 15f; // Rejection hurts
                }
            }
        }
    }

    public void SubmitApplication(ApplicationData application)
    {
        submittedApplications.Add(application);

        // Track CV usage
        switch (application.cvType)
        {
            case CVType.Real:
                realCVUsed++;
                break;
            case CVType.Safe:
                safeCVUsed++;
                authenticityScore -= 10f; // Using safe CV reduces authenticity
                break;
            case CVType.Diverse:
                diverseCVUsed++;
                authenticityScore -= 5f;
                break;
        }

        emotionalEnergy -= 10f; // Submitting application takes energy
    }
}