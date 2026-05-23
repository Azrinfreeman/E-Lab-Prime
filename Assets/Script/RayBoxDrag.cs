using UnityEngine;
using UnityEngine.EventSystems;

public class RayBoxDrag : MonoBehaviour, IDragHandler
{
    private RectTransform rectTransform;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Dapatkan posisi tepat tetikus di skrin dan tukar ke posisi dunia (World Space)
        Vector3 globalMousePos;
        if (
            RectTransformUtility.ScreenPointToWorldPointInRectangle(
                rectTransform.parent as RectTransform,
                eventData.position,
                eventData.pressEventCamera,
                out globalMousePos
            )
        )
        {
            // Gerakkan kotak tepat ke koordinat tetikus
            rectTransform.position = globalMousePos;
        }
    }
}
