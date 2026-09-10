using System.Collections;
using UnityEngine;
using ELab.Tutorial;

public class Lab3TutorialManager : MonoBehaviour
{
    [Header("Tutorial Text Objects (Legacy UI - Will be hidden)")]
    public GameObject step1_Salt;
    public GameObject step2_Goggles;
    public GameObject step3_Gas;
    public GameObject step4_Start;

    private int currentStep = 1;
    private GameObject currentBadge;

    private DragGaram3D garam3D;
    private GogglesInteraction3D goggles3D;
    private BunsenCollar3D bunsen3D;
    private HeaterPowerSwitch3D heater3D;

    private bool step1Done = false;
    private bool step2Done = false;
    private bool step3Done = false;
    private bool step4Done = false;

    void Start()
    {
        // Hide legacy 2D UI texts if assigned
        if (step1_Salt != null) step1_Salt.SetActive(false);
        if (step2_Goggles != null) step2_Goggles.SetActive(false);
        if (step3_Gas != null) step3_Gas.SetActive(false);
        if (step4_Start != null) step4_Start.SetActive(false);

        garam3D = Object.FindAnyObjectByType<DragGaram3D>();
        goggles3D = Object.FindAnyObjectByType<GogglesInteraction3D>();
        bunsen3D = Object.FindAnyObjectByType<BunsenCollar3D>();
        heater3D = Object.FindAnyObjectByType<HeaterPowerSwitch3D>();

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
        step4Done = false;

        StartCoroutine(TutorialSequence());
    }

    private IEnumerator TutorialSequence()
    {
        yield return new WaitForSeconds(1.0f);

        // Step 1: Salt
        Transform step1Target = null;
        if (garam3D != null) step1Target = garam3D.transform;

        currentBadge = Universal3DLabTutorial.CreateInstructionBadge(
            "1. Klik & Seret Bekas Garam ke dalam Bikar",
            step1Target,
            new Vector3(0.5f, 0.3f, 0f),
            Color.yellow,
            false // world space
        );

        while (!step1Done)
        {
            yield return null;
        }

        // Step 2: Goggles
        Transform step2Target = null;
        if (goggles3D != null) step2Target = goggles3D.transform;

        currentBadge = Universal3DLabTutorial.CreateInstructionBadge(
            "2. Klik Cermin Mata Keselamatan Untuk Memakai",
            step2Target,
            new Vector3(0f, 0.25f, 0f),
            new Color(0.2f, 1f, 0.8f),
            false
        );

        while (!step2Done)
        {
            yield return null;
        }

        // Step 3: Gas & Lighter - Positioned in front of the beakers to prevent clipping
        Transform step3Target = null;
        GameObject deskObj3 = GameObject.Find("Desk_1") ?? GameObject.Find("TeacherDeskTop");
        Vector3 step3Offset = new Vector3(0f, 0.25f, -0.82f);

        if (deskObj3 != null)
        {
            step3Target = deskObj3.transform;
        }
        else if (bunsen3D != null)
        {
            step3Target = bunsen3D.transform;
            step3Offset = new Vector3(0.4f, 0.25f, -0.82f);
        }

        currentBadge = Universal3DLabTutorial.CreateInstructionBadge(
            "3. Buka Injap Gas & Nyalakan Penunu Bunsen",
            step3Target,
            step3Offset,
            Color.yellow,
            false
        );

        GameObject startHeatBtn = GameObject.Find("Btn_StartHeating");
        UnityEngine.UI.Image startHeatImg = null;
        if (startHeatBtn != null)
        {
            startHeatImg = startHeatBtn.GetComponent<UnityEngine.UI.Image>();
        }

        while (!step3Done)
        {
            if (startHeatImg != null)
            {
                float pulse = Mathf.PingPong(Time.time * 3f, 1f);
                startHeatImg.color = Color.Lerp(new Color(0.15f, 0.7f, 0.15f), Color.white, pulse);
            }
            yield return null;
        }

        if (startHeatImg != null)
        {
            // Reset to the default 'Off' color (or whatever state WaterHeatingManager3D sets)
            startHeatImg.color = new Color(0.15f, 0.7f, 0.15f);
        }

        // Step 4: Start Heating & Record Temperature
        Transform step4Target = null;
        GameObject deskObj4 = GameObject.Find("Desk_1") ?? GameObject.Find("TeacherDeskTop");
        Vector3 step4Offset = new Vector3(0f, 0.25f, -0.87f);

        if (heater3D != null)
        {
            step4Target = heater3D.transform;
            step4Offset = new Vector3(0f, 0.25f, 0f);
        }
        else if (deskObj4 != null)
        {
            step4Target = deskObj4.transform;
        }

        currentBadge = Universal3DLabTutorial.CreateInstructionBadge(
            "4. Klik Pemetik Api untuk Mula Pemanasan & Rekod Suhu",
            step4Target,
            step4Offset,
            new Color(0.4f, 1f, 0.4f),
            false
        );

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
            if (step1_Salt != null) step1_Salt.SetActive(false);
        }
    }

    public void CompleteStep2()
    {
        if (currentStep == 2 && !step2Done)
        {
            step2Done = true;
            currentStep = 3;
            if (currentBadge != null) StartCoroutine(Universal3DLabTutorial.FadeAndDestroyBadge(currentBadge));
            if (step2_Goggles != null) step2_Goggles.SetActive(false);
        }
    }

    public void CompleteStep3()
    {
        if (currentStep == 3 && !step3Done)
        {
            step3Done = true;
            currentStep = 4;
            if (currentBadge != null) StartCoroutine(Universal3DLabTutorial.FadeAndDestroyBadge(currentBadge));
            if (step3_Gas != null) step3_Gas.SetActive(false);
        }
    }

    public void CompleteStep4()
    {
        if (currentStep == 4 && !step4Done)
        {
            step4Done = true;
            currentStep = 5;
            if (currentBadge != null) StartCoroutine(Universal3DLabTutorial.FadeAndDestroyBadge(currentBadge));
            if (step4_Start != null) step4_Start.SetActive(false);
        }
    }
}
