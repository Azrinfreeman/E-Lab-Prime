using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class LitmusPaper : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public enum LitmusType { Red, Blue }
    
    [Header("Select Paper Type")]
    public LitmusType paperType;

    private Image paperImage;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    
    private Vector2 originalPosition;
    private Color originalColor;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        paperImage = GetComponent<Image>();
        
        // Save the fresh color of the paper (Red or Blue)
        if (paperImage != null) originalColor = paperImage.color;

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalPosition = rectTransform.anchoredPosition;
        canvasGroup.alpha = 0.7f;
        canvasGroup.blocksRaycasts = false; 
        
        // Reset to its fresh original color whenever picked up again
        if (paperImage != null) paperImage.color = originalColor;
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / GetComponentInParent<Canvas>().scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        
        // Snap back to the shelf immediately after dropping
        rectTransform.anchoredPosition = originalPosition; 
    }

    // This function will be called by the Beaker when the paper is dropped on it
    public void DipIntoLiquid(float currentPH)
    {
        int roundedPH = Mathf.RoundToInt(currentPH);

        if (paperType == LitmusType.Red)
        {
            // Red litmus turns Blue in Alkali (pH > 7)
            if (roundedPH > 7) 
            {
                paperImage.color = Color.blue;
            }
        }
        else if (paperType == LitmusType.Blue)
        {
            // Blue litmus turns Red in Acid (pH < 7)
            if (roundedPH < 7) 
            {
                paperImage.color = Color.red;
            }
        }
        // If it's neutral (pH 7), colors do not change!
    }
}