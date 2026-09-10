#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsPanelOverhaulSetup
{
    [MenuItem("E-Lab/Setup/Overhaul Settings Panel")]
    public static void OverhaulSettingsPanel()
    {
        Canvas canvas = GameObject.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("No Canvas found!");
            return;
        }

        Transform settingsPanel = null;
        foreach (Transform t in canvas.GetComponentsInChildren<Transform>(true))
        {
            if (t.name == "PanelPengaturan" || t.name == "PengaturanPanel" || t.name.Contains("Pengaturan"))
            {
                settingsPanel = t;
                break;
            }
        }

        if (settingsPanel == null)
        {
            Debug.LogError("Settings panel not found!");
            return;
        }

        // Load assets
        Sprite glassSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI Flat/pb_rect_rounded.png");
        Sprite handleSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI Flat/node_icon.png");

        // Clean out existing children
        System.Collections.Generic.List<GameObject> toDestroy = new System.Collections.Generic.List<GameObject>();
        foreach (Transform child in settingsPanel)
        {
            toDestroy.Add(child.gameObject);
        }
        foreach (GameObject obj in toDestroy)
        {
            GameObject.DestroyImmediate(obj);
        }

        // Ensure settings panel covers the entire screen (for the dimming effect)
        RectTransform spRT = settingsPanel.GetComponent<RectTransform>();
        if (spRT != null)
        {
            spRT.anchorMin = Vector2.zero;
            spRT.anchorMax = Vector2.one;
            spRT.offsetMin = Vector2.zero;
            spRT.offsetMax = Vector2.zero;
        }

        // Add Dimmer Background directly to Settings Panel if not present
        Image bgImg = settingsPanel.GetComponent<Image>();
        if (bgImg == null) bgImg = settingsPanel.gameObject.AddComponent<Image>();
        bgImg.color = new Color(0, 0, 0, 0.7f); // Dark semi-transparent dimmer
        
        // Ensure CanvasGroup blocks raycasts (so you can't click through the settings panel)
        CanvasGroup spCG = settingsPanel.GetComponent<CanvasGroup>();
        if (spCG == null) spCG = settingsPanel.gameObject.AddComponent<CanvasGroup>();
        spCG.blocksRaycasts = true;

        // Create the Glass Modal Container
        GameObject modalObj = new GameObject("ModalContainer", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup));
        modalObj.transform.SetParent(settingsPanel, false);
        Image modalImg = modalObj.GetComponent<Image>();
        modalImg.sprite = glassSprite;
        modalImg.type = Image.Type.Sliced;
        modalImg.pixelsPerUnitMultiplier = 2f;
        modalImg.color = new Color(0.1f, 0.15f, 0.2f, 0.95f);
        
        RectTransform modalRT = modalObj.GetComponent<RectTransform>();
        modalRT.anchorMin = new Vector2(0.5f, 0.5f);
        modalRT.anchorMax = new Vector2(0.5f, 0.5f);
        modalRT.sizeDelta = new Vector2(600, 650);

        VerticalLayoutGroup modalVLG = modalObj.GetComponent<VerticalLayoutGroup>();
        modalVLG.padding = new RectOffset(40, 40, 40, 40);
        modalVLG.spacing = 25;
        modalVLG.childAlignment = TextAnchor.UpperCenter;
        modalVLG.childControlHeight = true;
        modalVLG.childControlWidth = true;
        modalVLG.childForceExpandHeight = false;

        // HEADER
        GameObject headerObj = new GameObject("Header", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement));
        headerObj.transform.SetParent(modalObj.transform, false);
        TextMeshProUGUI headerTxt = headerObj.GetComponent<TextMeshProUGUI>();
        headerTxt.text = "SETTINGS";
        headerTxt.fontSize = 36;
        headerTxt.fontStyle = FontStyles.Bold;
        headerTxt.color = Color.white;
        headerTxt.alignment = TextAlignmentOptions.Center;
        headerObj.GetComponent<LayoutElement>().minHeight = 50;

        // Separator
        GameObject separator = new GameObject("Separator", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
        separator.transform.SetParent(modalObj.transform, false);
        separator.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.15f);
        separator.GetComponent<LayoutElement>().minHeight = 2;

        // Content Area (Scrollable or just VLG)
        GameObject contentObj = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(LayoutElement));
        contentObj.transform.SetParent(modalObj.transform, false);
        VerticalLayoutGroup cvlg = contentObj.GetComponent<VerticalLayoutGroup>();
        cvlg.spacing = 20;
        cvlg.childControlHeight = true;
        cvlg.childControlWidth = true;
        cvlg.childForceExpandHeight = false;
        contentObj.GetComponent<LayoutElement>().flexibleHeight = 1;

        // AUDIO SECTION
        CreateSectionHeader(contentObj.transform, "AUDIO");
        Slider masterVol = CreateSliderRow(contentObj.transform, "MASTER VOLUME", glassSprite, handleSprite);
        Slider bgmVol = CreateSliderRow(contentObj.transform, "MUSIC VOLUME", glassSprite, handleSprite);
        Slider sfxVol = CreateSliderRow(contentObj.transform, "SFX VOLUME", glassSprite, handleSprite);

        // GRAPHICS SECTION
        CreateSectionHeader(contentObj.transform, "GRAPHICS");
        TMP_Dropdown graphicsDropdown = CreateDropdownRow(contentObj.transform, "QUALITY LEVEL", glassSprite);

        // DATA SECTION
        CreateSectionHeader(contentObj.transform, "DATA");
        Button resetButton = CreateButtonRow(contentObj.transform, "RESET PROGRESS", new Color(0.85f, 0.2f, 0.2f, 1f), glassSprite);

        // Spacer to push Close button down
        GameObject spacer = new GameObject("Spacer", typeof(RectTransform), typeof(LayoutElement));
        spacer.transform.SetParent(modalObj.transform, false);
        spacer.GetComponent<LayoutElement>().flexibleHeight = 1;

        // CLOSE BUTTON
        Button closeButton = CreateButtonRow(modalObj.transform, "CLOSE", new Color(0.2f, 0.85f, 0.45f, 1f), glassSprite);
        closeButton.GetComponent<LayoutElement>().minHeight = 60;

        // Wire Up Settings Manager
        SettingsManagerUI settingsManager = settingsPanel.gameObject.GetComponent<SettingsManagerUI>();
        if (settingsManager == null) settingsManager = settingsPanel.gameObject.AddComponent<SettingsManagerUI>();
        
        settingsManager.masterVolumeSlider = masterVol;
        settingsManager.bgmVolumeSlider = bgmVol;
        settingsManager.sfxVolumeSlider = sfxVol;
        settingsManager.qualityDropdown = graphicsDropdown;
        settingsManager.resetProgressButton = resetButton;

        // CONFIRMATION PANEL
        GameObject confirmPanel = CreateConfirmationPanel(settingsPanel, glassSprite, settingsManager);

        // Try wiring the Close button to MenuManager (TutupPengaturan)
        MainMenuManager menuManager = GameObject.FindFirstObjectByType<MainMenuManager>();
        if (menuManager != null)
        {
            UnityEditor.Events.UnityEventTools.AddPersistentListener(closeButton.onClick, new UnityEngine.Events.UnityAction(menuManager.TutupPengaturan));
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        Debug.Log("Settings Panel Overhauled!");
    }

    private static GameObject CreateConfirmationPanel(Transform parent, Sprite glassSprite, SettingsManagerUI manager)
    {
        // Full screen dimmer
        GameObject confirmBg = new GameObject("ConfirmationPanel", typeof(RectTransform), typeof(Image));
        confirmBg.transform.SetParent(parent, false);
        confirmBg.SetActive(false); // Hidden by default
        
        RectTransform cbgRT = confirmBg.GetComponent<RectTransform>();
        cbgRT.anchorMin = Vector2.zero; cbgRT.anchorMax = Vector2.one;
        cbgRT.offsetMin = Vector2.zero; cbgRT.offsetMax = Vector2.zero;
        
        Image bgImg = confirmBg.GetComponent<Image>();
        bgImg.color = new Color(0, 0, 0, 0.85f); // Very dark to hide the settings menu
        
        CanvasGroup cg = confirmBg.gameObject.AddComponent<CanvasGroup>();
        cg.blocksRaycasts = true;

        // Modal Box
        GameObject modalObj = new GameObject("ConfirmModal", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup));
        modalObj.transform.SetParent(confirmBg.transform, false);
        Image modalImg = modalObj.GetComponent<Image>();
        modalImg.sprite = glassSprite;
        modalImg.type = Image.Type.Sliced;
        modalImg.pixelsPerUnitMultiplier = 2f;
        modalImg.color = new Color(0.15f, 0.2f, 0.25f, 1f);
        
        RectTransform modalRT = modalObj.GetComponent<RectTransform>();
        modalRT.anchorMin = new Vector2(0.5f, 0.5f); modalRT.anchorMax = new Vector2(0.5f, 0.5f);
        modalRT.sizeDelta = new Vector2(500, 300);

        VerticalLayoutGroup vlg = modalObj.GetComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(30, 30, 40, 30);
        vlg.spacing = 20;
        vlg.childAlignment = TextAnchor.MiddleCenter;
        vlg.childControlHeight = true; vlg.childControlWidth = true;
        vlg.childForceExpandHeight = false;

        // Header
        GameObject headerObj = new GameObject("Header", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement));
        headerObj.transform.SetParent(modalObj.transform, false);
        TextMeshProUGUI headerTxt = headerObj.GetComponent<TextMeshProUGUI>();
        headerTxt.text = "WARNING";
        headerTxt.fontSize = 32;
        headerTxt.fontStyle = FontStyles.Bold;
        headerTxt.color = new Color(0.85f, 0.2f, 0.2f, 1f);
        headerTxt.alignment = TextAlignmentOptions.Center;
        
        // Body
        GameObject bodyObj = new GameObject("Body", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement));
        bodyObj.transform.SetParent(modalObj.transform, false);
        TextMeshProUGUI bodyTxt = bodyObj.GetComponent<TextMeshProUGUI>();
        bodyTxt.text = "Are you sure you want to reset all progress?\nThis action cannot be undone.";
        bodyTxt.fontSize = 20;
        bodyTxt.color = Color.white;
        bodyTxt.alignment = TextAlignmentOptions.Center;
        bodyTxt.enableWordWrapping = true;
        bodyObj.GetComponent<LayoutElement>().flexibleHeight = 1;

        // Button Row
        GameObject btnRow = new GameObject("ButtonRow", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        btnRow.transform.SetParent(modalObj.transform, false);
        HorizontalLayoutGroup hlg = btnRow.GetComponent<HorizontalLayoutGroup>();
        hlg.spacing = 20;
        hlg.childControlHeight = true; hlg.childControlWidth = true;
        hlg.childForceExpandHeight = false; hlg.childForceExpandWidth = true;

        Button cancelBtn = CreateButtonRow(btnRow.transform, "CANCEL", new Color(0.4f, 0.4f, 0.4f, 1f), glassSprite);
        Button confirmBtn = CreateButtonRow(btnRow.transform, "CONFIRM", new Color(0.85f, 0.2f, 0.2f, 1f), glassSprite);

        // Wire Manager
        manager.confirmationPanel = confirmBg;
        manager.confirmResetButton = confirmBtn;
        manager.cancelResetButton = cancelBtn;

        return confirmBg;
    }

    private static void CreateSectionHeader(Transform parent, string title)
    {
        GameObject headerObj = new GameObject("Section_" + title, typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement));
        headerObj.transform.SetParent(parent, false);
        TextMeshProUGUI headerTxt = headerObj.GetComponent<TextMeshProUGUI>();
        headerTxt.text = title;
        headerTxt.fontSize = 20;
        headerTxt.fontStyle = FontStyles.Bold;
        headerTxt.color = new Color(0.6f, 0.6f, 0.6f); // Light Grey
        headerTxt.alignment = TextAlignmentOptions.BottomLeft;
        
        LayoutElement le = headerObj.GetComponent<LayoutElement>();
        le.minHeight = 35;
        le.preferredHeight = 35;
    }

    private static Slider CreateSliderRow(Transform parent, string label, Sprite glassSprite, Sprite handleSprite)
    {
        GameObject row = new GameObject("Row_" + label, typeof(RectTransform), typeof(HorizontalLayoutGroup));
        row.transform.SetParent(parent, false);
        HorizontalLayoutGroup hlg = row.GetComponent<HorizontalLayoutGroup>();
        hlg.spacing = 20;
        hlg.childAlignment = TextAnchor.MiddleLeft;
        hlg.childControlHeight = true;
        hlg.childControlWidth = true;
        hlg.childForceExpandHeight = false;
        hlg.childForceExpandWidth = false;

        GameObject labelObj = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement));
        labelObj.transform.SetParent(row.transform, false);
        TextMeshProUGUI labelTxt = labelObj.GetComponent<TextMeshProUGUI>();
        labelTxt.text = label;
        labelTxt.fontSize = 18;
        labelTxt.fontStyle = FontStyles.Bold;
        labelTxt.color = Color.white;
        
        LayoutElement labelLE = labelObj.GetComponent<LayoutElement>();
        labelLE.flexibleWidth = 1;
        labelLE.minHeight = 30;

        // Slider Object
        GameObject sliderObj = new GameObject("Slider", typeof(RectTransform), typeof(Slider), typeof(LayoutElement));
        sliderObj.transform.SetParent(row.transform, false);
        
        LayoutElement sliderLE = sliderObj.GetComponent<LayoutElement>();
        sliderLE.minWidth = 250;
        sliderLE.minHeight = 30;
        
        Slider slider = sliderObj.GetComponent<Slider>();

        // Background
        GameObject bgObj = new GameObject("Background", typeof(RectTransform), typeof(Image));
        bgObj.transform.SetParent(sliderObj.transform, false);
        Image bgImg = bgObj.GetComponent<Image>();
        bgImg.sprite = glassSprite;
        bgImg.type = Image.Type.Sliced;
        bgImg.color = new Color(0f, 0f, 0f, 0.4f); // Neumorphic slot
        
        RectTransform bgRT = bgObj.GetComponent<RectTransform>();
        bgRT.anchorMin = new Vector2(0, 0.4f); bgRT.anchorMax = new Vector2(1, 0.6f);
        bgRT.offsetMin = Vector2.zero; bgRT.offsetMax = Vector2.zero;

        // Fill Area
        GameObject fillArea = new GameObject("Fill Area", typeof(RectTransform));
        fillArea.transform.SetParent(sliderObj.transform, false);
        RectTransform faRT = fillArea.GetComponent<RectTransform>();
        faRT.anchorMin = new Vector2(0, 0.4f); faRT.anchorMax = new Vector2(1, 0.6f);
        faRT.offsetMin = Vector2.zero; faRT.offsetMax = Vector2.zero;

        // Fill
        GameObject fillObj = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        fillObj.transform.SetParent(fillArea.transform, false);
        Image fillImg = fillObj.GetComponent<Image>();
        fillImg.sprite = glassSprite;
        fillImg.type = Image.Type.Sliced;
        fillImg.color = new Color(0.2f, 0.85f, 0.45f, 1f); // Vibrant green
        RectTransform fRT = fillObj.GetComponent<RectTransform>();
        fRT.offsetMin = Vector2.zero; fRT.offsetMax = Vector2.zero;

        // Handle Slide Area
        GameObject hsa = new GameObject("Handle Slide Area", typeof(RectTransform));
        hsa.transform.SetParent(sliderObj.transform, false);
        RectTransform hsaRT = hsa.GetComponent<RectTransform>();
        hsaRT.anchorMin = Vector2.zero; hsaRT.anchorMax = Vector2.one;
        hsaRT.offsetMin = new Vector2(10, 0); hsaRT.offsetMax = new Vector2(-10, 0);

        // Handle
        GameObject handle = new GameObject("Handle", typeof(RectTransform), typeof(Image));
        handle.transform.SetParent(hsa.transform, false);
        Image handleImg = handle.GetComponent<Image>();
        handleImg.sprite = handleSprite;
        handleImg.preserveAspect = true;
        
        RectTransform hRT = handle.GetComponent<RectTransform>();
        hRT.anchorMin = new Vector2(0, 0.5f); hRT.anchorMax = new Vector2(0, 0.5f);
        hRT.pivot = new Vector2(0.5f, 0.5f);
        hRT.sizeDelta = new Vector2(24, 24);

        // Assign to Slider
        slider.fillRect = fRT;
        slider.handleRect = hRT;
        slider.targetGraphic = handleImg;
        slider.direction = Slider.Direction.LeftToRight;
        slider.minValue = 0;
        slider.maxValue = 1;
        slider.value = 1;

        return slider;
    }

    private static TMP_Dropdown CreateDropdownRow(Transform parent, string label, Sprite glassSprite)
    {
        GameObject row = new GameObject("Row_" + label, typeof(RectTransform), typeof(HorizontalLayoutGroup));
        row.transform.SetParent(parent, false);
        HorizontalLayoutGroup hlg = row.GetComponent<HorizontalLayoutGroup>();
        hlg.spacing = 20;
        hlg.childAlignment = TextAnchor.MiddleLeft;
        hlg.childControlHeight = true;
        hlg.childControlWidth = true;
        hlg.childForceExpandHeight = false;
        hlg.childForceExpandWidth = false;

        GameObject labelObj = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement));
        labelObj.transform.SetParent(row.transform, false);
        TextMeshProUGUI labelTxt = labelObj.GetComponent<TextMeshProUGUI>();
        labelTxt.text = label;
        labelTxt.fontSize = 18;
        labelTxt.fontStyle = FontStyles.Bold;
        labelTxt.color = Color.white;
        labelObj.GetComponent<LayoutElement>().flexibleWidth = 1;

        GameObject dpObj = new GameObject("Dropdown", typeof(RectTransform), typeof(Image), typeof(TMP_Dropdown), typeof(LayoutElement));
        dpObj.transform.SetParent(row.transform, false);
        Image dpImg = dpObj.GetComponent<Image>();
        dpImg.sprite = glassSprite;
        dpImg.type = Image.Type.Sliced;
        dpImg.color = new Color(0f, 0f, 0f, 0.4f);
        LayoutElement dple = dpObj.GetComponent<LayoutElement>();
        dple.minWidth = 200;
        dple.minHeight = 40;

        TMP_Dropdown dropdown = dpObj.GetComponent<TMP_Dropdown>();
        dropdown.targetGraphic = dpImg;

        // Label for Dropdown
        GameObject dpLabel = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        dpLabel.transform.SetParent(dpObj.transform, false);
        TextMeshProUGUI dpLabelTxt = dpLabel.GetComponent<TextMeshProUGUI>();
        dpLabelTxt.fontSize = 16;
        dpLabelTxt.fontStyle = FontStyles.Bold;
        dpLabelTxt.color = Color.white;
        dpLabelTxt.alignment = TextAlignmentOptions.Center;
        RectTransform dpLRT = dpLabel.GetComponent<RectTransform>();
        dpLRT.anchorMin = Vector2.zero; dpLRT.anchorMax = Vector2.one;
        dpLRT.offsetMin = Vector2.zero; dpLRT.offsetMax = Vector2.zero;

        dropdown.captionText = dpLabelTxt;

        // Standard TMP_Dropdown needs a template, but we will leave it empty as Unity auto-generates it if we just provide standard prefabs.
        // For editor script simplicity without complex template building, we rely on standard TMP behaviour or assume template is built.
        // To prevent errors, we'll build a basic template.
        
        GameObject templateObj = new GameObject("Template", typeof(RectTransform), typeof(Image), typeof(ScrollRect));
        templateObj.transform.SetParent(dpObj.transform, false);
        templateObj.SetActive(false);
        Image tImg = templateObj.GetComponent<Image>();
        tImg.sprite = glassSprite;
        tImg.type = Image.Type.Sliced;
        tImg.color = new Color(0.1f, 0.15f, 0.2f, 1f);
        RectTransform tRT = templateObj.GetComponent<RectTransform>();
        tRT.anchorMin = new Vector2(0, 0); tRT.anchorMax = new Vector2(1, 0);
        tRT.pivot = new Vector2(0.5f, 1);
        tRT.anchoredPosition = new Vector2(0, -2);
        tRT.sizeDelta = new Vector2(0, 150);

        GameObject viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
        viewport.transform.SetParent(templateObj.transform, false);
        viewport.GetComponent<Image>().color = Color.white;
        viewport.GetComponent<Mask>().showMaskGraphic = false;
        RectTransform vRT = viewport.GetComponent<RectTransform>();
        vRT.anchorMin = Vector2.zero; vRT.anchorMax = Vector2.one;
        vRT.offsetMin = Vector2.zero; vRT.offsetMax = Vector2.zero;

        GameObject content = new GameObject("Content", typeof(RectTransform));
        content.transform.SetParent(viewport.transform, false);
        RectTransform cRT = content.GetComponent<RectTransform>();
        cRT.anchorMin = new Vector2(0, 1); cRT.anchorMax = new Vector2(1, 1);
        cRT.pivot = new Vector2(0.5f, 1);
        cRT.sizeDelta = new Vector2(0, 40);

        GameObject item = new GameObject("Item", typeof(RectTransform), typeof(Toggle));
        item.transform.SetParent(content.transform, false);
        RectTransform iRT = item.GetComponent<RectTransform>();
        iRT.anchorMin = new Vector2(0, 0.5f); iRT.anchorMax = new Vector2(1, 0.5f);
        iRT.sizeDelta = new Vector2(0, 40);

        GameObject itemBackground = new GameObject("Item Background", typeof(RectTransform), typeof(Image));
        itemBackground.transform.SetParent(item.transform, false);
        itemBackground.GetComponent<Image>().color = new Color(0.2f, 0.85f, 0.45f, 0.2f);
        RectTransform ibRT = itemBackground.GetComponent<RectTransform>();
        ibRT.anchorMin = Vector2.zero; ibRT.anchorMax = Vector2.one;
        ibRT.offsetMin = Vector2.zero; ibRT.offsetMax = Vector2.zero;

        GameObject itemLabel = new GameObject("Item Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        itemLabel.transform.SetParent(item.transform, false);
        TextMeshProUGUI ilTxt = itemLabel.GetComponent<TextMeshProUGUI>();
        ilTxt.fontSize = 16;
        ilTxt.color = Color.white;
        ilTxt.alignment = TextAlignmentOptions.Center;
        RectTransform ilRT = itemLabel.GetComponent<RectTransform>();
        ilRT.anchorMin = Vector2.zero; ilRT.anchorMax = Vector2.one;
        ilRT.offsetMin = Vector2.zero; ilRT.offsetMax = Vector2.zero;

        Toggle toggle = item.GetComponent<Toggle>();
        toggle.targetGraphic = itemBackground.GetComponent<Image>();

        dropdown.template = tRT;
        dropdown.itemText = ilTxt;

        ScrollRect scroll = templateObj.GetComponent<ScrollRect>();
        scroll.content = cRT;
        scroll.viewport = vRT;
        scroll.horizontal = false;
        scroll.movementType = ScrollRect.MovementType.Clamped;

        return dropdown;
    }

    private static Button CreateButtonRow(Transform parent, string label, Color color, Sprite glassSprite)
    {
        GameObject btnObj = new GameObject("Btn_" + label, typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
        btnObj.transform.SetParent(parent, false);
        Image btnImg = btnObj.GetComponent<Image>();
        btnImg.sprite = glassSprite;
        btnImg.type = Image.Type.Sliced;
        btnImg.pixelsPerUnitMultiplier = 2f;
        btnImg.color = color;

        LayoutElement le = btnObj.GetComponent<LayoutElement>();
        le.minHeight = 45;
        le.preferredHeight = 45;

        GameObject txtObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        txtObj.transform.SetParent(btnObj.transform, false);
        TextMeshProUGUI txt = txtObj.GetComponent<TextMeshProUGUI>();
        txt.text = label;
        txt.fontSize = 20;
        txt.fontStyle = FontStyles.Bold;
        txt.color = Color.white;
        txt.alignment = TextAlignmentOptions.Center;
        RectTransform txtRT = txtObj.GetComponent<RectTransform>();
        txtRT.anchorMin = Vector2.zero; txtRT.anchorMax = Vector2.one;
        txtRT.offsetMin = Vector2.zero; txtRT.offsetMax = Vector2.zero;

        // Defensive add BouncyButton (if exists in project)
        System.Type bouncyType = System.Type.GetType("BouncyButton, Assembly-CSharp");
        if (bouncyType != null)
        {
            btnObj.AddComponent(bouncyType);
        }

        return btnObj.GetComponent<Button>();
    }
}
#endif
