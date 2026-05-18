using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class PendulumInteraction : MonoBehaviour
{
    // Serializable class to group UI elements for each table row
    [System.Serializable]
    public class TableRowUI
    {
        public float targetLength; // Example: 20, 40, 60, 80, 100
        public TextMeshProUGUI time10SwingsText; // Column t (seconds)
        public TextMeshProUGUI periodText;       // Column T = t/10 (seconds)
        public TextMeshProUGUI periodSquaredText; // Column T² (seconds²)
    }

    [Header("Object References")]
    public LineRenderer lineRenderer;
    public Transform pivotPoint;

    [Header("Visual Settings")]
    public Vector3 hookOffset = new Vector3(0f, 0.5f, 0f);
    public float visualScaleMultiplier = 4f; 

    [Header("UI Canvas References (Right: Settings)")]
    public Slider lengthSlider;
    public TextMeshProUGUI lengthText;

    [Header("UI Canvas References (Left: Experiment Data)")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI swingCountText;
    public Button startButton;
    public Button resetButton;
    public Button saveButton; 

    [Header("Table Component References")]
    public List<TableRowUI> tableRows; // List to hold UI rows for the data table

    [Header("Clock Visual References")]
    public RectTransform clockHand; 
    public float secondsPerRevolution = 60f; 

    [Header("Real-World Physics")]
    public float gravity = 9.81f; 
    private float realLengthInMeters = 0.5f; 
    private float currentAngle = 0f; 
    private float angularVelocity = 0f; 

    private bool isDragging = false;
    private Camera mainCam;

    // --- TIMER & COUNTER VARIABLES ---
    private bool isExperimentRunning = false;
    private float timer = 0f;
    private int swingCount = 0;
    private int peakCount = 0;
    private float lastAngularVelocity = 0f;

    void Start()
    {
        mainCam = Camera.main;

        // Add listeners to slider and buttons
        if (lengthSlider != null)
        {
            lengthSlider.onValueChanged.AddListener(UpdatePendulumLength);
            UpdatePendulumLength(lengthSlider.value); 
        }

        if (startButton != null) startButton.onClick.AddListener(StartExperiment);
        if (resetButton != null) resetButton.onClick.AddListener(ResetExperiment);
        if (saveButton != null) saveButton.onClick.AddListener(SaveDataToTable);

        // Disable save button at the start of the simulation
        if (saveButton != null) saveButton.interactable = false; 

        currentAngle = 0f;
        UpdatePositionBasedOnAngle();
    }

    void Update()
    {
        // 1. DRAW THE STRING
        if (lineRenderer != null && pivotPoint != null)
        {
            lineRenderer.SetPosition(0, pivotPoint.position);
            
            // KUNCI PENYELESAIAN: Darabkan hookOffset dengan putaran (rotation) objek
            // Ini akan memaksa offset itu berpusing mengikut darjah kecondongan bola
            lineRenderer.SetPosition(1, transform.position + (transform.rotation * hookOffset));
        }

        // 2. MOUSE DRAG LOGIC
        if (isDragging)
        {
            // Convert mouse screen position to world coordinates, fixing Z depth issue
            Vector3 mouseScreenPos = Input.mousePosition;
            mouseScreenPos.z = Mathf.Abs(mainCam.transform.position.z); 
            Vector3 mousePos = mainCam.ScreenToWorldPoint(mouseScreenPos);
            mousePos.z = 0;

            Vector3 direction = mousePos - pivotPoint.position;
            currentAngle = Mathf.Atan2(direction.x, -direction.y);
            
            // Clamp the angle to prevent the pendulum from swinging over the top
            currentAngle = Mathf.Clamp(currentAngle, -Mathf.PI / 2.2f, Mathf.PI / 2.2f); 

            UpdatePositionBasedOnAngle();

            // Release the pendulum
            if (Input.GetMouseButtonUp(0))
            {
                isDragging = false;
            }
        }
        else 
        {
            // 3. PENDULUM PHYSICS LOGIC
            float angularAcceleration = -(gravity / realLengthInMeters) * Mathf.Sin(currentAngle);
            angularVelocity += angularAcceleration * Time.deltaTime;
            
            // Apply slight air resistance (damping)
            angularVelocity *= 0.9995f; 
            currentAngle += angularVelocity * Time.deltaTime;

            UpdatePositionBasedOnAngle();

            // 4. STOPWATCH AND SWING COUNTER LOGIC
            if (isExperimentRunning)
            {
                timer += Time.deltaTime;
                
                // Update UI Text
                if (timerText != null) timerText.text = "TIME: " + timer.ToString("F2") + " s";

                // Rotate the clock hand based on the timer
                if (clockHand != null)
                {
                    // Negative angle ensures clockwise rotation
                    float angle = - (timer / secondsPerRevolution) * 360f;
                    clockHand.localEulerAngles = new Vector3(0, 0, angle);
                }

                // Detect peaks (when angular velocity changes sign) to count swings
                if ((lastAngularVelocity > 0 && angularVelocity <= 0) || (lastAngularVelocity < 0 && angularVelocity >= 0))
                {
                    peakCount++;
                    
                    // 2 peaks (left and right) = 1 complete swing
                    if (peakCount >= 2)
                    {
                        swingCount++;
                        peakCount = 0; // Reset for the next swing
                        
                        if (swingCountText != null) 
                            swingCountText.text = "COMPLETE SWINGS: " + swingCount + "/10";

                        // Automatically stop the experiment at the 10th swing
                        if (swingCount >= 10)
                        {
                            isExperimentRunning = false;
                            
                            // Enable the save data button once 10 swings are completed
                            if (saveButton != null) saveButton.interactable = true; 
                        }
                    }
                }
            }
            // Store the current velocity for the next frame's comparison
            lastAngularVelocity = angularVelocity;
        }
    }

    void UpdatePositionBasedOnAngle()
    {
        // 1. Logik Kedudukan (Position) sedia ada
        float visualLength = realLengthInMeters * visualScaleMultiplier;
        float x = pivotPoint.position.x + Mathf.Sin(currentAngle) * visualLength;
        float y = pivotPoint.position.y - Mathf.Cos(currentAngle) * visualLength;
        transform.position = new Vector3(x, y, 0);

        // 2. LOGIK PUTARAN (ROTATION) BAHARU
        // Tukar sudut dari radian ke darjah
        float angleInDegrees = currentAngle * Mathf.Rad2Deg; 
        
        // Putarkan paksi Z objek bola ini
        transform.localEulerAngles = new Vector3(0, 0, angleInDegrees);
    }

    void OnMouseDown()
    {
        isDragging = true;
        angularVelocity = 0f; // Stop momentum when grabbed
        
        // Reset peak count if the user grabs the ball while the experiment is running
        if (isExperimentRunning) peakCount = 0; 
    }

    public void UpdatePendulumLength(float sliderValue)
    {
        if (lengthText != null) lengthText.text = sliderValue.ToString("0") + " cm";
        
        // Convert cm to meters for accurate physics calculation
        realLengthInMeters = sliderValue / 100f; 

        // Snap to center if it's not being dragged and is almost stationary
        if (!isDragging && Mathf.Abs(angularVelocity) < 0.05f) currentAngle = 0f;
        
        UpdatePositionBasedOnAngle();
    }

    // --- BUTTON FUNCTIONS ---
    public void StartExperiment()
    {
        isExperimentRunning = true;
        timer = 0f;
        swingCount = 0;
        peakCount = 0;
        
        if (swingCountText != null) swingCountText.text = "COMPLETE SWINGS: 0/10";
        if (timerText != null) timerText.text = "TIME: 0.00 s";
        if (clockHand != null) clockHand.localEulerAngles = Vector3.zero; // Reset clock hand
        if (saveButton != null) saveButton.interactable = false;
    }

    public void ResetExperiment()
    {
        isExperimentRunning = false;
        timer = 0f;
        swingCount = 0;
        peakCount = 0;
        
        if (swingCountText != null) swingCountText.text = "COMPLETE SWINGS: 0/10";
        if (timerText != null) timerText.text = "TIME: 0.00 s";
        if (clockHand != null) clockHand.localEulerAngles = Vector3.zero;
        if (saveButton != null) saveButton.interactable = false;

        // Clear the entire UI table when the Reset button is clicked
        foreach (var row in tableRows)
        {
            if (row.time10SwingsText != null) row.time10SwingsText.text = "-";
            if (row.periodText != null) row.periodText.text = "-";
            if (row.periodSquaredText != null) row.periodSquaredText.text = "-";
        }
    }

    // --- TABLE POPULATION LOGIC ---
    // --- TABLE POPULATION LOGIC WITH DEBUG LOGS ---
    public void SaveDataToTable()
    {
        // Get the current length value from the slider
        float currentLength = Mathf.Round(lengthSlider.value);
        
        // 1. Ini akan keluar di Console untuk memberitahu butang berjaya dikesan
        Debug.Log("Butang SIMPAN DATA ditekan! Nilai Slider Semasa: " + currentLength + "cm");

        if (tableRows == null || tableRows.Count == 0)
        {
            Debug.LogError("Ralat: Senarai 'Table Rows' di Inspector kosong! Sila isi.");
            return;
        }

        foreach (var row in tableRows)
        {
            // 2. Ini akan memaparkan proses semakan bagi setiap baris
            Debug.Log("Sedang menyemak baris jadual. Mencari Target Length: " + row.targetLength + "cm...");
            
            if (Mathf.Abs(row.targetLength - currentLength) < 2f) 
            {
                // 3. Jika padanan dijumpai, log ini akan keluar
                Debug.Log("Padanan DIJUMPAI untuk panjang: " + row.targetLength + "cm. Memasukkan data ke teks UI...");

                float t = timer;
                float T = t / 10f; 
                float T2 = T * T;  

                if (row.time10SwingsText != null) row.time10SwingsText.text = t.ToString("F2");
                if (row.periodText != null) row.periodText.text = T.ToString("F2");
                if (row.periodSquaredText != null) row.periodSquaredText.text = T2.ToString("F3");

                if (saveButton != null) saveButton.interactable = false;
                return; 
            }
        }
        
        // 4. Jika fungsi habis berpusing dan TIADA padanan dijumpai
        Debug.LogWarning("Amaran: Kod gagal mencari baris yang sepadan dengan nilai " + currentLength + "cm di dalam Table Rows!");
    }
}