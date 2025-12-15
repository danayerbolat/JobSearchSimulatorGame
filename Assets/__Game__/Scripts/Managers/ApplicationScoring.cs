using UnityEngine;
using System.Collections.Generic;

// Needs fine-tuning & balancing, but this is wired to your current CV options.
public class ApplicationScoring : MonoBehaviour
{
    public static ApplicationScoring Instance;

    [Header("Base Success Rates")]
    [Range(0f, 1f)]
    public float minimumSuccessRate = 0.05f;   // 5% - never impossible
    [Range(0f, 1f)]
    public float maximumSuccessRate = 0.40f;   // 40% - never guaranteed
    [Range(0f, 1f)]
    public float defaultBaseRate = 0.05f;    // 5% - default starting point

    [Header("CV Element Bonuses")]
    [Range(0f, 0.5f)]
    public float adaptedNameBonus = 0.08f;  // Using more Norwegian name
    [Range(0f, 0.5f)]
    public float norwegianCoverLetterBonus = 0.20f;  // Norsk/nøytral profil
    [Range(0f, 0.5f)]
    public float internationalCoverLetterBonus = 0.15f; // Profil med bakgrunn/inkludering
    [Range(0f, 0.2f)]
    public float hasPhotoBonus = 0.05f;  // Base photo bonus (scaled per company type)
    [Range(0f, 0.2f)]
    public float norwegianHobbiesBonus = 0.08f;  // Friluftsliv, hytteliv, osv.

    [Header("Difficulty Tuning")]
    [Tooltip("< 1 = harder, > 1 = easier")]
    [Range(0.1f, 2f)]
    public float difficultyMultiplier = 0.8f;

    [Header("Company Type Preferences")]
    [Tooltip("Companies where 'culture fit' / norskhet gir litt pluss")]
    public List<string> cultureFitCompanies = new List<string>
    {
        "Tech Startup",
        "Consultancy",
        "Produktselskap"
    };

    [Tooltip("Companies where mangfold og flerspråklighet gir pluss")]
    public List<string> internationalCompanies = new List<string>
    {
        "International",
        "Creative Tech",
        "Public Sector"
    };

    [Tooltip("Very traditional environments where 'looking Norwegian' is safest")]
    public List<string> traditionalCompanies = new List<string>
    {
        "Traditional Private",
        "Large Corporate"
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
    /// Calculate success chance for an application based on CV choices and job.
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
        score += ScorePhoto(cv.chosenPhoto, job.companyType);

        // === PROFILE / COVER LETTER SCORING ===
        score += ScoreCoverLetter(cv.chosenCoverLetter, job.companyType);

        // === HOBBIES SCORING ===
        score += ScoreHobbies(cv.chosenHobbies, job.companyType);

        // === LANGUAGE SCORING ===
        score += ScoreLanguages(cv.chosenLanguages, job.companyType);

        // === VOLUNTEER SCORING ===
        score += ScoreVolunteer(cv.chosenVolunteer, job.companyType);

        // Apply global difficulty
        score *= difficultyMultiplier;

        // Clamp to min/max
        score = Mathf.Clamp(score, minimumSuccessRate, maximumSuccessRate);

        Debug.Log($"[SCORING] {job.companyName} ({job.companyType}): {score:P0} success chance");

        return score;
    }

    // =========================================================
    // INDIVIDUAL SCORING METHODS
    // =========================================================

    private float ScoreName(string chosenName)
    {
        if (string.IsNullOrEmpty(chosenName))
            return 0f;

        // Strongly Norwegianised: biggest bonus
        if (chosenName == "Aika Moen")
        {
            Debug.Log($"  +{adaptedNameBonus:P0} (Sterkt fornorsket navn)");
            return adaptedNameBonus;
        }

        // Partially adapted: smaller bonus
        if (chosenName == "Aika Mukhan")
        {
            float bonus = adaptedNameBonus * 0.5f;
            Debug.Log($"  +{bonus:P0} (Delvis fornorsket navn)");
            return bonus;
        }

        // Fully original / other names: no bonus
        return 0f;
    }

    private float ScorePhoto(string chosenPhoto, string companyType)
    {
        if (string.IsNullOrEmpty(chosenPhoto))
            return 0f;

        string lower = chosenPhoto.ToLowerInvariant();

        bool isNoPhoto = lower.Contains("ikke inkluder") || lower.Contains("no photo");
        bool isCulturePhoto = lower.Contains("kultur") || lower.Contains("culture");
        bool isBizPhoto = lower.Contains("profesjonell") || lower.Contains("professional") ||
                              lower.Contains("business");

        float bonus = 0f;

        // 1) DIVERSITY-POSITIVE COMPANIES
        if (internationalCompanies.Contains(companyType))
        {
            if (isCulturePhoto)
            {
                bonus = hasPhotoBonus * 1.5f;
                Debug.Log($"  +{bonus:P0} (Culture photo matches {companyType})");
            }
            else if (isBizPhoto)
            {
                bonus = hasPhotoBonus * 0.5f;
                Debug.Log($"  +{bonus:P0} (Neutral business photo at {companyType})");
            }
            else if (isNoPhoto)
            {
                bonus = 0f;
                Debug.Log("  0% (No photo – neutral for diversity-focused company)");
            }
            else
            {
                bonus = hasPhotoBonus;
                Debug.Log($"  +{bonus:P0} (Generic photo at {companyType})");
            }
        }
        // 2) CULTURE-FIT COMPANIES
        else if (cultureFitCompanies.Contains(companyType))
        {
            if (isBizPhoto)
            {
                bonus = hasPhotoBonus;
                Debug.Log($"  +{bonus:P0} (Business photo fits {companyType})");
            }
            else if (isNoPhoto)
            {
                bonus = hasPhotoBonus * 0.6f;
                Debug.Log($"  +{bonus:P0} (No photo is 'safe' at {companyType})");
            }
            else if (isCulturePhoto)
            {
                bonus = -hasPhotoBonus * 0.4f;
                Debug.Log($"{bonus:P0} (Culture photo reads less 'professional' at {companyType})");
            }
            else
            {
                bonus = hasPhotoBonus * 0.5f;
                Debug.Log($"  +{bonus:P0} (Generic photo at {companyType})");
            }
        }
        // 3) VERY TRADITIONAL COMPANIES
        else if (traditionalCompanies.Contains(companyType))
        {
            if (isBizPhoto)
            {
                bonus = hasPhotoBonus * 1.2f;
                Debug.Log($"  +{bonus:P0} (Formal photo fits very traditional {companyType})");
            }
            else if (isNoPhoto)
            {
                bonus = hasPhotoBonus * 0.3f;
                Debug.Log($"  +{bonus:P0} (No photo – slightly safer at {companyType})");
            }
            else if (isCulturePhoto)
            {
                bonus = -hasPhotoBonus;
                Debug.Log($"{bonus:P0} (Culture photo clashes with traditional {companyType})");
            }
        }
        // 4) Fallback
        else
        {
            if (!isNoPhoto)
            {
                bonus = hasPhotoBonus;
                Debug.Log($"  +{bonus:P0} (Has photo)");
            }
        }

        return bonus;
    }

    private float ScoreCoverLetter(string profileText, string companyType)
    {
        if (string.IsNullOrEmpty(profileText))
            return 0f;

        float bonus = 0f;

        // Your two profiles:
        // 1) "Strukturert og lærevillig junior IT-utvikler ..."
        // 2) "Junior IT-utvikler med bakgrunn fra Kina ..."
        bool isNorwegianProfile =
            profileText.StartsWith("Strukturert og lærevillig");

        bool isBackgroundProfile =
            profileText.Contains("bakgrunn fra Kina") ||
            profileText.Contains("Flerspråklig");

        if (isNorwegianProfile)
        {
            // Culture-fit and traditional like "nøytral norsk" profil
            if (cultureFitCompanies.Contains(companyType))
            {
                bonus = norwegianCoverLetterBonus;
                Debug.Log($"  +{bonus:P0} (Norsk, nøytral profil matcher {companyType})");
            }
            else if (traditionalCompanies.Contains(companyType))
            {
                bonus = norwegianCoverLetterBonus * 1.2f;
                Debug.Log($"  +{bonus:P0} (Norsk, trygg profil verdsettes av {companyType})");
            }
            else if (internationalCompanies.Contains(companyType))
            {
                bonus = norwegianCoverLetterBonus * 0.5f;
                Debug.Log($"  +{bonus:P0} (Norsk profil – ok, men ikke avgjørende i {companyType})");
            }
        }
        else if (isBackgroundProfile)
        {
            // Diversity-positive environments value this most
            if (internationalCompanies.Contains(companyType))
            {
                bonus = internationalCoverLetterBonus;
                Debug.Log($"  +{bonus:P0} (Profil med minoritetsbakgrunn verdsettes i {companyType})");
            }
            else if (cultureFitCompanies.Contains(companyType))
            {
                bonus = internationalCoverLetterBonus * 0.5f;
                Debug.Log($"  +{bonus:P0} (Profil med bakgrunn gir litt ekstra i {companyType})");
            }
            else if (traditionalCompanies.Contains(companyType))
            {
                bonus = -internationalCoverLetterBonus * 0.5f;
                Debug.Log($"{bonus:P0} (Profil med bakgrunn kan leses som 'mindre trygg' i {companyType})");
            }
        }

        return bonus;
    }

    private float ScoreHobbies(string[] hobbies, string companyType)
    {
        if (hobbies == null || hobbies.Length == 0)
            return 0f;

        // Check for very "Norwegian" hobbies
        bool hasNorwegianHobbies = false;
        foreach (string hobby in hobbies)
        {
            if (hobby.Contains("Friluftsliv") ||
                hobby.Contains("Hytteliv") ||
                hobby.Contains("Langrenn") ||
                hobby.Contains("Soppsanking"))
            {
                hasNorwegianHobbies = true;
                break;
            }
        }

        if (hasNorwegianHobbies &&
            (cultureFitCompanies.Contains(companyType) || traditionalCompanies.Contains(companyType)))
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

        bool showsNorwegian = false;
        bool showsEnglish = false;
        bool showsChinese = false;
        bool showsKazakh = false;

        foreach (string lang in languages)
        {
            if (lang.Contains("Norsk") || lang.Contains("Norwegian"))
                showsNorwegian = true;
            if (lang.Contains("Engelsk") || lang.Contains("English"))
                showsEnglish = true;
            if (lang.Contains("Kinesisk") || lang.Contains("Mandarin"))
                showsChinese = true;
            if (lang.Contains("Kazakh") || lang.Contains("Kasakhisk"))
                showsKazakh = true;
        }

        // Diversity-positive companies value multilingual
        if (internationalCompanies.Contains(companyType))
        {
            if (showsChinese || showsKazakh)
            {
                bonus = 0.10f;
                Debug.Log($"  +{bonus:P0} (Flerspråklighet verdsettes i {companyType})");
            }
        }
        // Very traditional companies prefer only Norwegian + English
        else if (traditionalCompanies.Contains(companyType))
        {
            if (showsNorwegian && showsEnglish && !showsChinese && !showsKazakh)
            {
                bonus = 0.08f;
                Debug.Log($"  +{bonus:P0} (Bare Norsk + Engelsk passer best i {companyType})");
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
            // Diversity-focused volunteer work (mentor innvandrerungdom osv.)
            if (vol.Contains("innvandrer") || vol.Contains("Innvandrer") ||
                vol.Contains("minoritet"))
            {
                if (internationalCompanies.Contains(companyType))
                {
                    bonus = 0.08f;
                    Debug.Log($"  +{bonus:P0} (Diversity work valued by {companyType})");
                }
                else if (traditionalCompanies.Contains(companyType))
                {
                    bonus = -0.05f;
                    Debug.Log($"  {bonus:P0} (Diversity work not prioritized by {companyType})");
                }
            }
            // Tech/student org volunteer work (linjeforening, kodeklubb)
            else if (vol.Contains("linjeforening") ||
                     vol.Contains("kodeklubb") ||
                     vol.Contains("student"))
            {
                if (companyType == "Tech Startup" || companyType == "Consultancy")
                {
                    bonus = 0.06f;
                    Debug.Log($"  +{bonus:P0} (Tech/student volunteer work valued by {companyType})");
                }
            }
        }

        return bonus;
    }

    /// <summary>
    /// Text description for debug / potential in-game explanation.
    /// </summary>
    public string GetScoringBreakdown(ApplicationData app)
    {
        string breakdown = $"Søknad til {app.job.companyName} ({app.job.companyType}):\n";
        float baseShown = defaultBaseRate * difficultyMultiplier;
        breakdown += $"Base sjanse: {baseShown:P0}\n";

        float finalChance = CalculateSuccessChance(app);
        breakdown += $"\nEndelig sjanse: {finalChance:P0}";

        return breakdown;
    }
}


