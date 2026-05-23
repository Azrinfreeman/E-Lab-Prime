using UnityEngine;
using UnityEngine.EventSystems;

public class CylinderDrag : MonoBehaviour, IPointerDownHandler, IDragHandler, IEndDragHandler
{
    [Header("References")]
    public RectTransform targetDropZone;
    public AirCompositionManager manager;

    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;

    private Vector2 originalPosition;
    private bool isLocked = false;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        originalPosition = rectTransform.anchoredPosition;

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // Kod transform.SetAsLastSibling() telah dibuang di sini
        // supaya silinder kekal pada susunan asalnya dalam Hierarchy.
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isLocked)
        {
            rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
            canvasGroup.blocksRaycasts = false;
        }
    }

    // Fungsi untuk menetapkan semula status dan posisi silinder
    public void ResetCylinder()
    {
        isLocked = false;
        rectTransform.anchoredPosition = originalPosition;
        if (canvasGroup != null)
            canvasGroup.blocksRaycasts = true;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (isLocked)
            return;

        canvasGroup.blocksRaycasts = true;

        if (
            RectTransformUtility.RectangleContainsScreenPoint(
                targetDropZone,
                Input.mousePosition,
                eventData.pressEventCamera
            )
        )
        {
            isLocked = true;
            transform.position = targetDropZone.position;

            manager.StartExperiment();
        }
        else
        {
            rectTransform.anchoredPosition = originalPosition;
        }
    }
}
