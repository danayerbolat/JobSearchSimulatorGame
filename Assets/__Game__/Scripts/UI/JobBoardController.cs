using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class JobBoardController : MonoBehaviour
{
    [Header("UI References")]
    public Transform jobListContent;
    public GameObject jobCardPrefab;

    [Header("Window References")]
    public DraggableWindow cvBuilderWindow;
    public DraggableWindow jobBoardWindow;

    [Header("Job Data")]
    public List<JobData> availableJobs; // Drag JobData assets here

    private void Start()
    {
        // Populate job list first time
        PopulateJobList();
    }

    // This will be called every time the window is re-opened
    private void OnEnable()
    {
        PopulateJobList();
    }

    private void PopulateJobList()
    {
        if (jobListContent == null)
            return;

        // Clear any existing job cards
        foreach (Transform child in jobListContent)
        {
            Destroy(child.gameObject);
        }

        if (availableJobs == null)
            return;

        // Create a card for each job
        foreach (JobData job in availableJobs)
        {
            if (HasAppliedToJob(job))
                continue; // skip this job entirely

            CreateJobCard(job);
        }
    }

    private void CreateJobCard(JobData job)
    {
        // Instantiate job card
        GameObject card = Instantiate(jobCardPrefab, jobListContent);

        // Find and set text elements
        TextMeshProUGUI companyName = card.transform.Find("CompanyName")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI jobTitle = card.transform.Find("JobTitle")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI jobDescription = card.transform.Find("JobDescription")?.GetComponent<TextMeshProUGUI>();
        Button applyButton = card.transform.Find("ApplyButton")?.GetComponent<Button>();

        // Set values
        if (companyName != null)
            companyName.text = job.companyName;

        if (jobTitle != null)
            jobTitle.text = job.jobTitle;

        if (jobDescription != null)
            jobDescription.text = job.jobDescription;

        // Set up button
        if (applyButton != null)
        {
            // Just in case PopulateJobList runs multiple times
            applyButton.onClick.RemoveAllListeners();

            // Have we already applied to this job?
            bool alreadyApplied = HasAppliedToJob(job);

            TextMeshProUGUI buttonLabel = applyButton.GetComponentInChildren<TextMeshProUGUI>();

            if (alreadyApplied)
            {
                // Disable and mark as "Søkt"
                applyButton.interactable = false;
                if (buttonLabel != null)
                    buttonLabel.text = "Søkt";
            }
            else
            {
                applyButton.interactable = true;
                if (buttonLabel != null)
                    buttonLabel.text = "Søk";

                // Capture the button in the closure if you ever want to tweak its visuals later
                applyButton.onClick.AddListener(() => ApplyToJob(job));
            }
        }
    }

    private bool HasAppliedToJob(JobData job)
    {
        if (GameManager.Instance == null ||
            GameManager.Instance.submittedApplications == null)
            return false;

        foreach (ApplicationData app in GameManager.Instance.submittedApplications)
        {
            if (app != null && app.job == job)
            {
                return true; // Same JobData asset → already applied
            }
        }

        return false;
    }

    private void ApplyToJob(JobData job)
    {
        // Safety net: don’t allow double-apply even if something goes weird with UI
        if (HasAppliedToJob(job))
        {
            Debug.Log($"[JOB BOARD] Already applied to {job.jobTitle} at {job.companyName}, ignoring click.");
            return;
        }

        Debug.Log($"[JOB BOARD] Applying to: {job.jobTitle} at {job.companyName}");

        GameManager.Instance.currentJobApplyingTo = job;

        if (cvBuilderWindow != null)
        {
            cvBuilderWindow.OpenWindow();
        }
    }
}

//using UnityEngine;
//using UnityEngine.UI;
//using UnityEngine.SceneManagement;
//using TMPro;
//using System.Collections.Generic;

//public class JobBoardController : MonoBehaviour
//{
//    [Header("UI References")]
//    public Transform jobListContent; 
//    public GameObject jobCardPrefab; 

//    [Header("Window References")]
//    public DraggableWindow cvBuilderWindow;
//    public DraggableWindow jobBoardWindow;

//    [Header("Job Data")]
//    public List<JobData> availableJobs; // Drag JobData assets here

//    private void Start()
//    {
//        // Populate job list
//        PopulateJobList();
//    }

//    private void PopulateJobList()
//    {
//        // Clear any existing job cards
//        foreach (Transform child in jobListContent)
//        {
//            Destroy(child.gameObject);
//        }

//        // Create a card for each job
//        foreach (JobData job in availableJobs)
//        {
//            CreateJobCard(job);
//        }
//    }

//    private void CreateJobCard(JobData job)
//    {
//        // Instantiate job card
//        GameObject card = Instantiate(jobCardPrefab, jobListContent);

//        // Find and set text elements
//        TextMeshProUGUI companyName = card.transform.Find("CompanyName").GetComponent<TextMeshProUGUI>();
//        TextMeshProUGUI jobTitle = card.transform.Find("JobTitle").GetComponent<TextMeshProUGUI>();
//        TextMeshProUGUI jobDescription = card.transform.Find("JobDescription").GetComponent<TextMeshProUGUI>();
//        Button applyButton = card.transform.Find("ApplyButton").GetComponent<Button>();

//        // Set values
//        if (companyName != null)
//            companyName.text = job.companyName;

//        if (jobTitle != null)
//            jobTitle.text = job.jobTitle;

//        if (jobDescription != null)
//            jobDescription.text = job.jobDescription;

//        // Set up button
//        if (applyButton != null)
//        {
//            applyButton.onClick.AddListener(() => ApplyToJob(job));
//        }
//    }

//    private void ApplyToJob(JobData job)
//    {
//        Debug.Log($"Applying to: {job.jobTitle} at {job.companyName}");

//        GameManager.Instance.currentJobApplyingTo = job;

//        if (cvBuilderWindow != null)
//        {
//            cvBuilderWindow.OpenWindow();
//        }
//    }
//}