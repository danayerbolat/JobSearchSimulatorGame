using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class DropZone : MonoBehaviour, IDropHandler
{
    [Header("Drop Zone Info")]
    public string acceptedType;
    public bool allowMultiple = false; // NEW: Can hold multiple items?

    [Header("Display")]
    public TextMeshProUGUI displayText;

    [Header("Current Value")]
    public string currentValue = "";
    private DraggableOption currentOption = null; // NEW: Track which option is currently used

    public void OnDrop(PointerEventData eventData)
    {
        DraggableOption draggable = eventData.pointerDrag.GetComponent<DraggableOption>();

        if (draggable != null)
        {
            if (draggable.optionType == acceptedType)
            {
                AcceptOption(draggable);
            }
            else
            {
                Debug.Log($"Can't drop {draggable.optionType} into {acceptedType} field");
            }
        }
    }

    private void AcceptOption(DraggableOption option)
    {
        // If something was already here, return it to options panel
        if (currentOption != null && currentOption != option && !allowMultiple)
        {
            // IMPORTANT: Clear the old option's state completely
            currentOption.MarkAsUnused();
            currentOption = null; // Clear reference before setting new one
        }

        // Update to new option
        currentValue = option.optionValue;
        currentOption = option;

        if (displayText != null)
        {
            displayText.text = option.optionValue;
        }

        // Hide the new option from options panel
        option.MarkAsUsed();

        Debug.Log($"Dropped {option.optionValue} into {acceptedType} field");
    }

    public bool IsFilled()
    {
        return !string.IsNullOrEmpty(currentValue);
    }

    public void Clear()
    {
        // If something is here, return it
        if (currentOption != null)
        {
            currentOption.MarkAsUnused();
            currentOption = null;
        }

        currentValue = "";
        if (displayText != null)
        {
            displayText.text = $"[Drop {acceptedType} here]";
        }
    }
}