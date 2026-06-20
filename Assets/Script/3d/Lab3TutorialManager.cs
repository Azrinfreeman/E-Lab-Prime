using UnityEngine;

public class Lab3TutorialManager : MonoBehaviour
{
    [Header("Tutorial Text Objects")]
    public GameObject step1_Salt;
    public GameObject step2_Goggles;
    public GameObject step3_Gas;
    public GameObject step4_Start;

    private int currentStep = 1;

    void Start()
    {
        // Initial State
        if (step1_Salt != null) step1_Salt.SetActive(true);
        if (step2_Goggles != null) step2_Goggles.SetActive(false);
        if (step3_Gas != null) step3_Gas.SetActive(false);
        if (step4_Start != null) step4_Start.SetActive(false);
    }

    public void CompleteStep1()
    {
        if (currentStep == 1)
        {
            if (step1_Salt != null) step1_Salt.SetActive(false);
            if (step2_Goggles != null) step2_Goggles.SetActive(true);
            currentStep = 2;
        }
    }

    public void CompleteStep2()
    {
        if (currentStep == 2)
        {
            if (step2_Goggles != null) step2_Goggles.SetActive(false);
            if (step3_Gas != null) step3_Gas.SetActive(true);
            currentStep = 3;
        }
    }

    public void CompleteStep3()
    {
        if (currentStep == 3)
        {
            if (step3_Gas != null) step3_Gas.SetActive(false);
            if (step4_Start != null) step4_Start.SetActive(true);
            currentStep = 4;
        }
    }

    public void CompleteStep4()
    {
        if (currentStep == 4)
        {
            if (step4_Start != null) step4_Start.SetActive(false);
            currentStep = 5; // Tutorial complete
        }
    }
}
