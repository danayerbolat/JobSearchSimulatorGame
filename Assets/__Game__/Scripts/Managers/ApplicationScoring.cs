using UnityEngine;
using System.Collections.Generic;

//Needs a LOT of fine-tuning and balancing!!
public class ApplicationScoring : MonoBehaviour
{
    public static ApplicationScoring Instance;

    [Header("Base Success Rates")]
    [Range(0f, 1f)]
    public float minimumSuccessRate = 0.05f; // 5% - never impossible
    [Range(0f, 1f)]
    public float maximumSuccessRate = 0.70f; // 70% - never guaranteed
    [Range(0f, 1f)]
    public float defaultBaseRate = 0.20f;    // 20% - default starting point

    [Header("CV Element Bonuses")]
    [Range(0f, 0.5f)]
    public float adaptedNameBonus = 0.15f;           // Using "Joanna Bolat"
    [Range(0f, 0.5f)]
    public float norwegianCoverLetterBonus = 0.20f;  // Norwegian-focused
    [Range(0f, 0.5f)]
    public float internationalCoverLetterBonus = 0.25f; // International
    [Range(0f, 0.2f)]
    public float hasPhotoBonus = 0.05f;              // Including any photo
    [Range(0f, 0.2f)]
    public float norwegianHobbiesBonus = 0.10f;      // Norwegian hobbies

    [Header("Company Type Preferences")]
    [Tooltip("Which company types value Norwegian culture fit")]
    public List<string> cultureFitCompanies = new List<string>
    {
        "Tech Startup",
        "Traditional Norwegian"
    };

    [Tooltip("Which company types value international perspective")]
    public List<string> internationalCompanies = new List<string>
    {
        "International"
    };

    private void Awake()
    {
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

    /// <summary>
    /// Calculate success chance for an application based on CV choices and job
    /// </summary>
    public float CalculateSuccessChance(ApplicationData app)
    {
        if (app == null || app.cvChoices == null || app.job == null)
        {
            Debug.LogWarning("Invalid application data for scoring");
            return defaultBaseRate;
        }

        CVChoices cv = app.cvChoices;
        JobData job = app.job;

        float score = defaultBaseRate;

        // === NAME SCORING ===
        score += ScoreName(cv.chosenName);

        // === PHOTO SCORING ===
        score += ScorePhoto(cv.chosenPhoto);

        // === COVER LETTER SCORING ===
        score += ScoreCoverLetter(cv.chosenCoverLetter, job.companyType);

        // === HOBBIES SCORING ===
        score += ScoreHobbies(cv.chosenHobbies, job.companyType);

        // === LANGUAGE SCORING ===
        score += ScoreLanguages(cv.chosenLanguages, job.companyType);

        // === VOLUNTEER SCORING ===
        score += ScoreVolunteer(cv.chosenVolunteer, job.companyType);

        // Clamp to min/max
        score = Mathf.Clamp(score, minimumSuccessRate, maximumSuccessRate);

        Debug.Log($"[SCORING] {job.companyName} ({job.companyType}): {score:P0} success chance");

        return score;
    }

    // === INDIVIDUAL SCORING METHODS ===

    private float ScoreName(string chosenName)
    {
        // Using adapted Norwegian name improves chances
        if (chosenName == "Joanna Bolat")
        {
            Debug.Log($"  +{adaptedNameBonus:P0} (Adapted name)");
            return adaptedNameBonus;
        }

        // Using real name = no bonus (but not a penalty)
        return 0f;
    }

    private float ScorePhoto(string chosenPhoto)
    {
        // Having a photo generally helps (shows confidence, approachability)
        if (!string.IsNullOrEmpty(chosenPhoto) &&
            chosenPhoto != "No photo" &&
            chosenPhoto != "Ikke inkluder bilde")
        {
            Debug.Log($"  +{hasPhotoBonus:P0} (Has photo)");
            return hasPhotoBonus;
        }

        return 0f;
    }

    private float ScoreCoverLetter(string coverLetter, string companyType)
    {
        float bonus = 0f;

        // Norwegian-focused cover letter
        if (coverLetter.Contains("Norsk") || coverLetter.Contains("Norwegian"))
        {
            // Big bonus for traditional/public sector companies
            if (companyType == "Traditional Norwegian" || companyType == "Public Sector")
            {
                bonus = norwegianCoverLetterBonus;
                Debug.Log($"  +{bonus:P0} (Norwegian focus matches {companyType})");
            }
            else if (cultureFitCompanies.Contains(companyType))
            {
                // Smaller bonus for culture-fit companies
                bonus = norwegianCoverLetterBonus * 0.5f;
                Debug.Log($"  +{bonus:P0} (Norwegian focus helps with {companyType})");
            }
        }
        // International perspective
        else if (coverLetter.Contains("International"))
        {
            if (internationalCompanies.Contains(companyType))
            {
                bonus = internationalCoverLetterBonus;
                Debug.Log($"  +{bonus:P0} (International perspective matches {companyType})");
            }
        }

        return bonus;
    }

    private float ScoreHobbies(string[] hobbies, string companyType)
    {
        if (hobbies == null || hobbies.Length == 0)
            return 0f;

        // Check for Norwegian hobbies
        bool hasNorwegianHobbies = false;
        foreach (string hobby in hobbies)
        {
            if (hobby.Contains("Ski") || hobby.Contains("Hiking") ||
                hobby.Contains("Fottur") || hobby.Contains("Friluftsliv") ||
                hobby.Contains("Cabin") || hobby.Contains("Hytte"))
            {
                hasNorwegianHobbies = true;
                break;
            }
        }

        if (hasNorwegianHobbies && cultureFitCompanies.Contains(companyType))
        {
            Debug.Log($"  +{norwegianHobbiesBonus:P0} (Norwegian hobbies match {companyType})");
            return norwegianHobbiesBonus;
        }

        return 0f;
    }

    private float ScoreLanguages(string[] languages, string companyType)
    {
        if (languages == null || languages.Length == 0)
            return 0f;

        float bonus = 0f;

        // Check what languages are shown
        bool showsNorwegian = false;
        bool showsEnglish = false;
        bool showsMandarin = false;
        bool showsKazakh = false;

        foreach (string lang in languages)
        {
            if (lang.Contains("Norwegian") || lang.Contains("Norsk"))
                showsNorwegian = true;
            if (lang.Contains("English") || lang.Contains("Engelsk"))
                showsEnglish = true;
            if (lang.Contains("Mandarin"))
                showsMandarin = true;
            if (lang.Contains("Kazakh") || lang.Contains("Kasakhisk"))
                showsKazakh = true;
        }

        // International companies value multilingual
        if (internationalCompanies.Contains(companyType))
        {
            if (showsMandarin || showsKazakh)
            {
                bonus = 0.10f;
                Debug.Log($"  +{bonus:P0} (Multilingual valued by {companyType})");
            }
        }
        // Traditional companies prefer just Norwegian + English
        else if (companyType == "Traditional Norwegian")
        {
            if (showsNorwegian && showsEnglish && !showsMandarin && !showsKazakh)
            {
                bonus = 0.08f;
                Debug.Log($"  +{bonus:P0} (European languages only for {companyType})");
            }
        }

        return bonus;
    }

    private float ScoreVolunteer(string[] volunteer, string companyType)
    {
        if (volunteer == null || volunteer.Length == 0)
            return 0f;

        float bonus = 0f;

        foreach (string vol in volunteer)
        {
            // Diversity-focused volunteer work
            if (vol.Contains("Immigrant") || vol.Contains("Innvandrer") || vol.Contains("minoritet"))
            {
                // Good for international companies
                if (internationalCompanies.Contains(companyType))
                {
                    bonus = 0.08f;
                    Debug.Log($"  +{bonus:P0} (Diversity work valued by {companyType})");
                }
                // Slight penalty for very traditional companies
                else if (companyType == "Traditional Norwegian")
                {
                    bonus = -0.05f;
                    Debug.Log($"  {bonus:P0} (Diversity work not prioritized by {companyType})");
                }
            }
            // Tech/student org volunteer work
            else if (vol.Contains("Tech") || vol.Contains("student") || vol.Contains("linjeforening"))
            {
                // Positive for tech companies
                if (companyType == "Tech Startup" || companyType == "Consultancy")
                {
                    bonus = 0.06f;
                    Debug.Log($"  +{bonus:P0} (Tech volunteer work valued by {companyType})");
                }
            }
        }

        return bonus;
    }

    /// <summary>
    /// Get a text description of why the success rate is what it is (for debugging/player feedback)
    /// </summary>
    public string GetScoringBreakdown(ApplicationData app)
    {
        string breakdown = $"Søknad til {app.job.companyName}:\n";
        breakdown += $"Base sjanse: {defaultBaseRate:P0}\n";

        // Recalculate with logging
        float finalChance = CalculateSuccessChance(app);

        breakdown += $"\nEndelig sjanse: {finalChance:P0}";

        return breakdown;
    }
}