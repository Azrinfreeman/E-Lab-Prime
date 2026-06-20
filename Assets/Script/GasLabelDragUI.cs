using UnityEngine;
using UnityEngine.EventSystems;

public class GasLabelDragUI : MonoBehaviour, IPointerDownHandler, IDragHandler, IEndDragHandler
{
    [Header("Target & Manager References")]
    public RectTransform correctTargetRect;  // Untuk 2D
    public Collider target3DCollider;        // Untuk 3D (Tarik TestTube ke sini)
    public ElectrolysisManager electrolysisManager;
    public ElectrolysisManager3D electrolysisManager3D; // Sokongan untuk versi 3D
    
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
        
        canvasGroup.blocksRaycasts = true; // IMPORTANT: Reset raycast blocking

        bool isCorrectHit = false;

        // 1. Periksa Untuk UI 2D Target
        if (correctTargetRect != null && RectTransformUtility.RectangleContainsScreenPoint(correctTargetRect, Input.mousePosition, eventData.pressEventCamera))
        {
            isCorrectHit = true;
            transform.position = correctTargetRect.position;
        }
        // 2. Periksa Untuk Objek 3D Target (Raycast)
        else if (target3DCollider != null && Camera.main != null)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider == target3DCollider)
                {
                    isCorrectHit = true;
                    // Tukar posisi 3D ke posisi Skrin, kemudian ke posisi Canvas
                    Vector3 screenPos = Camera.main.WorldToScreenPoint(target3DCollider.transform.position);
                    RectTransformUtility.ScreenPointToWorldPointInRectangle(canvas.transform as RectTransform, screenPos, canvas.worldCamera, out Vector3 worldPoint);
                    transform.position = worldPoint;
                }
            }
        }

        if (isCorrectHit)
        {
            isPlacedCorrectly = true;
            Debug.Log("Sistem: Label " + gameObject.name + " telah dilepaskan pada objek yang betul!");
            
            if (electrolysisManager != null) electrolysisManager.LabelPlacedCorrectly();
            if (electrolysisManager3D != null) electrolysisManager3D.LabelPlacedCorrectly();
            else Debug.LogWarning("Amaran: Label " + gameObject.name + " tidak dipautkan dengan ruangan Electrolysis Manager 3D di Inspector!");
        }
        else
        {
            // Return to the original Layout Group parent and reset position
            transform.SetParent(originalParent, true);
        }
    }
}