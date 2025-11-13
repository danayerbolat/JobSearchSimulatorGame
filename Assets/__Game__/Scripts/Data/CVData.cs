using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New CV", menuName = "Game Data/CV Data")]
public class CVData : ScriptableObject  // ← ScriptableObject!
{
    [Header("Identity")]
    public string versionName;
    public string displayName;
    
    [Header("Content")]
    [TextArea(3, 10)]
    public string education;
    
    [TextArea(3, 10)]
    public string experience;
    
    public List<string> skills = new List<string>();
    public List<string> hobbies = new List<string>();
    public List<string> languages = new List<string>();
    
    [Header("Presentation")]
    public bool includePhoto;
    
    [Header("Stats")]
    [Range(0, 100)]
    public float authenticityValue;
    
    [Range(0, 100)]
    public float norwegianFitScore;
}