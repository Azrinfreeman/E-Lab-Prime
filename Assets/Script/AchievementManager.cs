using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class AchievementManager : MonoBehaviour
{
    [System.Serializable]
    public class AchievementRow
    {
        public string labName;
        public GameObject rowObject;
        public Image iconImage;
        public TextMeshProUGUI titleText;
        public TextMeshProUGUI descText;
        public GameObject lockBadge;
        public GameObject unlockBadge;
    }

    [Header("Achievements UI")]
    public AchievementRow[] achievementRows;

    [Header("Icons")]
    public Sprite lockSprite;
    public Sprite unlockSprite;

    [Header("Debug/Testing Panel")]
    public Button toggleAllButton;
    public TextMeshProUGUI toggleAllText;

    private bool isTestingUnlocked = false;

    void OnEnable()
    {
        RefreshAchievements();
    }

    void Start()
    {
        if (toggleAllButton != null)
        {
            toggleAllButton.onClick.RemoveAllListeners();
            toggleAllButton.onClick.AddListener(ToggleAllAchievements);
            UpdateToggleText();
        }
    }

    public void RefreshAchievements()
    {
        if (achievementRows == null) return;

        for (int i = 0; i < achievementRows.Length; i++)
        {
            var row = achievementRows[i];
            if (row == null || row.rowObject == null) continue;

            string key = $"Lab{i + 1}_3D_Completed";
            // Check progress variables in PlayerPrefs
            bool isCompleted = (PlayerPrefs.GetInt(key, 0) == 1) || 
                               (PlayerPrefs.GetInt($"Lab{i + 1}_Completed", 0) == 1) || 
                               isTestingUnlocked;

            // 1. Toggle Lock/Unlock badges
            if (row.lockBadge != null) row.lockBadge.SetActive(!isCompleted);
            if (row.unlockBadge != null) row.unlockBadge.SetActive(isCompleted);

            // 2. Update status icon
            if (row.iconImage != null)
            {
                if (isCompleted && unlockSprite != null)
                {
                    row.iconImage.sprite = unlockSprite;
                    row.iconImage.color = new Color(0.18f, 0.77f, 0.71f, 1f); // Vibrant cyan
                }
                else if (!isCompleted && lockSprite != null)
                {
                    row.iconImage.sprite = lockSprite;
                    row.iconImage.color = new Color(1f, 1f, 1f, 0.35f); // Dim lock
                }
            }

            // 3. Update text transparency to reflect lock state
            if (row.titleText != null)
            {
                row.titleText.color = isCompleted ? Color.white : new Color(1f, 1f, 1f, 0.45f);
            }
            if (row.descText != null)
            {
                row.descText.color = isCompleted ? new Color(0.8f, 0.85f, 0.9f, 1f) : new Color(1f, 1f, 1f, 0.25f);
            }

            // 4. Tint background row image
            Image bgImg = row.rowObject.GetComponent<Image>();
            if (bgImg != null)
            {
                bgImg.color = isCompleted 
                    ? new Color(0.12f, 0.22f, 0.32f, 0.92f) // Slate blue-teal
                    : new Color(0.08f, 0.1f, 0.14f, 0.8f);    // Dimmed locked
            }

            if (gameObject.scene.name == "MainMenu")
            {
                if (row.iconImage != null) row.iconImage.color = isCompleted ? KidFriendlyPalette.Success : KidFriendlyPalette.Muted;
                if (row.titleText != null) row.titleText.color = KidFriendlyPalette.Ink;
                if (row.descText != null) row.descText.color = KidFriendlyPalette.Muted;
                if (bgImg != null) bgImg.color = isCompleted ? KidFriendlyPalette.Slot : KidFriendlyPalette.Neutral;
            }
        }
    }

    private void ToggleAllAchievements()
    {
        isTestingUnlocked = !isTestingUnlocked;

        // Set PlayerPrefs for testing persistence
        for (int i = 1; i <= 4; i++)
        {
            PlayerPrefs.SetInt($"Lab{i}_3D_Completed", isTestingUnlocked ? 1 : 0);
            PlayerPrefs.SetInt($"Lab{i}_Completed", isTestingUnlocked ? 1 : 0);
        }
        PlayerPrefs.Save();

        RefreshAchievements();
        UpdateToggleText();
    }

    private void UpdateToggleText()
    {
        if (toggleAllText != null)
        {
            toggleAllText.text = isTestingUnlocked ? "KUNCI SEMUA (LOCK ALL)" : "BUKA SEMUA (UNLOCK ALL)";
        }
    }
}
