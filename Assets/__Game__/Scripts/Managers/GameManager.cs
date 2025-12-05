using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Game State")]
    public int currentMonth = 1;
    public int maxMonths; // Game duration is 10 months

    [Header("Player Stats")]
    public float emotionalEnergy = 100f;
    public float maxEnergy = 100f;
    public float authenticityScore = 100f; // Hidden from player, how to give feedback?
    public float energyDrainRate = 10f; //Energy lost by time progressing

    [Header("Energy Impact Settings")]
    public float callbackEnergyBoost = 25f;    // Energy gained from callback
    public float rejectionEnergyDrain = 20f;   // Energy lost from rejection
    public float applicationEnergyCost = 5f;   // Energy to submit application

    [Header("Desktop Backgrounds")]
    public GameObject fullEnergyBackground;
    public GameObject mediumEnergyBackground;  
    public GameObject lowEnergyBackground;     

    [Header("Applications")]
    public List<ApplicationData> submittedApplications = new List<ApplicationData>();
    public List<ApplicationData> callbacks = new List<ApplicationData>();
    public List<ApplicationData> rejections = new List<ApplicationData>();
    public List<ApplicationData> readEmails = new List<ApplicationData>();
    public List<ApplicationData> allEmails = new List<ApplicationData>();

    [Header("Current Application")]
    public JobData currentJobApplyingTo = null;

    private int updateBackgroundCallCount = 0; 

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

    private void Start()
    {
        // Initialize backgrounds at start
        Debug.Log($"[GAMEMANAGER] Initial energy: {emotionalEnergy}");
        UpdateBackgrounds();
    }

    public void AdvanceTime()
    {
        if (currentMonth >= maxMonths)
        {
            Debug.Log("Already at final month!");
            return;
        }

        // Advance by 1 month
        currentMonth += 1;

        if (currentMonth > maxMonths)
        {
            currentMonth = maxMonths;
        }

        // Drain energy
        emotionalEnergy -= energyDrainRate;
        emotionalEnergy = Mathf.Clamp(emotionalEnergy, 0, maxEnergy);

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

        float successChance = ApplicationScoring.Instance.CalculateSuccessChance(app);
        app.successChance = successChance;

        float roll = Random.Range(0f, 1f);

        if (roll <= successChance)
        {
            app.gotCallback = true;
            callbacks.Add(app);
            allEmails.Add(app);
        }
        else
        {
            app.gotCallback = false;
            rejections.Add(app);
            allEmails.Add(app);
        }
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
        else if (energyPercent > 20f)
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
        // Has unread if there are callbacks OR rejections that haven't been read
        return (callbacks.Count > 0 || rejections.Count > 0);
    }
}