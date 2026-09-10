using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class MainMenuSceneSetup
{
    [MenuItem("E-Lab/Setup/Setup Main Menu Scene UI")]
    public static void SetupMainMenuScene()
    {
        var scene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
        if (scene.name != "MainMenu")
        {
            Debug.LogError("Please open the MainMenu scene first!");
            return;
        }

        Canvas canvas = GameObject.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("No Canvas found!");
            return;
        }
        
        Undo.RegisterCompleteObjectUndo(canvas.gameObject, "Setup Main Menu Scene UI");
        Debug.Log("Starting Main Menu Setup...");

        // 1. Setup Background Sprite correctly
        string bgPath = "Assets/UI Flat/blurred_classroom_bg.png";
        TextureImporter importer = AssetImporter.GetAtPath(bgPath) as TextureImporter;
        if (importer != null)
        {
            if (importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.SaveAndReimport();
                Debug.Log("Reimported blurred background as Sprite.");
            }
        }
        else
        {
            Debug.LogError($"AssetImporter not found for {bgPath}");
        }

        Transform background = FindChildRecursive(canvas.transform, "background");
        if (background != null)
        {
            Image bgImg = background.GetComponent<Image>();
            Sprite blurredBg = AssetDatabase.LoadAssetAtPath<Sprite>(bgPath);
            if (bgImg != null && blurredBg != null)
            {
                bgImg.sprite = blurredBg;
                bgImg.color = Color.white;
                Debug.Log("Successfully set blurred background.");
            }
            else
            {
                Debug.LogError($"Failed to apply background. Image component present: {bgImg != null}, Sprite loaded: {blurredBg != null}");
            }
        }
        else
        {
            Debug.LogError("Failed to find 'background' Transform in Canvas.");
        }

        // 2. Style Major Panels
        Transform panelLogin = FindChildRecursive(canvas.transform, "PanelLogin");
        if (panelLogin != null) { StylePanel(panelLogin); Debug.Log("Styled PanelLogin"); }
        else Debug.LogError("Failed to find 'PanelLogin'");
        
        Transform amaliPanel = FindChildRecursive(canvas.transform, "AmaliPanel");
        if (amaliPanel != null) { StylePanel(amaliPanel); StyleAmaliCards(amaliPanel); Debug.Log("Styled AmaliPanel"); }
        else Debug.LogError("Failed to find 'AmaliPanel'");

        Transform panelPengaturan = FindChildRecursive(canvas.transform, "PanelPengaturan");
        if (panelPengaturan != null) { StylePanel(panelPengaturan); StyleSettings(panelPengaturan); Debug.Log("Styled PanelPengaturan"); }
        else Debug.LogError("Failed to find 'PanelPengaturan'");

        Transform panelPencapaian = FindChildRecursive(canvas.transform, "PanelPencapaian");
        if (panelPencapaian != null) { StylePanel(panelPencapaian); StyleAchievements(panelPencapaian); Debug.Log("Styled PanelPencapaian"); }
        else Debug.LogError("Failed to find 'PanelPencapaian'");

        Transform sideMenu = FindChildRecursive(canvas.transform, "SIdemenu");
        if (sideMenu != null) { StyleSideMenu(sideMenu); Debug.Log("Styled SIdemenu"); }
        else Debug.LogError("Failed to find 'SIdemenu'");

        // Style the main menu buttons in 'Middle'
        Transform middlePanel = FindChildRecursive(canvas.transform, "Middle");
        if (middlePanel != null)
        {
            Button[] middleButtons = middlePanel.GetComponentsInChildren<Button>(true);
            Debug.Log($"Found Middle Panel. Button count: {middleButtons.Length}");
            Sprite cleanSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI Flat/pb_rect_rounded.png");
            foreach (Button btn in middleButtons)
            {
                Image btnImg = btn.GetComponent<Image>();
                if (btnImg != null)
                {
                    if (cleanSprite != null) {
                        btnImg.sprite = cleanSprite;
                        btnImg.type = Image.Type.Sliced;
                    }
                    btnImg.color = new Color(0.1f, 0.15f, 0.2f, 0.7f); // Dark frosted glass
                    btnImg.pixelsPerUnitMultiplier = 2f;
                    if (btn.GetComponent<BouncyButton>() == null)
                        btn.gameObject.AddComponent<BouncyButton>();
                    Debug.Log($"Styled button in Middle: {btn.name}");
                }
            }
        }
        else
        {
            Debug.LogError("Failed to find 'Middle' panel.");
        }

        // 3. General Typography Sweep (AAA Clean Typography)
        CleanTypography(canvas.transform);
        
        EditorUtility.SetDirty(canvas.gameObject);
        MainMenuKidFriendlyColourSetup.Apply();
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
        Debug.Log("Main Menu UI successfully updated to AAA Glassmorphism!");
    }

    private static Transform FindChildRecursive(Transform parent, string name)
    {
        Transform[] allChildren = parent.GetComponentsInChildren<Transform>(true);
        foreach (var child in allChildren)
        {
            if (child.name == name) return child;
        }
        return null;
    }

    private static void StylePanel(Transform panel)
    {
        if (panel == null) return;
        Image img = panel.GetComponent<Image>();
        if (img != null)
        {
            Sprite cleanSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI Flat/pb_rect_rounded.png");
            if (cleanSprite != null)
            {
                img.sprite = cleanSprite;
                img.type = Image.Type.Sliced;
            }

            img.color = new Color(0.1f, 0.15f, 0.2f, 0.7f); // Dark Glassmorphism
            img.pixelsPerUnitMultiplier = 2f;
            
            // Log for verification
            Debug.Log($"Applied Dark Glassmorphism to {panel.name}");
        }
        else
        {
            Debug.LogError($"Panel {panel.name} does not have an Image component.");
        }
    }

    private static void StyleAmaliCards(Transform amaliPanel)
    {
        if (amaliPanel == null) return;
        Transform horizontal = FindChildRecursive(amaliPanel, "Horizontal");
        if (horizontal != null)
        {
            foreach (Transform card in horizontal)
            {
                if (card.name.StartsWith("Amali"))
                {
                    Image cardImg = card.GetComponent<Image>();
                    if (cardImg != null)
                    {
                        cardImg.color = new Color(0f, 0f, 0f, 0.35f); // Recessed glass slot
                        cardImg.pixelsPerUnitMultiplier = 2f;
                    }

                    if (card.GetComponent<BouncyButton>() == null)
                        card.gameObject.AddComponent<BouncyButton>();
                }
            }
            Debug.Log("Styled Amali cards");
        }
        else
        {
            Debug.LogWarning("Failed to find 'Horizontal' inside AmaliPanel");
        }
    }

    private static void StyleSettings(Transform settingsPanel)
    {
        if (settingsPanel == null) return;
        
        Slider[] sliders = settingsPanel.GetComponentsInChildren<Slider>(true);
        foreach (var slider in sliders)
        {
            Transform fillArea = slider.transform.Find("Fill Area/Fill");
            if (fillArea != null)
            {
                Image fillImg = fillArea.GetComponent<Image>();
                if (fillImg != null)
                {
                    fillImg.color = new Color(0.2f, 0.85f, 0.45f, 1f); // Vibrant Emerald Green
                }
            }
        }
    }

    private static void StyleAchievements(Transform achievementPanel)
    {
        if (achievementPanel == null) return;

        foreach (Transform child in achievementPanel)
        {
            if (child.name.StartsWith("Row_Lab"))
            {
                Image rowImg = child.GetComponent<Image>();
                if (rowImg != null)
                {
                    rowImg.color = new Color(0f, 0f, 0f, 0.35f); // Dark glass slot
                    rowImg.pixelsPerUnitMultiplier = 2f;
                }
            }
        }
    }

    private static void StyleSideMenu(Transform sideMenu)
    {
        if (sideMenu == null) return;
        
        Image img = sideMenu.GetComponent<Image>();
        if (img != null)
        {
            img.color = new Color(0.1f, 0.15f, 0.2f, 0.7f); // Floating dark glass dock
        }

        foreach (Transform child in sideMenu)
        {
            if (child.GetComponent<Button>() != null)
            {
                if (child.GetComponent<BouncyButton>() == null)
                    child.gameObject.AddComponent<BouncyButton>();
                
                // Remove outlines from side menu icons if any
                Image iconImg = child.GetComponent<Image>();
                if (iconImg != null)
                {
                    iconImg.color = Color.white;
                }
            }
        }
    }

    private static void CleanTypography(Transform root)
    {
        int count = 0;
        TextMeshProUGUI[] allTexts = root.GetComponentsInChildren<TextMeshProUGUI>(true);
        foreach (var txt in allTexts)
        {
            try 
            {
                if (txt.outlineWidth > 0f)
                {
                    txt.outlineWidth = 0f; // Remove thick outlines
                    count++;
                }
            } 
            catch (System.Exception) 
            {
                // Ignore canvasRenderer null errors on inactive components
            }
            
            if (txt.color == Color.black) 
            {
                txt.color = Color.white; // Ensure text is visible on dark glass
            }
        }
        Debug.Log($"Cleaned typography: removed outlines from {count} text elements.");
    }
}
