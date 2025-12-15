using UnityEngine;
using TMPro;
using System.Collections;
using FMODUnity;

public class AikaMonologueManager : MonoBehaviour
{
    public static AikaMonologueManager Instance;

    [Header("UI")]
    public GameObject rootPanel;          // A little speech bubble panel
    public TextMeshProUGUI lineText;      // TMP text inside the bubble
    public float visibleSeconds = 3f;

    [Header("Typing Effect")]
    public float charDelay = 0.02f;       // time between characters

    [Header("FMOD SFX")]
    [SerializeField] private EventReference sighEvent;

    [Header("First CV lines")]
    [TextArea]
    public string[] firstCvLines = new string[]
    {
        "Ok, første ordentlige CV. Hva er det verste som kan skje.",
        "Sånn. Nå finnes jeg offisielt i PDF-format.",
        "Send. Der gikk ett stykk selvbilde inn i søknadsportalen."
    };

    [Header("First email lines – rejection")]
    [TextArea]
    public string[] firstRejectionLines = new string[]
    {
        "Der kom første nei. Statistikken har offisielt startet.",
        "Jaja, alltid hyggelig å få bekreftet at «vi hadde mange kvalifiserte søkere».",
        "Første avslag. Fint at noen fikk øvd seg på høflig standardtekst."
    };

    [Header("CV submit lines")]
    [TextArea]
    public string[] cvVeryAuthenticLines = new string[]
    {
        "Ok, dette ser faktisk ut som meg. Skummelt.",
        "Navn, språk, hobbyer… ja, dette er i hvert fall ærlig.",
        "Hvis de ikke liker denne versjonen, liker de nok ikke en «norskere» versjon heller."
    };

    [TextArea]
    public string[] cvMixedLines = new string[]
    {
        "Litt mer polert, litt mindre kaos. Kanskje greit.",
        "Halvparten meg, halvparten LinkedIn-robot. 50/50 sjanse.",
        "Ok, vi later som hytteliv er en personlighet."
    };

    [TextArea]
    public string[] cvVeryStrategicLines = new string[]
    {
        // Removed Aika Moen specifically so it never lies about her name
        "Gratulerer til den nye, veldig ansettelige versjonen av meg.",
        "Dette er mindre CV og mer cosplay nå.",
        "Hvis mamma ser denne, kommer hun til å spørre hvem jeg har blitt."
    };

    [Header("Email lines – callbacks")]
    [TextArea]
    public string[] callbackHighAuthLines = new string[]
    {
        "Vent… de likte faktisk den ærlige versjonen? Hjelp.",
        "Ok, tydeligvis kan jeg være meg selv OG få intervju. Diversity wins, I guess.",
        "Note to self: kanskje jeg ikke trenger å gjemme alt."
    };

    [TextArea]
    public string[] callbackLowAuthLines = new string[]
    {
        "Jaja, identitetsvasking funka visst.",
        "«Du passer godt inn hos oss»… ja, det var jo litt poenget.",
        "Hyggelig med intervju. Litt mindre hyggelig at jeg ikke helt kjenner hun på CV-en."
    };

    [Header("Email lines – avslag")]
    [TextArea]
    public string[] rejectionHighAuthLines = new string[]
    {
        "De sa nei, men i det minste sa de nei til meg, ikke til en maske.",
        "Au. Men jeg angrer ikke på at jeg skrev det sånn.",
        "Ok, ikke dra inn selvtilliten i dette også, takk."
    };

    [TextArea]
    public string[] rejectionLowAuthLines = new string[]
    {
        "Så jeg solgte sjela på tilbud… og *fikk* ikke jobben en gang.",
        "Bra, da vet vi i hvert fall at den super-norske versjonen ikke er magisk heller.",
        "Tenk å få avslag på en CV jeg ikke engang liker selv."
    };

    // --- state ---
    private bool hasShownFirstCvLine = false;
    private bool hasShownFirstRejectionLine = false;
    private Coroutine hideRoutine;
    private Coroutine typingRoutine;

    // indices so we cycle instead of random
    private int firstCvIndex = 0;
    private int firstRejectionIndex = 0;
    private int cvVeryAuthIndex = 0;
    private int cvMixedIndex = 0;
    private int cvStrategicIndex = 0;
    private int callbackHighIndex = 0;
    private int callbackLowIndex = 0;
    private int rejectionHighIndex = 0;
    private int rejectionLowIndex = 0;

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
            return;
        }

        if (rootPanel != null)
            rootPanel.SetActive(false);
    }

    public void ShowLine(string line)
    {
        if (lineText == null || rootPanel == null || string.IsNullOrEmpty(line))
            return;

        if (typingRoutine != null)
            StopCoroutine(typingRoutine);
        if (hideRoutine != null)
            StopCoroutine(hideRoutine);

        rootPanel.SetActive(true);
        lineText.text = string.Empty;

        typingRoutine = StartCoroutine(TypeLine(line));
    }

    private IEnumerator TypeLine(string line)
    {
        lineText.text = "";

        foreach (char c in line)
        {
            lineText.text += c;
            yield return new WaitForSeconds(charDelay);
        }

        typingRoutine = null;
        hideRoutine = StartCoroutine(HideAfterDelay());
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(visibleSeconds);
        rootPanel.SetActive(false);
        hideRoutine = null;
    }

    // helper: cycle through a pool
    private string NextFromPool(ref int index, string[] pool)
    {
        if (pool == null || pool.Length == 0) return null;

        if (index >= pool.Length)
            index = 0;

        string line = pool[index];
        index++;
        return line;
    }

    /// <summary>
    /// Call this rett etter at du har beregnet authenticityCost for EN CV.
    /// authenticityCost = resultatet fra CalculateAuthenticityCost(cv).
    /// </summary>
    public void OnCvSubmitted(float authenticityCost)
    {
        string line;

        // Første CV: egen liten “åh shit nå er vi i gang”-kommentar
        if (!hasShownFirstCvLine)
        {
            line = NextFromPool(ref firstCvIndex, firstCvLines);
            hasShownFirstCvLine = true;

            ShowLine(line);
            return;
        }

        // Etterpå: bruk bare den ene CV-en sin kostnad.
        if (authenticityCost <= 4f)
        {
            // Nesten ingenting justert → veldig autentisk
            line = NextFromPool(ref cvVeryAuthIndex, cvVeryAuthenticLines);
            // (ingen sukk her)
        }
        else if (authenticityCost <= 15f)
        {
            // Noe justert → blanding
            line = NextFromPool(ref cvMixedIndex, cvMixedLines);
            PlaySigh();   // <-- mixed CV sigh
        }
        else
        {
            // Høy kostnad → ganske strategisk
            line = NextFromPool(ref cvStrategicIndex, cvVeryStrategicLines);
            PlaySigh();   // <-- very strategic CV sigh
        }

        Debug.Log($"[MONOLOG] authenticityCost={authenticityCost} → line='{line}'");
        ShowLine(line);
    }

    /// <summary>
    /// Call this når spilleren åpner en e-post og energi/autentisitet akkurat er oppdatert.
    /// </summary>
    public void OnEmailOpened(ApplicationData app, float oldEnergy, float newEnergy, float authenticityCost)
    {
        if (app == null) return;

        bool gotCallback = app.gotCallback;
        bool lowAuth = authenticityCost > 15f;

        string line;

        // Første respons er alltid avslag nå → prioriter "første avslag"-replikken
        if (!gotCallback && !hasShownFirstRejectionLine)
        {
            line = NextFromPool(ref firstRejectionIndex, firstRejectionLines);
            hasShownFirstRejectionLine = true;

            PlaySigh();         // <-- first rejection sigh
            ShowLine(line);
            return;
        }

        // Alle senere e-poster: bruk vanlig logikk, men med syklus
        if (gotCallback && !lowAuth)
        {
            line = NextFromPool(ref callbackHighIndex, callbackHighAuthLines);
            // no sigh, this is good news
        }
        else if (gotCallback && lowAuth)
        {
            line = NextFromPool(ref callbackLowIndex, callbackLowAuthLines);
        }
        else if (!gotCallback && !lowAuth)
        {
            line = NextFromPool(ref rejectionHighIndex, rejectionHighAuthLines);
            PlaySigh();         // <-- rejection sigh
        }
        else
        {
            line = NextFromPool(ref rejectionLowIndex, rejectionLowAuthLines);
            PlaySigh();         // <-- rejection sigh
        }

        ShowLine(line);
    }

    private void PlaySigh()
    {
        if (!sighEvent.IsNull)
        {
            RuntimeManager.PlayOneShot(sighEvent);  // 2D UI sound
        }
    }
}
