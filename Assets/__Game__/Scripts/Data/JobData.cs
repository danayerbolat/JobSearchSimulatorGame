using UnityEngine;

[CreateAssetMenu(fileName = "New Job", menuName = "Game Data/Job Posting")]
public class JobData : ScriptableObject  // ← ScriptableObject!
{
    [Header("Job Info")]
    public string companyName;
    public string jobTitle;
    [TextArea(3, 6)]
    public string jobDescription;

    [Header("Job Culture (affects scoring)")]
    [Tooltip("Startup, Traditional Norwegian, International Corp, etc.")]
    public string companyType;

    [Header("Success Rates (will implement scoring later)")]
    [Range(0f, 1f)]
    public float authenticCVSuccessRate = 0.1f;
    [Range(0f, 1f)]
    public float strategicCVSuccessRate = 0.5f;
}