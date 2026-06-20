using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class TooltipHover3D : MonoBehaviour
{
    public string tooltipText = "Tooltip";
    private static GameObject tooltipUI;
    private static TextMeshProUGUI textMesh;
    private static RectTransform backgroundRect;
    private static CanvasGroup canvasGroup;
    
    private bool isHovering = false;

    private void Start()
    {
        if (tooltipUI == null)
        {
            CreateTooltipUI();
        }
    }

    private void CreateTooltipUI()
    {
        // Create Canvas
        GameObject canvasObj = new GameObject("TooltipCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        // Removed GraphicRaycaster to ensure it never intercepts 3D raycasts
        DontDestroyOnLoad(canvasObj);

        // Create Background
        tooltipUI = new GameObject("TooltipBackground");
        tooltipUI.transform.SetParent(canvasObj.transform, false);
        backgroundRect = tooltipUI.AddComponent<RectTransform>();
        Image bgImage = tooltipUI.AddComponent<Image>();
        bgImage.color = new Color(0.05f, 0.05f, 0.05f, 0.95f);
        
        // Add Outline
        Outline outline = tooltipUI.AddComponent<Outline>();
        outline.effectColor = new Color(0f, 0.8f, 1f, 1f); // Electric blue
        outline.effectDistance = new Vector2(1, -1);
        
        canvasGroup = tooltipUI.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0;
        canvasGroup.blocksRaycasts = false;

        // Create Text
        GameObject textObj = new GameObject("TooltipText");
        textObj.transform.SetParent(tooltipUI.transform, false);
        textMesh = textObj.AddComponent<TextMeshProUGUI>();
        textMesh.color = Color.white;
        textMesh.fontSize = 14;
        textMesh.alignment = TextAlignmentOptions.Center;
        
        // Layout
        ContentSizeFitter fitter = tooltipUI.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        
        HorizontalLayoutGroup layout = tooltipUI.AddComponent<HorizontalLayoutGroup>();
        layout.padding = new RectOffset(8, 8, 4, 4);
        layout.childControlHeight = true;
        layout.childControlWidth = true;
        
        tooltipUI.SetActive(false);
    }

    private void OnMouseEnter()
    {
        // If pointer is over UI, we might want to skip, but sometimes it's over the drag area.
        // We will just show it anyway, as it's helpful.
        isHovering = true;
        if (tooltipUI != null)
        {
            tooltipUI.SetActive(true);
            textMesh.text = tooltipText;
            canvasGroup.alpha = 1f;
        }
    }

    private void OnMouseExit()
    {
        isHovering = false;
        if (tooltipUI != null)
        {
            tooltipUI.SetActive(false);
        }
    }

    private void OnDisable()
    {
        if (isHovering && tooltipUI != null)
        {
            tooltipUI.SetActive(false);
            isHovering = false;
        }
    }

    private void Update()
    {
        if (isHovering && tooltipUI != null && tooltipUI.activeSelf)
        {
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                backgroundRect.parent as RectTransform, 
                Input.mousePosition, 
                null, 
                out localPoint);
            
            // Add offset so it doesn't cover mouse
            localPoint.x += backgroundRect.rect.width / 2f + 15f;
            localPoint.y -= backgroundRect.rect.height / 2f + 15f;
            
            backgroundRect.localPosition = localPoint;
        }
    }
}
