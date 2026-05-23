using UnityEngine;
using UnityEngine.EventSystems;

public class GasLabelDragUI : MonoBehaviour, IPointerDownHandler, IDragHandler, IEndDragHandler
{
    [Header("Target & Manager References")]
    public RectTransform correctTargetRect; 
    public ElectrolysisManager electrolysisManager;
    
    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    
    private Vector2 originalPosition;
    private Transform originalParent; // Added to remember the Layout Group parent
    private bool isPlacedCorrectly = false; 

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        
        // Save original parent and position
        originalParent = transform.parent;
        originalPosition = rectTransform.anchoredPosition;
        
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) 
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!isPlacedCorrectly)
        {
            // Move the button out of the Layout Group and into the main Canvas
            transform.SetParent(canvas.transform, true);
            transform.SetAsLastSibling();
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isPlacedCorrectly)
        {
            rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
            canvasGroup.blocksRaycasts = false; 
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (isPlacedCorrectly) return;
        
        canvasGroup.blocksRaycasts = true;

        if (RectTransformUtility.RectangleContainsScreenPoint(correctTargetRect, Input.mousePosition, eventData.pressEventCamera))
        {
            isPlacedCorrectly = true;
            
            // Snap to target center
            transform.position = correctTargetRect.position;
            
            electrolysisManager.LabelPlacedCorrectly();
        }
        else
        {
            // Return to the original Layout Group parent and reset position
            transform.SetParent(originalParent, true);
        }
    }
}