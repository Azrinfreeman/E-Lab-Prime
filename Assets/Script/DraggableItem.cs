using UnityEngine;
using UnityEngine.EventSystems; // Required for UI Drag & Drop

public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Specific pH Value for this Item")]
    public float itemPHValue = 7f; 

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector2 originalPosition;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        
        // CanvasGroup allows us to bypass raycasts (clicks) while dragging
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) 
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Save the original position so it can snap back later
        originalPosition = rectTransform.anchoredPosition;
        
        // Make the item slightly transparent and ignore raycasts while dragging
        canvasGroup.alpha = 0.7f;
        canvasGroup.blocksRaycasts = false; 
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Move the item based on mouse/touch position
        rectTransform.anchoredPosition += eventData.delta / GetComponentInParent<Canvas>().scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // Reset transparency and raycast blocking, then snap back to original position
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        rectTransform.anchoredPosition = originalPosition; 
    }
}