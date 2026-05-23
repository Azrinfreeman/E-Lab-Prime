using UnityEngine;
using UnityEngine.EventSystems;

public class SaltDragUI : MonoBehaviour, IPointerDownHandler, IDragHandler, IEndDragHandler
{
    [Header("Object References")]
    public RectTransform beakerBRect; 
    public WaterHeatingManager heatingManager; 
    
    [Header("Animation Settings")]
    public float pourAngle = -45f; 

    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    
    private Vector2 originalPosition;
    private Quaternion originalRotation; 

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        
        // Store original position and rotation for the Reset function
        originalPosition = rectTransform.anchoredPosition;
        originalRotation = rectTransform.localRotation;
        
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) 
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // Tilt the salt bottle when clicked
        rectTransform.localRotation = Quaternion.Euler(0, 0, pourAngle);
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Move the UI element with the mouse
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
        
        // Disable raycasts so the pointer can detect the beaker behind the salt
        canvasGroup.blocksRaycasts = false; 
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // Re-enable raycasts
        canvasGroup.blocksRaycasts = true;
        
        // Reset the rotation to upright when released
        rectTransform.localRotation = originalRotation;

        // Check if dropped inside Beaker B's RectTransform
        if (RectTransformUtility.RectangleContainsScreenPoint(beakerBRect, Input.mousePosition, eventData.pressEventCamera))
        {
            heatingManager.AddImpurities();
            gameObject.SetActive(false); // Hide the salt bottle
        }
        else
        {
            // Snap back to original position if dropped outside
            rectTransform.anchoredPosition = originalPosition;
        }
    }
    
    public void ResetPosition()
    {
        rectTransform.anchoredPosition = originalPosition;
        rectTransform.localRotation = originalRotation; 
        gameObject.SetActive(true);
    }
}