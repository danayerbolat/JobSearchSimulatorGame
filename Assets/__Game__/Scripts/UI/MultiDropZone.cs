using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;

public class MultiDropZone : MonoBehaviour, IDropHandler
{
    [Header("Drop Zone Info")]
    public string acceptedType;
    public int maxItems = 5; // Maximum items that can be dropped here

    [Header("Display")]
    public TextMeshProUGUI displayText;
    public Transform itemContainer; // Where to show individual items

    [Header("Current Values")]
    private List<string> currentValues = new List<string>();
    private List<DraggableOption> currentOptions = new List<DraggableOption>();

    private void Start()
    {
        UpdateDisplay();
    }

    public void OnDrop(PointerEventData eventData)
    {
        DraggableOption draggable = eventData.pointerDrag.GetComponent<DraggableOption>();

        if (draggable != null)
        {
            if (draggable.optionType == acceptedType)
            {
                // Check if already at max capacity
                if (currentOptions.Count >= maxItems)
                {
                    Debug.Log($"Maximum {maxItems} items allowed in this field");
                    return;
                }

                // Check if this option is already added
                if (currentOptions.Contains(draggable))
                {
                    Debug.Log("This item is already added");
                    return;
                }

                AddOption(draggable);
            }
            else
            {
                Debug.Log($"Can't drop {draggable.optionType} into {acceptedType} field");
            }
        }
    }

    private void AddOption(DraggableOption option)
    {
        // Add to lists
        currentValues.Add(option.optionValue);
        currentOptions.Add(option);

        // Hide from options panel
        option.MarkAsUsed();

        // Update display
        UpdateDisplay();

        Debug.Log($"Added {option.optionValue} to {acceptedType} field");
    }

    public void RemoveOption(DraggableOption option)
    {
        if (currentOptions.Contains(option))
        {
            currentOptions.Remove(option);
            currentValues.Remove(option.optionValue);

            // Show in options panel again
            option.MarkAsUnused();

            UpdateDisplay();
        }
    }

    private void UpdateDisplay()
    {
        if (displayText != null)
        {
            if (currentValues.Count == 0)
            {
                displayText.text = $"[Drop {acceptedType} here - can add multiple]";
            }
            else
            {
                // Show all values separated by comma
                displayText.text = string.Join(", ", currentValues);
            }
        }
    }

    public bool IsFilled()
    {
        return currentValues.Count > 0;
    }

    public List<string> GetValues()
    {
        return new List<string>(currentValues);
    }

    public void Clear()
    {
        // Return all options
        foreach (var option in currentOptions)
        {
            option.MarkAsUnused();
        }

        currentOptions.Clear();
        currentValues.Clear();
        UpdateDisplay();
    }
}