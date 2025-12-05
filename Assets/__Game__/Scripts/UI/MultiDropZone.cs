using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

//MultiDropZone: accepts multiple DraggableOption items, shows them as removable "cards"
public class MultiDropZone : MonoBehaviour, IDropHandler
{
    [Header("Drop Zone Info")]
    public string acceptedType;
    public int maxItems = 5; // Maximum items that can be dropped here

    [Header("Display")]
    public TextMeshProUGUI displayText;
    public Transform itemContainer;           // Where to show individual items
    public GameObject cardPrefab_short;       // Narrow card (språk, hobbyer)
    public GameObject cardPrefab_long;        // Wider card (verv, long text)

    [Header("Card length logic")]
    [Tooltip("If optionValue length is greater than this and a long prefab exists, use the long card.")]
    public int longTextThreshold = 45;

    [Header("Current Values")]
    private List<string> currentValues = new List<string>();
    private List<DraggableOption> currentOptions = new List<DraggableOption>();

    // Map each option to its visual card so we can destroy it on remove
    private Dictionary<DraggableOption, GameObject> optionCards =
        new Dictionary<DraggableOption, GameObject>();

    private void Start()
    {
        UpdateDisplay();
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null) return;

        DraggableOption draggable = eventData.pointerDrag.GetComponent<DraggableOption>();
        if (draggable == null) return;

        if (draggable.optionType != acceptedType)
        {
            Debug.Log($"[MULTI DROP ZONE] Can't drop {draggable.optionType} into {acceptedType} field");
            return;
        }

        if (currentOptions.Count >= maxItems)
        {
            Debug.Log($"[MULTI DROP ZONE] Maximum {maxItems} items allowed in this field");
            return;
        }

        if (currentOptions.Contains(draggable))
        {
            Debug.Log("[MULTI DROP ZONE] This item is already added");
            return;
        }

        AddOption(draggable);
    }

    // Decide which prefab to use for this option
    private GameObject GetCardPrefabForOption(DraggableOption option)
    {
        if (option == null) return cardPrefab_short;

        string value = option.optionValue ?? "";

        // If we have a long prefab and the text is long, use it
        if (cardPrefab_long != null && value.Length > longTextThreshold)
            return cardPrefab_long;

        // Otherwise use the short one
        return cardPrefab_short;
    }

    private void AddOption(DraggableOption option)
    {
        currentValues.Add(option.optionValue);
        currentOptions.Add(option);

        option.MarkAsUsed();

        if (itemContainer != null)
        {
            // Decide which prefab to use
            GameObject prefabToUse = cardPrefab_short;

            // If long prefab assigned, use it when text is long (for VERV)
            if (cardPrefab_long != null && !string.IsNullOrEmpty(option.optionValue))
            {
                if (option.optionValue.Length > 45)   // tweak threshold as needed
                    prefabToUse = cardPrefab_long;
            }

            GameObject card = Instantiate(prefabToUse, itemContainer);

            TextMeshProUGUI label = card.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null)
            {
                label.text = option.optionValue;
                // Long cards: we want wrapping
                label.textWrappingMode = TextWrappingModes.Normal;
            }

            Button removeButton = card.GetComponentInChildren<Button>();
            if (removeButton != null)
            {
                DraggableOption capturedOption = option;
                removeButton.onClick.AddListener(() => RemoveOption(capturedOption));
            }

            optionCards[option] = card;

            var rt = itemContainer as RectTransform;
            if (rt != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
            }
        }

        UpdateDisplay();
        Debug.Log($"[MULTI DROP ZONE] Added {option.optionValue} to {acceptedType} field");
    }

    public void RemoveOption(DraggableOption option)
    {
        if (!currentOptions.Contains(option))
            return;

        currentOptions.Remove(option);
        currentValues.Remove(option.optionValue);

        option.MarkAsUnused();

        if (optionCards.TryGetValue(option, out GameObject card))
        {
            if (card != null)
                Destroy(card);

            optionCards.Remove(option);
        }

        UpdateDisplay();
        Debug.Log($"[MULTI DROP ZONE] Removed {option.optionValue} from {acceptedType} field");
    }

    private void UpdateDisplay()
    {
        if (displayText == null) return;

        if (currentValues.Count == 0)
        {
            displayText.text = $"[Drop {acceptedType} here - can add multiple]";
        }
        else
        {
            displayText.text = "";
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
        foreach (var option in currentOptions)
            option.MarkAsUnused();

        currentOptions.Clear();
        currentValues.Clear();

        foreach (var kvp in optionCards)
        {
            if (kvp.Value != null)
                Destroy(kvp.Value);
        }
        optionCards.Clear();

        UpdateDisplay();
    }

    public void ClearDropZone()
    {
        Clear();
        Debug.Log($"[MULTI DROP ZONE] {gameObject.name} cleared");
    }
}


//using UnityEngine;
//using UnityEngine.EventSystems;
//using UnityEngine.UI;
//using TMPro;
//using System.Collections.Generic;

////Important: Need to implememnt a way to remove individual items from the MultiDropZone
//public class MultiDropZone : MonoBehaviour, IDropHandler
//{
//    [Header("Drop Zone Info")]
//    public string acceptedType;
//    public int maxItems = 5; // Maximum items that can be dropped here

//    [Header("Display")]
//    public TextMeshProUGUI displayText;
//    public Transform itemContainer; // Where to show individual items
//    public GameObject cardPrefab; // Prefab for visual representation of each item

//    [Header("Current Values")]
//    private List<string> currentValues = new List<string>();
//    private List<DraggableOption> currentOptions = new List<DraggableOption>();

//    // Map each option to its visual card so we can destroy it on remove
//    private Dictionary<DraggableOption, GameObject> optionCards =
//        new Dictionary<DraggableOption, GameObject>();


//    private void Start()
//    {
//        UpdateDisplay();
//    }

//    public void OnDrop(PointerEventData eventData)
//    {
//        DraggableOption draggable = eventData.pointerDrag.GetComponent<DraggableOption>();

//        if (draggable == null)
//            return;

//        if (draggable.optionType != acceptedType)
//        {
//            Debug.Log($"[MULTI DROP ZONE] Can't drop {draggable.optionType} into {acceptedType} field");
//            return;
//        }

//        // Check if already at max capacity
//        if (currentOptions.Count >= maxItems)
//        {
//            Debug.Log($"[MULTI DROP ZONE] Maximum {maxItems} items allowed in this field");
//            return;
//        }

//        // Check if this option is already added
//        if (currentOptions.Contains(draggable))
//        {
//            Debug.Log("[MULTI DROP ZONE] This item is already added");
//            return;
//        }

//        AddOption(draggable);
//    }

//    private void AddOption(DraggableOption option)
//    {
//        // Add to lists
//        currentValues.Add(option.optionValue);
//        currentOptions.Add(option);

//        // Hide from options panel
//        option.MarkAsUsed();

//        // Create visual card if we have the setup
//        if (itemContainer != null && cardPrefab != null)
//        {
//            GameObject card = GameObject.Instantiate(cardPrefab, itemContainer);

//            // Set label text on the card
//            TextMeshProUGUI label = card.GetComponentInChildren<TextMeshProUGUI>();
//            if (label != null)
//            {
//                label.text = option.optionValue;
//                // avoid obsolete warning:
//                label.textWrappingMode = TextWrappingModes.NoWrap;
//            }

//            // Hook up the X button
//            Button removeButton = card.GetComponentInChildren<Button>();
//            if (removeButton != null)
//            {
//                DraggableOption capturedOption = option; // capture for closure
//                removeButton.onClick.AddListener(() =>
//                {
//                    RemoveOption(capturedOption);
//                });
//            }

//            optionCards[option] = card;
//        }

//        UpdateDisplay();

//        Debug.Log($"[MULTI DROP ZONE] Added {option.optionValue} to {acceptedType} field");
//    }

//    public void RemoveOption(DraggableOption option)
//    {
//        if (!currentOptions.Contains(option))
//            return;

//        currentOptions.Remove(option);
//        currentValues.Remove(option.optionValue);

//        // Show the original option back in the options panel
//        option.MarkAsUnused();

//        // Destroy the visual card if it exists
//        if (optionCards.TryGetValue(option, out GameObject card))
//        {
//            if (card != null)
//            {
//                Destroy(card);
//            }
//            optionCards.Remove(option);
//        }

//        UpdateDisplay();

//        Debug.Log($"[MULTI DROP ZONE] Removed {option.optionValue} from {acceptedType} field");
//    }

//    private void UpdateDisplay()
//    {
//        if (displayText == null)
//            return;

//        if (currentValues.Count == 0)
//        {
//            // show placeholder text when empty
//            displayText.text = $"[Drop {acceptedType} here - can add multiple]";
//        }
//        else
//        {
//            // if you prefer you can hide the text instead of listing values:
//            displayText.text = "";
//            // or if you want a summary as well:
//            // displayText.text = string.Join(", ", currentValues);
//        }
//    }

//    public bool IsFilled()
//    {
//        return currentValues.Count > 0;
//    }

//    public List<string> GetValues()
//    {
//        return new List<string>(currentValues);
//    }

//    public void Clear()
//    {
//        // Return all options to unused state
//        foreach (var option in currentOptions)
//        {
//            option.MarkAsUnused();
//        }

//        currentOptions.Clear();
//        currentValues.Clear();

//        // Destroy all existing cards
//        foreach (var kvp in optionCards)
//        {
//            if (kvp.Value != null)
//            {
//                Destroy(kvp.Value);
//            }
//        }
//        optionCards.Clear();

//        UpdateDisplay();
//    }

//    public void ClearDropZone()
//    {
//        Clear(); // Just call your existing Clear() method!
//        Debug.Log($"[MULTI DROP ZONE] {gameObject.name} cleared");
//    }
//}