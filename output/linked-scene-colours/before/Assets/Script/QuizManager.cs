using UnityEngine;
using TMPro;

public class QuizManager : MonoBehaviour
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
    }

    public void ShowQuiz()
    {
        quizPanel.SetActive(true);
        feedbackText.gameObject.SetActive(false);
    }

    // Function to be called by the Answer Buttons
    public void CheckAnswer(bool isCorrect)
    {
        feedbackText.gameObject.SetActive(true);

        if (isCorrect)
        {
            feedbackText.text = "<color=green>Correct! Impurities increase the boiling point of water.</color>";
            
            if (PlayerPrefs.GetInt("Lab3_3D_Completed", 0) == 0)
            {
                if (EconomyManager.Instance != null)
                {
                    EconomyManager.Instance.AddXP(500);
                    EconomyManager.Instance.AddCredits(100);
                }
            }
            PlayerPrefs.SetInt("Lab3_3D_Completed", 1);
            PlayerPrefs.Save();

            // Hide the quiz automatically after 4 seconds
            Invoke("HideQuiz", 4f); 
        }
        else
        {
            feedbackText.text = "<color=red>Incorrect. Please look at the graph observations again.</color>";
        }
    }

    private void HideQuiz()
    {
        quizPanel.SetActive(false);
        if (summaryPanel != null)
        {
            summaryPanel.SetActive(true);
        }
    }
}