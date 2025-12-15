using System.Linq;   // add this at the top if it's not already there

public enum EndingType
{
    None,
    Burnout,
    HollowSuccess,
    StayedYourself,
    InBetween,
    AuthenticSuccess
}

public static class EndingData
{
    public static EndingType endingType = EndingType.None;

    public static int totalApplications;
    public static int totalCallbacks;
    public static int totalRejections;

    public static float finalEnergy;
    public static float finalAuthenticity;
    public static int finalMonth;

    /// <summary>
    /// Copy all end-of-run stats from the GameManager so EndingScene can read them.
    /// </summary>
    public static void ApplyFromGame(GameManager gm, EndingType type)
    {
        if (gm == null)
        {
            UnityEngine.Debug.LogWarning("EndingData.ApplyFromGame called with null GameManager");
            return;
        }

        endingType = type;

        // How many applications were actually sent
        totalApplications = gm.submittedApplications != null
            ? gm.submittedApplications.Count
            : 0;

        // Count callbacks and rejections across ALL emails (read + unread)
        if (gm.allEmails != null)
        {
            totalCallbacks = gm.allEmails.Count(a => a.responseReceived && a.gotCallback);
            totalRejections = gm.allEmails.Count(a => a.responseReceived && !a.gotCallback);
        }
        else
        {
            totalCallbacks = 0;
            totalRejections = 0;
        }

        // Final state
        finalEnergy = gm.emotionalEnergy;
        finalAuthenticity = gm.authenticityScore;   // you already track 0–100
        finalMonth = gm.currentMonth;
    }
}
