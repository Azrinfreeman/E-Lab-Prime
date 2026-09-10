using UnityEngine;
using TMPro;

public class QuizManagerLab5 : MonoBehaviour
{
    [Header("Quiz UI Elements")]
    public GameObject quizPanel;
    public TextMeshProUGUI feedbackText;
    public GameObject summaryPanel;

    void Start()
    {
        // Ensure quiz is hidden at the start
        if (quizPanel != null) quizPanel.SetActive(false);
        if (feedbackText != null) feedbackText.gameObject.SetActive(false);

        // Bind answer buttons dynamically at runtime to ensure they persist in play mode
        if (quizPanel != null)
        {
            UnityEngine.UI.Button btn0 = quizPanel.transform.Find("Ans0")?.GetComponent<UnityEngine.UI.Button>();
            if (btn0 != null)
            {
                btn0.onClick.RemoveAllListeners();
                btn0.onClick.AddListener(() => CheckAnswer(false));
            }

            UnityEngine.UI.Button btn1 = quizPanel.transform.Find("Ans1")?.GetComponent<UnityEngine.UI.Button>();
            if (btn1 != null)
            {
                btn1.onClick.RemoveAllListeners();
                btn1.onClick.AddListener(() => CheckAnswer(false));
            }

            UnityEngine.UI.Button btn2 = quizPanel.transform.Find("Ans2")?.GetComponent<UnityEngine.UI.Button>();
            if (btn2 != null)
            {
                btn2.onClick.RemoveAllListeners();
                btn2.onClick.AddListener(() => CheckAnswer(false));
            }

            UnityEngine.UI.Button btn3 = quizPanel.transform.Find("Ans3")?.GetComponent<UnityEngine.UI.Button>();
            if (btn3 != null)
            {
                btn3.onClick.RemoveAllListeners();
                btn3.onClick.AddListener(() => CheckAnswer(true));
            }
        }

        // Bind summary close button dynamically
        if (summaryPanel != null)
        {
            UnityEngine.UI.Button closeBtn = summaryPanel.transform.Find("CloseBtn")?.GetComponent<UnityEngine.UI.Button>();
            if (closeBtn != null)
            {
                closeBtn.onClick.RemoveAllListeners();
                closeBtn.onClick.AddListener(() => { summaryPanel.SetActive(false); });
            }
        }
    }

    public void ShowQuiz()
    {
        if (quizPanel != null) quizPanel.SetActive(true);
        if (feedbackText != null) feedbackText.gameObject.SetActive(false);
    }

    // Function to be called by the Answer Buttons
    public void CheckAnswer(bool isCorrect)
    {
        if (feedbackText != null) feedbackText.gameObject.SetActive(true);

        if (isCorrect)
        {
            if (feedbackText != null) KidFriendlySceneTheme.SetFeedback(feedbackText, "<color=green>Betul! Suhu tinggi, kadar kacauan tinggi, dan saiz gula kecil meningkatkan kadar keterlarutan.</color>");
            
            if (PlayerPrefs.GetInt("Lab5_3D_Completed", 0) == 0)
            {
                if (EconomyManager.Instance != null)
                {
                    EconomyManager.Instance.AddXP(500);
                    EconomyManager.Instance.AddCredits(100);
                }
            }
            PlayerPrefs.SetInt("Lab5_3D_Completed", 1);
            PlayerPrefs.Save();

            // Hide the quiz automatically after 4 seconds
            Invoke("HideQuiz", 4f); 
        }
        else
        {
            if (feedbackText != null) KidFriendlySceneTheme.SetFeedback(feedbackText, "<color=red>Salah. Sila perhatikan kesan pembolehubah dan jawab semula.</color>");
        }
    }

    private void HideQuiz()
    {
        if (quizPanel != null) quizPanel.SetActive(false);
        if (summaryPanel != null)
        {
            summaryPanel.SetActive(true);
        }
    }

    public void ResetQuiz()
    {
        CancelInvoke("HideQuiz");
        if (quizPanel != null) quizPanel.SetActive(false);
        if (feedbackText != null) feedbackText.gameObject.SetActive(false);
        if (summaryPanel != null) summaryPanel.SetActive(false);
    }
}
