using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SolubilityManager3D : MonoBehaviour
{
    [Header("3D Visual References")]
    public Transform rodVisual;     
    public Transform sugarCubeVisual;   
    public Transform sugarFineVisual;
    public MeshRenderer heatingPlateRenderer;       
    public ParticleSystem particleBuih; 
    public ParticleSystem particleSteam;
    public ParticleSystem particleHaze;
    public MeshRenderer waterRenderer;

    [Header("Control Sliders")]
    public Slider tempSlider;   
    public Slider stirSlider;   
    public Slider sizeSlider;   

    [Header("UI Text References")]
    public TextMeshProUGUI solubilityRateText; 
    public TextMesh hologramText; 

    [Header("Heater Power Switch")]
    public SolubilityHeaterPowerSwitch3D powerSwitch;
    public Renderer ledIndicator; 
    public Transform heaterKnob; 

    [Header("Graph References")]
    public SolubilityGraph3D graph3D;

    [Header("Engine Settings")]
    public float baseSpeed = 0.5f; 

    public bool experimentStarted = false;
    public bool experimentFinished = false;

    private bool isDissolving = false;
    private float currentRate = 0f;
    private Vector3 saizAsalCube;
    private Vector3 saizAsalFine;
    private bool isHeaterPowerOn = false;
    private float elapsedTime = 0f;

    private MaterialPropertyBlock propBlock;

    void Start()
    {
        if (sugarCubeVisual != null) saizAsalCube = sugarCubeVisual.localScale;
        if (sugarFineVisual != null) saizAsalFine = sugarFineVisual.localScale;

        if (particleBuih != null) particleBuih.Stop();
        
        propBlock = new MaterialPropertyBlock();

        if (graph3D != null) graph3D.ResetGraph();

        // Force-enable URP emission keywords on local material instances at runtime
        if (heatingPlateRenderer != null)
        {
            foreach (var mat in heatingPlateRenderer.materials)
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", Color.black);
            }
        }
        if (ledIndicator != null)
        {
            foreach (var mat in ledIndicator.materials)
            {
                mat.EnableKeyword("_EMISSION");
            }
        }

        // Find Tablet and bind references dynamically at runtime
        GameObject tablet = GameObject.Find("Lab5_3DTablet");
        if (tablet != null)
        {
            Transform leftTrans = tablet.transform.Find("TabletCanvas/Left");
            if (leftTrans != null)
            {
                tempSlider = leftTrans.Find("GameObject/SuhuSlider")?.GetComponent<Slider>();
                stirSlider = leftTrans.Find("GameObject/KadarKacauSlider")?.GetComponent<Slider>();
                sizeSlider = leftTrans.Find("GameObject/SizeSlider")?.GetComponent<Slider>();

                if (tempSlider != null) tempSlider.value = 0f;
                if (stirSlider != null) stirSlider.value = 0f;
                if (sizeSlider != null) sizeSlider.value = 0f;

                solubilityRateText = leftTrans.Find("Bottom/KadarPenyejatan/Text (TMP)")?.GetComponent<TextMeshProUGUI>();

                Button startBtn = leftTrans.Find("Bottom/Button")?.GetComponent<Button>();
                if (startBtn != null)
                {
                    startBtn.onClick.RemoveAllListeners();
                    startBtn.onClick.AddListener(MulaPelarutan);
                }

                Button resetBtn = leftTrans.Find("Bottom/ResetBtn")?.GetComponent<Button>();
                if (resetBtn != null)
                {
                    resetBtn.onClick.RemoveAllListeners();
                    resetBtn.onClick.AddListener(ResetEksperimen);
                }
            }
        }

        SetHeaterPower(false);
    }

    public void SetHeaterPower(bool on)
    {
        isHeaterPowerOn = on;
        
        // Update LED emission
        if (ledIndicator != null)
        {
            MaterialPropertyBlock block = new MaterialPropertyBlock();
            ledIndicator.GetPropertyBlock(block);
            block.SetColor("_EmissionColor", on ? new Color(1f, 0.1f, 0.1f) : Color.black);
            ledIndicator.SetPropertyBlock(block);
        }
    }

    void Update()
    {
        // 1. CALCULATE SOLUBILITY RATE
        float faktorSuhu = (tempSlider != null && isHeaterPowerOn) ? tempSlider.value : 0f;
        float faktorKacauan = stirSlider != null ? stirSlider.value : 0f;
        float faktorSaiz = sizeSlider != null ? sizeSlider.value : 0f; 

        // Realistic chemical dissolving formula (takes ~8s at max settings, ~33s at min settings)
        currentRate = (0.006f + (faktorSuhu * 0.008f) + (faktorKacauan * 0.007f) + (faktorSaiz * 0.004f));

        if (solubilityRateText != null && !experimentFinished)
        {
            solubilityRateText.text = "Kadar Keterlarutan: " + currentRate.ToString("F3") + " unit/s";
        }

        if (hologramText != null && !experimentFinished)
        {
            hologramText.text = "Kadar Keterlarutan:\n" + currentRate.ToString("F3") + " unit/s";
        }

        // 2. TOGGLE SUGAR MODEL LIVE (only if not currently dissolving)
        if (!isDissolving)
        {
            bool useFineSugar = faktorSaiz >= 0.5f;
            if (sugarCubeVisual != null) sugarCubeVisual.gameObject.SetActive(!useFineSugar);
            if (sugarFineVisual != null) sugarFineVisual.gameObject.SetActive(useFineSugar);
        }

        // 3. 3D GLASS ROD STIRRING ANIMATION
        if (rodVisual != null)
        {
            if (faktorKacauan > 0)
            {
                float stirSpeed = faktorKacauan * 20f;
                rodVisual.localRotation = Quaternion.Euler(
                    Mathf.Sin(Time.time * stirSpeed) * 15f, 
                    Time.time * stirSpeed * 100f, 
                    Mathf.Cos(Time.time * stirSpeed) * 15f
                );
            }
            else
            {
                rodVisual.localRotation = Quaternion.Lerp(rodVisual.localRotation, Quaternion.Euler(15f, 0, 0), Time.deltaTime * 5f);
            }
        }

        // 4. HOT PLATE GLOW & BUBBLES
        if (heatingPlateRenderer != null)
        {
            float intensity = isHeaterPowerOn ? Mathf.Clamp01((faktorSuhu - 0.2f) / 0.8f) : 0f;
            if (intensity > 0)
            {
                Color glowColor = new Color(0.9f, 0.2f, 0.02f) * intensity * 0.45f; // Professional subtle amber-red hot plate glow
                if (propBlock == null) propBlock = new MaterialPropertyBlock();
                heatingPlateRenderer.GetPropertyBlock(propBlock);
                propBlock.SetColor("_EmissionColor", glowColor);
                heatingPlateRenderer.SetPropertyBlock(propBlock);
            }
            else
            {
                if (propBlock == null) propBlock = new MaterialPropertyBlock();
                heatingPlateRenderer.GetPropertyBlock(propBlock);
                propBlock.SetColor("_EmissionColor", Color.black);
                heatingPlateRenderer.SetPropertyBlock(propBlock);
            }
        }

        // Animate Knob
        if (heaterKnob != null && tempSlider != null)
        {
            heaterKnob.localEulerAngles = new Vector3(0f, 0f, -tempSlider.value * 270f);
        }

        // Animate LED Indicator (black if off, green if standby, red if heating)
        if (ledIndicator != null)
        {
            if (propBlock == null) propBlock = new MaterialPropertyBlock();
            ledIndicator.GetPropertyBlock(propBlock);
            Color ledColor = Color.black;
            if (isHeaterPowerOn)
            {
                if (tempSlider != null && tempSlider.value > 0.05f)
                {
                    ledColor = new Color(2f, 0f, 0f); // Red active heating
                }
                else
                {
                    ledColor = new Color(0f, 1.5f, 0f); // Green standby
                }
            }
            propBlock.SetColor("_EmissionColor", ledColor);
            ledIndicator.SetPropertyBlock(propBlock);
        }

        if (particleBuih != null)
        {
            var buihEmission = particleBuih.emission;
            if (isHeaterPowerOn && faktorSuhu > 0.5f)
            {
                if (!particleBuih.isPlaying) particleBuih.Play();
                buihEmission.rateOverTime = (faktorSuhu - 0.5f) * 60f; 
            }
            else
            {
                if (particleBuih.isPlaying) particleBuih.Stop();
                buihEmission.rateOverTime = 0f;
            }
        }

        if (particleSteam != null)
        {
            var steamEmission = particleSteam.emission;
            if (isHeaterPowerOn && faktorSuhu > 0.3f)
            {
                if (!particleSteam.isPlaying) particleSteam.Play();
                steamEmission.rateOverTime = (faktorSuhu - 0.3f) * 25f;
            }
            else
            {
                if (particleSteam.isPlaying) particleSteam.Stop();
                steamEmission.rateOverTime = 0f;
            }
        }

        if (particleHaze != null)
        {
            var hazeEmission = particleHaze.emission;
            if (isHeaterPowerOn && faktorSuhu > 0.4f)
            {
                if (!particleHaze.isPlaying) particleHaze.Play();
                hazeEmission.rateOverTime = (faktorSuhu - 0.4f) * 15f;
            }
            else
            {
                if (particleHaze.isPlaying) particleHaze.Stop();
                hazeEmission.rateOverTime = 0f;
            }
        }

        // Animated Water Surface Tint & Ripple
        if (waterRenderer != null && waterRenderer.material != null)
        {
            Color coolBlue = new Color(0.12f, 0.58f, 0.85f, 0.82f);
            Color warmTeal = new Color(0.15f, 0.75f, 0.80f, 0.82f);
            Color heatedCyan = new Color(0.20f, 0.88f, 0.92f, 0.82f);
            float t = isHeaterPowerOn ? Mathf.Clamp01(faktorSuhu) : 0f;
            Color targetColor = t < 0.5f ? Color.Lerp(coolBlue, warmTeal, t * 2f) : Color.Lerp(warmTeal, heatedCyan, (t - 0.5f) * 2f);
            waterRenderer.material.color = targetColor;
            if (waterRenderer.material.HasProperty("_BaseColor"))
            {
                waterRenderer.material.SetColor("_BaseColor", targetColor);
            }
            if (waterRenderer.material.HasProperty("_MainTex"))
            {
                Vector2 offset = waterRenderer.material.GetTextureOffset("_MainTex");
                offset += new Vector2(Time.deltaTime * (0.02f + faktorKacauan * 0.15f), Time.deltaTime * (0.01f + faktorKacauan * 0.1f));
                waterRenderer.material.SetTextureOffset("_MainTex", offset);
            }
        }

        // 5. DISSOLVING PROCESS
        if (isDissolving)
        {
            elapsedTime += Time.deltaTime;
            Transform activeSugar = (sugarFineVisual != null && sugarFineVisual.gameObject.activeSelf) ? sugarFineVisual : sugarCubeVisual;
            Vector3 originalSize = (activeSugar == sugarFineVisual) ? saizAsalFine : saizAsalCube;

            if (activeSugar != null && activeSugar.localScale.x > 0)
            {
                Vector3 newSize = activeSugar.localScale;
                newSize.x -= currentRate * Time.deltaTime;
                newSize.y -= currentRate * Time.deltaTime;
                newSize.z -= currentRate * Time.deltaTime;
                
                if (newSize.x <= 0) 
                {
                    newSize = Vector3.zero;
                    isDissolving = false;
                    experimentFinished = true;
                    if (solubilityRateText != null) solubilityRateText.text = "Eksperimen Selesai! Klik [Log Data] Di Buku";
                    if (hologramText != null) hologramText.text = "SELESAI!\nSila Klik [Log Data]\npada Buku Laporan";
                    if (graph3D != null) graph3D.FinishGraph(elapsedTime);
                }
                
                activeSugar.localScale = newSize;

                // Update graph
                float remainingRatio = originalSize.x > 0 ? (newSize.x / originalSize.x) : 0f;
                float sugarRemaining = remainingRatio * 10f; // 10g max
                if (graph3D != null)
                {
                    graph3D.AddDataPoint(elapsedTime, sugarRemaining);
                }

                // Alpha fade as sugar shrinks
                MeshRenderer sugarRenderer = activeSugar.GetComponent<MeshRenderer>();
                if (sugarRenderer != null && sugarRenderer.material != null)
                {
                    Color c = sugarRenderer.material.color;
                    c.a = Mathf.Clamp(remainingRatio, 0.05f, 1.0f);
                    sugarRenderer.material.color = c;
                    if (sugarRenderer.material.HasProperty("_BaseColor"))
                    {
                        sugarRenderer.material.SetColor("_BaseColor", c);
                    }
                }
            }
        }
    }

    public void MulaPelarutan()
    {
        Transform activeSugar = (sugarFineVisual != null && sugarFineVisual.gameObject.activeSelf) ? sugarFineVisual : sugarCubeVisual;
        if (activeSugar != null && activeSugar.localScale.x > 0)
        {
            isDissolving = true;
            experimentStarted = true;
            experimentFinished = false;
            elapsedTime = 0f;
            if (graph3D != null) graph3D.ResetGraph();
        }
    }

    public void ResetEksperimen()
    {
        isDissolving = false;
        experimentStarted = false;
        experimentFinished = false;
        elapsedTime = 0f;
        if (graph3D != null) graph3D.ResetGraph();
        
        if (sugarCubeVisual != null) 
        {
            sugarCubeVisual.gameObject.SetActive(true);
            sugarCubeVisual.localScale = saizAsalCube;
            ResetSugarAlpha(sugarCubeVisual);
        }
        if (sugarFineVisual != null) 
        {
            sugarFineVisual.gameObject.SetActive(false);
            sugarFineVisual.localScale = saizAsalFine;
            ResetSugarAlpha(sugarFineVisual);
        }

        if (powerSwitch != null)
        {
            powerSwitch.ResetSwitch();
        }
        SetHeaterPower(false);

        if (heaterKnob != null) heaterKnob.localEulerAngles = Vector3.zero;
        if (rodVisual != null) rodVisual.localRotation = Quaternion.Euler(15f, 0f, 0f);

        if (particleBuih != null) { particleBuih.Stop(); var em = particleBuih.emission; em.rateOverTime = 0f; }
        if (particleSteam != null) { particleSteam.Stop(); var em = particleSteam.emission; em.rateOverTime = 0f; }
        if (particleHaze != null) { particleHaze.Stop(); var em = particleHaze.emission; em.rateOverTime = 0f; }

        if (waterRenderer != null && waterRenderer.material != null)
        {
            Color coolBlue = new Color(0.12f, 0.58f, 0.85f, 0.82f);
            waterRenderer.material.color = coolBlue;
            if (waterRenderer.material.HasProperty("_BaseColor")) waterRenderer.material.SetColor("_BaseColor", coolBlue);
        }

        if (tempSlider != null) tempSlider.value = 0f;
        if (stirSlider != null) stirSlider.value = 0f;
        if (sizeSlider != null) sizeSlider.value = 0f;

        currentRate = 0.006f;
        if (solubilityRateText != null)
        {
            solubilityRateText.text = "Kadar Keterlarutan: " + currentRate.ToString("F3") + " unit/s";
        }
        if (hologramText != null)
        {
            hologramText.text = "Kadar Keterlarutan:\n" + currentRate.ToString("F3") + " unit/s";
        }

        var nb = Object.FindAnyObjectByType<InteractiveNotebookLab5>();
        if (nb != null) nb.ResetNotebook();

        var tutMgr = Object.FindAnyObjectByType<Lab5TutorialManager>();
        if (tutMgr != null) tutMgr.ResetTutorial(true);
    }

    private void ResetSugarAlpha(Transform sugarTrans)
    {
        MeshRenderer r = sugarTrans.GetComponent<MeshRenderer>();
        if (r != null && r.material != null)
        {
            Color c = r.material.color;
            c.a = 1.0f;
            r.material.color = c;
            if (r.material.HasProperty("_BaseColor")) r.material.SetColor("_BaseColor", c);
        }
    }
}
