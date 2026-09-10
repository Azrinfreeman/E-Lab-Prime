using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AmaliCardProgressUI : MonoBehaviour
{
    [Header("Configuration")]
    public int labIndex = 1;

    [Header("UI References")]
    public RectTransform fillRect;
    public TextMeshProUGUI percentageText;
    public TextMeshProUGUI statusText;

    private void OnEnable()
    {
        UpdateProgress();
    }

    public void UpdateProgress()
    {
        // 1. Check if the lab is unlocked
        // The user requested all levels to be unlocked for now.
        bool isUnlocked = true; // (labIndex == 1) || (PlayerPrefs.GetInt($"Lab{labIndex - 1}_3D_Completed", 0) == 1) || (PlayerPrefs.GetInt($"Lab{labIndex - 1}_Completed", 0) == 1);
        
        // Developer shortcut to unlock all (if used in AchievementManager)
        if (PlayerPrefs.GetInt($"Lab{labIndex}_Unlocked", 0) == 1) isUnlocked = true;

        // 2. Check if the lab is fully completed
        bool isCompleted = (PlayerPrefs.GetInt($"Lab{labIndex}_3D_Completed", 0) == 1) || (PlayerPrefs.GetInt($"Lab{labIndex}_Completed", 0) == 1);

        // 3. Read granular progress, override if fully completed
        float progress = PlayerPrefs.GetFloat($"Lab{labIndex}_Progress", 0f);
        if (isCompleted)
        {
            progress = 1f;
        }

        // If not unlocked, force to 0
        if (!isUnlocked)
        {
            progress = 0f;
        }

        // 4. Update UI Visuals
        if (fillRect != null)
        {
            fillRect.anchorMax = new Vector2(progress, 1f);
        }

        if (percentageText != null)
        {
            percentageText.text = Mathf.RoundToInt(progress * 100) + "%";
        }

        if (statusText != null)
        {
            if (isCompleted)
            {
                statusText.text = "COMPLETED";
                statusText.color = gameObject.scene.name == "MainMenu" ? KidFriendlyPalette.Ink : new Color(0.2f, 0.85f, 0.45f); // Green
            }
            else if (isUnlocked)
            {
                statusText.text = "ACTIVE";
                statusText.color = gameObject.scene.name == "MainMenu" ? KidFriendlyPalette.Ink : new Color(0.2f, 0.85f, 0.45f); // Keep Active Green for consistency with current style
            }
            else
            {
                statusText.text = "LOCKED";
                statusText.color = new Color(0.5f, 0.5f, 0.5f); // Grey
            }
        }
        
        // Update Canvas Group interactability and alpha based on unlocked state
        CanvasGroup cg = GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.alpha = isUnlocked ? 1.0f : 0.6f;
            cg.interactable = isUnlocked;
            cg.blocksRaycasts = isUnlocked;
        }
    }
}
