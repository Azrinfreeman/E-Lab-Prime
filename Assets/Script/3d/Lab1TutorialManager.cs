using System.Collections;
using UnityEngine;
using ELab.Tutorial;

public class Lab1TutorialManager : MonoBehaviour
{
    public Lab1VoiceInstructions voiceInstructions;
    private int currentStep = 1;
    private GameObject currentBadge;

    private PengawalPanjangTali panjangTali;
    private DragBandul dragBandul;
    private JamRandik jamRandik;
    private PengurusDataAmali pengurusData;

    private bool step1Done = false;
    private bool step2Done = false;
    private bool step3Done = false;
    private bool step4Done = false;

    void Start()
    {
        panjangTali = Object.FindAnyObjectByType<PengawalPanjangTali>();
        dragBandul = Object.FindAnyObjectByType<DragBandul>();
        jamRandik = Object.FindAnyObjectByType<JamRandik>();
        pengurusData = Object.FindAnyObjectByType<PengurusDataAmali>();

        StartCoroutine(TutorialSequence());
    }

    public void ResetTutorial()
    {
        StopAllCoroutines();
        if (voiceInstructions != null) voiceInstructions.ResetPresentation();
        if (currentBadge != null) Destroy(currentBadge);
        Universal3DLabTutorial.CleanAllBadges();

        currentStep = 1;
        step1Done = false;
        step2Done = false;
        step3Done = false;
        step4Done = false;

        StartCoroutine(TutorialSequence());
    }

    private IEnumerator TutorialSequence()
    {
        yield return new WaitForSeconds(1.0f);

        if (dragBandul == null) dragBandul = Object.FindAnyObjectByType<DragBandul>();
        Transform centerAnchor = dragBandul != null ? dragBandul.transform : null;

        if (voiceInstructions != null) voiceInstructions.Present(0);
        // Step 1: Adjust string length (pointing toward Right Panel)
        currentBadge = Universal3DLabTutorial.CreateInstructionBadge(
            "1. Laraskan Panjang Tali Bandul (Panel Kanan)",
            centerAnchor,
            new Vector3(1.2f, -1.8f, -0.5f),
            Color.yellow,
            false // world space
        );

        float initialSliderVal = panjangTali != null && panjangTali.sliderPanjang != null ? panjangTali.sliderPanjang.value : -1f;
        while (!step1Done)
        {
            if (panjangTali != null && panjangTali.sliderPanjang != null)
            {
                if (Mathf.Abs(panjangTali.sliderPanjang.value - initialSliderVal) > 1.0f)
                {
                    CompleteStep1();
                }
            }
            yield return null;
        }

        if (voiceInstructions != null) voiceInstructions.Present(1);
        // Step 2: Drag and Release Pendulum (hovering above golden bob)
        currentBadge = Universal3DLabTutorial.CreateInstructionBadge(
            "2. Tarik & Lepaskan Bandul (< 10°)",
            centerAnchor,
            new Vector3(0f, -3.2f, -0.5f),
            new Color(0.2f, 1f, 0.8f),
            false // world space
        );

        while (!step2Done)
        {
            yield return null;
        }

        if (voiceInstructions != null) voiceInstructions.Present(2);
        // Step 3: Start Stopwatch (pointing toward Left Panel)
        currentBadge = Universal3DLabTutorial.CreateInstructionBadge(
            "3. Mulakan Jam Randik untuk 10 Ayunan",
            centerAnchor,
            new Vector3(-1.2f, -1.8f, -0.5f),
            Color.yellow,
            false // world space
        );

        while (!step3Done)
        {
            yield return null;
        }

        // Step 4: Record Data
        while (jamRandik != null && jamRandik.DapatkanJumlahAyunan() < 10)
        {
            yield return null;
        }

        if (currentBadge != null) StartCoroutine(Universal3DLabTutorial.FadeAndDestroyBadge(currentBadge));

        currentBadge = Universal3DLabTutorial.CreateInstructionBadge(
            "4. Rekod Pemerhatian di Jadual Data",
            centerAnchor,
            new Vector3(-1.2f, -2.5f, -0.5f),
            new Color(0.4f, 1f, 0.4f),
            false // world space
        );

        if (voiceInstructions != null) voiceInstructions.Present(3);

        while (!step4Done)
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

    public void CompleteStep4()
    {
        if (currentStep == 4 && !step4Done)
        {
            step4Done = true;
            currentStep = 5;
            if (voiceInstructions != null) voiceInstructions.Present(4);
            if (currentBadge != null) StartCoroutine(Universal3DLabTutorial.FadeAndDestroyBadge(currentBadge));
        }
    }
}

