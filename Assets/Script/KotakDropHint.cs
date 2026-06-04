using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class KotakDropHint : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    private Image imejKotak;
    public Color warnaAsal = new Color(1f, 1f, 1f, 0.3f); // Putih pudar
    public Color warnaHint = new Color(0.6f, 1f, 0.6f, 0.8f); // Hijau terang

    void Start()
    {
        imejKotak = GetComponent<Image>();
        if (imejKotak != null) imejKotak.color = warnaAsal;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null && imejKotak != null)
        {
            imejKotak.color = warnaHint; // Menyala bila label lalu di atasnya
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (imejKotak != null) imejKotak.color = warnaAsal; // Padam bila label keluar
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (imejKotak != null) imejKotak.color = warnaAsal; // Padam bila label dilepaskan
    }
}