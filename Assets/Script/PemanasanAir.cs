using UnityEngine;
using TMPro;

public class WaterHeatingManager : MonoBehaviour
{
    [Header("Main UI & Visual Elements")]
    public System.Action OnExperimentComplete; // Added for future pop-up quiz trigger
    public TextMeshProUGUI tempTextA;
    public TextMeshProUGUI tempTextB;
    public GameObject bunsenFlameA;
    public GameObject bunsenFlameB;
    public SaltDragUI saltScript;
    public QuizManager quizManager;
    private bool quizTriggered = false; // Memastikan kuiz hanya dipanggil sekali
    [Header("Particle Systems")]
    public GameObject boilingBubblesA; 
    public GameObject boilingBubblesB; 
    
    [Header("State & Observation UI")]
    public TextMeshProUGUI stateTextA;
    public TextMeshProUGUI stateTextB;
    public TextMeshProUGUI observationTextA;
    public TextMeshProUGUI observationTextB;

    [Header("Warning UI")]
    public GameObject warningTextObject;

    [Header("Graph References")]
    public TemperatureGraphUI temperatureGraph; 
    private float experimentTimer = 0f;
    private float plotInterval = 1f; 
    private float nextPlotTime = 0f;

    [Header("Water Evaporation Visuals")]
    public RectTransform waterVisualA; // Drag Beaker A's water image here
    public RectTransform waterVisualB; // Drag Beaker B's water image here
    public float evaporationRate = 0.015f; // Speed of water level dropping
    private Vector3 originalWaterScaleA;
    private Vector3 originalWaterScaleB;

    [Header("Simulation Variables")]
    private float tempA = 27f; 
    private float tempB = 27f;
    private bool isHeating = false;
    private bool hasImpurities = false;
    
    private bool isBoilingA = false;
    private bool isBoilingB = false;

    private float heatingRate = 3.0f; // Kept at 1.0f for realistic 100s graph pacing
    private float boilingPointA = 100f; 
    private float boilingPointB = 104f; 

    void Start()
    {
        // Store the original scales of the water images for resetting later
        if (waterVisualA != null) originalWaterScaleA = waterVisualA.localScale;
        if (waterVisualB != null) originalWaterScaleB = waterVisualB.localScale;

        ResetExperimentUI();
        
        bunsenFlameA.SetActive(false);
        bunsenFlameB.SetActive(false);
        
        boilingBubblesA.SetActive(false);
        boilingBubblesB.SetActive(false);
        
        if (warningTextObject != null)
        {
            warningTextObject.SetActive(false);
        }
    }

    void Update()
    {
        if (isHeating)
        {
            // --- BEAKER A LOGIC (PURE WATER) ---
            if (tempA < boilingPointA)
            {
                tempA += heatingRate * Time.deltaTime;
                stateTextA.text = "State: Heating...";
            }
            else 
            {
                if (!isBoilingA)
                {
                    tempA = boilingPointA;
                    isBoilingA = true;
                    stateTextA.text = "State: Boiling!";
                    observationTextA.text = "Boiling Point A: " + boilingPointA.ToString("F1") + " °C";
                    boilingBubblesA.SetActive(true);
                }

                // Evaporation Effect for Beaker A
                EvaporateWater(waterVisualA);
            }

            // --- BEAKER B LOGIC (IMPURE WATER) ---
            if (tempB < boilingPointB)
            {
                tempB += heatingRate * Time.deltaTime;
                stateTextB.text = "State: Heating...";
            }
            else 
            {
                if (!isBoilingB)
                {
                    tempB = boilingPointB;
                    isBoilingB = true;
                    stateTextB.text = "State: Boiling!";
                    observationTextB.text = "Boiling Point B: " + boilingPointB.ToString("F1") + " °C";
                    boilingBubblesB.SetActive(true);
                }

                // Evaporation Effect for Beaker B
                EvaporateWater(waterVisualB);
            }

            UpdateTempUI();

            // --- GRAPH PLOTTING LOGIC ---
            experimentTimer += Time.deltaTime;
            if (experimentTimer >= nextPlotTime)
            {
                if (temperatureGraph != null)
                {
                    temperatureGraph.PlotPoint(experimentTimer, tempA, tempB);
                }
                nextPlotTime += plotInterval; 
            }
            // --- TRIGGER QUIZ LOGIC ---
         if (isBoilingA && isBoilingB && !quizTriggered)
         {
             quizTriggered = true;
             Invoke("TriggerQuizPopup", 3f); // Tunggu 3 saat sebelum kuiz keluar
         }
        }
    }

    private void EvaporateWater(RectTransform waterRect)
    {
        if (waterRect != null && waterRect.localScale.y > 0.3f) // Clamp so water doesn't completely disappear
        {
            Vector3 currentScale = waterRect.localScale;
            currentScale.y -= evaporationRate * Time.deltaTime;
            waterRect.localScale = currentScale;
        }
    }

    public void AddImpurities()
    {
        hasImpurities = true;
        HideWarning();
    }

    public void StartHeating()
    {
        if (hasImpurities && !isHeating)
        {
            isHeating = true;
            bunsenFlameA.SetActive(true);
            bunsenFlameB.SetActive(true);
            HideWarning();
        }
        else if (!hasImpurities)
        {
            ShowWarning();
        }
    }

    private void ShowWarning()
    {
        if (warningTextObject != null)
        {
            warningTextObject.SetActive(true);
            Invoke("HideWarning", 2.5f); 
        }
    }

    private void HideWarning()
    {
        if (warningTextObject != null)
        {
            warningTextObject.SetActive(false);
        }
    }

    public void ResetExperiment()
    {
        isHeating = false;
        hasImpurities = false;
        isBoilingA = false;
        isBoilingB = false;
        tempA = 27f;
        tempB = 27f;
        
        bunsenFlameA.SetActive(false);
        bunsenFlameB.SetActive(false);
        
        boilingBubblesA.SetActive(false);
        boilingBubblesB.SetActive(false);
        
        // Reset water levels back to full
        if (waterVisualA != null) waterVisualA.localScale = originalWaterScaleA;
        if (waterVisualB != null) waterVisualB.localScale = originalWaterScaleB;

        saltScript.ResetPosition();
        HideWarning();
        
        ResetExperimentUI();

        experimentTimer = 0f;
        nextPlotTime = 0f;
        if (temperatureGraph != null) 
        {
            temperatureGraph.ResetGraph();
        }
        quizTriggered = false; // Letakkan di dalam ResetExperiment()
    }
private void TriggerQuizPopup()
 {
     if (quizManager != null)
     {
         quizManager.ShowQuiz();
     }
 }
    private void UpdateTempUI()
    {
        tempTextA.text = tempA.ToString("F1") + "°C";
        tempTextB.text = tempB.ToString("F1") + "°C";
    }

    private void ResetExperimentUI()
    {
        UpdateTempUI();
        stateTextA.text = "State: Ready";
        stateTextB.text = "State: Ready";
        observationTextA.text = "Boiling Point A: ___ °C";
        observationTextB.text = "Boiling Point B: ___ °C";
    }
}