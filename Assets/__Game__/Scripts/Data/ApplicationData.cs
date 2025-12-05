using UnityEngine;

[System.Serializable]
public class ApplicationData
{
    public JobData job;                    // Which job 
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
    public string[] chosenLanguages;
    public string[] chosenHobbies;      
    public string[] chosenVolunteer;    
}