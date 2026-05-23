using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI; 

public class BeakerDropZone : MonoBehaviour, IDropHandler
{
    [Header("Main pH Slider Reference")]
    public Slider mainPHSlider; 

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null && mainPHSlider != null)
        {
            // 1. Check if the dropped item is a DraggableItem (Household chemicals)
            DraggableItem droppedChemical = eventData.pointerDrag.GetComponent<DraggableItem>();
            if (droppedChemical != null)
            {
                // Change the liquid's pH based on the chemical dropped
                mainPHSlider.value = droppedChemical.itemPHValue;
            }

            // 2. Check if the dropped item is Litmus Paper (Testing the current liquid)
            LitmusPaper droppedLitmus = eventData.pointerDrag.GetComponent<LitmusPaper>();
            if (droppedLitmus != null)
            {
                // Tell the litmus paper to test the CURRENT pH value of the beaker
                droppedLitmus.DipIntoLiquid(mainPHSlider.value);
            }
        }
    }
}