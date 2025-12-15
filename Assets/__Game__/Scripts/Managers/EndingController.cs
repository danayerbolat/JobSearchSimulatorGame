using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class EndingController : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI bodyText;
    public TextMeshProUGUI statsText;
    public Button quitButton;

    [Header("Ending Image")]
    public Image endingImage;          // optional – can be null
    public Sprite burnoutSprite;
    public Sprite hollowSuccessSprite;
    public Sprite stayedYourselfSprite;
    public Sprite inBetweenSprite;

    private void Start()
    {
        ShowEnding();

        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuitClicked);
    }

    private void ShowEnding()
    {
        if (titleText == null || bodyText == null || statsText == null)
            return;

        // Choose text + image based on EndingData.endingType
        switch (EndingData.endingType)
        {
            case EndingType.Burnout:
                titleText.text = "Slutt: Batteri 0 %";

                bodyText.text =
                    "Det begynte greit nok.\n" +
                    "Du var full av energi. Klar til å ta verden med storm.\n\n" +
                    "Etter hvert ble det bare flere faner, flere skjema, flere \"send\".\n"+
                    "Og færre svar.\n\n" +
                    "Nå sitter du og stirrer på skjermen.\n" +
                    "Du vet du burde skrive én søknad til.\n" +
                    "Men hjernen din har sagt opp for tre dager siden uten å gi beskjed.\n\n" +
                    "Du lukker laptopen.\n" +
                    "Den får ligge.\n" +
                    "Du trenger en pause.";

                SetEndingSprite(burnoutSprite);
                break;

            case EndingType.HollowSuccess:
                titleText.text = "Slutt: \"Riktig kandidat\"";

                bodyText.text =
                    "Du fikk faktisk noen intervjuer!\n" +
                    "Gratulerer til deg og CV_endelig_endelig_siste_2.pdf.\n\n" +
                    "Du ser på siste utgaven:\n" +
                    "– Nytt navn (overraskende lett å endre, forresten)\n" +
                    "– Trygt bilde\n" +
                    "– Hobbyer du er bare sånn halvveis glad i" +
                    "– Null tegn til hvem du egentlig er\n\n" +
                    "Folk sier ting som \"du passer godt inn hos oss\" og at du har en \"veldig profesjonell profil\".\n" +
                    "De mente sikkert bare godt.\n\n" +
                    "Det føles... fint. Litt rart. Men fint.\n" +
                    "Du vet bare ikke helt om det er deg de vil ha,\n" +
                    "eller versjonen som passet inn i skjemaet.\n\n" +
                    "Jobbmulighetene er i hvert fall nærmere nå.\n" +
                    "Kanksje det er godt nok?\n";

                SetEndingSprite(hollowSuccessSprite);
                break;

            case EndingType.StayedYourself:
                titleText.text = "Slutt: Fremdeles deg";

                bodyText.text =
                    "Innboksen er ikke akkurat full av suksesshistorier.\n" +
                    "Noen robotsvar. Noen høflige avslag. Mye ingenting.\n\n" +
                    "Men du kikker gjennom CV-versjonene likevel.\n" +
                    "Navnet ditt står der, som du faktisk heter.\n" +
                    "Språkene du kan. Vervene du tok fordi du brydde deg.\n\n" +
                    "Du fikk ikke napp denne runden.\n" +
                    "Det suger. Og det er litt skummelt.\n\n" +
                    "Men når du leser CV-en, kjenner du i det minste igjen personen på arket.\n" +
                    "Det må være verdt noe. Håper du.";

                SetEndingSprite(stayedYourselfSprite);
                break;

            case EndingType.InBetween:
            default:
                titleText.text = "Slutt: Et sted midt imellom";

                bodyText.text =
                    "Du endte et sted i midten.\n" +
                    "Noen søknader var safe og 'etter boka'.\n" +
                    "Andre var mer... deg.\n\n" +
                    "Ikke alle gjorde inntrykk, verken på deg eller arbeidsgiverne.\n\n" +
                    "Du fikk noen nei, noen \"vi går videre med andre kandidater\",\n" +
                    "og kanskje et par intervjuinnkallinger.\n\n" +
                    "Du har ikke redigert bort alt av deg selv.\n" +
                    "Men du har heller ikke stått helt stille.\n\n" +
                    "Laptopen står fortsatt på pultet.\n" +
                    "Du kommer til å åpne den igjen.\n" +
                    "Neste gang vet du i det minste litt mer\n" +
                    "om hva du vil justere, og hva du vil beholde.";

                SetEndingSprite(inBetweenSprite);
                break;
            case EndingType.AuthenticSuccess:
                titleText.text = "Slutt: Kanskje det er ikke så ille som man tror?";

                bodyText.text =
                     "Det har ikke vært enkelt.\n\n" +
                     "Du flere avslag enn intervjuer.\n" +
                     "Mange kvelder med tvil.\n\n" +
                     "Men du holdt på deg selv.\n" +
                     "Navnet ditt. Bakgrunnen din. Tingene som gjør deg til deg.\n\n" +
                     "Og noen så det.\n" +
                     "Noen så deg, og ville ha akkurat deg.\n\n" +
                     "Det tok tid. Det var sårbart.\n" +
                     "Men du kom deg dit uten å gjemme bort hvem du er.\n\n" +
                     "Det er noe.";
                break;
        }

        const int selfImageMax = 100;

        statsText.text =
            $"Søknader sendt: {EndingData.totalApplications}\n" +
            $"Intervju-innkallinger: {EndingData.totalCallbacks}\n" +
            $"Avslag: {EndingData.totalRejections}\n" +
            $"Slutt-energi: {Mathf.RoundToInt(EndingData.finalEnergy)}\n" +
            $"Autentisitet: {Mathf.RoundToInt(EndingData.finalAuthenticity)} / {selfImageMax}";
    }

    private void SetEndingSprite(Sprite sprite)
    {
        if (endingImage == null)
            return;

        if (sprite != null)
        {
            endingImage.sprite = sprite;
            endingImage.gameObject.SetActive(true);
        }
        else
        {
            endingImage.gameObject.SetActive(false);
        }
    }

    private void OnQuitClicked()
    {
        // In a built game this will close the application.
        Application.Quit();

        // Optional: so it also stops play mode in the editor:
        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}
