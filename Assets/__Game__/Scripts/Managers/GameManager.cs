using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Game State")]
    public int currentMonth = 1;
    public int maxMonths = 19; // Game duration is 10 months

    [Header("Player Stats")]
    public float emotionalEnergy = 100f;
    public float maxEnergy = 100f;
    public float authenticityScore = 100f; // Hidden from player
    public float energyDrainRate = 10f; //Energy lost by time progressing

    [Header("Desktop Backgrounds")]
    public GameObject fullEnergyBackground;    // Bright, cute background
    public GameObject mediumEnergyBackground;  // Darker, muted background
    public GameObject lowEnergyBackground;     // Dark, oppressive background

    //Maybe a way to track each element of authenticity separately?
    [Header("CV Tracking")]
    public int realCVUsed = 0;
    public int safeCVUsed = 0;
    public int diverseCVUsed = 0;

    [Header("Applications")]
    public List<ApplicationData> submittedApplications = new List<ApplicationData>();
    public List<ApplicationData> callbacks = new List<ApplicationData>();
    public List<ApplicationData> rejections = new List<ApplicationData>();

    [Header("Current Application")]
    public JobData currentJobApplyingTo = null;

    private int updateBackgroundCallCount = 0; // NEW - for debugging

    private void Awake()
    {
        Debug.Log($"[GAMEMANAGER] Awake called! Instance exists: {Instance != null}");
        // Singleton pattern - only one GameManager exists
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log($"[GAMEMANAGER] ✅ This is THE instance. ID: {GetInstanceID()}");
        }
        else
        {
            Debug.LogWarning($"[GAMEMANAGER] ❌ DUPLICATE! Destroying this one. Original ID: {Instance.GetInstanceID()}, This ID: {GetInstanceID()}");
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Initialize backgrounds at start
        UpdateBackgrounds();
    }

    public void AdvanceWeek()
    {
        if (currentMonth >= maxMonths)
        {
            Debug.Log("Already at final month!");
            return;
        }

        // Advance by 1-2 months
        int monthsToAdvance = Random.Range(1, 3);
        currentMonth += monthsToAdvance;

        if (currentMonth > maxMonths)
        {
            currentMonth = maxMonths;
        }

        Debug.Log($"Time advanced to Month {currentMonth}");

        // Drain energy
        Debug.Log($"[ENERGY] Before time drain: {emotionalEnergy}");
        emotionalEnergy -= energyDrainRate * monthsToAdvance;
        emotionalEnergy = Mathf.Clamp(emotionalEnergy, 0, maxEnergy);
        Debug.Log($"[ENERGY] After time drain (-{energyDrainRate * monthsToAdvance}): {emotionalEnergy}");

        UpdateBackgrounds();
        ProcessApplications();
    }

    private void ProcessApplications()
    {
        List<ApplicationData> toProcess = new List<ApplicationData>();

        foreach (ApplicationData app in submittedApplications)
        {
            if (!app.responseReceived)
            {
                toProcess.Add(app);
            }
        }

        Debug.Log($"Processing {toProcess.Count} pending applications");

        foreach (ApplicationData app in toProcess)
        {
            GenerateResponse(app);
        }
    }

    private void GenerateResponse(ApplicationData app)
    {
        app.responseReceived = true;

        // USE SCORING SYSTEM TO CALCULATE CHANCE
        float successChance = ApplicationScoring.Instance.CalculateSuccessChance(app);
        app.successChance = successChance;

        // Roll for success
        float roll = Random.Range(0f, 1f);

        if (roll <= successChance)
        {
            // CALLBACK
            app.gotCallback = true;
            callbacks.Add(app);

            Debug.Log($"[ENERGY] Before callback bonus: {emotionalEnergy}");
            emotionalEnergy += 25f;
            emotionalEnergy = Mathf.Clamp(emotionalEnergy, 0, maxEnergy);
            Debug.Log($"[ENERGY] After callback (+25): {emotionalEnergy}");
            Debug.Log($"✅ CALLBACK from {app.job.companyName}! (Chance was {successChance:P0})");
        }
        else
        {
            // REJECTION
            app.gotCallback = false;
            rejections.Add(app);
            Debug.Log($"[ENERGY] Before rejection penalty: {emotionalEnergy}");
            emotionalEnergy -= 20f;
            emotionalEnergy = Mathf.Clamp(emotionalEnergy, 0, maxEnergy);
            Debug.Log($"[ENERGY] After rejection (-20): {emotionalEnergy}");
            Debug.Log($"❌ REJECTION from {app.job.companyName} (Chance was {successChance:P0})");
        }

        UpdateBackgrounds();
    }

    public void UpdateBackgrounds()
    {
        // Calculate energy percentage (0-100)
        float energyPercent = (emotionalEnergy / maxEnergy) * 100f;
        Debug.Log($"[BACKGROUND #{updateBackgroundCallCount}] Energy: {emotionalEnergy}/{maxEnergy} = {energyPercent:F1}%");

        // Disable all backgrounds first
        if (fullEnergyBackground != null)
            fullEnergyBackground.SetActive(false);
        if (mediumEnergyBackground != null)
            mediumEnergyBackground.SetActive(false);
        if (lowEnergyBackground != null)
            lowEnergyBackground.SetActive(false);

        // Enable the correct background based on energy level
        if (energyPercent > 50f)
        {
            // High energy: Show bright, cute background
            if (fullEnergyBackground != null)
            {
                fullEnergyBackground.SetActive(true);
                //Debug.Log("Background: Full Energy (bright)");
            }
        }
        else if (energyPercent > 10f)
        {
            // Medium energy: Show darker background
            if (mediumEnergyBackground != null)
            {
                mediumEnergyBackground.SetActive(true);
                //Debug.Log("Background: Medium Energy (darker)");
            }
        }
        else
        {
            // Low energy: Show oppressive dark background
            if (lowEnergyBackground != null)
            {
                lowEnergyBackground.SetActive(true);
                //Debug.Log("Background: Low Energy (dark)");
            }
        }
    }

    // HELPER METHODS
    public bool HasPendingApplications()
    {
        foreach (ApplicationData app in submittedApplications)
        {
            if (!app.responseReceived)
            {
                return true;
            }
        }
        return false;
    }

    public bool HasUnreadResponses()
    {
        return (callbacks.Count > 0 || rejections.Count > 0);
    }
}