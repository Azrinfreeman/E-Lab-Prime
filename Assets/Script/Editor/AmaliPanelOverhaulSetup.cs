using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class AmaliPanelOverhaulSetup
{
    [MenuItem("E-Lab/Setup/Overhaul Amali Panel")]
    public static void OverhaulAmaliPanel()
    {
        Canvas canvas = GameObject.FindFirstObjectByType<Canvas>();
        if (canvas == null) { Debug.LogError("No Canvas found"); return; }

        Transform amaliPanel = FindChildRecursive(canvas.transform, "AmaliPanel");
        if (amaliPanel == null) { Debug.LogError("AmaliPanel not found"); return; }

        // Clean root junk
        Transform oldHeaderTxt = amaliPanel.Find("Text (TMP)");
        if (oldHeaderTxt != null) GameObject.DestroyImmediate(oldHeaderTxt.gameObject);
        Transform oldHeaderBg = amaliPanel.Find("Image (1)");
        if (oldHeaderBg != null) GameObject.DestroyImmediate(oldHeaderBg.gameObject);
        Transform oldVert = amaliPanel.Find("Verticacl");
        if (oldVert != null) GameObject.DestroyImmediate(oldVert.gameObject);

        Transform mainContainer = amaliPanel.Find("Image");
        if (mainContainer == null) { Debug.LogError("MainContainer (Image) not found inside AmaliPanel"); return; }

        // Stretch main container to fill screen
        RectTransform mcRT = mainContainer.GetComponent<RectTransform>();
        if (mcRT != null) {
            mcRT.anchorMin = new Vector2(0.05f, 0.05f);
            mcRT.anchorMax = new Vector2(0.95f, 0.95f);
            mcRT.offsetMin = Vector2.zero;
            mcRT.offsetMax = Vector2.zero;
        }

        // Make MainContainer a dark glass panel, fully opaque
        Image mcImg = mainContainer.GetComponent<Image>();
        Sprite glassSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI Flat/pb_rect_rounded.png");
        Sprite nodeSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI Flat/node_icon.png");

        if (mcImg != null)
        {
            mcImg.sprite = glassSprite;
            mcImg.type = Image.Type.Sliced;
            mcImg.color = new Color(0.08f, 0.1f, 0.13f, 1f);
            mcImg.pixelsPerUnitMultiplier = 1.5f;
        }

        // Setup Main Container Layout correctly
        VerticalLayoutGroup mcLayout = mainContainer.GetComponent<VerticalLayoutGroup>();
        if (mcLayout == null) mcLayout = mainContainer.gameObject.AddComponent<VerticalLayoutGroup>();
        mcLayout.padding = new RectOffset(40, 40, 40, 40);
        mcLayout.spacing = 15;
        mcLayout.childControlHeight = true; // MUST BE TRUE to let LayoutElements control height
        mcLayout.childControlWidth = true;
        mcLayout.childForceExpandHeight = false; // Prevents Header from stretching
        mcLayout.childForceExpandWidth = true;

        // Find the 8 buttons and pull them out temporarily
        List<Transform> amaliButtons = new List<Transform>();
        for (int i = 1; i <= 8; i++)
        {
            Transform btn = FindChildRecursive(amaliPanel, "Amali" + i);
            if (btn != null)
            {
                amaliButtons.Add(btn);
                btn.SetParent(amaliPanel); // move out temporarily
            }
        }

        // Destroy ALL existing inner contents of MainContainer
        List<GameObject> toDestroy = new List<GameObject>();
        foreach (Transform child in mainContainer)
        {
            toDestroy.Add(child.gameObject);
        }
        foreach (GameObject obj in toDestroy)
        {
            GameObject.DestroyImmediate(obj);
        }

        // --- 1. HEADER ---
        GameObject headerObj = new GameObject("Header", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
        headerObj.transform.SetParent(mainContainer, false);
        HorizontalLayoutGroup hLayout = headerObj.GetComponent<HorizontalLayoutGroup>();
        hLayout.childAlignment = TextAnchor.LowerLeft;
        hLayout.childControlHeight = true;
        hLayout.childControlWidth = true;
        LayoutElement headerLE = headerObj.GetComponent<LayoutElement>();
        headerLE.minHeight = 50;
        headerLE.preferredHeight = 50;

        GameObject logoObj = new GameObject("Logo", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
        logoObj.transform.SetParent(headerObj.transform, false);
        Image logoImg = logoObj.GetComponent<Image>();
        logoImg.sprite = nodeSprite;
        logoImg.preserveAspect = true;
        LayoutElement logoLE = logoObj.GetComponent<LayoutElement>();
        logoLE.minWidth = 55; logoLE.preferredWidth = 55;
        logoLE.minHeight = 55; logoLE.preferredHeight = 55;

        GameObject titleObj = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement));
        titleObj.transform.SetParent(headerObj.transform, false);
        TextMeshProUGUI titleTxt = titleObj.GetComponent<TextMeshProUGUI>();
        titleTxt.text = " LEVEL SELECT";
        titleTxt.fontSize = 42;
        titleTxt.fontStyle = FontStyles.Bold;
        titleTxt.color = Color.white;
        titleTxt.alignment = TextAlignmentOptions.BottomLeft;
        titleTxt.enableWordWrapping = false;
        titleTxt.overflowMode = TextOverflowModes.Overflow;
        titleObj.GetComponent<LayoutElement>().flexibleWidth = 1; // takes remaining space

        GameObject statsObj = new GameObject("Stats", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement));
        statsObj.transform.SetParent(headerObj.transform, false);
        TextMeshProUGUI statsTxt = statsObj.GetComponent<TextMeshProUGUI>();
        statsTxt.text = "<color=#aaaaaa>CREDITS:</color> 1540   <color=#aaaaaa>LVL 9:</color> 8750 XP";
        statsTxt.fontSize = 26;
        statsTxt.fontStyle = FontStyles.Bold;
        statsTxt.color = Color.white;
        statsTxt.alignment = TextAlignmentOptions.BottomRight;
        statsTxt.enableWordWrapping = false;
        statsTxt.overflowMode = TextOverflowModes.Overflow;
        statsObj.GetComponent<LayoutElement>().minWidth = 350;
        
        // Attach Economy UI Updater
        if (statsObj.GetComponent<AmaliPanelStatsUI>() == null)
            statsObj.AddComponent<AmaliPanelStatsUI>();

        // --- 2. SEPARATOR ---
        GameObject separator = new GameObject("Separator", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
        separator.transform.SetParent(mainContainer, false);
        separator.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.15f);
        LayoutElement sepLE = separator.GetComponent<LayoutElement>();
        sepLE.minHeight = 2; // Strict 2px height
        sepLE.preferredHeight = 2;
        sepLE.flexibleHeight = 0; // Don't stretch vertically
        sepLE.flexibleWidth = 1;

        // --- 3. GRID AREA ---
        GameObject gridObj = new GameObject("Grid", typeof(RectTransform), typeof(GridLayoutGroup), typeof(LayoutElement));
        gridObj.transform.SetParent(mainContainer, false);
        GridLayoutGroup grid = gridObj.GetComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(480, 140);
        grid.spacing = new Vector2(30, 15); // Tighter vertical spacing
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 2;
        LayoutElement gridLE = gridObj.GetComponent<LayoutElement>();
        gridLE.flexibleHeight = 1; // Expands to fill remaining panel height
        gridLE.flexibleWidth = 1;

        // --- REBUILD BUTTONS ---
        for (int i = 0; i < amaliButtons.Count; i++)
        {
            Transform btn = amaliButtons[i];
            btn.SetParent(gridObj.transform, false);
            bool isLocked = (i != 0);
            
            // Add CanvasGroup for fading locked cards
            CanvasGroup cg = btn.GetComponent<CanvasGroup>();
            if (cg == null) cg = btn.gameObject.AddComponent<CanvasGroup>();
            cg.alpha = isLocked ? 0.6f : 1.0f;
            cg.interactable = !isLocked;
            
            // Clean old layout groups on the button
            if (btn.GetComponent<LayoutGroup>()) GameObject.DestroyImmediate(btn.GetComponent<LayoutGroup>());
            
            // Rescue visuals
            Transform oldIconCont = btn.Find("IconContainer");
            if (oldIconCont != null) {
                List<Transform> rescue = new List<Transform>();
                foreach(Transform c in oldIconCont) rescue.Add(c);
                foreach(Transform c in rescue) c.SetParent(btn, false);
                GameObject.DestroyImmediate(oldIconCont.gameObject);
            }
            Transform oldTextCont = btn.Find("TextContainer");
            if (oldTextCont != null) GameObject.DestroyImmediate(oldTextCont.gameObject);

            // Make button a glass card
            Image btnImg = btn.GetComponent<Image>();
            if (btnImg != null)
            {
                btnImg.sprite = glassSprite;
                btnImg.type = Image.Type.Sliced;
                btnImg.color = new Color(0.15f, 0.18f, 0.22f, 0.9f);
                btnImg.pixelsPerUnitMultiplier = 2f;
            }

            // Create Horizontal Layout for the card
            HorizontalLayoutGroup cardLayout = btn.gameObject.AddComponent<HorizontalLayoutGroup>();
            cardLayout.padding = new RectOffset(15, 15, 15, 15);
            cardLayout.spacing = 15;
            cardLayout.childAlignment = TextAnchor.MiddleLeft;
            cardLayout.childControlHeight = true;
            cardLayout.childControlWidth = true;
            cardLayout.childForceExpandHeight = true;
            cardLayout.childForceExpandWidth = true;

            // Gather all existing children (the custom icons/text)
            List<Transform> oldChildren = new List<Transform>();
            foreach (Transform c in btn) oldChildren.Add(c);

            // 1. Left Icon Container
            GameObject iconContainer = new GameObject("IconContainer", typeof(RectTransform), typeof(Image), typeof(Mask), typeof(LayoutElement));
            iconContainer.transform.SetParent(btn, false);
            iconContainer.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.3f);
            iconContainer.GetComponent<Image>().sprite = glassSprite;
            iconContainer.GetComponent<Image>().type = Image.Type.Sliced;
            iconContainer.GetComponent<Image>().pixelsPerUnitMultiplier = 2f;
            iconContainer.GetComponent<Mask>().showMaskGraphic = true;
            
            LayoutElement iconLE = iconContainer.GetComponent<LayoutElement>();
            iconLE.minWidth = 110; iconLE.preferredWidth = 110;
            iconLE.minHeight = 110; iconLE.preferredHeight = 110;
            iconLE.flexibleWidth = 0; iconLE.flexibleHeight = 0; // Strict square

            // Move old visuals into IconContainer
            foreach (Transform c in oldChildren)
            {
                c.SetParent(iconContainer.transform, false);
            }

            // Deep clean old TMP texts inside the icon container
            TextMeshProUGUI[] oldTexts = iconContainer.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var txt in oldTexts)
            {
                if (txt.gameObject.name != "WaterPercentageText")
                {
                    GameObject.DestroyImmediate(txt.gameObject);
                }
            }

            // Stretch visuals to fit icon container properly
            foreach (Transform c in iconContainer.transform)
            {
                RectTransform crt = c.GetComponent<RectTransform>();
                if (crt != null && c.name != "Image") // Adjust inner elements
                {
                    crt.anchorMin = Vector2.zero;
                    crt.anchorMax = Vector2.one;
                    crt.offsetMin = new Vector2(20, 20);
                    crt.offsetMax = new Vector2(-20, -20);
                }
            }

            // 2. Right Text Container
            GameObject textContainer = new GameObject("TextContainer", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(LayoutElement));
            textContainer.transform.SetParent(btn, false);
            VerticalLayoutGroup tcLayout = textContainer.GetComponent<VerticalLayoutGroup>();
            tcLayout.childControlHeight = true;
            tcLayout.childControlWidth = true;
            tcLayout.childForceExpandHeight = false; // We use LayoutElements for heights
            tcLayout.childForceExpandWidth = true;
            tcLayout.spacing = 0; // Tighten spacing
            textContainer.GetComponent<LayoutElement>().flexibleWidth = 1;
            textContainer.GetComponent<LayoutElement>().flexibleHeight = 1;

            GameObject cTitle = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement));
            cTitle.transform.SetParent(textContainer.transform, false);
            TextMeshProUGUI tTxt = cTitle.GetComponent<TextMeshProUGUI>();
            tTxt.text = "Amali " + (i + 1);
            tTxt.fontSize = 28;
            tTxt.fontStyle = FontStyles.Bold;
            tTxt.color = Color.white;
            tTxt.enableWordWrapping = false;
            tTxt.overflowMode = TextOverflowModes.Overflow;
            LayoutElement tLE = cTitle.GetComponent<LayoutElement>();
            tLE.minHeight = 35; tLE.preferredHeight = 35;

            GameObject cDesc = new GameObject("Desc", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement));
            cDesc.transform.SetParent(textContainer.transform, false);
            TextMeshProUGUI dTxt = cDesc.GetComponent<TextMeshProUGUI>();
            dTxt.text = "Digital science simulation.";
            dTxt.fontSize = 18;
            dTxt.color = new Color(0.7f, 0.7f, 0.7f);
            dTxt.enableWordWrapping = false;
            dTxt.overflowMode = TextOverflowModes.Ellipsis;
            LayoutElement descLE = cDesc.GetComponent<LayoutElement>();
            descLE.minHeight = 25; descLE.flexibleHeight = 1; // Expands in middle

            // Progress Area
            GameObject progArea = new GameObject("Progress", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            progArea.transform.SetParent(textContainer.transform, false);
            HorizontalLayoutGroup pHL = progArea.GetComponent<HorizontalLayoutGroup>();
            pHL.childAlignment = TextAnchor.LowerLeft;
            pHL.childControlHeight = true; pHL.childControlWidth = true;
            pHL.childForceExpandHeight = true; pHL.childForceExpandWidth = false;
            LayoutElement pLE = progArea.GetComponent<LayoutElement>();
            pLE.minHeight = 30; pLE.preferredHeight = 30;

            GameObject status = new GameObject("Status", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement));
            status.transform.SetParent(progArea.transform, false);
            TMPro.TextMeshProUGUI sTxt = status.GetComponent<TMPro.TextMeshProUGUI>();
            sTxt.text = "LOCKED";
            sTxt.fontSize = 18;
            sTxt.fontStyle = TMPro.FontStyles.Bold;
            sTxt.color = new UnityEngine.Color(0.5f, 0.5f, 0.5f);
            sTxt.enableWordWrapping = false;
            sTxt.overflowMode = TextOverflowModes.Overflow;
            sTxt.alignment = TextAlignmentOptions.BottomLeft;

            GameObject pSpacer = new GameObject("Spacer", typeof(RectTransform), typeof(LayoutElement));
            pSpacer.transform.SetParent(progArea.transform, false);
            pSpacer.GetComponent<LayoutElement>().flexibleWidth = 1;

            // Visual Progress Bar Container
            GameObject pbContainer = new GameObject("VisualProgressBar", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            pbContainer.transform.SetParent(progArea.transform, false);
            LayoutElement pbLe = pbContainer.GetComponent<LayoutElement>();
            pbLe.minWidth = 150; // Width of the bar area

            HorizontalLayoutGroup pbHL = pbContainer.GetComponent<HorizontalLayoutGroup>();
            pbHL.childAlignment = TextAnchor.MiddleRight;
            pbHL.spacing = 10;
            pbHL.childControlHeight = true; pbHL.childControlWidth = true;
            pbHL.childForceExpandHeight = false;

            // The actual bar graphic (Grey background)
            GameObject barBg = new GameObject("BarBG", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            barBg.transform.SetParent(pbContainer.transform, false);
            Image barImg = barBg.GetComponent<Image>();
            barImg.sprite = glassSprite;
            barImg.type = Image.Type.Sliced;
            barImg.pixelsPerUnitMultiplier = 3f;
            barImg.color = new Color(0f, 0f, 0f, 0.4f);
            LayoutElement bgLE = barBg.GetComponent<LayoutElement>();
            bgLE.minWidth = 100; bgLE.preferredWidth = 100;
            bgLE.minHeight = 6; bgLE.preferredHeight = 6;
            
            RectTransform bgRT = barBg.GetComponent<RectTransform>();
            bgRT.sizeDelta = new Vector2(100, 6);

            // The fill graphic (Green/Empty)
            GameObject barFill = new GameObject("BarFill", typeof(RectTransform), typeof(Image));
            barFill.transform.SetParent(barBg.transform, false);
            Image fillImg = barFill.GetComponent<Image>();
            fillImg.sprite = glassSprite;
            fillImg.type = Image.Type.Sliced;
            fillImg.pixelsPerUnitMultiplier = 3f;
            fillImg.color = new Color(0.2f, 0.85f, 0.45f, 1f); // Always green, width determines visibility
            
            // Stretch fill graphic based on percentage (Initially 0)
            RectTransform fillRT = barFill.GetComponent<RectTransform>();
            fillRT.anchorMin = new Vector2(0, 0);
            fillRT.anchorMax = new Vector2(0f, 1);
            fillRT.offsetMin = Vector2.zero;
            fillRT.offsetMax = Vector2.zero;

            // Percentage Text
            GameObject pText = new GameObject("PText", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement));
            pText.transform.SetParent(pbContainer.transform, false);
            TextMeshProUGUI pTxt = pText.GetComponent<TextMeshProUGUI>();
            pTxt.text = "0%";
            pTxt.fontSize = 18;
            pTxt.fontStyle = FontStyles.Bold;
            pTxt.color = Color.white;
            pTxt.enableWordWrapping = false;
            pTxt.overflowMode = TextOverflowModes.Overflow;
            pTxt.alignment = TextAlignmentOptions.BottomRight;
            LayoutElement pTxtLE = pText.GetComponent<LayoutElement>();
            pTxtLE.minWidth = 40;
            
            // Ensure Button exists for interactivity
            if (btn.GetComponent<Button>() == null) btn.gameObject.AddComponent<Button>();

            // Attach dynamic progress UI
            AmaliCardProgressUI progUI = btn.GetComponent<AmaliCardProgressUI>();
            if (progUI == null) progUI = btn.gameObject.AddComponent<AmaliCardProgressUI>();
            progUI.labIndex = i + 1;
            progUI.fillRect = fillRT;
            progUI.percentageText = pTxt;
            progUI.statusText = sTxt;

            if (btn.GetComponent<BouncyButton>() == null) btn.gameObject.AddComponent<BouncyButton>();
        }

        EditorUtility.SetDirty(canvas.gameObject);
    }

    private static Transform FindChildRecursive(Transform parent, string name)
    {
        if (parent.name == name) return parent;
        foreach (Transform child in parent)
        {
            Transform result = FindChildRecursive(child, name);
            if (result != null) return result;
        }
        return null;
    }
}
