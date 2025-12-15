using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[System.Serializable]
public class IntroPage
{
    [TextArea(3, 6)]
    public string text;
    public Sprite image; // optional – leave empty if no image on this page
}

public class IntroController : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI introText;
    public Image introImage;          // <- assign an Image in the Canvas
    public Button nextButton;

    [Header("Scenes")]
    public string gameSceneName = "GameScene"; // set this to your main desktop scene

    [Header("Pages")]
    public IntroPage[] pages;         // set up pages in Inspector

    [Header("Typing Effect")]
    public float charDelay = 0.02f;   // time between characters
    public float lineDelay = 0.3f;    // pause between lines

    private int currentPage = 0;

    private Coroutine typingCoroutine;
    private bool isTyping = false;
    private string currentPageText = "";

    private void Start()
    {
        // If no pages set in Inspector, fall back to a default script
        if (pages == null || pages.Length == 0)
        {
            pages = new IntroPage[4];

            pages[0] = new IntroPage
            {
                text = "Et spill om å søke jobb når du føler at du må bevise noe ingen andre trenger å bevise.\n\n" +
                       "Om å redigere deg selv\n" +
                       "– ikke bare for å virke profesjonell, men for å passe inn.\n\n" +
                       "Om å lure på om det var kvalifikasjonene dine som ikke holdt\n" +
                       "– eller om det var noe helt annet."
            };
            pages[1] = new IntroPage
            {
                text = "Du heter Aikerim (Aika, for de som ikke helt klarer å uttale det riktig) Mukhan.\n" +
                       "24 år, fersk informatikk-bachelor fra NTNU.\n\n" +
                       "Studentlivet: ferdig.\n" +
                       "Husleia: løpende.\n" +
                       "Foreldrene: forventningsfulle.\n" +
                       "Jobb: ...jobber med saken."
            };

            pages[2] = new IntroPage
            {
                text = "Du åpner CV-en. Kikker gjennom.\n" +
                       "Navn. Bilde. Språk. Hobbyer. Verv.\n" +
                       "Alt kan justeres.\n\n" +
                       "Hver søknad tar energi.\n" +
                       "Hver endring i hvem du er tar noe annet.\n\n" +
                       "Spørsmålet er bare:\n" +
                       "Hvor mye skal du endre før det føles ut som du søker jobb for noen andre?\n"
            };

            pages[3] = new IntroPage
            {
                text = "PC-en er åpen.\n" +
                       "Jobbannonsene venter.\n" +
                       "Du er klar. Kinda."
            };
        }

        if (nextButton != null)
            nextButton.onClick.AddListener(OnNextClicked);

        ShowPage(0);
    }

    private void ShowPage(int index)
    {
        if (pages == null || pages.Length == 0)
            return;

        currentPage = Mathf.Clamp(index, 0, pages.Length - 1);
        IntroPage page = pages[currentPage];

        currentPageText = page.text;

        // Stop any previous typing
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        // Reset text and start typing effect
        if (introText != null)
        {
            introText.text = "";
        }

        // Update button label (text)
        if (nextButton != null)
        {
            var label = nextButton.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null)
            {
                label.text = (currentPage == pages.Length - 1) ? "Start" : "Fortsett";
            }

            // Disable button while typing
            nextButton.interactable = false;
        }

        // Start typewriter coroutine
        typingCoroutine = StartCoroutine(TypePageText(currentPageText));
    }

    private System.Collections.IEnumerator TypePageText(string fullText)
    {
        isTyping = true;

        if (introText == null)
            yield break;

        introText.text = "";

        // Split into lines so we can do "line by line" feel
        string[] lines = fullText.Split('\n');

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];

            // Type this line character by character
            for (int c = 0; c < line.Length; c++)
            {
                introText.text += line[c];
                yield return new WaitForSeconds(charDelay);
            }

            // After each line, add newline (even if it was empty)
            if (i < lines.Length - 1)
            {
                introText.text += "\n";
                yield return new WaitForSeconds(lineDelay);
            }
        }

        // Done typing
        isTyping = false;

        if (nextButton != null)
            nextButton.interactable = true;
    }

    private void OnNextClicked()
    {
        // If still typing, skip animation and show full text instantly
        if (isTyping)
        {
            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);

            isTyping = false;

            if (introText != null)
                introText.text = currentPageText;

            if (nextButton != null)
                nextButton.interactable = true;

            return;
        }

        // Otherwise, move on
        if (currentPage < pages.Length - 1)
        {
            ShowPage(currentPage + 1);
        }
        else
        {
            SceneManager.LoadScene(gameSceneName);
        }
    }
}
