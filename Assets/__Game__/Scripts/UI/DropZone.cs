using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;

public class DropZone : MonoBehaviour, IDropHandler
{
    [Header("Drop Zone Info")]
    public string acceptedType;
    public bool allowMultiple = false; // Can hold multiple items?

    [Header("Display")]
    public TextMeshProUGUI displayText;
    [Tooltip("Custom label to show in placeholder text. E.g., 'NAVN', 'FOTO', 'PROFIL'")]
    public string customDisplayLabel = ""; // If empty, will use acceptedType

    [Header("Image Display (optional)")]
    public bool useImageForValue = false; // tick this for the photo field
    public Image targetImage;             // Image inside [Drop photo here] frame

    [Header("Current Value")]
    public string currentValue = "";
    private DraggableOption currentOption = null; // Track which option is currently used

    [Header("Text Formatting")]
    public bool adjustFontSizeOnDrop = false; // turn this on only for Profil
    public float droppedFontSize = 18f;       // smaller size for the dropped text

    private float originalFontSize;

    private void Awake()
    {
        if (displayText != null)
        {
            originalFontSize = displayText.fontSize;
        }
    }

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
        // If something was already here, return it
        if (currentOption != null && currentOption != option && !allowMultiple)
        {
            currentOption.MarkAsUnused();
            currentOption = null;
        }

        currentValue = option.optionValue;
        currentOption = option;

        // PHOTO-style zone
        if (useImageForValue && targetImage != null && option.displaySprite != null)
        {
            // show sprite
            targetImage.sprite = option.displaySprite;
            targetImage.enabled = true;

            // hide placeholder text
            if (displayText != null)
            {
                displayText.text = "";
                // we don't care about font size here
            }
        }
        // NORMAL text zone (Profil, Name, etc.)
        else if (displayText != null)
        {
            displayText.text = option.optionValue;

            if (adjustFontSizeOnDrop)
            {
                displayText.fontSize = droppedFontSize;
                displayText.alignment = TextAlignmentOptions.Left;
                displayText.lineSpacing = -30f;
            }
        }

        option.MarkAsUsed();

        Debug.Log($"Dropped {option.optionValue} into {acceptedType} field");
    }

    public bool IsFilled()
    {
        return !string.IsNullOrEmpty(currentValue);
    }

    public void Clear()
    {
        if (currentOption != null)
        {
            currentOption.MarkAsUnused();
            currentOption = null;
        }

        currentValue = "";

        // reset image if this zone uses one
        if (useImageForValue && targetImage != null)
        {
            targetImage.sprite = null;
            targetImage.enabled = false;
        }

        if (displayText != null)
        {
            // Use custom label if provided, otherwise use acceptedType
            string label = string.IsNullOrEmpty(customDisplayLabel) ? acceptedType : customDisplayLabel;
            displayText.text = $"DRA INN {label} HER";

            if (adjustFontSizeOnDrop)
            {
                displayText.fontSize = originalFontSize;
            }
        }
    }

    public void ClearDropZone()
    {
        Clear();
    }
}