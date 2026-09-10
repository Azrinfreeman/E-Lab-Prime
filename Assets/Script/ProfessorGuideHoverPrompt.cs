using UnityEngine;
using UnityEngine.EventSystems;

public class ProfessorGuideHoverPrompt : MonoBehaviour, IPointerEnterHandler, ISelectHandler
{
    public MainMenuProfessorGuide guide;
    public ProfessorGuideContext context;

    public void OnPointerEnter(PointerEventData eventData)
    {
        ShowPrompt();
    }

    public void OnSelect(BaseEventData eventData)
    {
        ShowPrompt();
    }

    private void ShowPrompt()
    {
        if (guide == null)
        {
            guide = FindAnyObjectByType<MainMenuProfessorGuide>(FindObjectsInactive.Include);
        }

        if (guide != null)
        {
            guide.PlayPrompt(context);
        }
    }
}
