using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggableOption : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Option Info")]
    public string optionType;
    public string optionValue;

    [Header("Optional Visual")]
    public Sprite displaySprite;   // <-- sprite to show in drop zone (e.g. photo)

    [Header("References")]
    private Canvas canvas;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Transform originalParent;
    private Vector2 originalAnchoredPosition; // Store exact position!

    [Header("State")]
    public bool isCurrentlyUsed = false;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        originalParent = transform.parent;
        originalAnchoredPosition = rectTransform.anchoredPosition; // STORE POSITION!
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;

        transform.SetParent(canvas.transform, true);
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;

        if (transform.parent == canvas.transform)
        {
            ReturnToOriginalParent();
        }
    }

    private void ReturnToOriginalParent()
    {
        transform.SetParent(originalParent, false);
        rectTransform.anchoredPosition = originalAnchoredPosition; // USE STORED POSITION!
    }

    public void MarkAsUsed()
    {
        isCurrentlyUsed = true;

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
        }

        gameObject.SetActive(false);
    }

    public void MarkAsUnused()
    {
        isCurrentlyUsed = false;

        if (transform.parent != originalParent)
        {
            transform.SetParent(originalParent, false);
            rectTransform.anchoredPosition = originalAnchoredPosition; // EXACT POSITION!
        }

        gameObject.SetActive(true);

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
        }
    }

    // Reset option to its original position and state
    public void ReturnToStart()
    {
        MarkAsUnused();
    }
}