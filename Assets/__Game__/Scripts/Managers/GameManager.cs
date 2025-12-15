using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using FMODUnity;
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
    public float energyDrainRate; //Energy lost by time progressing

    [Header("Energy Impact Settings")]
    public float callbackEnergyBoost;    // Energy gained from callback
    public float rejectionEnergyDrain;   // Energy lost from rejection
    public float applicationEnergyCost;   // Energy to submit application

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

    [Header("Ending Thresholds")]
    public float lowAuthenticityThreshold = 40f;
    public int highCallbackThreshold = 3;

    [Header("Narrative Tweaks")]
    public bool forceFirstApplicationFail = true;
    private bool hasForcedFirstApplication = false;

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
        CheckForGameEnd();
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

        // --- 1) FORCE FIRST APPLICATION TO FAIL (narrative) ---
        if (forceFirstApplicationFail && !hasForcedFirstApplication)
        {
            ApplicationData firstApp = (submittedApplications.Count > 0)
                ? submittedApplications[0]
                : null;

            if (app == firstApp)
            {
                hasForcedFirstApplication = true;

                app.successChance = 0f;
                app.gotCallback = false;
                rejections.Add(app);
                allEmails.Add(app);

                Debug.Log("[GM] Forced first application to fail for narrative reasons.");
                return;
            }
        }

        // --- 2) Normal scoring ---
        float successChance = ApplicationScoring.Instance.CalculateSuccessChance(app);
        app.successChance = successChance;

        string breakdown = ApplicationScoring.Instance.GetScoringBreakdown(app);
        Debug.Log(breakdown);

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
                mediumEnergyBackground.SetActive(false);
                lowEnergyBackground.SetActive(false);
                //Debug.Log("Background: Full Energy (bright)");
            }
        }
        else if (energyPercent > 25f)
        {
            // Medium energy: Show darker background
            if (mediumEnergyBackground != null)
            {
                fullEnergyBackground.SetActive(false);
                mediumEnergyBackground.SetActive(true);
                lowEnergyBackground.SetActive(false);
                //Debug.Log("Background: Medium Energy (darker)");
            }
        }
        else
        {
            // Low energy: Show oppressive dark background
            if (lowEnergyBackground != null)
            {
                fullEnergyBackground.SetActive(false);
                mediumEnergyBackground.SetActive(false);
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

    public void GoToEnding(EndingType chosenEnding)
    {
        EndingData.ApplyFromGame(this, chosenEnding);

        // Use the same scene name everywhere
        SceneManager.LoadScene("1EndingScene");
    }

    public void CheckForGameEnd()
    {
        bool outOfEnergy = emotionalEnergy <= 0f;
        bool timeUp = currentMonth >= maxMonths;

        if (!outOfEnergy && !timeUp)
            return; // Game continues

        // Decide ending type based on final state
        EndingType chosenEnding;

        if (outOfEnergy)
        {
            chosenEnding = EndingType.Burnout;
        }
        else
        {
            bool lowAuth = authenticityScore < lowAuthenticityThreshold;

            // "Any callbacks" looks at allEmails
            bool anyCallbacks = allEmails != null &&
                                allEmails.Any(a => a.responseReceived && a.gotCallback);
            int totalCallbacks = allEmails.Count(a => a.responseReceived && a.gotCallback);

            if (lowAuth && anyCallbacks)
            {
                // Got some external success, but lost a lot av seg selv
                chosenEnding = EndingType.HollowSuccess;
            }
            else if (!lowAuth && !anyCallbacks)
            {
                // Held onto self, but system didn’t reward
                chosenEnding = EndingType.StayedYourself;
            }
            else if ((authenticityScore == 100) && (totalCallbacks >= 2) )
            {
                // Best case: kept self and got callbacks
                chosenEnding = EndingType.AuthenticSuccess;
            }
            else
            {
                // Messy middle
                chosenEnding = EndingType.InBetween;
            }
        }

        // This will also fill EndingData and load the ending scene
        GoToEnding(chosenEnding);
    }
}