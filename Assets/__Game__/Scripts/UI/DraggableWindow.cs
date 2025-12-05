using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggableWindow : MonoBehaviour, IDragHandler, IPointerDownHandler
{
    [Header("Window Components")]
    public RectTransform windowRect;
    public RectTransform dragHandle; // The header bar that you drag

    [Header("Window Buttons")]
    public Button closeButton;
    public Button minimizeButton;

    private Canvas canvas;

    private void Awake()
    {
        if (windowRect == null)
            windowRect = GetComponent<RectTransform>();

        canvas = GetComponentInParent<Canvas>();

        // Setup button listeners
        if (closeButton != null)
            closeButton.onClick.AddListener(CloseWindow);

        if (minimizeButton != null)
            minimizeButton.onClick.AddListener(MinimizeWindow);
    }

    // When clicking anywhere on window, bring to front
    public void OnPointerDown(PointerEventData eventData)
    {
        BringToFront();
    }

    // Drag the window by the header
    public void OnDrag(PointerEventData eventData)
    {
        // Only allow dragging if clicking on the drag handle
        if (dragHandle != null &&
            RectTransformUtility.RectangleContainsScreenPoint(dragHandle, eventData.position, eventData.pressEventCamera))
        {
            windowRect.anchoredPosition += eventData.delta / canvas.scaleFactor;
        }
    }

    // Bring this window to front (render on top)
    public void BringToFront()
    {
        transform.SetAsLastSibling();
    }

    // Open/show the window
    public void OpenWindow()
    {
        gameObject.SetActive(true);
        BringToFront();

        ResetWindow();
    }

    // Close/hide the window
    public void CloseWindow()
    {
        gameObject.SetActive(false);
    }

    // Minimize (for now just closes, can extend later)
    public void MinimizeWindow()
    {
        gameObject.SetActive(false);
    }

    // Check if this window has a resettable component
    private void ResetWindow()
    {
        IResettable resettable = GetComponent<IResettable>();
        if (resettable != null)
        {
            resettable.ResetState();
        }
    }
}