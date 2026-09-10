#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class AchievementSceneSetup : EditorWindow
{
    [MenuItem("E-Lab Tools/Setup Achievements UI")]
    public static void SetupAchievementsUI()
    {
        // 1. Check active scene
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if (scene.name != "MainMenu")
        {
            Debug.LogError("Sila buka scene 'MainMenu' terlebih dahulu sebelum menjalankan setup!");
            return;
        }

        // 2. Locate Canvas and PanelPencapaian
        GameObject canvasObj = GameObject.Find("Canvas");
        if (canvasObj == null)
        {
            Debug.LogError("Canvas tidak dijumpai!");
            return;
        }

        Transform panelPencapaianTrans = canvasObj.transform.Find("background/PanelPencapaian");
        if (panelPencapaianTrans == null)
        {
            Debug.LogError("PanelPencapaian tidak dijumpai dalam scene!");
            return;
        }
        GameObject panelPencapaian = panelPencapaianTrans.gameObject;

        Transform verticalTrans = panelPencapaian.transform.Find("Verticacl");
        if (verticalTrans == null)
        {
            Debug.LogError("GameObject 'Verticacl' di bawah PanelPencapaian tidak dijumpai!");
            return;
        }

        Undo.RegisterCompleteObjectUndo(panelPencapaian, "Setup Achievements UI");

        // 3. Clean up any existing rows or debug buttons to prevent duplicates
        string[] existingChildren = { "Row_Lab1", "Row_Lab2", "Row_Lab3", "Row_Lab4", "Btn_DebugToggle" };
        foreach (var childName in existingChildren)
        {
            Transform child = verticalTrans.Find(childName);
            if (child != null)
            {
                DestroyImmediate(child.gameObject);
            }
        }

        // Adjust VerticalLayoutGroup padding and spacing
        VerticalLayoutGroup vlg = verticalTrans.GetComponent<VerticalLayoutGroup>();
        if (vlg != null)
        {
            vlg.spacing = 10f;
            vlg.padding = new RectOffset(20, 20, 15, 15);
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childAlignment = TextAnchor.UpperCenter;
        }

        // 4. Load common assets
        Sprite bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI Flat/btn_gray_flat.png");
        Sprite tickSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI Flat/tick.png");
        Sprite crossSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI Flat/btn_close_flat.png");
        TMP_FontAsset fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/FredokaOne SDF.asset");

        if (bgSprite == null || fontAsset == null)
        {
            Debug.LogError("Gagal memuatkan aset sprite atau font! Sila pastikan fail wujud.");
            return;
        }

        string[] labTitles = {
            "AMALI 1: AYUNAN BANDUL",
            "AMALI 2: ELEKTROLISIS",
            "AMALI 3: PEMANASAN AIR",
            "AMALI 4: PENYEJATAN AIR"
        };

        string[] labDescs = {
            "Mengkaji hubungan panjang tali bandul dengan tempoh ayunannya.",
            "Mengkaji proses penguraian molekul air kepada gas oksigen & hidrogen.",
            "Mengkaji kesan kehadiran bendasing (garam) ke atas takat didih air.",
            "Mengkaji faktor-faktor fizikal yang mempengaruhi kadar penyejatan air."
        };

        AchievementManager.AchievementRow[] rows = new AchievementManager.AchievementRow[4];

        // 5. Generate 4 Rows
        for (int i = 0; i < 4; i++)
        {
            GameObject rowObj = new GameObject($"Row_Lab{i + 1}");
            rowObj.transform.SetParent(verticalTrans, false);

            // Add Background Image
            Image rowImg = rowObj.AddComponent<Image>();
            rowImg.sprite = bgSprite;
            rowImg.type = Image.Type.Sliced;
            rowImg.color = new Color(0.08f, 0.1f, 0.14f, 0.8f);

            // Add Horizontal Layout
            HorizontalLayoutGroup hlg = rowObj.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(15, 15, 10, 10);
            hlg.spacing = 15f;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;

            // Layout Element
            LayoutElement le = rowObj.AddComponent<LayoutElement>();
            le.minHeight = 85f;
            le.preferredHeight = 85f;

            // --- Left: Icon Container ---
            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(rowObj.transform, false);
            Image iconImg = iconObj.AddComponent<Image>();
            iconImg.sprite = crossSprite; // Default locked
            iconImg.color = new Color(1f, 1f, 1f, 0.35f);
            
            LayoutElement iconLe = iconObj.AddComponent<LayoutElement>();
            iconLe.preferredWidth = 45f;
            iconLe.preferredHeight = 45f;
            iconLe.minWidth = 45f;
            iconLe.minHeight = 45f;

            // --- Middle: Text details ---
            GameObject textContainer = new GameObject("TextContainer");
            textContainer.transform.SetParent(rowObj.transform, false);
            VerticalLayoutGroup textVlg = textContainer.AddComponent<VerticalLayoutGroup>();
            textVlg.spacing = 4f;
            textVlg.childControlWidth = true;
            textVlg.childControlHeight = true;
            textVlg.childForceExpandWidth = true;
            textVlg.childForceExpandHeight = false;

            LayoutElement textLe = textContainer.AddComponent<LayoutElement>();
            textLe.preferredWidth = 440f;
            textLe.preferredHeight = 65f;

            // Title
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(textContainer.transform, false);
            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = labTitles[i];
            titleText.font = fontAsset;
            titleText.fontSize = 18f;
            titleText.color = new Color(1f, 1f, 1f, 0.45f); // Locked default
            titleText.alignment = TextAlignmentOptions.Left;

            // Description
            GameObject descObj = new GameObject("Description");
            descObj.transform.SetParent(textContainer.transform, false);
            TextMeshProUGUI descText = descObj.AddComponent<TextMeshProUGUI>();
            descText.text = labDescs[i];
            descText.font = fontAsset;
            descText.fontSize = 12f;
            descText.color = new Color(1f, 1f, 1f, 0.25f); // Locked default
            descText.alignment = TextAlignmentOptions.Left;

            // --- Right: Status Badges Container ---
            GameObject badgeContainer = new GameObject("BadgeContainer");
            badgeContainer.transform.SetParent(rowObj.transform, false);
            
            LayoutElement badgeLe = badgeContainer.AddComponent<LayoutElement>();
            badgeLe.preferredWidth = 110f;
            badgeLe.preferredHeight = 35f;
            badgeLe.minWidth = 110f;
            badgeLe.minHeight = 35f;

            RectTransform badgeRect = badgeContainer.GetComponent<RectTransform>();
            badgeRect.sizeDelta = new Vector2(110f, 35f);

            // Locked Badge
            GameObject lockedBadge = new GameObject("LockedBadge");
            lockedBadge.transform.SetParent(badgeContainer.transform, false);
            Image lockBg = lockedBadge.AddComponent<Image>();
            lockBg.sprite = bgSprite;
            lockBg.type = Image.Type.Sliced;
            lockBg.color = new Color(0.35f, 0.35f, 0.38f, 0.4f);

            RectTransform lockRect = lockedBadge.GetComponent<RectTransform>();
            lockRect.anchorMin = Vector2.zero;
            lockRect.anchorMax = Vector2.one;
            lockRect.sizeDelta = Vector2.zero;
            lockRect.anchoredPosition = Vector2.zero;

            GameObject lockTxtObj = new GameObject("Text");
            lockTxtObj.transform.SetParent(lockedBadge.transform, false);
            TextMeshProUGUI lockTxt = lockTxtObj.AddComponent<TextMeshProUGUI>();
            lockTxt.text = "TERKUNCI";
            lockTxt.font = fontAsset;
            lockTxt.fontSize = 11f;
            lockTxt.alignment = TextAlignmentOptions.Center;
            lockTxt.color = new Color(1f, 1f, 1f, 0.8f);
            
            RectTransform lockTxtRect = lockTxtObj.GetComponent<RectTransform>();
            lockTxtRect.anchorMin = Vector2.zero;
            lockTxtRect.anchorMax = Vector2.one;
            lockTxtRect.sizeDelta = Vector2.zero;
            lockTxtRect.anchoredPosition = Vector2.zero;

            // Unlocked Badge
            GameObject unlockedBadge = new GameObject("UnlockedBadge");
            unlockedBadge.transform.SetParent(badgeContainer.transform, false);
            Image unlockBg = unlockedBadge.AddComponent<Image>();
            unlockBg.sprite = bgSprite;
            unlockBg.type = Image.Type.Sliced;
            unlockBg.color = new Color(0.1f, 0.65f, 0.25f, 1f); // Vibrant emerald green

            RectTransform unlockRect = unlockedBadge.GetComponent<RectTransform>();
            unlockRect.anchorMin = Vector2.zero;
            unlockRect.anchorMax = Vector2.one;
            unlockRect.sizeDelta = Vector2.zero;
            unlockRect.anchoredPosition = Vector2.zero;

            GameObject unlockTxtObj = new GameObject("Text");
            unlockTxtObj.transform.SetParent(unlockedBadge.transform, false);
            TextMeshProUGUI unlockTxt = unlockTxtObj.AddComponent<TextMeshProUGUI>();
            unlockTxt.text = "SELESAI";
            unlockTxt.font = fontAsset;
            unlockTxt.fontSize = 11f;
            unlockTxt.alignment = TextAlignmentOptions.Center;
            unlockTxt.color = Color.white;

            RectTransform unlockTxtRect = unlockTxtObj.GetComponent<RectTransform>();
            unlockTxtRect.anchorMin = Vector2.zero;
            unlockTxtRect.anchorMax = Vector2.one;
            unlockTxtRect.sizeDelta = Vector2.zero;
            unlockTxtRect.anchoredPosition = Vector2.zero;

            // Set default active states (unlocked starts hidden)
            unlockedBadge.SetActive(false);
            lockedBadge.SetActive(true);

            // Populate rows mapping
            rows[i] = new AchievementManager.AchievementRow
            {
                labName = $"Lab {i + 1}",
                rowObject = rowObj,
                iconImage = iconImg,
                titleText = titleText,
                descText = descText,
                lockBadge = lockedBadge,
                unlockBadge = unlockedBadge
            };
        }

        // 6. Generate Debug Toggle Button at the bottom
        GameObject debugBtnObj = new GameObject("Btn_DebugToggle");
        debugBtnObj.transform.SetParent(verticalTrans, false);

        Image btnImg = debugBtnObj.AddComponent<Image>();
        btnImg.sprite = bgSprite;
        btnImg.type = Image.Type.Sliced;
        btnImg.color = new Color(0.18f, 0.77f, 0.71f, 1f); // Cyan

        Button btnComp = debugBtnObj.AddComponent<Button>();
        
        LayoutElement btnLe = debugBtnObj.AddComponent<LayoutElement>();
        btnLe.preferredWidth = 350f;
        btnLe.preferredHeight = 45f;
        btnLe.minWidth = 350f;
        btnLe.minHeight = 45f;

        debugBtnObj.AddComponent<BouncyButton>();

        GameObject btnTxtObj = new GameObject("Text");
        btnTxtObj.transform.SetParent(debugBtnObj.transform, false);
        TextMeshProUGUI btnTxt = btnTxtObj.AddComponent<TextMeshProUGUI>();
        btnTxt.text = "BUKA SEMUA (UNLOCK ALL)";
        btnTxt.font = fontAsset;
        btnTxt.fontSize = 13f;
        btnTxt.fontStyle = FontStyles.Bold;
        btnTxt.alignment = TextAlignmentOptions.Center;
        btnTxt.color = Color.white;
        
        RectTransform btnTxtRect = btnTxtObj.GetComponent<RectTransform>();
        btnTxtRect.anchorMin = Vector2.zero;
        btnTxtRect.anchorMax = Vector2.one;
        btnTxtRect.sizeDelta = Vector2.zero;
        btnTxtRect.anchoredPosition = Vector2.zero;

        // 7. Attach and populate AchievementManager on PanelPencapaian
        AchievementManager mgr = panelPencapaian.GetComponent<AchievementManager>();
        if (mgr == null) mgr = panelPencapaian.AddComponent<AchievementManager>();

        mgr.achievementRows = rows;
        mgr.lockSprite = crossSprite;
        mgr.unlockSprite = tickSprite;
        mgr.toggleAllButton = btnComp;
        mgr.toggleAllText = btnTxt;

        // 8. Save Scene
        EditorUtility.SetDirty(panelPencapaian);
        EditorUtility.SetDirty(verticalTrans.gameObject);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);

        Debug.Log("Achievements UI setup successfully completed!");
    }
}
#endif
