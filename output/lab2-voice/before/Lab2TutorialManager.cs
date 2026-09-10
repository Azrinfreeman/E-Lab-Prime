using System.Collections;
using UnityEngine;
using ELab.Tutorial;

public class Lab2TutorialManager : MonoBehaviour
{
    private int currentStep = 1;
    private GameObject currentBadge;

    private PhysicalSwitch3D switch3D;
    private ElectrolysisManager3D manager3D;
    private InteractiveNotebook3D notebook3D;

    private bool step1Done = false;
    private bool step2Done = false;
    private bool step3Done = false;

    void Start()
    {
        switch3D = Object.FindAnyObjectByType<PhysicalSwitch3D>();
        manager3D = Object.FindAnyObjectByType<ElectrolysisManager3D>();
        notebook3D = Object.FindAnyObjectByType<InteractiveNotebook3D>();

        StartCoroutine(TutorialSequence());
    }

    public void ResetTutorial()
    {
        StopAllCoroutines();
        if (currentBadge != null) Destroy(currentBadge);
        Universal3DLabTutorial.CleanAllBadges();

        currentStep = 1;
        step1Done = false;
        step2Done = false;
        step3Done = false;

        StartCoroutine(TutorialSequence());
    }

    private IEnumerator TutorialSequence()
    {
        yield return new WaitForSeconds(1.0f);

        // Step 1: Turn On Switch - Offset forward to avoid clipping into switch/ammeter
        Transform step1Target = null;
        if (switch3D != null) step1Target = switch3D.transform;

        currentBadge = Universal3DLabTutorial.CreateInstructionBadge(
            "1. Klik Suis Untuk Lengkapkan Litar",
            step1Target,
            new Vector3(0.15f, 0.25f, -0.35f),
            Color.yellow,
            false // world space
        );

        while (!step1Done)
        {
            if (switch3D != null && switch3D.isOn)
            {
                CompleteStep1();
            }
            yield return null;
        }

        // Step 2: Observe Electrolysis & Bubbles - Positioned in front of beaker (-0.35m Z) to prevent clipping inside water
        Transform step2Target = null;
        GameObject deskObj2 = GameObject.Find("Desk_1") ?? GameObject.Find("TeacherDeskTop");
        Vector3 step2Offset = new Vector3(0f, 0.25f, -0.35f);

        if (deskObj2 != null)
        {
            step2Target = deskObj2.transform;
        }
        else if (manager3D != null && manager3D.waterCathodePivot != null)
        {
            step2Target = manager3D.waterCathodePivot;
            step2Offset = new Vector3(0f, 0.15f, -0.4f);
        }

        currentBadge = Universal3DLabTutorial.CreateInstructionBadge(
            "2. Perhatikan Gelembung Gas pada Elektrod",
            step2Target,
            step2Offset,
            new Color(0.2f, 1f, 0.8f),
            false // world space
        );

        while (!step2Done)
        {
            yield return null;
        }

        // Step 3: Record in Notebook - Positioned above notebook on the desk
        Transform step3Target = null;
        GameObject deskObj3 = GameObject.Find("Desk_1") ?? GameObject.Find("TeacherDeskTop");
        Vector3 step3Offset = new Vector3(0f, 0.45f, -0.35f);

        if (deskObj3 != null)
        {
            step3Target = deskObj3.transform;
        }
        else if (notebook3D != null)
        {
            step3Target = notebook3D.transform;
            step3Offset = new Vector3(-0.3f, 0.5f, -0.2f); // Move up and left to avoid tablet
        }

        currentBadge = Universal3DLabTutorial.CreateInstructionBadge(
            "3. Rekod Pemerhatian & Jawab Kuiz",
            step3Target,
            step3Offset,
            new Color(0.4f, 1f, 0.4f),
            false // world space
        );

        while (!step3Done)
        {
            yield return null;
        }

        if (currentBadge != null) StartCoroutine(Universal3DLabTutorial.FadeAndDestroyBadge(currentBadge));
    }

    public void CompleteStep1()
    {
        if (currentStep == 1 && !step1Done)
        {
            step1Done = true;
            currentStep = 2;
            if (currentBadge != null) StartCoroutine(Universal3DLabTutorial.FadeAndDestroyBadge(currentBadge));
        }
    }

    public void CompleteStep2()
    {
        if (currentStep == 2 && !step2Done)
        {
            step2Done = true;
            currentStep = 3;
            if (currentBadge != null) StartCoroutine(Universal3DLabTutorial.FadeAndDestroyBadge(currentBadge));
        }
    }

    public void CompleteStep3()
    {
        if (currentStep == 3 && !step3Done)
        {
            step3Done = true;
            currentStep = 4;
            if (currentBadge != null) StartCoroutine(Universal3DLabTutorial.FadeAndDestroyBadge(currentBadge));
        }
    }
}
