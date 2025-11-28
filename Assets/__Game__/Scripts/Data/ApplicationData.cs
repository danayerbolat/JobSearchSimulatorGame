using UnityEngine;

[System.Serializable]
public class ApplicationData
{
    public JobData job;                    // Which job (using JobData, not JobData)
    public CVChoices cvChoices;            // Detailed CV choices
    public int submittedMonth;             // When submitted
    public bool responseReceived = false;  // Has response been generated
    public bool gotCallback = false;       // Did they get callback?
    public float successChance = 0f;       // Calculated success chance

    // Constructor for easy creation
    public ApplicationData(JobData jobData, CVChoices cv, int month)
    {
        job = jobData;
        cvChoices = cv;
        submittedMonth = month;
        responseReceived = false;
        gotCallback = false;
        successChance = 0f;
    }
}

[System.Serializable]
public class CVChoices
{
    // Single choices
    public string chosenName;           // "Zhanna Bolatkhan" or "Joanna Bolat"
    public string chosenPhoto;          // "Casual" / "Professional" / "None"
    public string chosenCoverLetter;    // "International" / "Neutral" / "Norwegian"

    // Multiple choices (arrays)
    public string[] chosenLanguages;    // ["Norwegian", "English", "Mandarin"]
    public string[] chosenHobbies;      // ["Crochet", "Video Games", "Skiing"]
    public string[] chosenVolunteer;    // ["Immigrant org", "Tech student org"]
}

// OLD ENUM - Keeping for any legacy code, but not using anymore
public enum CVType
{
    Real,
    Safe,
    Diverse
}