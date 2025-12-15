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
    public float authenticityCost = 0f;    // Logs authenticity cost of CV

    // Constructor for easy creation
    public ApplicationData(JobData jobData, CVChoices cv, int month)
    {
        job = jobData;
        cvChoices = cv;
        submittedMonth = month;
        responseReceived = false;
        gotCallback = false;
        successChance = 0f;
        authenticityCost = 0f;
    }
}

[System.Serializable]
public class CVChoices
{
    // Single choices
    public string chosenName;           
    public string chosenPhoto;          
    public string chosenCoverLetter;    

    // Multiple choices (arrays)
    public string[] chosenLanguages;
    public string[] chosenHobbies;      
    public string[] chosenVolunteer;    
}