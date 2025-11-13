using UnityEngine;

[System.Serializable]
public class ApplicationData
{
    public JobPosting job;
    public CVType cvType;
    public int submittedWeek;
    public bool responseReceived = false;
    public float successChance; // Calculated when application submitted

    public ApplicationData(JobPosting jobPosting, CVType cv, int week)
    {
        job = jobPosting;
        cvType = cv;
        submittedWeek = week;
        responseReceived = false;

        // Calculate success chance based on CV type and job
        successChance = CalculateSuccessChance();
    }

    private float CalculateSuccessChance()
    {
        switch (cvType)
        {
            case CVType.Real:
                return job.realCVChance;
            case CVType.Safe:
                return job.safeCVChance;
            case CVType.Diverse:
                return job.diverseCVChance;
            default:
                return 0.3f;
        }
    }
}

public enum CVType
{
    Real,
    Safe,
    Diverse
}