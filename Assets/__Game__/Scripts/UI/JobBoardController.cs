using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
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
        // Populate job list
        PopulateJobList();
    }

    private void PopulateJobList()
    {
        // Clear any existing job cards
        foreach (Transform child in jobListContent)
        {
            Destroy(child.gameObject);
        }

        // Create a card for each job
        foreach (JobData job in availableJobs)
        {
            CreateJobCard(job);
        }
    }

    private void CreateJobCard(JobData job)
    {
        // Instantiate job card
        GameObject card = Instantiate(jobCardPrefab, jobListContent);

        // Find and set text elements
        TextMeshProUGUI companyName = card.transform.Find("CompanyName").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI jobTitle = card.transform.Find("JobTitle").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI jobDescription = card.transform.Find("JobDescription").GetComponent<TextMeshProUGUI>();
        Button applyButton = card.transform.Find("ApplyButton").GetComponent<Button>();

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
            applyButton.onClick.AddListener(() => ApplyToJob(job));
        }
    }

    private void ApplyToJob(JobData job)
    {
        Debug.Log($"Applying to: {job.jobTitle} at {job.companyName}");

        GameManager.Instance.currentJobApplyingTo = job;

        if (cvBuilderWindow != null)
        {
            cvBuilderWindow.OpenWindow();
        }
    }
}