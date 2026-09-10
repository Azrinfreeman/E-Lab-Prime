using System.Collections;
using UnityEngine;
using ELab.Tutorial;

public class Lab4TutorialManager : MonoBehaviour
{
    [Header("Tutorial Text Objects (Legacy UI - Will be hidden)")]
    public GameObject step1_Evaporate;
    public GameObject step2_ReadBook;

    public Lab4VoiceInstructions voiceInstructions;
    private int currentStep = 1;
    private GameObject currentBadge;

    private EvaporationManager3D evapManager;
    private InteractiveNotebookLab4 notebookLab4;

    private bool heaterDone = false;
    private bool step1Done = false;
    private bool step2Done = false;

    void Start()
    {
        if (step1_Evaporate != null) step1_Evaporate.SetActive(false);
        if (step2_ReadBook != null) step2_ReadBook.SetActive(false);

        evapManager = Object.FindAnyObjectByType<EvaporationManager3D>();
        notebookLab4 = Object.FindAnyObjectByType<InteractiveNotebookLab4>();

        StartCoroutine(TutorialSequence());
    }

    public void ResetTutorial()
    {
        StopAllCoroutines();
        if (voiceInstructions != null) voiceInstructions.ResetPresentation();
        if (currentBadge != null) Destroy(currentBadge);
        Universal3DLabTutorial.CleanAllBadges();

        currentStep = 1;
        heaterDone = false;
        step1Done = false;
        step2Done = false;

        StartCoroutine(TutorialSequence());
    }

    private IEnumerator TutorialSequence()
    {
        yield return new WaitForSeconds(1.0f);

        if (voiceInstructions != null) voiceInstructions.Present(0);
        // Step 1: Turn on heater switch
        Transform heaterSwitchTarget = null;
        if (evapManager != null && evapManager.powerSwitch != null)
        {
            heaterSwitchTarget = evapManager.powerSwitch.transform;
        }
        else
        {
            var p3d = GameObject.Find("Pemanas_3D");
            if (p3d != null) heaterSwitchTarget = p3d.transform;
        }

        Vector3 off1 = new Vector3(0f, 0.25f, -0.1f);
        currentBadge = Universal3DLabTutorial.CreateInstructionBadge(
            "1. Klik Untuk Hidupkan Pemanas",
            heaterSwitchTarget != null ? heaterSwitchTarget : (evapManager != null ? evapManager.transform : null),
            off1,
            Color.yellow,
            false // world space
        );

        while (!heaterDone)
        {
            if (evapManager != null && (evapManager.IsHeaterPowerOn || evapManager.IsEvaporationStarted || evapManager.IsExperimentFinished))
            {
                heaterDone = true;
                break;
            }
            if (step1Done)
            {
                heaterDone = true;
                break;
            }
            yield return null;
        }

        currentStep = 2;
        if (currentBadge != null) StartCoroutine(Universal3DLabTutorial.FadeAndDestroyBadge(currentBadge));

        if (voiceInstructions != null) voiceInstructions.Present(1);
        // Step 2: Adjust evaporation factors & start experiment
        Transform step2Target = null;
        var cg = GameObject.Find("Container_Group") ?? GameObject.Find("Pemanas_3D");
        if (cg != null) step2Target = cg.transform;
        else if (evapManager != null) step2Target = evapManager.transform;

        currentBadge = Universal3DLabTutorial.CreateInstructionBadge(
            "2. Laraskan Faktor Penyejatan & Mulakan Eksperimen",
            step2Target,
            new Vector3(0f, 0.45f, 0f),
            new Color(0.2f, 1f, 0.8f),
            false // world space
        );

        while (!step1Done)
        {
            if (evapManager != null && evapManager.IsExperimentFinished)
            {
                step1Done = true;
                break;
            }
            yield return null;
        }

        currentStep = 3;
        if (currentBadge != null) StartCoroutine(Universal3DLabTutorial.FadeAndDestroyBadge(currentBadge));

        if (voiceInstructions != null) voiceInstructions.Present(2);
        // Step 3: Read Book & Record
        Transform step3Target = null;
        if (notebookLab4 != null) step3Target = notebookLab4.transform;

        currentBadge = Universal3DLabTutorial.CreateInstructionBadge(
            "3. Rekod Pemerhatian & Jawab Kuiz di Buku Amali",
            step3Target,
            new Vector3(0f, 0.25f, 0f),
            new Color(0.4f, 1f, 0.4f),
            false // world space
        );

        while (!step2Done)
        {
            yield return null;
        }

        currentStep = 4;
        if (currentBadge != null) StartCoroutine(Universal3DLabTutorial.FadeAndDestroyBadge(currentBadge));
    }

    public void CompleteStep1()
    {
        if (!step1Done)
        {
            step1Done = true;
            if (step1_Evaporate != null) step1_Evaporate.SetActive(false);
        }
    }

    public void CompleteStep2()
    {
        if (!step2Done)
        {
            step2Done = true;
            if (step2_ReadBook != null) step2_ReadBook.SetActive(false);
        }
    }

    public void CompleteStep3()
    {
        CompleteStep2();
    }
}

