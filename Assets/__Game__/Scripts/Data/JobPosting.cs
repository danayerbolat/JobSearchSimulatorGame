using UnityEngine;

[CreateAssetMenu(fileName = "New Job", menuName = "Game Data/Job Posting")]
public class JobPosting : ScriptableObject  // ← ScriptableObject!
{
    [Header("Company Info")]
    public string companyName;
    public string jobTitle;

    [Header("Job Description")]
    [TextArea(5, 15)]
    public string fullDescription;

    [Header("Culture Signals")]
    [TextArea(3, 5)]
    public string cultureDescription;

    [Header("Success Rates per CV Type")]
    [Range(0f, 1f)]
    public float realCVChance = 0.3f;

    [Range(0f, 1f)]
    public float safeCVChance = 0.7f;

    [Range(0f, 1f)]
    public float diverseCVChance = 0.5f;
}