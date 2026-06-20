using UnityEngine;

public class Lab4TutorialManager : MonoBehaviour
{
    [Header("Tutorial Text Objects")]
    public GameObject step1_Evaporate;
    public GameObject step2_ReadBook;

    private int currentStep = 1;

    void Start()
    {
        // Initial State
        if (step1_Evaporate != null) step1_Evaporate.SetActive(true);
        if (step2_ReadBook != null) step2_ReadBook.SetActive(false);
    }

    public void CompleteStep1()
    {
        if (currentStep == 1)
        {
            if (step1_Evaporate != null) step1_Evaporate.SetActive(false);
            if (step2_ReadBook != null) step2_ReadBook.SetActive(true);
            currentStep = 2;
        }
    }

    public void CompleteStep2()
    {
        if (currentStep == 2)
        {
            if (step2_ReadBook != null) step2_ReadBook.SetActive(false);
            currentStep = 3; // Tutorial complete
        }
    }
}
