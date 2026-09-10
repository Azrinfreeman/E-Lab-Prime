#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class Lab3_3D_Setup : EditorWindow
{
    [MenuItem("E-Lab Tools/Generate 3D Lab 3 (Water Heating)")]
    public static void ShowWindow()
    {
        GenerateLab3_3D();
    }

    private static void GenerateLab3_3D()
    {
        if (EditorSceneManager.GetActiveScene().name != "Lab3_3D")
        {
            EditorSceneManager.OpenScene("Assets/Scenes/Lab3_3D.unity", OpenSceneMode.Single);
        }

        DestroyDuplicateLights();
        
        // 0. Import Environment from Lab 1 and calculate table offset
        Vector3 experimentOffset = Vector3.zero;
        GameObject env = CopyEnvironmentFromLab1();
        if (env != null)
        {
            // Clean up Lab 1 specific objects that came with the environment
            string[] lab1Objects = { "Beaker_1", "Beaker_2", "Liquid_1", "Liquid_2", "BurnerBase", "BurnerTube" };
            foreach (string objName in lab1Objects)
            {
                Transform oldObj = env.transform.Find(objName);
                if (oldObj != null) DestroyImmediate(oldObj.gameObject);
            }

            // Change Whiteboard Title
            GameObject titleObj = GameObject.Find("Whiteboard_Title");
            if (titleObj != null)
            {
                var tmps = titleObj.GetComponentsInChildren<TMPro.TextMeshPro>(true);
                foreach (var t in tmps) 
                {
                    t.text = "AMALI 3 PEMANASAN AIR";
                    t.color = Color.white;
                }
                
                var tmpUIs = titleObj.GetComponentsInChildren<TMPro.TextMeshProUGUI>(true);
                foreach (var t in tmpUIs) 
                {
                    t.text = "AMALI 3 PEMANASAN AIR";
                    t.color = Color.white;
                }
                
                var txts = titleObj.GetComponentsInChildren<UnityEngine.UI.Text>(true);
                foreach (var t in txts) 
                {
                    t.text = "AMALI 3 PEMANASAN AIR";
                    t.color = Color.white;
                }
            }

            // Darken the whiteboard
            MeshRenderer[] envRenderers = env.GetComponentsInChildren<MeshRenderer>(true);
            foreach (MeshRenderer r in envRenderers)
            {
                string rName = r.gameObject.name.ToLower();
                if (rName.Contains("whiteboard") || rName.Contains("board"))
                {
                    if (r.sharedMaterial != null)
                    {
                        Material darkMat = new Material(r.sharedMaterial);
                        darkMat.color = new Color(0.25f, 0.28f, 0.32f, 1f); // Dark slate grey
                        r.material = darkMat;
                    }
                }
            }

            // Set Directional Lights and Ambient Lighting
            Light[] lights = Object.FindObjectsByType<Light>(FindObjectsInactive.Include);
            foreach(Light l in lights)
            {
                if (l.type == LightType.Directional)
                {
                    l.intensity = 1.6f;
                    l.color = new Color(0.98f, 0.99f, 1f); 
                    l.shadows = LightShadows.Soft;
                    l.shadowStrength = 0.6f;
                }
            }

            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = 0.015f;
            RenderSettings.fogColor = new Color(0.1f, 0.15f, 0.2f);
            if (Camera.main != null)
            {
                Camera.main.clearFlags = CameraClearFlags.SolidColor;
                Camera.main.backgroundColor = new Color(0.1f, 0.15f, 0.2f);
            }

            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.85f, 0.9f, 0.95f);

            GameObject deskTop = GameObject.Find("Desk_1");
            if (deskTop == null) deskTop = GameObject.Find("TeacherDeskTop");
            if (deskTop != null)
            {
                MeshRenderer rend = deskTop.GetComponent<MeshRenderer>();
                if (rend != null)
                {
                    float tableY = rend.bounds.max.y;
                    experimentOffset = new Vector3(deskTop.transform.position.x, tableY, deskTop.transform.position.z);
                }
            }

            // Apply DeskMat and WoodMat to desk/table materials (or darken fallback)
            Material deskMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/DeskMat.mat");
            Material woodMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/WoodMat.mat");
            MeshRenderer[] allEnvRenderers = env.GetComponentsInChildren<MeshRenderer>(true);
            foreach (MeshRenderer r in allEnvRenderers)
            {
                if (r.sharedMaterial != null)
                {
                    string objName = r.gameObject.name.ToLower();
                    string matName = r.sharedMaterial.name.ToLower();
                    
                    if (objName.Contains("stoolseat"))
                    {
                        if (woodMat != null)
                        {
                            r.sharedMaterial = woodMat;
                        }
                    }
                    else if (objName.Contains("desk") || objName.Contains("table") || matName.Contains("epoxy") || matName.Contains("wood"))
                    {
                        bool isTop = objName.Contains("top") || objName.Contains("surface") || matName.Contains("epoxy") || objName == "desk_1";
                        if (isTop && deskMat != null)
                        {
                            r.sharedMaterial = deskMat;
                        }
                        else if (woodMat != null)
                        {
                            r.sharedMaterial = woodMat;
                        }
                        else if (deskMat != null)
                        {
                            r.sharedMaterial = deskMat;
                        }
                        else
                        {
                            Material tableDarkMat = new Material(r.sharedMaterial);
                            if (tableDarkMat.HasProperty("_Color")) 
                            {
                                tableDarkMat.color = new Color(0.1f, 0.12f, 0.15f, 1f);
                            }
                            if (tableDarkMat.HasProperty("_Metallic")) tableDarkMat.SetFloat("_Metallic", 0.8f);
                            if (tableDarkMat.HasProperty("_Glossiness")) tableDarkMat.SetFloat("_Glossiness", 0.75f);
                            if (tableDarkMat.HasProperty("_Smoothness")) tableDarkMat.SetFloat("_Smoothness", 0.75f);
                            
                            r.material = tableDarkMat;
                        }
                    }
                }
            }

            // Adjust Main Camera to look at the table setup
            if (Camera.main != null)
            {
                Camera.main.transform.position = experimentOffset + new Vector3(0, 1.2f, -2.5f);
                Camera.main.transform.LookAt(experimentOffset + new Vector3(0, 0.6f, 0));
                Camera.main.nearClipPlane = 0.05f;
                
                AudioSource camAudio = Camera.main.GetComponent<AudioSource>();
                if (camAudio != null)
                {
                    camAudio.volume = 0.02f; // Make ambient hum extremely subtle
                }
            }

            Canvas mainCanvas = Object.FindAnyObjectByType<Canvas>();
            if (mainCanvas != null)
            {
                mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            }
        }

        // 1. Clean up existing generated objects to ensure clean regeneration
        // Lab 3 specific objects
        DestroyIfExists("BeakerA_3D");
        DestroyIfExists("BeakerB_3D");
        DestroyIfExists("TripodStandA_3D");
        DestroyIfExists("TripodStandB_3D");
        DestroyIfExists("BurnerA_3D");
        DestroyIfExists("BurnerB_3D");
        DestroyIfExists("RetortStandA_3D");
        DestroyIfExists("RetortStandB_3D");
        DestroyIfExists("SaltBottle_3D");
        DestroyIfExists("Lighter_3D");
        DestroyIfExists("SafetyGoggles_3D");
        DestroyIfExists("Lab3_RealTimeGraph");
        DestroyIfExists("HologramDataBoard");
        DestroyIfExists("Lab3_3DTablet");
        DestroyIfExists("Lab3_SummaryPanel");
        DestroyIfExists("Lab3_Notebook");
        DestroyIfExists("Lab3_GridMat");
        DestroyIfExists("RotateCameraButton");
        DestroyIfExists("WaterHeatingManager3D_Controller");
        DestroyIfExists("Global_PostProcessing");
        DestroyIfExists("TutorialManager");
        DestroyIfExists("Tut_Step1");
        DestroyIfExists("Tut_Step2");
        DestroyIfExists("Tut_Step3");
        DestroyIfExists("Tut_Step4");
        DestroyIfExists("Stopwatch_3D");
        DestroyIfExists("AmbientAudio_3D");
        if (AssetDatabase.LoadAssetAtPath<Object>("Assets/Materials/Lab3_VolumeProfile.asset") != null)
            AssetDatabase.DeleteAsset("Assets/Materials/Lab3_VolumeProfile.asset");

        // Lab 3 unnamed duplicates or helper dummies
        DestroyIfExists("WaterAPivot");
        DestroyIfExists("WaterBPivot");
        DestroyIfExists("Thermometer");
        DestroyIfExists("Beaker_3D");

        // Lab 2 specific objects (Clean up remnants if any)
        DestroyIfExists("BeakerGlowLight");
        DestroyIfExists("TestTube_Anode (Oxygen)");
        DestroyIfExists("AnodeWaterPivot");
        DestroyIfExists("Electrode_Anode");
        DestroyIfExists("TestTube_Cathode (Hydrogen)");
        DestroyIfExists("CathodeWaterPivot");
        DestroyIfExists("Electrode_Cathode");
        DestroyIfExists("Battery_9V_3D");
        DestroyIfExists("Ammeter_3D");
        DestroyIfExists("PhysicalSwitch_3D");
        DestroyIfExists("Wire_Cathode (Black)");
        DestroyIfExists("Wire_Bat_Ammeter (Red)");
        DestroyIfExists("Wire_Ammeter_Switch (Red)");
        DestroyIfExists("Wire_Anode (Red)");
        DestroyIfExists("RetortStand_3D");
        DestroyIfExists("Lab2_GridMat");
        DestroyIfExists("Lab2_Notebook");
        DestroyIfExists("Lab2_ChemicalBottle_H2SO4");
        DestroyIfExists("Lab2_ChemicalBottle_H2O");
        DestroyIfExists("Lab2_ChemicalBottle_Indicator");
        DestroyIfExists("Lab2_3DTablet");
        DestroyIfExists("Electrolysis_Experiment");
        DestroyIfExists("ElectrolysisManager3D_Controller");
        DestroyIfExists("Lab2_RealTimeGraph");

        // 2. Create or Load Materials properly as Assets
        Material glassMat = GetOrCreateMaterial("GlassMat_Lab3", new Color(0.9f, 0.93f, 0.96f, 0.24f), true);
        Material waterMat = GetOrCreateMaterial("WaterMat_Lab3", new Color(0.12f, 0.58f, 0.85f, 0.82f), true);
        Material metalMat = GetOrCreateMaterial("MetalMat_Lab3", new Color(0.15f, 0.15f, 0.15f, 1f), false);
        Material rodMat = GetOrCreateMaterial("RodMat_Lab3", new Color(0.7f, 0.7f, 0.7f, 1f), false);
        Material flameMat = GetOrCreateMaterial("FlameMat_Lab3", new Color(0.1f, 0.5f, 1f, 0.8f), true); // Hot blue Bunsen flame
        flameMat.EnableKeyword("_EMISSION");
        flameMat.SetColor("_EmissionColor", new Color(0f, 0.4f, 1f));

        Material redCapillaryMat = GetOrCreateMaterial("RedCapillaryMat_Lab3", new Color(1f, 0f, 0f, 1f), false);
        Material saltBottleMat = GetOrCreateMaterial("SaltBottleMat_Lab3", new Color(0.9f, 0.9f, 0.92f, 1f), false);

        // Adjust transparent sorting
        waterMat.renderQueue = 3000;
        glassMat.renderQueue = 3002;
        flameMat.renderQueue = 3004;

        AssetDatabase.SaveAssets();

        // 3. Programmatic Tripod Stand Construction (Beaker A at -0.4f, Beaker B at 0.4f)
        GameObject tripodA = CreateTripodStand("TripodStandA_3D", experimentOffset + new Vector3(-0.4f, 0f, 0f), metalMat, rodMat);
        GameObject tripodB = CreateTripodStand("TripodStandB_3D", experimentOffset + new Vector3(0.4f, 0f, 0f), metalMat, rodMat);

        // 4. Create 3D Beakers sitting on wire gauze (Y = 0.33m, beaker center lifted slightly to Y = 0.625m to clear gauze)
        GameObject beakerA = new GameObject("BeakerA_3D");
        beakerA.transform.position = experimentOffset + new Vector3(-0.4f, 0.625f, 0f);
        beakerA.transform.localScale = new Vector3(0.4f, 0.28f, 0.4f);
        beakerA.AddComponent<MeshFilter>().sharedMesh = GenerateOpenBeakerMesh();
        beakerA.AddComponent<MeshRenderer>().sharedMaterial = glassMat;
        CreateBeakerOutline(beakerA);

        // Condensation cylinder inside Beaker A
        Material condensationMat = GetOrCreateMaterial("CondensationMat_Lab3", new Color(1f, 1f, 1f, 0f), true);
        GameObject condA = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        condA.name = "CondensationMesh";
        condA.transform.SetParent(beakerA.transform, false);
        condA.transform.localPosition = new Vector3(0f, 0.5f, 0f);
        condA.transform.localScale = new Vector3(0.97f, 0.45f, 0.97f);
        condA.GetComponent<MeshRenderer>().sharedMaterial = condensationMat;
        DestroyImmediate(condA.GetComponent<Collider>());

        GameObject beakerB = new GameObject("BeakerB_3D");
        beakerB.transform.position = experimentOffset + new Vector3(0.4f, 0.625f, 0f);
        beakerB.transform.localScale = new Vector3(0.4f, 0.28f, 0.4f);
        beakerB.AddComponent<MeshFilter>().sharedMesh = beakerA.GetComponent<MeshFilter>().sharedMesh;
        beakerB.AddComponent<MeshRenderer>().sharedMaterial = glassMat;
        CreateBeakerOutline(beakerB);

        // Condensation cylinder inside Beaker B
        GameObject condB = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        condB.name = "CondensationMesh";
        condB.transform.SetParent(beakerB.transform, false);
        condB.transform.localPosition = new Vector3(0f, 0.5f, 0f);
        condB.transform.localScale = new Vector3(0.97f, 0.45f, 0.97f);
        condB.GetComponent<MeshRenderer>().sharedMaterial = condensationMat;
        DestroyImmediate(condB.GetComponent<Collider>());

        // 5. Beaker Waters with Pivot at bottom (scale Y animation)
        GameObject waterAPivot = new GameObject("WaterAPivot");
        waterAPivot.transform.position = beakerA.transform.position + new Vector3(0f, -0.27f, 0f); // Bottom inside beaker
        waterAPivot.transform.localScale = new Vector3(0.4f, 0.28f, 0.4f);
        GameObject waterA = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        waterA.name = "WaterVolume";
        waterA.transform.SetParent(waterAPivot.transform, false);
        waterA.transform.localPosition = new Vector3(0f, 0.4f, 0f);
        waterA.transform.localScale = new Vector3(0.92f, 0.4f, 0.92f);
        waterA.GetComponent<MeshRenderer>().sharedMaterial = waterMat;
        DestroyImmediate(waterA.GetComponent<Collider>());

        GameObject waterBPivot = new GameObject("WaterBPivot");
        waterBPivot.transform.position = beakerB.transform.position + new Vector3(0f, -0.27f, 0f); // Bottom inside beaker
        waterBPivot.transform.localScale = new Vector3(0.4f, 0.28f, 0.4f);
        GameObject waterB = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        waterB.name = "WaterVolume";
        waterB.transform.SetParent(waterBPivot.transform, false);
        waterB.transform.localPosition = new Vector3(0f, 0.4f, 0f);
        waterB.transform.localScale = new Vector3(0.92f, 0.4f, 0.92f);
        waterB.GetComponent<MeshRenderer>().sharedMaterial = waterMat;
        DestroyImmediate(waterB.GetComponent<Collider>());

        // 6. Create Bunsen Burners under tripods
        GameObject burnerA = CreateBunsenBurner("BurnerA_3D", experimentOffset + new Vector3(-0.4f, 0f, 0f), metalMat, rodMat, flameMat, out GameObject flameAObj);
        GameObject burnerB = CreateBunsenBurner("BurnerB_3D", experimentOffset + new Vector3(0.4f, 0f, 0f), metalMat, rodMat, flameMat, out GameObject flameBObj);

        // 7. Retort stands and Thermometers (lifted Y to 0.46f to match beaker lift)
        GameObject thermometerA = CreateThermometerSetup("RetortStandA_3D", experimentOffset + new Vector3(-0.4f, 0f, 0.35f), experimentOffset + new Vector3(-0.4f, 0.46f, 0f), metalMat, rodMat, glassMat, redCapillaryMat);
        GameObject thermometerB = CreateThermometerSetup("RetortStandB_3D", experimentOffset + new Vector3(0.4f, 0f, 0.35f), experimentOffset + new Vector3(0.4f, 0.46f, 0f), metalMat, rodMat, glassMat, redCapillaryMat);

        // 8. Particle Systems for Boiling, Steam, and Salt
        CreateBoilingBubbles(waterAPivot, "BoilingBubblesA", new Color(1f, 1f, 1f, 0.6f), out GameObject bubblesAObj);
        CreateBoilingBubbles(waterBPivot, "BoilingBubblesB", new Color(1f, 1f, 1f, 0.6f), out GameObject bubblesBObj);

        CreateSteamEffect(beakerA, "SteamA", out GameObject steamAObj);
        CreateSteamEffect(beakerB, "SteamB", out GameObject steamBObj);

        // 9. Salt Bottle on the desk (right side)
        GameObject saltBottle = CreateSaltBottle("SaltBottle_3D", experimentOffset + new Vector3(0.85f, 0.01f, -0.2f), saltBottleMat, glassMat, out ParticleSystem saltPS);

        // 9.5 Lighter on the desk
        GameObject lighter = CreateLighter("Lighter_3D", experimentOffset + new Vector3(0.7f, 0.01f, -0.1f));

        // 9.7 Safety Goggles on the desk (Orange frame, transparent lenses)
        Material orangeGogglesMat = GetOrCreateMaterial("SafetyOrangeMat_Lab3", new Color(1.0f, 0.45f, 0.0f, 1.0f), false);
        GameObject safetyGoggles = CreateSafetyGoggles("SafetyGoggles_3D", experimentOffset + new Vector3(0.55f, 0.01f, -0.25f), orangeGogglesMat, glassMat);

        // 10. Floating Hologram Data Board
        Material hologramGlassMat = GetOrCreateMaterial("HologramGlassMat_Lab3", new Color(0f, 0.15f, 0.3f, 0.85f), true);
        Material hologramBorderMat = GetOrCreateMaterial("HologramBorderMat_Lab3", new Color(0f, 0.9f, 1f, 1f), false);
        GameObject hologramBoard = CreateHologramBoard(experimentOffset, hologramGlassMat, hologramBorderMat, out TextMesh holoTM);

        // 11. Real-time Graph Monitor
        SetupRealTimeGraph(experimentOffset);

        // 12. World Space Tablet
        Setup3DTablet(experimentOffset, metalMat, rodMat);

        // 13. Environmental Props (Grid Mat & Notebook)
        SetupEnvironmentalProps(experimentOffset);

        // 13.5. Stopwatch on the desk
        Material darkGreyMat = GetOrCreateMaterial("DarkGreyMat_Lab3", new Color(0.15f, 0.15f, 0.15f, 1f), false);
        GameObject stopwatchObj = SetupStopwatch(experimentOffset, darkGreyMat, glassMat);

        // 14. Main 3D Manager Setup
        GameObject managerObj = new GameObject("WaterHeatingManager3D_Controller");
        WaterHeatingManager3D managerScript = managerObj.AddComponent<WaterHeatingManager3D>();

        // Wire 3D references
        managerScript.waterAPivot = waterAPivot.transform;
        managerScript.waterBPivot = waterBPivot.transform;
        managerScript.flameA = flameAObj;
        managerScript.flameB = flameBObj;
        managerScript.bubblesA = bubblesAObj;
        managerScript.bubblesB = bubblesBObj;
        managerScript.steamA = steamAObj;
        managerScript.steamB = steamBObj;
        managerScript.hologramText = holoTM;
        
        Transform pivotA = thermometerA.transform.Find("CapillaryPivot");
        Transform pivotB = thermometerB.transform.Find("CapillaryPivot");
        if (pivotA != null) managerScript.thermoPivotA = pivotA;
        if (pivotB != null) managerScript.thermoPivotB = pivotB;

        // Wire Condensation meshes
        Transform condATrans = beakerA.transform.Find("CondensationMesh");
        if (condATrans != null) managerScript.condensationA = condATrans.GetComponent<Renderer>();
        Transform condBTrans = beakerB.transform.Find("CondensationMesh");
        if (condBTrans != null) managerScript.condensationB = condBTrans.GetComponent<Renderer>();

        // Wire Collars
        Transform collarATrans = burnerA.transform.Find("Collar");
        if (collarATrans != null) managerScript.collarA = collarATrans.GetComponent<BunsenCollar3D>();
        Transform collarBTrans = burnerB.transform.Find("Collar");
        if (collarBTrans != null) managerScript.collarB = collarBTrans.GetComponent<BunsenCollar3D>();

        // Wire Salt Shaker
        GameObject dissolvingParticlesObj = CreateDissolvingParticles(waterBPivot, "DissolvingParticles");
        
        DragGaram3D dragGaram = saltBottle.AddComponent<DragGaram3D>();
        dragGaram.heatingManager = managerScript;
        dragGaram.saltParticles = saltPS;
        dragGaram.dissolvingParticles = dissolvingParticlesObj.GetComponent<ParticleSystem>();
        dragGaram.targetBeakerB = beakerB.transform;
        managerScript.saltBottle = dragGaram;

        // Wire Lighter
        DragLighter3D dragLighter = lighter.AddComponent<DragLighter3D>();
        dragLighter.heatingManager = managerScript;

        // Wire Goggles reference
        managerScript.safetyGoggles = safetyGoggles;

        // Wire Notebook & Create UI Log Panel
        GameObject notebookObj = GameObject.Find("Lab3_Notebook");
        if (notebookObj != null)
        {
            InteractiveNotebook3D notebookScript = notebookObj.GetComponent<InteractiveNotebook3D>();
            managerScript.notebook = notebookScript;
            notebookScript.heatingManager = managerScript;

            GameObject canvasObj = GameObject.Find("Canvas");
            if (canvasObj != null)
            {
                CreateNotebookUI(canvasObj, managerScript, notebookScript);
            }
        }

        // Wire Stopwatch
        Stopwatch3D stopwatchScript = stopwatchObj.GetComponent<Stopwatch3D>();
        managerScript.stopwatch = stopwatchScript;

        // Bind Canvas Overlay references dynamically
        WaterHeatingManager manager2D = Object.FindAnyObjectByType<WaterHeatingManager>();
        if (manager2D != null)
        {
            manager2D.enabled = false; // Disable 2D logic to prevent conflicts
            managerScript.tempTextA = manager2D.tempTextA;
            managerScript.tempTextB = manager2D.tempTextB;
            managerScript.stateTextA = manager2D.stateTextA;
            managerScript.stateTextB = manager2D.stateTextB;
            managerScript.observationTextA = manager2D.observationTextA;
            managerScript.observationTextB = manager2D.observationTextB;
            managerScript.warningTextObject = manager2D.warningTextObject;
            
            // Link Quiz Manager
            managerScript.quizManager = manager2D.quizManager;
            if (manager2D.quizManager != null)
            {
                managerScript.successPanel = manager2D.quizManager.gameObject;
                SetupPostExperimentSummary(GameObject.Find("Canvas"), manager2D.quizManager);
            }

            // Disable the old 2D manager component so it doesn't run in parallel
            manager2D.enabled = false;
        }

        // Hide 2D visuals in the scene Canvas
        Hide2DExperimentVisuals();

        // Setup Camera dragging
        SetupCameraDragButton();

        // Final Polish: Post-processing cinematic
        SetupPostProcessing();

        // Setup Ambient Audio
        GameObject ambientAudio = new GameObject("AmbientAudio_3D");
        AudioSource bgm = ambientAudio.AddComponent<AudioSource>();
        AudioClip humClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/RoomHum.wav");
        if (humClip != null)
        {
            bgm.clip = humClip;
            bgm.loop = true;
            bgm.volume = 0.05f;
            bgm.spatialBlend = 0f; // 2D sound
            bgm.playOnAwake = true;
            bgm.Play();
        }

        // Setup Tooltips for Lab Equipment
        SetupTooltips();

        // Setup Tutorial Overlay
        Lab3TutorialManager tutManager = SetupTutorialOverlay(experimentOffset);
        managerScript.tutorialManager = tutManager;

        // Mark scene dirty and save
        var activeScene = EditorSceneManager.GetActiveScene();
        if (KidFriendlySceneTheme.Includes(activeScene)) KidFriendlySceneColourSetup.Apply(activeScene, false);
        Lab3VoiceSetup.Apply(activeScene);
        EditorSceneManager.MarkSceneDirty(activeScene);
        EditorSceneManager.SaveScene(activeScene);

        Debug.Log("3D Lab 3 generated and converted successfully!");
    }

    private static void SetupTooltips()
    {
        AddTooltip("BeakerA_3D", "Bikar A (Air Tulen)");
        AddTooltip("BeakerB_3D", "Bikar B (Air Garam)");
        AddTooltip("BurnerA_3D", "Penunu Bunsen A", false);
        AddTooltip("BurnerB_3D", "Penunu Bunsen B", false);
        AddTooltip("RetortStandA_3D", "Kaki Retort A", false);
        AddTooltip("RetortStandB_3D", "Kaki Retort B", false);
        AddTooltip("TripodStandA_3D", "Tungku Kaki Tiga A", false);
        AddTooltip("TripodStandB_3D", "Tungku Kaki Tiga B", false);
        AddTooltip("SafetyGoggles_3D", "Gogal Keselamatan");
        AddTooltip("Lighter_3D", "Pemetik Api");
        AddTooltip("SaltBottle_3D", "Botol Garam");
        AddTooltip("Lab3_Notebook", "Buku Laporan");
        AddTooltip("Lab3_RealTimeGraph", "Graf Suhu");
        AddTooltip("HologramDataBoard", "Skrin Data Hologram");
        
        // Find Thermometers
        GameObject standA = GameObject.Find("RetortStandA_3D");
        if (standA != null)
        {
            Transform thermoA = standA.transform.Find("Thermometer");
            if (thermoA != null) AddTooltipToObject(thermoA.gameObject, "Termometer A", true);
        }

        GameObject standB = GameObject.Find("RetortStandB_3D");
        if (standB != null)
        {
            Transform thermoB = standB.transform.Find("Thermometer");
            if (thermoB != null) AddTooltipToObject(thermoB.gameObject, "Termometer B", true);
        }
    }

    private static void AddTooltip(string objectName, string tooltipText, bool forceBoxCollider = false)
    {
        GameObject obj = GameObject.Find(objectName);
        if (obj != null)
        {
            AddTooltipToObject(obj, tooltipText, forceBoxCollider);
        }
    }

    private static void AddTooltipToObject(GameObject obj, string tooltipText, bool forceBoxCollider = false)
    {
        if (obj.GetComponent<Collider>() == null)
        {
            if (forceBoxCollider || obj.GetComponent<MeshFilter>() != null)
            {
                BoxCollider col = obj.AddComponent<BoxCollider>();
                // If the object has many children (like stands or burners), encapsulate them in the collider bounds
                if (forceBoxCollider)
                {
                    Bounds bounds = new Bounds(obj.transform.position, Vector3.zero);
                    Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
                    foreach (Renderer renderer in renderers)
                    {
                        if (renderer is ParticleSystemRenderer) continue;
                        bounds.Encapsulate(renderer.bounds);
                    }
                    col.center = obj.transform.InverseTransformPoint(bounds.center);
                    col.size = bounds.size;
                }
            }
        }

        TooltipHover3D tooltip = obj.GetComponent<TooltipHover3D>();
        if (tooltip == null)
        {
            tooltip = obj.AddComponent<TooltipHover3D>();
        }
        tooltip.tooltipText = tooltipText;
        EditorUtility.SetDirty(obj);
    }

    private static void Hide2DExperimentVisuals()
    {
        // Find and deactivate old 2D visuals
        string[] target2DNames = { "BeakerA", "BeakerB", "BunsenBurner", "FlameA", "FlameB", "Garam", "BubblesA", "BubblesB", "Water_Mask" };
        GameObject canvas = GameObject.Find("Canvas");
        if (canvas != null)
        {
            foreach (string name in target2DNames)
            {
                Transform t = canvas.transform.Find(name);
                if (t != null) t.gameObject.SetActive(false);

                // Check recursively in children
                Transform[] allChildren = canvas.GetComponentsInChildren<Transform>(true);
                foreach (var child in allChildren)
                {
                    if (child.name == name) child.gameObject.SetActive(false);
                }
            }
        }
    }

    private static GameObject CopyEnvironmentFromLab1()
    {
        GameObject existingEnv = GameObject.Find("Classroom Environment");
        if (existingEnv != null) return existingEnv;

        try
        {
            Scene currentScene = EditorSceneManager.GetActiveScene();
            Scene lab1Scene = EditorSceneManager.OpenScene("Assets/Scenes/Lab1_3D.unity", OpenSceneMode.Additive);
            
            foreach (GameObject rootObj in lab1Scene.GetRootGameObjects())
            {
                if (rootObj.name == "Classroom Environment" || rootObj.GetComponent<Light>() != null)
                {
                    GameObject clone = Instantiate(rootObj);
                    clone.name = rootObj.name;
                    SceneManager.MoveGameObjectToScene(clone, currentScene);
                }
            }
            
            EditorSceneManager.CloseScene(lab1Scene, true);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("Failed to copy environment from Lab1_3D. Error: " + e.Message);
        }

        return GameObject.Find("Classroom Environment");
    }

    private static void DestroyIfExists(string name)
    {
        GameObject[] allObjects = Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include);
        foreach (GameObject obj in allObjects)
        {
            if (obj && obj.name == name)
            {
                try { DestroyImmediate(obj); } catch {}
            }
        }
    }

    private static void DestroyDuplicateLights()
    {
        Light[] allLights = Object.FindObjectsByType<Light>(FindObjectsInactive.Include);
        int dirLightCount = 0;
        foreach (Light l in allLights)
        {
            if (l.type == LightType.Directional && l.gameObject.name == "Directional Light")
            {
                dirLightCount++;
                if (dirLightCount > 1)
                {
                    try { DestroyImmediate(l.gameObject); } catch {}
                }
            }
        }
    }

    private static GameObject CreateTripodStand(string name, Vector3 pos, Material ringMat, Material legMat)
    {
        GameObject tripod = new GameObject(name);
        tripod.transform.position = pos;

        // Top platform ring
        GameObject ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        ring.name = "TopRing";
        ring.transform.SetParent(tripod.transform, false);
        ring.transform.localPosition = new Vector3(0f, 0.32f, 0f);
        ring.transform.localScale = new Vector3(0.35f, 0.015f, 0.35f);
        ring.GetComponent<MeshRenderer>().sharedMaterial = ringMat;
        DestroyImmediate(ring.GetComponent<Collider>());

        // Wire Gauze mesh (flat square sitting on top)
        GameObject gauze = GameObject.CreatePrimitive(PrimitiveType.Cube);
        gauze.name = "WireGauze";
        gauze.transform.SetParent(tripod.transform, false);
        gauze.transform.localPosition = new Vector3(0f, 0.332f, 0f);
        gauze.transform.localScale = new Vector3(0.38f, 0.005f, 0.38f);
        gauze.GetComponent<MeshRenderer>().sharedMaterial = ringMat;
        DestroyImmediate(gauze.GetComponent<Collider>());

        // Three diagonal legs (rotated 180 degrees around Y to clear the front view for flame particles)
        CreateLeg(tripod.transform, "Leg1", new Vector3(0.12f, 0.165f, -0.07f), new Vector3(0f, 0f, -15f), legMat);
        CreateLeg(tripod.transform, "Leg2", new Vector3(-0.12f, 0.165f, -0.07f), new Vector3(0f, 0f, 15f), legMat);
        CreateLeg(tripod.transform, "Leg3", new Vector3(0f, 0.165f, 0.14f), new Vector3(-15f, 0f, 0f), legMat);

        BoxCollider topCol = tripod.AddComponent<BoxCollider>();
        topCol.center = new Vector3(0f, 0.32f, 0f);
        topCol.size = new Vector3(0.35f, 0.05f, 0.35f);

        return tripod;
    }

    private static void CreateLeg(Transform parent, string name, Vector3 localPos, Vector3 rot, Material mat)
    {
        GameObject leg = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        leg.name = name;
        leg.transform.SetParent(parent, false);
        leg.transform.localPosition = localPos;
        leg.transform.localRotation = Quaternion.Euler(rot);
        leg.transform.localScale = new Vector3(0.015f, 0.165f, 0.015f); // 33cm tall
        leg.GetComponent<MeshRenderer>().sharedMaterial = mat;
        DestroyImmediate(leg.GetComponent<Collider>());
    }

    private static GameObject CreateBunsenBurner(string name, Vector3 pos, Material baseMat, Material tubeMat, Material flameMat, out GameObject flameObj)
    {
        GameObject burner = new GameObject(name);
        burner.transform.position = pos;

        // Base plate (made thicker and slightly higher to prevent sinking into the table/mat)
        GameObject basePlate = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        basePlate.name = "BasePlate";
        basePlate.transform.SetParent(burner.transform, false);
        basePlate.transform.localPosition = new Vector3(0f, 0.015f, 0f);
        basePlate.transform.localScale = new Vector3(0.18f, 0.015f, 0.18f); // 3cm thick
        basePlate.GetComponent<MeshRenderer>().sharedMaterial = baseMat;
        DestroyImmediate(basePlate.GetComponent<Collider>());

        // Vertical tube (made slightly thicker for visibility)
        GameObject tube = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        tube.name = "Tube";
        tube.transform.SetParent(burner.transform, false);
        tube.transform.localPosition = new Vector3(0f, 0.11f, 0f);
        tube.transform.localScale = new Vector3(0.04f, 0.08f, 0.04f); // 16cm tall, 4cm thick
        tube.GetComponent<MeshRenderer>().sharedMaterial = tubeMat;
        DestroyImmediate(tube.GetComponent<Collider>());

        // Flame object (Particle System)
        flameObj = new GameObject("FlameVisual");
        flameObj.transform.SetParent(burner.transform, false);
        flameObj.transform.localPosition = new Vector3(0f, 0.19f, 0f); // Base of flame at tube top
        flameObj.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f); // Force vertical rotation on creation

        ParticleSystem ps = flameObj.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.duration = 1f;
        main.loop = true;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.07f, 0.09f); // Dies exactly at gauze (approx 14cm tall) to prevent clipping
        main.startSpeed = new ParticleSystem.MinMaxCurve(1.3f, 1.6f); // High-speed jet
        main.startSize3D = true;
        main.startSizeX = new ParticleSystem.MinMaxCurve(0.04f, 0.06f);
        main.startSizeY = new ParticleSystem.MinMaxCurve(0.12f, 0.16f);
        main.startSizeZ = new ParticleSystem.MinMaxCurve(0.04f, 0.06f);

        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] { 
                new GradientColorKey(new Color(0.7f, 0.95f, 1f), 0.0f), // Bright white-cyan core
                new GradientColorKey(new Color(0.1f, 0.6f, 1f), 0.3f), // Cyan-blue inner flame
                new GradientColorKey(new Color(0.05f, 0.2f, 0.9f), 0.7f), // Blue outer envelope
                new GradientColorKey(new Color(0f, 0.05f, 0.6f), 1.0f) // Deep indigo tip
            },
            new GradientAlphaKey[] { 
                new GradientAlphaKey(1.0f, 0.0f), 
                new GradientAlphaKey(0.8f, 0.5f), 
                new GradientAlphaKey(0.0f, 1.0f) // Fade out completely at the tip
            }
        );
        main.startColor = new ParticleSystem.MinMaxGradient(grad);
        main.simulationSpace = ParticleSystemSimulationSpace.Local;

        var emission = ps.emission;
        emission.rateOverTime = 250f; // Dense solid-looking flame

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 1.5f; // Sharp jet
        shape.radius = 0.005f; // Focused base

        var velocity = ps.velocityOverLifetime;
        velocity.enabled = false; // Go straight up

        var size = ps.sizeOverLifetime;
        size.enabled = true;
        size.size = new ParticleSystem.MinMaxCurve(1.0f, 0.2f); // Taper at tip

        ParticleSystemRenderer psr = flameObj.GetComponent<ParticleSystemRenderer>();
        psr.sharedMaterial = UnityEditor.AssetDatabase.GetBuiltinExtraResource<Material>("Default-ParticleSystem.mat");
        psr.renderMode = ParticleSystemRenderMode.VerticalBillboard;
        psr.pivot = new Vector3(0f, -0.5f, 0f); // Pivot at bottom to strictly enforce rendering upwards

        // Enable Particle System Lights module
        var psLights = ps.lights;
        psLights.enabled = true;
        psLights.ratio = 0.05f; // Only 5% of particles emit light to prevent overwhelming the renderer/flicker bugs
        psLights.intensityMultiplier = 5f; // High intensity for visible glow under the beaker
        psLights.rangeMultiplier = 1.2f; // Ensure light reaches the table and beaker

        // Smoke object
        GameObject smokeObj = new GameObject("SmokeVisual");
        smokeObj.transform.SetParent(burner.transform, false);
        smokeObj.transform.localPosition = new Vector3(0f, 0.22f, 0f);
        smokeObj.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);

        ParticleSystem smokePs = smokeObj.AddComponent<ParticleSystem>();
        var smain = smokePs.main;
        smain.duration = 1f;
        smain.loop = true;
        smain.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 0.8f);
        smain.startSpeed = new ParticleSystem.MinMaxCurve(0.3f, 0.5f);
        smain.startSize = new ParticleSystem.MinMaxCurve(0.01f, 0.04f);
        smain.startColor = new Color(0.1f, 0.1f, 0.1f, 0.5f); // Dark soot

        var semiss = smokePs.emission;
        semiss.rateOverTime = 0f; // Handled by BunsenCollar3D

        var sshape = smokePs.shape;
        sshape.shapeType = ParticleSystemShapeType.Cone;
        sshape.angle = 8f;
        sshape.radius = 0.01f;

        ParticleSystemRenderer spsr = smokeObj.GetComponent<ParticleSystemRenderer>();
        spsr.sharedMaterial = UnityEditor.AssetDatabase.GetBuiltinExtraResource<Material>("Default-ParticleSystem.mat");
        spsr.renderMode = ParticleSystemRenderMode.Billboard;

        // Collar Ring at the base of the tube (bright red for high visibility and click hint)
        Material brassMat = GetOrCreateMaterial("BrassMat_Lab3", new Color(0.9f, 0.1f, 0.1f, 1f), false);
        GameObject collar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        collar.name = "Collar";
        collar.transform.SetParent(burner.transform, false);
        collar.transform.localPosition = new Vector3(0f, 0.04f, 0f);
        collar.transform.localScale = new Vector3(0.046f, 0.015f, 0.046f);
        collar.GetComponent<MeshRenderer>().sharedMaterial = brassMat;
        
        DestroyImmediate(collar.GetComponent<Collider>());
        BoxCollider collarCol = collar.AddComponent<BoxCollider>();
        collarCol.center = Vector3.zero;
        collarCol.size = new Vector3(1.3f, 1f, 1.3f);

        BunsenCollar3D collarScript = collar.AddComponent<BunsenCollar3D>();
        collarScript.targetFlame = ps;

        // Add hover highlight for interactive feedback
        InteractableHighlight3D highlight = collar.AddComponent<InteractableHighlight3D>();
        highlight.highlightColor = new Color(0.25f, 0.25f, 0.25f, 1f);

        // BoxCollider for the base plate
        BoxCollider baseCol = burner.AddComponent<BoxCollider>();
        baseCol.center = new Vector3(0f, 0.015f, 0f);
        baseCol.size = new Vector3(0.18f, 0.03f, 0.18f);

        // BoxCollider for the top half of the tube (avoids the collar)
        BoxCollider tubeCol = burner.AddComponent<BoxCollider>();
        tubeCol.center = new Vector3(0f, 0.12f, 0f);
        tubeCol.size = new Vector3(0.06f, 0.14f, 0.06f);

        return burner;
    }

    private static GameObject CreateThermometerSetup(string name, Vector3 standPos, Vector3 beakerPos, Material standMat, Material rodMat, Material glassMat, Material redMat)
    {
        GameObject stand = new GameObject(name);
        stand.transform.position = standPos;

        // Base
        GameObject baseObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        baseObj.name = "StandBase";
        baseObj.transform.SetParent(stand.transform, false);
        baseObj.transform.localPosition = new Vector3(0f, 0.01f, 0f);
        baseObj.transform.localScale = new Vector3(0.2f, 0.02f, 0.15f);
        baseObj.GetComponent<MeshRenderer>().sharedMaterial = standMat;
        DestroyImmediate(baseObj.GetComponent<Collider>());

        // Vertical Rod
        GameObject rod = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        rod.name = "StandRod";
        rod.transform.SetParent(stand.transform, false);
        rod.transform.localPosition = new Vector3(0f, 0.5f, 0f);
        rod.transform.localScale = new Vector3(0.015f, 0.5f, 0.015f); // 100cm rod (made taller to clamp above the beaker)
        rod.GetComponent<MeshRenderer>().sharedMaterial = rodMat;
        DestroyImmediate(rod.GetComponent<Collider>());

        // Arm joint
        GameObject joint = GameObject.CreatePrimitive(PrimitiveType.Cube);
        joint.name = "ClampJoint";
        joint.transform.SetParent(stand.transform, false);
        joint.transform.localPosition = new Vector3(0f, 0.98f, 0f); // Placed above the beaker top rim (world Y = 2.53)
        joint.transform.localScale = new Vector3(0.04f, 0.04f, 0.04f);
        joint.GetComponent<MeshRenderer>().sharedMaterial = standMat;
        DestroyImmediate(joint.GetComponent<Collider>());

        // Arm pointing forward (stands are 0.35m behind beaker center)
        GameObject arm = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        arm.name = "ClampArm";
        arm.transform.SetParent(stand.transform, false);
        arm.transform.localPosition = new Vector3(0f, 0.98f, -0.175f); // Placed above the beaker top rim
        arm.transform.localScale = new Vector3(0.01f, 0.175f, 0.01f);
        arm.transform.localEulerAngles = new Vector3(90f, 0f, 0f);
        arm.GetComponent<MeshRenderer>().sharedMaterial = rodMat;
        DestroyImmediate(arm.GetComponent<Collider>());

        // Thermometer holding ring
        GameObject ring = new GameObject("ClampRing");
        ring.transform.SetParent(stand.transform, false);
        ring.transform.localPosition = new Vector3(0f, 0.98f, -0.35f); // Placed above the beaker top rim
        MeshFilter mf = ring.AddComponent<MeshFilter>();
        mf.sharedMesh = GenerateRingMesh(0.015f, 0.025f, 0.015f, 16);
        MeshRenderer mr = ring.AddComponent<MeshRenderer>();
        mr.sharedMaterial = standMat;

        // Suspended Thermometer (positioned at beakerPos, hovering down)
        GameObject thermo = new GameObject("Thermometer");
        thermo.transform.SetParent(stand.transform, true);
        thermo.transform.position = beakerPos + new Vector3(0f, 0.27f, 0f); // Center lifted, bulb still submerged (world Y = 2.014)
        thermo.transform.localScale = new Vector3(0.024f, 0.28f, 0.024f); // Made longer (Y=0.28 scale -> 56cm) to reach above the beaker

        // Glass Tube
        GameObject glass = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        glass.name = "GlassTube";
        glass.transform.SetParent(thermo.transform, false);
        glass.transform.localPosition = Vector3.zero;
        glass.transform.localScale = Vector3.one;
        glass.GetComponent<MeshRenderer>().sharedMaterial = glassMat;
        DestroyImmediate(glass.GetComponent<Collider>());

        // Capillary inner tube pivot
        GameObject capillaryPivot = new GameObject("CapillaryPivot");
        capillaryPivot.transform.SetParent(thermo.transform, false);
        capillaryPivot.transform.localPosition = new Vector3(0f, -0.9f, 0f);

        GameObject capillary = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        capillary.name = "CapillaryTube";
        capillary.transform.SetParent(capillaryPivot.transform, false);
        capillary.transform.localPosition = new Vector3(0f, 1f, 0f); // Center is 1 unit up
        capillary.transform.localScale = new Vector3(0.2f, 1f, 0.2f); // Default scale
        capillary.GetComponent<MeshRenderer>().sharedMaterial = redMat;
        DestroyImmediate(capillary.GetComponent<Collider>());

        // Bulb at bottom
        GameObject bulb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        bulb.name = "Bulb";
        bulb.transform.SetParent(thermo.transform, false);
        bulb.transform.localPosition = new Vector3(0f, -0.95f, 0f);
        bulb.transform.localScale = new Vector3(1.6f, 0.22f, 1.6f);
        bulb.GetComponent<MeshRenderer>().sharedMaterial = redMat;
        DestroyImmediate(bulb.GetComponent<Collider>());

        BoxCollider standCol = stand.AddComponent<BoxCollider>();
        standCol.center = new Vector3(0f, 0.5f, -0.05f);
        standCol.size = new Vector3(0.2f, 1f, 0.2f);

        return thermo;
    }

    private static void CreateBeakerOutline(GameObject beaker)
    {
        GameObject outline = new GameObject("Outline");
        outline.transform.SetParent(beaker.transform, false);
        
        LineRenderer lr = outline.AddComponent<LineRenderer>();
        lr.useWorldSpace = false;
        lr.loop = true;
        lr.startWidth = 0.02f;
        lr.endWidth = 0.02f;
        Material outlineMat = new Material(Shader.Find("Sprites/Default"));
        lr.sharedMaterial = outlineMat;
        lr.startColor = new Color(0.1f, 0.1f, 0.1f, 0.8f);
        lr.endColor = new Color(0.1f, 0.1f, 0.1f, 0.8f);

        int segments = 32;
        lr.positionCount = segments;
        for (int i = 0; i < segments; i++)
        {
            float angle = (i * Mathf.PI * 2f) / segments;
            lr.SetPosition(i, new Vector3(Mathf.Cos(angle) * 0.5f, 1.0f, Mathf.Sin(angle) * 0.5f));
        }
    }
    private static GameObject CreateLighter(string name, Vector3 pos)
    {
        Material plasticMat = GetOrCreateMaterial("LighterPlastic_Lab3", new Color(0.9f, 0.1f, 0.1f, 1f), false);
        Material metalMat = GetOrCreateMaterial("LighterMetal_Lab3", new Color(0.7f, 0.7f, 0.7f, 1f), false);
        
        GameObject lighter = new GameObject(name);
        lighter.transform.position = pos;

        // Body
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
        body.name = "Body";
        body.transform.SetParent(lighter.transform, false);
        body.transform.localPosition = new Vector3(0f, 0.03f, 0f);
        body.transform.localScale = new Vector3(0.02f, 0.06f, 0.04f);
        body.GetComponent<MeshRenderer>().sharedMaterial = plasticMat;
        
        // Remove default collider, we'll add one to the root
        DestroyImmediate(body.GetComponent<Collider>());

        // Metal Head
        GameObject head = GameObject.CreatePrimitive(PrimitiveType.Cube);
        head.name = "Head";
        head.transform.SetParent(lighter.transform, false);
        head.transform.localPosition = new Vector3(0f, 0.07f, 0f);
        head.transform.localScale = new Vector3(0.018f, 0.02f, 0.038f);
        head.GetComponent<MeshRenderer>().sharedMaterial = metalMat;
        DestroyImmediate(head.GetComponent<Collider>());

        // Add Root Box Collider for dragging
        BoxCollider col = lighter.AddComponent<BoxCollider>();
        col.center = new Vector3(0f, 0.04f, 0f);
        col.size = new Vector3(0.04f, 0.1f, 0.06f);

        // Dynamic guide indicator (initially hidden)
        GameObject indicator = new GameObject("Indicator");
        indicator.transform.SetParent(lighter.transform, false);
        indicator.transform.localPosition = new Vector3(0f, 0.16f, 0f); // Hovering above

        // Add a nice neon green glow light to guide player
        Light indLight = indicator.AddComponent<Light>();
        indLight.type = LightType.Point;
        indLight.color = new Color(0.2f, 1.0f, 0.4f); // Neon green
        indLight.intensity = 0.18f; // Extremely subtle, soft glow
        indLight.range = 0.25f; // Small localized range
        indLight.shadows = LightShadows.None;

        indicator.SetActive(false); // Hidden by default

        return lighter;
    }

    private static GameObject CreateSafetyGoggles(string name, Vector3 pos, Material frameMat, Material lensMat)
    {
        GameObject goggles = new GameObject(name);
        goggles.transform.position = pos;
        goggles.transform.localRotation = Quaternion.identity;

        // Frame structure - left lens frame
        GameObject leftFrame = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        leftFrame.name = "LeftFrame";
        leftFrame.transform.SetParent(goggles.transform, false);
        leftFrame.transform.localPosition = new Vector3(-0.035f, 0.01f, 0f);
        leftFrame.transform.localScale = new Vector3(0.05f, 0.005f, 0.04f);
        leftFrame.transform.localEulerAngles = new Vector3(90f, 0f, 0f);
        leftFrame.GetComponent<MeshRenderer>().sharedMaterial = frameMat;
        DestroyImmediate(leftFrame.GetComponent<Collider>());

        // Right lens frame
        GameObject rightFrame = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        rightFrame.name = "RightFrame";
        rightFrame.transform.SetParent(goggles.transform, false);
        rightFrame.transform.localPosition = new Vector3(0.035f, 0.01f, 0f);
        rightFrame.transform.localScale = new Vector3(0.05f, 0.005f, 0.04f);
        rightFrame.transform.localEulerAngles = new Vector3(90f, 0f, 0f);
        rightFrame.GetComponent<MeshRenderer>().sharedMaterial = frameMat;
        DestroyImmediate(rightFrame.GetComponent<Collider>());

        // Left glass lens
        GameObject leftLens = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        leftLens.name = "LeftLens";
        leftLens.transform.SetParent(leftFrame.transform, false);
        leftLens.transform.localPosition = new Vector3(0f, 0.2f, 0f); // Extrude slightly forward
        leftLens.transform.localScale = new Vector3(0.85f, 1.2f, 0.85f);
        leftLens.GetComponent<MeshRenderer>().sharedMaterial = lensMat;
        DestroyImmediate(leftLens.GetComponent<Collider>());

        // Right glass lens
        GameObject rightLens = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        rightLens.name = "RightLens";
        rightLens.transform.SetParent(rightFrame.transform, false);
        rightLens.transform.localPosition = new Vector3(0f, 0.2f, 0f);
        rightLens.transform.localScale = new Vector3(0.85f, 1.2f, 0.85f);
        rightLens.GetComponent<MeshRenderer>().sharedMaterial = lensMat;
        DestroyImmediate(rightLens.GetComponent<Collider>());

        // Bridge
        GameObject bridge = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bridge.name = "Bridge";
        bridge.transform.SetParent(goggles.transform, false);
        bridge.transform.localPosition = new Vector3(0f, 0.01f, 0.002f);
        bridge.transform.localScale = new Vector3(0.02f, 0.01f, 0.01f);
        bridge.GetComponent<MeshRenderer>().sharedMaterial = frameMat;
        DestroyImmediate(bridge.GetComponent<Collider>());

        // Left temple/arm (arm extending back)
        GameObject leftTemple = GameObject.CreatePrimitive(PrimitiveType.Cube);
        leftTemple.name = "LeftTemple";
        leftTemple.transform.SetParent(goggles.transform, false);
        leftTemple.transform.localPosition = new Vector3(-0.06f, 0.01f, 0.035f);
        leftTemple.transform.localScale = new Vector3(0.004f, 0.008f, 0.07f);
        leftTemple.GetComponent<MeshRenderer>().sharedMaterial = frameMat;
        DestroyImmediate(leftTemple.GetComponent<Collider>());

        // Right temple/arm
        GameObject rightTemple = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rightTemple.name = "RightTemple";
        rightTemple.transform.SetParent(goggles.transform, false);
        rightTemple.transform.localPosition = new Vector3(0.06f, 0.01f, 0.035f);
        rightTemple.transform.localScale = new Vector3(0.004f, 0.008f, 0.07f);
        rightTemple.GetComponent<MeshRenderer>().sharedMaterial = frameMat;
        DestroyImmediate(rightTemple.GetComponent<Collider>());

        // Add Root Collider for click detection
        BoxCollider col = goggles.AddComponent<BoxCollider>();
        col.center = new Vector3(0f, 0.01f, 0.02f);
        col.size = new Vector3(0.14f, 0.05f, 0.1f);

        // Add Indicator for guide flashing
        GameObject indicator = new GameObject("Indicator");
        indicator.transform.SetParent(goggles.transform, false);
        indicator.transform.localPosition = new Vector3(0f, 0.08f, 0f);

        Light indLight = indicator.AddComponent<Light>();
        indLight.type = LightType.Point;
        indLight.color = new Color(1.0f, 0.5f, 0.0f); // Bright orange highlight
        indLight.intensity = 0.2f;
        indLight.range = 0.3f;
        indLight.shadows = LightShadows.None;
        indicator.SetActive(false);

        // Add click script
        goggles.AddComponent<GogglesInteraction3D>();

        return goggles;
    }

    private static GameObject CreateSaltBottle(string name, Vector3 pos, Material bottleMat, Material glassMat, out ParticleSystem saltPS)
    {
        Material metalMat = GetOrCreateMaterial("MetalMat_Lab3", new Color(0.15f, 0.15f, 0.15f, 1f), false);
        GameObject bottle = new GameObject(name);
        bottle.transform.position = pos;

        // Container Body
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        body.name = "Body";
        body.transform.SetParent(bottle.transform, false);
        body.transform.localPosition = new Vector3(0f, 0.06f, 0f);
        body.transform.localScale = new Vector3(0.08f, 0.06f, 0.08f);
        body.GetComponent<MeshRenderer>().sharedMaterial = bottleMat;
        DestroyImmediate(body.GetComponent<Collider>());

        // Neck
        GameObject neck = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        neck.name = "Neck";
        neck.transform.SetParent(bottle.transform, false);
        neck.transform.localPosition = new Vector3(0f, 0.13f, 0f);
        neck.transform.localScale = new Vector3(0.05f, 0.012f, 0.05f);
        neck.GetComponent<MeshRenderer>().sharedMaterial = bottleMat;
        DestroyImmediate(neck.GetComponent<Collider>());

        // Cap
        GameObject cap = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cap.name = "Cap";
        cap.transform.SetParent(bottle.transform, false);
        cap.transform.localPosition = new Vector3(0f, 0.148f, 0f);
        cap.transform.localScale = new Vector3(0.055f, 0.006f, 0.055f);
        cap.GetComponent<MeshRenderer>().sharedMaterial = metalMat;
        DestroyImmediate(cap.GetComponent<Collider>());

        // Salt label
        GameObject label = GameObject.CreatePrimitive(PrimitiveType.Cube);
        label.name = "LabelMesh";
        label.transform.SetParent(bottle.transform, false);
        label.transform.localPosition = new Vector3(0f, 0.06f, -0.042f);
        label.transform.localScale = new Vector3(0.05f, 0.05f, 0.002f);
        label.GetComponent<MeshRenderer>().sharedMaterial = GetOrCreateMaterial("LabelMat_Salt", Color.white, false);
        DestroyImmediate(label.GetComponent<Collider>());

        GameObject labelTxt = new GameObject("LabelText");
        labelTxt.transform.SetParent(bottle.transform, false);
        labelTxt.transform.localPosition = new Vector3(0f, 0.06f, -0.044f);
        labelTxt.transform.localScale = new Vector3(0.008f, 0.008f, 0.008f);
        TextMesh tm = labelTxt.AddComponent<TextMesh>();
        tm.text = "GARAM";
        tm.fontSize = 24;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        tm.color = Color.black;

        // Add CapsuleCollider for clicking/dragging
        CapsuleCollider col = bottle.AddComponent<CapsuleCollider>();
        col.radius = 0.05f;
        col.height = 0.16f;
        col.center = new Vector3(0f, 0.08f, 0f);

        // Add Salt particles under the cap pointing down
        GameObject psObj = new GameObject("SaltPourParticles");
        psObj.transform.SetParent(bottle.transform, false);
        psObj.transform.localPosition = new Vector3(0f, 0.16f, 0f);
        psObj.transform.localRotation = Quaternion.Euler(0f, 0f, 0f); // Relative to bottle rotation

        saltPS = psObj.AddComponent<ParticleSystem>();
        var main = saltPS.main;
        main.duration = 2f;
        main.loop = true;
        main.startLifetime = 1.0f;
        main.startSpeed = 0.1f;
        main.startSize = new ParticleSystem.MinMaxCurve(0.007f, 0.011f);
        main.startColor = Color.white;
        main.gravityModifier = 1.0f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = saltPS.emission;
        emission.rateOverTime = 180f;

        var shape = saltPS.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.01f;

        var velocity = saltPS.velocityOverLifetime;
        velocity.enabled = false;

        ParticleSystemRenderer psr = psObj.GetComponent<ParticleSystemRenderer>();
        psr.material = new Material(Shader.Find("Particles/Standard Unlit"));

        return bottle;
    }

    private static void CreateBoilingBubbles(GameObject waterPivot, string name, Color color, out GameObject bubblesObj)
    {
        bubblesObj = new GameObject(name);
        bubblesObj.transform.SetParent(waterPivot.transform, false);
        bubblesObj.transform.localPosition = new Vector3(0f, 0.1f, 0f); // Centered inside water volume pivot

        ParticleSystem ps = bubblesObj.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.duration = 5f;
        main.loop = true;
        main.scalingMode = ParticleSystemScalingMode.Hierarchy; // Scales with beaker's X/Y/Z local scale
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.4f, 0.8f);
        main.startSpeed = 0f;
        main.startSize = new ParticleSystem.MinMaxCurve(0.01f, 0.04f); // Dynamic size in world space (~4mm to 16mm)
        main.startColor = new Color(color.r, color.g, color.b, 0.5f);
        main.gravityModifier = 0f;

        var emission = ps.emission;
        emission.rateOverTime = 80f; // More intense boiling bubbles

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 0f; // Cylindrical column
        shape.radius = 0.4f; // Fits nicely inside beaker radius (local radius 0.4 * scale 0.4 = world radius 0.16m, beaker is 0.20m)
        shape.length = 0.1f;
        shape.rotation = new Vector3(90f, 0f, 0f);

        var velocity = ps.velocityOverLifetime;
        velocity.enabled = true;
        // Particle velocity curves must all be in the same mode (Two Constants here)
        velocity.x = new ParticleSystem.MinMaxCurve(-0.2f, 0.2f); // Wobbles side to side
        velocity.y = new ParticleSystem.MinMaxCurve(1.2f, 2.0f); // Rises dynamically
        velocity.z = new ParticleSystem.MinMaxCurve(-0.2f, 0.2f); // Wobbles front to back

        // Dynamic bubble expansion over lifetime
        var sizeModule = ps.sizeOverLifetime;
        sizeModule.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0.0f, 0.4f);
        sizeCurve.AddKey(1.0f, 1.2f);
        sizeModule.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

        // Smooth fade-in and fade-out of bubbles to prevent harsh pops
        var colorModule = ps.colorOverLifetime;
        colorModule.enabled = true;
        Gradient bubbleGradient = new Gradient();
        bubbleGradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.white, 0.0f), new GradientColorKey(Color.white, 1.0f) },
            new GradientAlphaKey[] { 
                new GradientAlphaKey(0.0f, 0.0f), 
                new GradientAlphaKey(0.6f, 0.15f), 
                new GradientAlphaKey(0.6f, 0.7f), 
                new GradientAlphaKey(0.0f, 1.0f) 
            }
        );
        colorModule.color = new ParticleSystem.MinMaxGradient(bubbleGradient);

        ParticleSystemRenderer psr = bubblesObj.GetComponent<ParticleSystemRenderer>();
        psr.sharedMaterial = AssetDatabase.GetBuiltinExtraResource<Material>("Default-ParticleSystem.mat");
    }

    private static void CreateSteamEffect(GameObject beaker, string name, out GameObject steamObj)
    {
        steamObj = new GameObject(name);
        steamObj.transform.SetParent(beaker.transform, false);
        steamObj.transform.localPosition = new Vector3(0f, 1.1f, 0f); // Hovering above mouth of beaker

        ParticleSystem ps = steamObj.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.duration = 5f;
        main.loop = true;
        main.scalingMode = ParticleSystemScalingMode.Hierarchy; // Respects beaker scale
        main.startLifetime = new ParticleSystem.MinMaxCurve(1.5f, 2.5f); // Drifts higher before dissipating
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.1f, 0.2f); // Gentle upward drift
        main.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.25f); // World size ~4cm to 10cm
        main.startColor = new Color(0.95f, 0.95f, 0.98f, 0.12f);
        main.gravityModifier = new ParticleSystem.MinMaxCurve(-0.02f); // Slower rise

        var emission = ps.emission;
        emission.rateOverTime = 12f;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 12f; // Soft expanding dispersion
        shape.radius = 0.4f; // Fits the mouth of beaker (local 0.4 * scale 0.4 = world radius 0.16m)
        shape.rotation = new Vector3(-90f, 0f, 0f); // Point cone straight UP

        // Expand steam over lifetime
        var sizeModule = ps.sizeOverLifetime;
        sizeModule.enabled = true;
        AnimationCurve curve = new AnimationCurve();
        curve.AddKey(0.0f, 0.8f);
        curve.AddKey(1.0f, 2.5f); // Dissipates/grows
        sizeModule.size = new ParticleSystem.MinMaxCurve(1f, curve);

        // Soft steam alpha fade in and fade out
        var colorModule = ps.colorOverLifetime;
        colorModule.enabled = true;
        Gradient steamGradient = new Gradient();
        steamGradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(new Color(0.95f, 0.95f, 0.98f), 0.0f), new GradientColorKey(new Color(0.95f, 0.95f, 0.98f), 1.0f) },
            new GradientAlphaKey[] { 
                new GradientAlphaKey(0.0f, 0.0f), 
                new GradientAlphaKey(0.15f, 0.15f), 
                new GradientAlphaKey(0.12f, 0.5f), 
                new GradientAlphaKey(0.0f, 1.0f) 
            }
        );
        colorModule.color = new ParticleSystem.MinMaxGradient(steamGradient);

        // Add subtle rotation over lifetime for swirling vapor effect
        var rotModule = ps.rotationOverLifetime;
        rotModule.enabled = true;
        rotModule.z = new ParticleSystem.MinMaxCurve(-0.5f, 0.5f);

        // Add subtle noise for organic wavy movement
        var noise = ps.noise;
        noise.enabled = true;
        noise.strength = new ParticleSystem.MinMaxCurve(0.04f);
        noise.frequency = 0.5f;
        noise.scrollSpeed = new ParticleSystem.MinMaxCurve(0.15f);
        noise.damping = true;

        ParticleSystemRenderer psr = steamObj.GetComponent<ParticleSystemRenderer>();
        psr.sharedMaterial = AssetDatabase.GetBuiltinExtraResource<Material>("Default-ParticleSystem.mat");
    }

    private static GameObject CreateHologramBoard(Vector3 experimentOffset, Material glassMat, Material borderMat, out TextMesh holoTM)
    {
        GameObject hologramBoard = new GameObject("HologramDataBoard");
        hologramBoard.transform.position = experimentOffset + new Vector3(-1.15f, 1.351f, -0.538f);
        hologramBoard.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);

        Vector3 camPos = experimentOffset + new Vector3(0f, 0.95f, -1.8f);
        hologramBoard.transform.LookAt(hologramBoard.transform.position + (hologramBoard.transform.position - camPos).normalized);

        hologramBoard.AddComponent<LookAtCamera3D>();

        // Glass Plate
        GameObject glassPlate = GameObject.CreatePrimitive(PrimitiveType.Cube);
        glassPlate.name = "HologramGlassPlate";
        glassPlate.transform.SetParent(hologramBoard.transform, false);
        glassPlate.transform.localPosition = Vector3.zero;
        glassPlate.transform.localScale = new Vector3(0.85f, 0.55f, 0.01f);
        glassPlate.GetComponent<MeshRenderer>().sharedMaterial = glassMat;
        DestroyImmediate(glassPlate.GetComponent<Collider>());

        // Borders
        float halfW = 0.85f * 0.5f;
        float halfH = 0.55f * 0.5f;
        float thick = 0.015f;

        CreateBorder(hologramBoard.transform, "BorderTop", new Vector3(0f, halfH + thick*0.5f, 0f), new Vector3(0.85f + thick*2f, thick, thick), borderMat);
        CreateBorder(hologramBoard.transform, "BorderBottom", new Vector3(0f, -halfH - thick*0.5f, 0f), new Vector3(0.85f + thick*2f, thick, thick), borderMat);
        CreateBorder(hologramBoard.transform, "BorderLeft", new Vector3(-halfW - thick*0.5f, 0f, 0f), new Vector3(thick, 0.55f, thick), borderMat);
        CreateBorder(hologramBoard.transform, "BorderRight", new Vector3(halfW + thick*0.5f, 0f, 0f), new Vector3(thick, 0.55f, thick), borderMat);

        // Text
        GameObject textObj = new GameObject("HologramText");
        textObj.transform.SetParent(hologramBoard.transform, false);
        textObj.transform.localPosition = new Vector3(0f, 0f, -0.015f);
        textObj.transform.localScale = new Vector3(0.07f, 0.07f, 0.07f);

        holoTM = textObj.AddComponent<TextMesh>();
        holoTM.text = "STATUS PEMANASAN\n\nBeaker A: 27.0°C (SEDIA)\nBeaker B: 27.0°C (SEDIA)";
        holoTM.characterSize = 0.05f;
        holoTM.fontSize = 72;
        holoTM.fontStyle = FontStyle.Bold;
        holoTM.anchor = TextAnchor.MiddleCenter;
        holoTM.alignment = TextAlignment.Center;
        holoTM.color = Color.white;

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        holoTM.font = font;
        if (font != null)
        {
            textObj.GetComponent<MeshRenderer>().sharedMaterial = font.material;
        }

        return hologramBoard;
    }

    private static void CreateBorder(Transform parent, string name, Vector3 pos, Vector3 scale, Material mat)
    {
        GameObject border = GameObject.CreatePrimitive(PrimitiveType.Cube);
        border.name = name;
        border.transform.SetParent(parent, false);
        border.transform.localPosition = pos;
        border.transform.localScale = scale;
        border.GetComponent<MeshRenderer>().sharedMaterial = mat;
        DestroyImmediate(border.GetComponent<Collider>());
    }

    private static void SetupRealTimeGraph(Vector3 experimentOffset)
    {
        GameObject graphObj = new GameObject("Lab3_RealTimeGraph");
        graphObj.transform.position = experimentOffset + new Vector3(-1.59f, 0.023f, -0.68f);
        graphObj.transform.rotation = Quaternion.Euler(0f, 340.7f, 0f);
        graphObj.transform.localScale = new Vector3(1.71f, 1.71f, 1.71f);

        RealTimeTempGraph3D graphScript = graphObj.AddComponent<RealTimeTempGraph3D>();

        Material metalMat = GetOrCreateMaterial("MetalMat_Lab3", new Color(0.15f, 0.15f, 0.15f, 1f), false);
        Material rodMat = GetOrCreateMaterial("RodMat_Lab3", new Color(0.7f, 0.7f, 0.7f, 1f), false);
        Material screenMat = GetOrCreateMaterial("GraphScreenMat_Lab3", new Color(0.02f, 0.06f, 0.12f, 0.85f), true);

        // Base
        GameObject baseStand = GameObject.CreatePrimitive(PrimitiveType.Cube);
        baseStand.name = "BaseStand";
        baseStand.transform.SetParent(graphObj.transform, false);
        baseStand.transform.localPosition = new Vector3(0f, 0.006f, 0f);
        baseStand.transform.localScale = new Vector3(0.18f, 0.012f, 0.12f);
        baseStand.GetComponent<MeshRenderer>().sharedMaterial = metalMat;
        DestroyImmediate(baseStand.GetComponent<Collider>());

        // Neck
        GameObject neck = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        neck.name = "Neck";
        neck.transform.SetParent(graphObj.transform, false);
        neck.transform.localPosition = new Vector3(0f, 0.13f, 0.02f);
        neck.transform.localScale = new Vector3(0.025f, 0.12f, 0.025f);
        neck.GetComponent<MeshRenderer>().sharedMaterial = rodMat;
        DestroyImmediate(neck.GetComponent<Collider>());

        // Backboard
        GameObject backboard = GameObject.CreatePrimitive(PrimitiveType.Cube);
        backboard.name = "Backboard";
        backboard.transform.SetParent(graphObj.transform, false);
        backboard.transform.localPosition = new Vector3(0f, 0.35f, 0f);
        backboard.transform.localScale = new Vector3(0.36f, 0.26f, 0.015f);
        backboard.GetComponent<MeshRenderer>().sharedMaterial = metalMat;
        DestroyImmediate(backboard.GetComponent<Collider>());

        // Screen
        GameObject screen = GameObject.CreatePrimitive(PrimitiveType.Cube);
        screen.name = "Screen";
        screen.transform.SetParent(graphObj.transform, false);
        screen.transform.localPosition = new Vector3(0f, 0.35f, -0.006f);
        screen.transform.localScale = new Vector3(0.33f, 0.23f, 0.004f);
        screen.GetComponent<MeshRenderer>().sharedMaterial = screenMat;
        DestroyImmediate(screen.GetComponent<Collider>());

        // Axis
        GameObject xAxis = GameObject.CreatePrimitive(PrimitiveType.Cube);
        xAxis.name = "X_Axis";
        xAxis.transform.SetParent(graphObj.transform, false);
        xAxis.transform.localPosition = new Vector3(0.01f, 0.26f, -0.009f);
        xAxis.transform.localScale = new Vector3(0.28f, 0.002f, 0.002f);
        xAxis.GetComponent<MeshRenderer>().sharedMaterial = rodMat;
        DestroyImmediate(xAxis.GetComponent<Collider>());

        GameObject yAxis = GameObject.CreatePrimitive(PrimitiveType.Cube);
        yAxis.name = "Y_Axis";
        yAxis.transform.SetParent(graphObj.transform, false);
        yAxis.transform.localPosition = new Vector3(-0.13f, 0.35f, -0.009f);
        yAxis.transform.localScale = new Vector3(0.002f, 0.20f, 0.002f);
        yAxis.GetComponent<MeshRenderer>().sharedMaterial = rodMat;
        DestroyImmediate(yAxis.GetComponent<Collider>());

        // Labels
        CreateGraphText("GRAF SUHU VS MASA", graphObj, new Vector3(0f, 0.44f, -0.01f), 0.0018f, 36, new Color(0.15f, 0.75f, 1f, 1f));
        CreateGraphText("Suhu (degC)", graphObj, new Vector3(-0.175f, 0.35f, -0.01f), 0.0014f, 28, Color.white, TextAnchor.MiddleCenter, 90f);
        CreateGraphText("Masa (s)", graphObj, new Vector3(0.01f, 0.21f, -0.01f), 0.0014f, 28, Color.white);

        CreateGraphText("110", graphObj, new Vector3(-0.145f, 0.44f, -0.01f), 0.0012f, 24, Color.gray, TextAnchor.MiddleRight);
        CreateGraphText("60", graphObj, new Vector3(-0.145f, 0.35f, -0.01f), 0.0012f, 24, Color.gray, TextAnchor.MiddleRight);
        CreateGraphText("27", graphObj, new Vector3(-0.145f, 0.26f, -0.01f), 0.0012f, 24, Color.gray, TextAnchor.MiddleRight);

        CreateGraphText("0.0", graphObj, new Vector3(-0.13f, 0.24f, -0.01f), 0.0012f, 24, Color.gray);
        CreateGraphText("15", graphObj, new Vector3(0f, 0.24f, -0.01f), 0.0012f, 24, Color.gray);
        CreateGraphText("30", graphObj, new Vector3(0.13f, 0.24f, -0.01f), 0.0012f, 24, Color.gray);

        // Legend
        CreateGraphText("A:Tulen (Cyan)", graphObj, new Vector3(0.05f, 0.44f, -0.01f), 0.0012f, 24, Color.cyan, TextAnchor.MiddleLeft);
        CreateGraphText("B:Garam (Red)", graphObj, new Vector3(0.05f, 0.41f, -0.01f), 0.0012f, 24, Color.red, TextAnchor.MiddleLeft);

        // Glowing Line Renderers
        Shader lineShader = Shader.Find("Sprites/Default");
        if (lineShader == null) lineShader = Shader.Find("Particles/Standard Unlit");

        Material lineAMat = new Material(lineShader);
        lineAMat.color = Color.cyan;
        GameObject lineAObj = new GameObject("Line_BeakerA");
        lineAObj.transform.SetParent(graphObj.transform, false);
        LineRenderer lrA = lineAObj.AddComponent<LineRenderer>();
        lrA.sharedMaterial = lineAMat;
        graphScript.lineBeakerA = lrA;

        Material lineBMat = new Material(lineShader);
        lineBMat.color = Color.red;
        GameObject lineBObj = new GameObject("Line_BeakerB");
        lineBObj.transform.SetParent(graphObj.transform, false);
        LineRenderer lrB = lineBObj.AddComponent<LineRenderer>();
        lrB.sharedMaterial = lineBMat;
        graphScript.lineBeakerB = lrB;
    }

    private static void CreateGraphText(string text, GameObject parent, Vector3 localPos, float scale, int fontSize, Color color, TextAnchor anchor = TextAnchor.MiddleCenter, float rotZ = 0f)
    {
        GameObject txtObj = new GameObject("Label_" + text.Replace(" ", "_").Replace(":", "_"));
        txtObj.transform.SetParent(parent.transform, false);
        txtObj.transform.localPosition = localPos;
        txtObj.transform.localRotation = Quaternion.Euler(0f, 0f, rotZ);
        txtObj.transform.localScale = new Vector3(scale, scale, scale);

        TextMesh tm = txtObj.AddComponent<TextMesh>();
        tm.text = text;
        tm.color = color;
        tm.fontSize = fontSize;
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        tm.font = font;
        if (font != null)
        {
            txtObj.GetComponent<MeshRenderer>().sharedMaterial = font.material;
        }
        tm.anchor = anchor;
        tm.alignment = TextAlignment.Center;
    }

    private static void Setup3DTablet(Vector3 experimentOffset, Material metalMat, Material rodMat)
    {
        GameObject tabletObj = new GameObject("Lab3_3DTablet");
        GameObject mainCam = GameObject.Find("Main Camera");
        if (mainCam != null)
        {
            tabletObj.transform.SetParent(mainCam.transform, false);
            tabletObj.transform.localPosition = new Vector3(0.55f, -0.22f, 0.8f);
            tabletObj.transform.localRotation = Quaternion.Euler(-70f, 0f, 15f);
            tabletObj.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
        }
        else
        {
            tabletObj.transform.position = experimentOffset + new Vector3(0.584f, 0.819f, -1.769f);
            tabletObj.transform.rotation = Quaternion.Euler(303f, 0f, 15f);
        }

        // Casing
        GameObject casing = GameObject.CreatePrimitive(PrimitiveType.Cube);
        casing.name = "TabletCasing";
        casing.transform.SetParent(tabletObj.transform, false);
        casing.transform.localScale = new Vector3(0.46f, 0.012f, 0.34f);
        casing.GetComponent<MeshRenderer>().sharedMaterial = metalMat;
        DestroyImmediate(casing.GetComponent<Collider>());

        // Screen
        GameObject screen = GameObject.CreatePrimitive(PrimitiveType.Cube);
        screen.name = "TabletScreen";
        screen.transform.SetParent(tabletObj.transform, false);
        screen.transform.localPosition = new Vector3(0f, 0.007f, 0f);
        screen.transform.localScale = new Vector3(0.43f, 0.002f, 0.31f);
        Material screenMat = GetOrCreateMaterial("TabletScreenMat_Lab3", new Color(0.02f, 0.06f, 0.12f, 1f), false);
        screen.GetComponent<MeshRenderer>().sharedMaterial = screenMat;
        DestroyImmediate(screen.GetComponent<Collider>());

        // World Space Canvas
        GameObject tabletCanvasObj = new GameObject("TabletCanvas");
        tabletCanvasObj.transform.SetParent(tabletObj.transform, false);
        tabletCanvasObj.transform.localPosition = new Vector3(0f, 0.009f, 0f); // Position above screen mesh to prevent occlusion
        tabletCanvasObj.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        tabletCanvasObj.transform.localScale = new Vector3(0.001f, 0.001f, 0.001f);

        Canvas canvas = tabletCanvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = mainCam.GetComponent<Camera>(); // Ensure we use the exact camera we attached the tablet to

        RectTransform canvasRect = tabletCanvasObj.GetComponent<RectTransform>();
        if (canvasRect != null)
        {
            canvasRect.sizeDelta = new Vector2(430f, 310f); // Match physical screen dimensions
        }

        tabletCanvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        // Camera POV Controls Panel
        GameObject panelObj = new GameObject("CameraPOVPanel");
        panelObj.transform.SetParent(tabletCanvasObj.transform, false);
        UnityEngine.UI.Image panelImg = panelObj.AddComponent<UnityEngine.UI.Image>();
        panelImg.color = new Color(0.04f, 0.12f, 0.22f, 0.85f);
        RectTransform panelRect = panelObj.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0f, 0.5f);
        panelRect.anchorMax = new Vector2(0f, 0.5f);
        panelRect.pivot = new Vector2(0f, 0.5f);
        panelRect.anchoredPosition = new Vector2(10f, 0f);
        panelRect.sizeDelta = new Vector2(180f, 280f);

        string[] views = { "< BACK", "Right View", "Left View", "Behind View", "Top View", "Textbook View", "Thermo A", "Thermo B" };
        string[] buttonNames = { "Btn_FrontView", "Btn_RightView", "Btn_LeftView", "Btn_BehindView", "Btn_TopView", "Btn_TextbookView", "Btn_ThermoA", "Btn_ThermoB" };
        for (int i = 0; i < views.Length; i++)
        {
            GameObject btn = new GameObject(buttonNames[i]);
            btn.transform.SetParent(panelObj.transform, false);
            UnityEngine.UI.Image btnImg = btn.AddComponent<UnityEngine.UI.Image>();
            if (i == 0)
                btnImg.color = new Color(0.85f, 0.15f, 0.15f, 0.9f); // Red for BACK
            else
                btnImg.color = new Color(0.1f, 0.45f, 0.85f, 0.85f); // Blue for others
            btn.AddComponent<UnityEngine.UI.Button>();

            RectTransform btnRect = btn.GetComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0.5f, 1f);
            btnRect.anchorMax = new Vector2(0.5f, 1f);
            btnRect.pivot = new Vector2(0.5f, 1f);
            btnRect.anchoredPosition = new Vector2(0f, -6 - i * 32);
            btnRect.sizeDelta = new Vector2(160f, 28f);

            GameObject btnTextObj = new GameObject("Text");
            btnTextObj.transform.SetParent(btn.transform, false);
            UnityEngine.UI.Text btnTxt = btnTextObj.AddComponent<UnityEngine.UI.Text>();
            btnTxt.text = views[i];
            btnTxt.alignment = TextAnchor.MiddleCenter;
            btnTxt.color = Color.white;
            btnTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            btnTxt.fontSize = 13;
            btnTxt.fontStyle = FontStyle.Bold;
            btnTxt.raycastTarget = false;

            RectTransform textRect = btnTextObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
        }

        // Title
        GameObject titleObj = new GameObject("TabletTitle");
        titleObj.transform.SetParent(tabletCanvasObj.transform, false);
        UnityEngine.UI.Text titleTxt = titleObj.AddComponent<UnityEngine.UI.Text>();
        titleTxt.text = "E-LAB CONTROL PAD";
        titleTxt.alignment = TextAnchor.MiddleCenter;
        titleTxt.color = new Color(0.15f, 0.75f, 1f, 1f);
        titleTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        titleTxt.fontSize = 18;
        titleTxt.fontStyle = FontStyle.Bold;

        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(1f, 1f);
        titleRect.anchorMax = new Vector2(1f, 1f);
        titleRect.pivot = new Vector2(1f, 1f);
        titleRect.anchoredPosition = new Vector2(-15f, -15f);
        titleRect.sizeDelta = new Vector2(200f, 30f);

        // Control Panel (Start/Heat button)
        GameObject controlPanel = new GameObject("ControlPanel");
        controlPanel.transform.SetParent(tabletCanvasObj.transform, false);
        UnityEngine.UI.Image cpImg = controlPanel.AddComponent<UnityEngine.UI.Image>();
        cpImg.color = new Color(0.03f, 0.08f, 0.16f, 0.9f);
        RectTransform cpRect = controlPanel.GetComponent<RectTransform>();
        cpRect.anchorMin = new Vector2(1f, 0.5f);
        cpRect.anchorMax = new Vector2(1f, 0.5f);
        cpRect.pivot = new Vector2(1f, 0.5f);
        cpRect.anchoredPosition = new Vector2(-15f, 40f);
        cpRect.sizeDelta = new Vector2(200f, 75f);

        GameObject startHeatBtnObj = new GameObject("Btn_StartHeating");
        startHeatBtnObj.transform.SetParent(controlPanel.transform, false);
        UnityEngine.UI.Image startHeatImg = startHeatBtnObj.AddComponent<UnityEngine.UI.Image>();
        startHeatImg.color = new Color(0.15f, 0.7f, 0.15f, 0.9f);
        
        UnityEngine.UI.Button startHeatBtn = startHeatBtnObj.AddComponent<UnityEngine.UI.Button>();
        // Bind StartHeating directly via script in playmode, but let's wire it to search dynamically
        startHeatBtn.onClick.AddListener(() => {
            WaterHeatingManager3D mgr = Object.FindAnyObjectByType<WaterHeatingManager3D>();
            if (mgr != null) mgr.StartHeating();
        });

        RectTransform shRect = startHeatBtnObj.GetComponent<RectTransform>();
        shRect.anchorMin = Vector2.zero;
        shRect.anchorMax = Vector2.one;
        shRect.offsetMin = new Vector2(10f, 10f);
        shRect.offsetMax = new Vector2(-10f, -10f);

        GameObject shTextObj = new GameObject("Text");
        shTextObj.transform.SetParent(startHeatBtnObj.transform, false);
        UnityEngine.UI.Text shTxt = shTextObj.AddComponent<UnityEngine.UI.Text>();
        shTxt.text = "MULA PEMANASAN";
        shTxt.alignment = TextAnchor.MiddleCenter;
        shTxt.color = Color.white;
        shTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        shTxt.fontSize = 15;
        shTxt.fontStyle = FontStyle.Bold;
        shTxt.raycastTarget = false;

        RectTransform shTextRect = shTextObj.GetComponent<RectTransform>();
        shTextRect.anchorMin = Vector2.zero;
        shTextRect.anchorMax = Vector2.one;

        // Reset Button
        GameObject resetBtnObj = new GameObject("Btn_ResetExperiment");
        resetBtnObj.transform.SetParent(tabletCanvasObj.transform, false);
        UnityEngine.UI.Image resetImg = resetBtnObj.AddComponent<UnityEngine.UI.Image>();
        resetImg.color = new Color(0.7f, 0.15f, 0.15f, 0.9f);
        resetBtnObj.AddComponent<UnityEngine.UI.Button>();

        RectTransform resetRect = resetBtnObj.GetComponent<RectTransform>();
        resetRect.anchorMin = new Vector2(1f, 0f);
        resetRect.anchorMax = new Vector2(1f, 0f);
        resetRect.pivot = new Vector2(1f, 0f);
        resetRect.anchoredPosition = new Vector2(-15f, 15f);
        resetRect.sizeDelta = new Vector2(200f, 48f);

        GameObject resetTextObj = new GameObject("Text");
        resetTextObj.transform.SetParent(resetBtnObj.transform, false);
        UnityEngine.UI.Text resetTxt = resetTextObj.AddComponent<UnityEngine.UI.Text>();
        resetTxt.text = "RESET SIMULASI";
        resetTxt.alignment = TextAnchor.MiddleCenter;
        resetTxt.color = Color.white;
        resetTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        resetTxt.fontSize = 15;
        resetTxt.fontStyle = FontStyle.Bold;
        resetTxt.raycastTarget = false;

        RectTransform resetTextRect = resetTextObj.GetComponent<RectTransform>();
        resetTextRect.anchorMin = Vector2.zero;
        resetTextRect.anchorMax = Vector2.one;
    }

    private static void SetupEnvironmentalProps(Vector3 experimentOffset)
    {
        // 1. Grid Cutting Mat
        Material gridMatMat = GetOrCreateMaterial("GridMatMat_Lab3", new Color(0.1f, 0.35f, 0.3f, 1f), false);
        GameObject gridMat = GameObject.CreatePrimitive(PrimitiveType.Cube);
        gridMat.name = "Lab3_GridMat";
        gridMat.transform.position = experimentOffset + new Vector3(-0.4f, 0.004f, 0.25f);
        gridMat.transform.localScale = new Vector3(2.5f, 0.008f, 1.4f);
        gridMat.GetComponent<MeshRenderer>().sharedMaterial = gridMatMat;
        DestroyImmediate(gridMat.GetComponent<Collider>());

        // 2. Open Notebook
        Material notebookCoverMat = GetOrCreateMaterial("NotebookCoverMat_Lab3", new Color(0.08f, 0.08f, 0.12f, 1f), false);
        if (notebookCoverMat.HasProperty("_Smoothness")) notebookCoverMat.SetFloat("_Smoothness", 0f);
        if (notebookCoverMat.HasProperty("_Glossiness")) notebookCoverMat.SetFloat("_Glossiness", 0f);
        if (notebookCoverMat.HasProperty("_Metallic")) notebookCoverMat.SetFloat("_Metallic", 0f);
        if (notebookCoverMat.HasProperty("_SpecularHighlights")) notebookCoverMat.SetFloat("_SpecularHighlights", 0f);
        if (notebookCoverMat.HasProperty("_SpecColor")) notebookCoverMat.SetColor("_SpecColor", new Color(0.04f, 0.04f, 0.04f, 1f));
        EditorUtility.SetDirty(notebookCoverMat);

        Material notebookPageMat = GetOrCreateMaterial("NotebookPageMat_Lab3", new Color(0.5f, 0.48f, 0.45f, 1f), false);
        if (notebookPageMat.HasProperty("_Smoothness")) notebookPageMat.SetFloat("_Smoothness", 0f);
        if (notebookPageMat.HasProperty("_Glossiness")) notebookPageMat.SetFloat("_Glossiness", 0f);
        if (notebookPageMat.HasProperty("_Metallic")) notebookPageMat.SetFloat("_Metallic", 0f);
        if (notebookPageMat.HasProperty("_SpecularHighlights")) notebookPageMat.SetFloat("_SpecularHighlights", 0f);
        if (notebookPageMat.HasProperty("_SpecColor")) notebookPageMat.SetColor("_SpecColor", new Color(0.04f, 0.04f, 0.04f, 1f));
        EditorUtility.SetDirty(notebookPageMat);
        AssetDatabase.SaveAssets();

        Material penBodyMat = GetOrCreateMaterial("PenBodyMat_Lab3", new Color(0.1f, 0.2f, 0.6f, 1f), false);
        Material penTipMat = GetOrCreateMaterial("PenTipMat_Lab3", new Color(0.2f, 0.2f, 0.2f, 1f), false);

        GameObject notebook = new GameObject("Lab3_Notebook");
        notebook.transform.position = experimentOffset + new Vector3(0.85f, 0.005f, -0.6f);
        notebook.transform.rotation = Quaternion.Euler(0f, 345f, 0f);

        // Cover
        GameObject cover = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cover.name = "NotebookCover";
        cover.transform.SetParent(notebook.transform, false);
        cover.transform.localScale = new Vector3(0.32f, 0.008f, 0.42f);
        cover.GetComponent<MeshRenderer>().sharedMaterial = notebookCoverMat;
        DestroyImmediate(cover.GetComponent<Collider>());

        // Left Page
        GameObject leftPage = GameObject.CreatePrimitive(PrimitiveType.Cube);
        leftPage.name = "LeftPage";
        leftPage.transform.SetParent(notebook.transform, false);
        leftPage.transform.localPosition = new Vector3(-0.08f, 0.005f, 0f);
        leftPage.transform.localScale = new Vector3(0.14f, 0.004f, 0.38f);
        leftPage.transform.localEulerAngles = new Vector3(0f, 0f, -2f);
        leftPage.GetComponent<MeshRenderer>().sharedMaterial = notebookPageMat;
        DestroyImmediate(leftPage.GetComponent<Collider>());

        GameObject leftTextObj = new GameObject("LeftPageText");
        leftTextObj.transform.SetParent(notebook.transform, false);
        leftTextObj.transform.localPosition = new Vector3(-0.08f, 0.01f, 0f);
        leftTextObj.transform.localEulerAngles = new Vector3(90f, 0f, 0f);
        leftTextObj.transform.localScale = new Vector3(0.0022f, 0.0022f, 0.0022f);
        TextMesh leftTM = leftTextObj.AddComponent<TextMesh>();
        leftTM.text = "AMALI 3: PEMANASAN AIR\n\nTujuan:\nMengkaji kesan bendasing\nterhadap takat didih air.\n\nHipotesis:\nBendasing (garam)\nmeningkatkan takat didih.";
        leftTM.fontSize = 32;
        leftTM.fontStyle = FontStyle.Bold;
        leftTM.anchor = TextAnchor.MiddleCenter;
        leftTM.alignment = TextAlignment.Left;
        leftTM.color = Color.black;
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        leftTM.font = font;
        if (font != null) leftTextObj.GetComponent<MeshRenderer>().sharedMaterial = font.material;

        // Right Page
        GameObject rightPage = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rightPage.name = "RightPage";
        rightPage.transform.SetParent(notebook.transform, false);
        rightPage.transform.localPosition = new Vector3(0.08f, 0.005f, 0f);
        rightPage.transform.localScale = new Vector3(0.14f, 0.004f, 0.38f);
        rightPage.transform.localEulerAngles = new Vector3(0f, 0f, 2f);
        rightPage.GetComponent<MeshRenderer>().sharedMaterial = notebookPageMat;
        DestroyImmediate(rightPage.GetComponent<Collider>());

        GameObject rightTextObj = new GameObject("RightPageText");
        rightTextObj.transform.SetParent(notebook.transform, false);
        rightTextObj.transform.localPosition = new Vector3(0.08f, 0.01f, 0f);
        rightTextObj.transform.localEulerAngles = new Vector3(90f, 0f, 0f);
        rightTextObj.transform.localScale = new Vector3(0.0022f, 0.0022f, 0.0022f);
        TextMesh rightTM = rightTextObj.AddComponent<TextMesh>();
        rightTM.text = "PEMERHATIAN\n\nTakat Didih:\nBeaker A (Tulen): 100 degC\nBeaker B (Garam): 104 degC\n\nGraf menunjukkan air\ngaram mendidih lambat\ndan pada suhu tinggi.";
        rightTM.fontSize = 32;
        rightTM.fontStyle = FontStyle.Bold;
        rightTM.anchor = TextAnchor.MiddleCenter;
        rightTM.alignment = TextAlignment.Left;
        rightTM.color = Color.black;
        if (font != null) rightTextObj.GetComponent<MeshRenderer>().sharedMaterial = font.material;

        // Pen
        GameObject pen = new GameObject("Pen");
        pen.transform.SetParent(notebook.transform, false);
        pen.transform.localPosition = new Vector3(0.06f, 0.012f, -0.12f);
        pen.transform.localEulerAngles = new Vector3(0f, 25f, 90f);

        GameObject penBody = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        penBody.name = "PenBody";
        penBody.transform.SetParent(pen.transform, false);
        penBody.transform.localPosition = Vector3.zero;
        penBody.transform.localScale = new Vector3(0.012f, 0.06f, 0.012f);
        penBody.GetComponent<MeshRenderer>().sharedMaterial = penBodyMat;
        DestroyImmediate(penBody.GetComponent<Collider>());

        GameObject penTip = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        penTip.name = "PenTip";
        penTip.transform.SetParent(pen.transform, false);
        penTip.transform.localPosition = new Vector3(0f, -0.065f, 0f);
        penTip.transform.localScale = new Vector3(0.006f, 0.012f, 0.006f);
        penTip.GetComponent<MeshRenderer>().sharedMaterial = penTipMat;
        DestroyImmediate(penTip.GetComponent<Collider>());

        // Add BoxCollider to notebook for click detection
        BoxCollider noteCol = notebook.AddComponent<BoxCollider>();
        noteCol.center = new Vector3(0f, 0.01f, 0f);
        noteCol.size = new Vector3(0.35f, 0.02f, 0.45f);

        // Add notebook script
        notebook.AddComponent<InteractiveNotebook3D>();
    }

    private static void SetupCameraDragButton()
    {
        GameObject canvasObj = GameObject.Find("Canvas");
        if (canvasObj != null)
        {
            Transform oldRotateBtn = canvasObj.transform.Find("RotateCameraButton");
            if (oldRotateBtn != null) DestroyImmediate(oldRotateBtn.gameObject);

            GameObject btnObj = new GameObject("RotateCameraButton");
            btnObj.transform.SetParent(canvasObj.transform, false);
            UnityEngine.UI.Image img = btnObj.AddComponent<UnityEngine.UI.Image>();
            Sprite btnBlue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI Flat/btn_light_blue.png");
            img.sprite = btnBlue;
            img.type = UnityEngine.UI.Image.Type.Sliced;
            img.color = Color.white;
            img.pixelsPerUnitMultiplier = 2f;
            EditorUtility.SetDirty(img);

            RectTransform rect = btnObj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 0);
            rect.anchorMax = new Vector2(0, 0);
            rect.pivot = new Vector2(0, 0);
            rect.anchoredPosition = new Vector2(20, 20);
            rect.sizeDelta = new Vector2(90, 90);

            // Add CameraIcon inside the button
            GameObject iconObj = new GameObject("CameraIcon");
            iconObj.transform.SetParent(btnObj.transform, false);
            UnityEngine.UI.Image iconImg = iconObj.AddComponent<UnityEngine.UI.Image>();
            Sprite camSpr = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Textures/CameraIcon_Premium.png");
            iconImg.sprite = camSpr;
            iconImg.color = Color.white;
            iconImg.raycastTarget = false;
            
            RectTransform iconRect = iconObj.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.5f, 0.55f);
            iconRect.anchorMax = new Vector2(0.5f, 0.55f);
            iconRect.pivot = new Vector2(0.5f, 0.5f);
            iconRect.anchoredPosition = Vector2.zero;
            iconRect.sizeDelta = new Vector2(45, 45);

            // Add CameraLabel text below the icon
            GameObject labelObj = new GameObject("CameraLabel", typeof(RectTransform));
            labelObj.transform.SetParent(btnObj.transform, false);
            
            RectTransform labelRect = labelObj.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0, 0);
            labelRect.anchorMax = new Vector2(1, 0.32f);
            labelRect.offsetMin = new Vector2(4, 2);
            labelRect.offsetMax = new Vector2(-4, 0);
            
            TMPro.TextMeshProUGUI labelTmp = labelObj.AddComponent<TMPro.TextMeshProUGUI>();
            string[] fontAssets = AssetDatabase.FindAssets("FredokaOne SDF t:TMP_FontAsset");
            if (fontAssets.Length > 0)
            {
                labelTmp.font = AssetDatabase.LoadAssetAtPath<TMPro.TMP_FontAsset>(AssetDatabase.GUIDToAssetPath(fontAssets[0]));
            }
            labelTmp.text = "KAMERA";
            labelTmp.fontSize = 12;
            labelTmp.fontStyle = TMPro.FontStyles.Bold;
            labelTmp.color = Color.white;
            labelTmp.alignment = TMPro.TextAlignmentOptions.Center;
            labelTmp.enableWordWrapping = false;
            labelTmp.overflowMode = TMPro.TextOverflowModes.Truncate;
            labelTmp.raycastTarget = false;

            CameraDragArea dragScript = btnObj.AddComponent<CameraDragArea>();
            dragScript.rotationSpeed = 0.5f;

            // Calculate desk offset
            Vector3 offset = new Vector3(0f, 0.75f, 0f);
            GameObject deskTop = GameObject.Find("Desk_1");
            if (deskTop == null) deskTop = GameObject.Find("TeacherDeskTop");
            if (deskTop != null)
            {
                MeshRenderer rend = deskTop.GetComponent<MeshRenderer>();
                if (rend != null) offset = new Vector3(deskTop.transform.position.x, rend.bounds.max.y, deskTop.transform.position.z);
            }

            // Make sure Beaker_3D centering dummy exists for POV transitions
            GameObject beakerCenterDummy = GameObject.Find("Beaker_3D");
            if (beakerCenterDummy == null)
            {
                beakerCenterDummy = new GameObject("Beaker_3D");
            }
            // Position it exactly between Beaker A and Beaker B (lifted Y to 0.625f to match beaker lift)
            beakerCenterDummy.transform.position = offset + new Vector3(0f, 0.625f, 0f);
        }
    }

    private static void SetupPostProcessing()
    {
        GameObject volumeObj = GameObject.Find("Global_PostProcessing");
        if (volumeObj == null)
        {
            volumeObj = new GameObject("Global_PostProcessing");
            Volume v = volumeObj.AddComponent<Volume>();
            v.isGlobal = true;

            VolumeProfile vp = ScriptableObject.CreateInstance<VolumeProfile>();
            vp.name = "Lab3_VolumeProfile";
            AssetDatabase.CreateAsset(vp, "Assets/Materials/Lab3_VolumeProfile.asset");

            // Bloom for neon cyber glows
            Bloom bloom = vp.Add<Bloom>();
            bloom.active = true;
            bloom.threshold.Override(1.1f);
            bloom.intensity.Override(0.8f);
            bloom.scatter.Override(0.5f);
            bloom.tint.Override(new Color(0.6f, 0.85f, 1f));
            AssetDatabase.AddObjectToAsset(bloom, vp);

            // Color Adjustments (Vibrant Sci-Fi style)
            ColorAdjustments ca = vp.Add<ColorAdjustments>();
            ca.active = true;
            ca.postExposure.Override(0.15f);
            ca.contrast.Override(15f);
            ca.saturation.Override(25f);
            AssetDatabase.AddObjectToAsset(ca, vp);

            // Tone mapping
            Tonemapping tm = vp.Add<Tonemapping>();
            tm.active = true;
            tm.mode.Override(TonemappingMode.ACES);
            AssetDatabase.AddObjectToAsset(tm, vp);

            AssetDatabase.SaveAssets();
            v.sharedProfile = vp;
        }
        else
        {
            // Update existing volume profile bloom settings
            Volume v = volumeObj.GetComponent<Volume>();
            if (v != null && v.sharedProfile != null)
            {
                Bloom bloom;
                if (v.sharedProfile.TryGet<Bloom>(out bloom))
                {
                    bloom.threshold.value = 1.1f;
                    bloom.intensity.value = 0.8f;
                    bloom.scatter.value = 0.5f;
                    bloom.tint.value = new Color(0.6f, 0.85f, 1f);
                    EditorUtility.SetDirty(v.sharedProfile);
                }
            }
        }

        // Configure main camera rendering
        if (Camera.main != null)
        {
            var camData = Camera.main.gameObject.GetComponent<UniversalAdditionalCameraData>();
            if (camData == null)
            {
                camData = Camera.main.gameObject.AddComponent<UniversalAdditionalCameraData>();
            }
            camData.renderPostProcessing = true;
            camData.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
        }
    }

    private static Material GetOrCreateMaterial(string name, Color color, bool isTransparent)
    {
        string path = "Assets/Materials/" + name + ".mat";
        
        if (!AssetDatabase.IsValidFolder("Assets/Materials"))
        {
            AssetDatabase.CreateFolder("Assets", "Materials");
        }

        Shader shader = null;
        if (name.Contains("Glass") || name.Contains("Water") || name.Contains("Line") || name.Contains("Flame") || name.Contains("Condensation"))
        {
            shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null) shader = Shader.Find("Unlit/Transparent");
        }
        else
        {
            shader = Shader.Find("Universal Render Pipeline/Simple Lit");
            if (shader == null) shader = Shader.Find("Universal Render Pipeline/Lit");
        }
        if (shader == null) shader = Shader.Find("Standard");

        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat != null)
        {
            if (shader != null) mat.shader = shader;
            if (shader != null && shader.name.Contains("Universal Render Pipeline"))
            {
                mat.SetColor("_BaseColor", color);
                if (isTransparent)
                {
                    mat.SetFloat("_Surface", 1);
                    mat.SetInt("_Blend", 0);
                    mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    mat.SetInt("_ZWrite", 0); // Disable ZWrite for transparent materials to avoid sorting flicker
                    mat.SetInt("_Cull", 2);
                    mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                    mat.DisableKeyword("_SURFACE_TYPE_OPAQUE");
                }
            }
            else
            {
                mat.color = color;
                if (isTransparent)
                {
                    mat.SetInt("_ZWrite", 0); // Disable ZWrite for transparent materials
                    mat.SetInt("_Cull", 2);
                }
            }
            EditorUtility.SetDirty(mat);
            AssetDatabase.SaveAssets();
            return mat;
        }

        mat = new Material(shader);
        if (shader.name.Contains("Universal Render Pipeline"))
        {
            mat.SetColor("_BaseColor", color);
            if (isTransparent)
            {
                mat.SetFloat("_Surface", 1);
                mat.SetInt("_Blend", 0);
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0); // Disable ZWrite for transparent materials
                mat.SetInt("_Cull", 2);
                mat.renderQueue = 3000;
                mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                mat.DisableKeyword("_SURFACE_TYPE_OPAQUE");
            }
        }
        else
        {
            mat.color = color;
            if (isTransparent)
            {
                mat.SetFloat("_Mode", 3);
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0); // Disable ZWrite for transparent materials
                mat.SetInt("_Cull", 2);
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.DisableKeyword("_ALPHABLEND_ON");
                mat.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.renderQueue = 3000;
            }
        }

        AssetDatabase.CreateAsset(mat, path);
        AssetDatabase.SaveAssets();
        return mat;
    }

    private static Mesh GenerateOpenBeakerMesh()
    {
        Mesh mesh = new Mesh();
        mesh.name = "OpenBeakerMesh";

        int segments = 32;
        float radius = 0.5f;
        float height = 2f;
        float yBottom = -height / 2f;
        float yTop = height / 2f;

        Vector3[] vertices = new Vector3[segments * 2 + 1];
        int[] triangles = new int[segments * 6 + segments * 3];

        vertices[0] = new Vector3(0, yBottom, 0);

        for (int i = 0; i < segments; i++)
        {
            float angle = (float)i / segments * Mathf.PI * 2f;
            float x = Mathf.Sin(angle) * radius;
            float z = Mathf.Cos(angle) * radius;

            vertices[i + 1] = new Vector3(x, yBottom, z);
            vertices[i + 1 + segments] = new Vector3(x, yTop, z);
        }

        int tIndex = 0;
        for (int i = 0; i < segments; i++)
        {
            int next = (i + 1) % segments;
            triangles[tIndex++] = 0;
            triangles[tIndex++] = i + 1;
            triangles[tIndex++] = next + 1;
        }

        for (int i = 0; i < segments; i++)
        {
            int next = (i + 1) % segments;
            int b1 = i + 1;
            int b2 = next + 1;
            int t1 = i + 1 + segments;
            int t2 = next + 1 + segments;

            triangles[tIndex++] = b1;
            triangles[tIndex++] = t1;
            triangles[tIndex++] = b2;

            triangles[tIndex++] = b2;
            triangles[tIndex++] = t1;
            triangles[tIndex++] = t2;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        return mesh;
    }

    private static Mesh GenerateRingMesh(float innerRadius, float outerRadius, float height, int segments)
    {
        Mesh mesh = new Mesh();
        mesh.name = "RingMesh";

        int numVertices = segments * 4;
        Vector3[] vertices = new Vector3[numVertices];
        int[] triangles = new int[segments * 24];

        int vIndex = 0;
        int tIndex = 0;
        float halfH = height / 2f;

        for (int i = 0; i < segments; i++)
        {
            float angle1 = (float)i / segments * Mathf.PI * 2f;
            float angle2 = (float)(i + 1) / segments * Mathf.PI * 2f;

            float cos1 = Mathf.Cos(angle1);
            float sin1 = Mathf.Sin(angle1);
            float cos2 = Mathf.Cos(angle2);
            float sin2 = Mathf.Sin(angle2);

            int bInner1 = vIndex + 0;
            int tInner1 = vIndex + 1;
            int bOuter1 = vIndex + 2;
            int tOuter1 = vIndex + 3;

            vertices[bInner1] = new Vector3(sin1 * innerRadius, -halfH, cos1 * innerRadius);
            vertices[tInner1] = new Vector3(sin1 * innerRadius, halfH, cos1 * innerRadius);
            vertices[bOuter1] = new Vector3(sin1 * outerRadius, -halfH, cos1 * outerRadius);
            vertices[tOuter1] = new Vector3(sin1 * outerRadius, halfH, cos1 * outerRadius);

            vIndex += 4;
        }

        for (int i = 0; i < segments; i++)
        {
            int curr = i * 4;
            int next = ((i + 1) % segments) * 4;

            // Outer wall
            triangles[tIndex++] = curr + 2;
            triangles[tIndex++] = curr + 3;
            triangles[tIndex++] = next + 2;

            triangles[tIndex++] = next + 2;
            triangles[tIndex++] = curr + 3;
            triangles[tIndex++] = next + 3;

            // Inner wall
            triangles[tIndex++] = next + 0;
            triangles[tIndex++] = curr + 1;
            triangles[tIndex++] = curr + 0;

            triangles[tIndex++] = next + 1;
            triangles[tIndex++] = curr + 1;
            triangles[tIndex++] = next + 0;

            // Top
            triangles[tIndex++] = curr + 1;
            triangles[tIndex++] = next + 1;
            triangles[tIndex++] = curr + 3;

            triangles[tIndex++] = curr + 3;
            triangles[tIndex++] = next + 1;
            triangles[tIndex++] = next + 3;

            // Bottom
            triangles[tIndex++] = curr + 0;
            triangles[tIndex++] = curr + 2;
            triangles[tIndex++] = next + 0;

            triangles[tIndex++] = next + 0;
            triangles[tIndex++] = curr + 2;
            triangles[tIndex++] = next + 2;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        return mesh;
    }

    private static void CreateNotebookUI(GameObject canvasObj, WaterHeatingManager3D managerScript, InteractiveNotebook3D notebookScript)
    {
        Transform oldPanel = canvasObj.transform.Find("NotebookLogPanel");
        if (oldPanel != null) DestroyImmediate(oldPanel.gameObject);

        GameObject panelObj = new GameObject("NotebookLogPanel");
        panelObj.transform.SetParent(canvasObj.transform, false);
        
        RectTransform panelRect = panelObj.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(420f, 420f);

        UnityEngine.UI.Image bgImage = panelObj.AddComponent<UnityEngine.UI.Image>();
        bgImage.color = new Color(0.96f, 0.95f, 0.90f, 1f); // Parchment cream
        
        GameObject outlineObj = new GameObject("Outline");
        outlineObj.transform.SetParent(panelObj.transform, false);
        RectTransform outlineRect = outlineObj.AddComponent<RectTransform>();
        outlineRect.anchorMin = Vector2.zero;
        outlineRect.anchorMax = Vector2.one;
        outlineRect.sizeDelta = new Vector2(-12f, -12f);
        
        UnityEngine.UI.Image outlineImg = outlineObj.AddComponent<UnityEngine.UI.Image>();
        outlineImg.color = new Color(0f, 0f, 0f, 0.05f);

        Outline outlineComp = outlineObj.AddComponent<Outline>();
        outlineComp.effectColor = new Color(0.3f, 0.25f, 0.2f, 1f);
        outlineComp.effectDistance = new Vector2(3f, 3f);

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        GameObject titleObj = new GameObject("TitleText");
        titleObj.transform.SetParent(panelObj.transform, false);
        RectTransform titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0f, 1f);
        titleRect.anchorMax = new Vector2(1f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = new Vector2(0f, -25f);
        titleRect.sizeDelta = new Vector2(-40f, 40f);
        Text titleTxt = titleObj.AddComponent<Text>();
        titleTxt.text = "BUKU REKOD EKSPERIMEN";
        titleTxt.font = font;
        titleTxt.fontSize = 22;
        titleTxt.fontStyle = FontStyle.Bold;
        titleTxt.color = new Color(0.15f, 0.15f, 0.15f, 1f);
        titleTxt.alignment = TextAnchor.MiddleCenter;

        GameObject descObj = new GameObject("DescText");
        descObj.transform.SetParent(panelObj.transform, false);
        RectTransform descRect = descObj.AddComponent<RectTransform>();
        descRect.anchorMin = new Vector2(0f, 1f);
        descRect.anchorMax = new Vector2(1f, 1f);
        descRect.pivot = new Vector2(0.5f, 1f);
        descRect.anchoredPosition = new Vector2(0f, -70f);
        descRect.sizeDelta = new Vector2(-40f, 50f);
        Text descTxt = descObj.AddComponent<Text>();
        descTxt.text = "Catat takat didih yang diperhatikan daripada termometer atau graf suhu:";
        descTxt.font = font;
        descTxt.fontSize = 13;
        descTxt.color = new Color(0.25f, 0.25f, 0.25f, 1f);
        descTxt.alignment = TextAnchor.MiddleLeft;

        // Label A
        GameObject labelA = new GameObject("LabelA");
        labelA.transform.SetParent(panelObj.transform, false);
        RectTransform labelARect = labelA.AddComponent<RectTransform>();
        labelARect.anchorMin = new Vector2(0f, 1f);
        labelARect.anchorMax = new Vector2(1f, 1f);
        labelARect.pivot = new Vector2(0.5f, 1f);
        labelARect.anchoredPosition = new Vector2(0f, -130f);
        labelARect.sizeDelta = new Vector2(-40f, 20f);
        Text txtA = labelA.AddComponent<Text>();
        txtA.text = "Takat Didih Air Tulen (Beaker A) dalam °C:";
        txtA.font = font;
        txtA.fontSize = 13;
        txtA.fontStyle = FontStyle.Bold;
        txtA.color = new Color(0.15f, 0.15f, 0.15f, 1f);

        // Input Field A
        GameObject inputAContainer = new GameObject("InputA");
        inputAContainer.transform.SetParent(panelObj.transform, false);
        RectTransform inputARect = inputAContainer.AddComponent<RectTransform>();
        inputARect.anchorMin = new Vector2(0f, 1f);
        inputARect.anchorMax = new Vector2(1f, 1f);
        inputARect.pivot = new Vector2(0.5f, 1f);
        inputARect.anchoredPosition = new Vector2(0f, -155f);
        inputARect.sizeDelta = new Vector2(-40f, 30f);
        
        UnityEngine.UI.Image inputABg = inputAContainer.AddComponent<UnityEngine.UI.Image>();
        inputABg.color = Color.white;
        Outline inputAOutline = inputAContainer.AddComponent<Outline>();
        inputAOutline.effectColor = new Color(0.7f, 0.7f, 0.7f, 1f);

        GameObject inputATextObj = new GameObject("Text");
        inputATextObj.transform.SetParent(inputAContainer.transform, false);
        RectTransform inputATextRect = inputATextObj.AddComponent<RectTransform>();
        inputATextRect.anchorMin = Vector2.zero;
        inputATextRect.anchorMax = Vector2.one;
        inputATextRect.offsetMin = new Vector2(8f, 2f);
        inputATextRect.offsetMax = new Vector2(-8f, -2f);
        Text inputATextComp = inputATextObj.AddComponent<Text>();
        inputATextComp.font = font;
        inputATextComp.fontSize = 14;
        inputATextComp.color = Color.black;
        inputATextComp.alignment = TextAnchor.MiddleLeft;

        GameObject inputAPlaceholderObj = new GameObject("Placeholder");
        inputAPlaceholderObj.transform.SetParent(inputAContainer.transform, false);
        RectTransform inputAPlaceholderRect = inputAPlaceholderObj.AddComponent<RectTransform>();
        inputAPlaceholderRect.anchorMin = Vector2.zero;
        inputAPlaceholderRect.anchorMax = Vector2.one;
        inputAPlaceholderRect.offsetMin = new Vector2(8f, 2f);
        inputAPlaceholderRect.offsetMax = new Vector2(-8f, -2f);
        Text inputAPlaceholderComp = inputAPlaceholderObj.AddComponent<Text>();
        inputAPlaceholderComp.text = "Taip bacaan Beaker A...";
        inputAPlaceholderComp.font = font;
        inputAPlaceholderComp.fontSize = 13;
        inputAPlaceholderComp.fontStyle = FontStyle.Italic;
        inputAPlaceholderComp.color = Color.gray;
        inputAPlaceholderComp.alignment = TextAnchor.MiddleLeft;

        InputField inputA = inputAContainer.AddComponent<InputField>();
        inputA.textComponent = inputATextComp;
        inputA.placeholder = inputAPlaceholderComp;
        inputA.contentType = InputField.ContentType.DecimalNumber;

        // Label B
        GameObject labelB = new GameObject("LabelB");
        labelB.transform.SetParent(panelObj.transform, false);
        RectTransform labelBRect = labelB.AddComponent<RectTransform>();
        labelBRect.anchorMin = new Vector2(0f, 1f);
        labelBRect.anchorMax = new Vector2(1f, 1f);
        labelBRect.pivot = new Vector2(0.5f, 1f);
        labelBRect.anchoredPosition = new Vector2(0f, -200f);
        labelBRect.sizeDelta = new Vector2(-40f, 20f);
        Text txtB = labelB.AddComponent<Text>();
        txtB.text = "Takat Didih Air Garam (Beaker B) dalam °C:";
        txtB.font = font;
        txtB.fontSize = 13;
        txtB.fontStyle = FontStyle.Bold;
        txtB.color = new Color(0.15f, 0.15f, 0.15f, 1f);

        // Input Field B
        GameObject inputBContainer = new GameObject("InputB");
        inputBContainer.transform.SetParent(panelObj.transform, false);
        RectTransform inputBRect = inputBContainer.AddComponent<RectTransform>();
        inputBRect.anchorMin = new Vector2(0f, 1f);
        inputBRect.anchorMax = new Vector2(1f, 1f);
        inputBRect.pivot = new Vector2(0.5f, 1f);
        inputBRect.anchoredPosition = new Vector2(0f, -225f);
        inputBRect.sizeDelta = new Vector2(-40f, 30f);
        
        UnityEngine.UI.Image inputBBg = inputBContainer.AddComponent<UnityEngine.UI.Image>();
        inputBBg.color = Color.white;
        Outline inputBOutline = inputBContainer.AddComponent<Outline>();
        inputBOutline.effectColor = new Color(0.7f, 0.7f, 0.7f, 1f);

        GameObject inputBTextObj = new GameObject("Text");
        inputBTextObj.transform.SetParent(inputBContainer.transform, false);
        RectTransform inputBTextRect = inputBTextObj.AddComponent<RectTransform>();
        inputBTextRect.anchorMin = Vector2.zero;
        inputBTextRect.anchorMax = Vector2.one;
        inputBTextRect.offsetMin = new Vector2(8f, 2f);
        inputBTextRect.offsetMax = new Vector2(-8f, -2f);
        Text inputBTextComp = inputBTextObj.AddComponent<Text>();
        inputBTextComp.font = font;
        inputBTextComp.fontSize = 14;
        inputBTextComp.color = Color.black;
        inputBTextComp.alignment = TextAnchor.MiddleLeft;

        GameObject inputBPlaceholderObj = new GameObject("Placeholder");
        inputBPlaceholderObj.transform.SetParent(inputBContainer.transform, false);
        RectTransform inputBPlaceholderRect = inputBPlaceholderObj.AddComponent<RectTransform>();
        inputBPlaceholderRect.anchorMin = Vector2.zero;
        inputBPlaceholderRect.anchorMax = Vector2.one;
        inputBPlaceholderRect.offsetMin = new Vector2(8f, 2f);
        inputBPlaceholderRect.offsetMax = new Vector2(-8f, -2f);
        Text inputBPlaceholderComp = inputBPlaceholderObj.AddComponent<Text>();
        inputBPlaceholderComp.text = "Taip bacaan Beaker B...";
        inputBPlaceholderComp.font = font;
        inputBPlaceholderComp.fontSize = 13;
        inputBPlaceholderComp.fontStyle = FontStyle.Italic;
        inputBPlaceholderComp.color = Color.gray;
        inputBPlaceholderComp.alignment = TextAnchor.MiddleLeft;

        InputField inputB = inputBContainer.AddComponent<InputField>();
        inputB.textComponent = inputBTextComp;
        inputB.placeholder = inputBPlaceholderComp;
        inputB.contentType = InputField.ContentType.DecimalNumber;

        // Feedback
        GameObject feedObj = new GameObject("FeedbackText");
        feedObj.transform.SetParent(panelObj.transform, false);
        RectTransform feedRect = feedObj.AddComponent<RectTransform>();
        feedRect.anchorMin = new Vector2(0f, 1f);
        feedRect.anchorMax = new Vector2(1f, 1f);
        feedRect.pivot = new Vector2(0.5f, 1f);
        feedRect.anchoredPosition = new Vector2(0f, -265f);
        feedRect.sizeDelta = new Vector2(-40f, 30f);
        Text feedTxt = feedObj.AddComponent<Text>();
        feedTxt.font = font;
        feedTxt.fontSize = 12;
        feedTxt.color = new Color(0.2f, 0.2f, 0.2f);
        feedTxt.alignment = TextAnchor.MiddleCenter;

        // Buttons Container
        GameObject btnContainer = new GameObject("Buttons");
        btnContainer.transform.SetParent(panelObj.transform, false);
        RectTransform btnContainerRect = btnContainer.AddComponent<RectTransform>();
        btnContainerRect.anchorMin = new Vector2(0f, 0f);
        btnContainerRect.anchorMax = new Vector2(1f, 0f);
        btnContainerRect.pivot = new Vector2(0.5f, 0f);
        btnContainerRect.anchoredPosition = new Vector2(0f, 25f);
        btnContainerRect.sizeDelta = new Vector2(-40f, 36f);

        // Submit Button
        GameObject submitBtnObj = new GameObject("Btn_SubmitLog");
        submitBtnObj.transform.SetParent(btnContainer.transform, false);
        RectTransform submitRect = submitBtnObj.AddComponent<RectTransform>();
        submitRect.anchorMin = new Vector2(0f, 0f);
        submitRect.anchorMax = new Vector2(0.5f, 1f);
        submitRect.pivot = new Vector2(0.5f, 0.5f);
        submitRect.anchoredPosition = new Vector2(0f, 0f);
        submitRect.sizeDelta = new Vector2(0f, 0f);

        UnityEngine.UI.Image submitImg = submitBtnObj.AddComponent<UnityEngine.UI.Image>();
        submitImg.color = new Color(0.12f, 0.58f, 0.25f, 1f); // Green
        submitBtnObj.AddComponent<Outline>().effectColor = new Color(0.08f, 0.4f, 0.18f, 1f);
        Button submitBtn = submitBtnObj.AddComponent<Button>();

        GameObject submitTextObj = new GameObject("Text");
        submitTextObj.transform.SetParent(submitBtnObj.transform, false);
        RectTransform submitTextRect = submitTextObj.AddComponent<RectTransform>();
        submitTextRect.anchorMin = Vector2.zero;
        submitTextRect.anchorMax = Vector2.one;
        submitTextRect.offsetMin = Vector2.zero;
        submitTextRect.offsetMax = Vector2.zero;
        Text submitTxt = submitTextObj.AddComponent<Text>();
        submitTxt.text = "HANTAR REKOD";
        submitTxt.font = font;
        submitTxt.fontSize = 13;
        submitTxt.fontStyle = FontStyle.Bold;
        submitTxt.color = Color.white;
        submitTxt.alignment = TextAnchor.MiddleCenter;

        submitBtn.onClick.AddListener(notebookScript.SubmitRecords);

        // Close Button
        GameObject closeBtnObj = new GameObject("Btn_CloseLog");
        closeBtnObj.transform.SetParent(btnContainer.transform, false);
        RectTransform closeRect = closeBtnObj.AddComponent<RectTransform>();
        closeRect.anchorMin = new Vector2(0.5f, 0f);
        closeRect.anchorMax = new Vector2(1f, 1f);
        closeRect.pivot = new Vector2(0.5f, 0.5f);
        closeRect.anchoredPosition = new Vector2(0f, 0f);
        closeRect.sizeDelta = new Vector2(0f, 0f);

        UnityEngine.UI.Image closeImg = closeBtnObj.AddComponent<UnityEngine.UI.Image>();
        closeImg.color = new Color(0.65f, 0.2f, 0.2f, 1f); // Red
        closeBtnObj.AddComponent<Outline>().effectColor = new Color(0.45f, 0.12f, 0.12f, 1f);
        Button closeBtn = closeBtnObj.AddComponent<Button>();

        GameObject closeTextObj = new GameObject("Text");
        closeTextObj.transform.SetParent(closeBtnObj.transform, false);
        RectTransform closeTextRect = closeTextObj.AddComponent<RectTransform>();
        closeTextRect.anchorMin = Vector2.zero;
        closeTextRect.anchorMax = Vector2.one;
        closeTextRect.offsetMin = Vector2.zero;
        closeTextRect.offsetMax = Vector2.zero;
        Text closeTxt = closeTextObj.AddComponent<Text>();
        closeTxt.text = "TUTUP";
        closeTxt.font = font;
        closeTxt.fontSize = 13;
        closeTxt.fontStyle = FontStyle.Bold;
        closeTxt.color = Color.white;
        closeTxt.alignment = TextAnchor.MiddleCenter;

        closeBtn.onClick.AddListener(notebookScript.ClosePanel);

        // Link references to notebookScript
        notebookScript.uiPanel = panelObj;
        notebookScript.inputA = inputA;
        notebookScript.inputB = inputB;
        notebookScript.feedbackText = feedTxt;

        // Set all panel children to UI layer (5) to match Canvas
        SetLayerRecursive(panelObj, 5);

        panelObj.SetActive(false);
    }

    private static void SetLayerRecursive(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursive(child.gameObject, layer);
        }
    }
    private static GameObject SetupStopwatch(Vector3 experimentOffset, Material bodyMat, Material glassMat)
    {
        GameObject stopwatch = new GameObject("Stopwatch_3D");
        stopwatch.transform.position = experimentOffset + new Vector3(-1.25f, 0.013f, -0.6f);
        stopwatch.transform.rotation = Quaternion.Euler(0f, 348.57f, 0f);

        // Main body
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        body.name = "Body";
        body.transform.SetParent(stopwatch.transform, false);
        body.transform.localScale = new Vector3(0.15f, 0.015f, 0.15f);
        body.GetComponent<MeshRenderer>().sharedMaterial = bodyMat;
        
        // Screen/Glass
        GameObject screen = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        screen.name = "Screen";
        screen.transform.SetParent(stopwatch.transform, false);
        screen.transform.localPosition = new Vector3(0f, 0.016f, 0f);
        screen.transform.localScale = new Vector3(0.13f, 0.002f, 0.13f);
        screen.GetComponent<MeshRenderer>().sharedMaterial = glassMat;
        DestroyImmediate(screen.GetComponent<Collider>());

        // Text
        GameObject textObj = new GameObject("TimeText");
        textObj.transform.SetParent(stopwatch.transform, false);
        textObj.transform.localPosition = new Vector3(0f, 0.018f, 0f);
        textObj.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        textObj.transform.localScale = new Vector3(0.005f, 0.005f, 0.005f);

        TextMesh tm = textObj.AddComponent<TextMesh>();
        tm.text = "00:00.00";
        tm.fontSize = 64;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        tm.color = Color.green;
        tm.fontStyle = FontStyle.Bold;

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        tm.font = font;
        if (font != null)
        {
            textObj.GetComponent<MeshRenderer>().sharedMaterial = font.material;
        }

        // Add script
        Stopwatch3D swScript = stopwatch.AddComponent<Stopwatch3D>();
        swScript.timeText = tm;

        return stopwatch;
    }
    private static GameObject CreateDissolvingParticles(GameObject parentObj, string name)
    {
        GameObject psObj = new GameObject(name);
        psObj.transform.SetParent(parentObj.transform, false);
        psObj.transform.localPosition = new Vector3(0f, 0.4f, 0f);
        
        ParticleSystem ps = psObj.AddComponent<ParticleSystem>();
        
        var main = ps.main;
        main.duration = 2.0f;
        main.loop = false;
        main.startLifetime = 1.5f;
        main.startSpeed = new ParticleSystem.MinMaxCurve(-0.1f, -0.2f); // Moving downwards slowly
        main.startSize = new ParticleSystem.MinMaxCurve(0.008f, 0.013f);
        main.startColor = Color.white;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.playOnAwake = false;

        var emission = ps.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0.0f, 60) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.05f;
        shape.rotation = new Vector3(90f, 0f, 0f); // Point downwards

        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.white, 0.0f), new GradientColorKey(Color.white, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(0.0f, 1.0f) }
        );
        colorOverLifetime.color = grad;

        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve curve = new AnimationCurve();
        curve.AddKey(0.0f, 1.0f);
        curve.AddKey(1.0f, 0.0f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1.0f, curve);

        ParticleSystemRenderer psr = psObj.GetComponent<ParticleSystemRenderer>();
        psr.material = new Material(Shader.Find("Particles/Standard Unlit"));

        return psObj;
    }
    private static void SetupPostExperimentSummary(GameObject canvasObj, QuizManager quizManager)
    {
        if (canvasObj == null || quizManager == null) return;

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        // Main Panel
        GameObject summaryPanelObj = new GameObject("Lab3_SummaryPanel");
        summaryPanelObj.transform.SetParent(canvasObj.transform, false);
        RectTransform panelRect = summaryPanelObj.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(600f, 400f);

        UnityEngine.UI.Image panelImg = summaryPanelObj.AddComponent<UnityEngine.UI.Image>();
        panelImg.color = new Color(0.95f, 0.95f, 0.95f, 1f); // White paper
        
        Outline outline = summaryPanelObj.AddComponent<Outline>();
        outline.effectColor = new Color(0.2f, 0.2f, 0.2f, 1f);
        outline.effectDistance = new Vector2(2f, -2f);

        // Title
        GameObject titleObj = new GameObject("TitleText");
        titleObj.transform.SetParent(summaryPanelObj.transform, false);
        RectTransform titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0f, 1f);
        titleRect.anchorMax = new Vector2(1f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = new Vector2(0f, -20f);
        titleRect.sizeDelta = new Vector2(-40f, 40f);

        UnityEngine.UI.Text titleTxt = titleObj.AddComponent<UnityEngine.UI.Text>();
        titleTxt.text = "LAPORAN EKSPERIMEN: TAKAT DIDIH";
        titleTxt.font = font;
        titleTxt.fontSize = 24;
        titleTxt.fontStyle = FontStyle.Bold;
        titleTxt.color = new Color(0.1f, 0.1f, 0.3f, 1f);
        titleTxt.alignment = TextAnchor.MiddleCenter;

        // Content
        GameObject contentObj = new GameObject("ContentText");
        contentObj.transform.SetParent(summaryPanelObj.transform, false);
        RectTransform contentRect = contentObj.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0f, 0f);
        contentRect.anchorMax = new Vector2(1f, 1f);
        contentRect.pivot = new Vector2(0.5f, 0.5f);
        contentRect.anchoredPosition = new Vector2(0f, -40f);
        contentRect.sizeDelta = new Vector2(-60f, -120f);

        UnityEngine.UI.Text contentTxt = contentObj.AddComponent<UnityEngine.UI.Text>();
        contentTxt.text = "<b>Hipotesis:</b>\nKehadiran bendasing akan meningkatkan takat didih air.\n\n" +
                          "<b>Data Keputusan:</b>\n- Beaker A (Air Tulen): 100°C\n- Beaker B (Air + Garam): 104°C\n\n" +
                          "<b>Kesimpulan:</b>\nEksperimen ini berjaya membuktikan bahawa penambahan bendasing seperti garam ke dalam air tulen meningkatkan takat didihnya. Hipotesis diterima.";
        contentTxt.font = font;
        contentTxt.fontSize = 18;
        contentTxt.color = new Color(0.15f, 0.15f, 0.15f, 1f);
        contentTxt.alignment = TextAnchor.UpperLeft;
        
        // Close Button
        GameObject closeBtnObj = new GameObject("Btn_CloseSummary");
        closeBtnObj.transform.SetParent(summaryPanelObj.transform, false);
        RectTransform closeRect = closeBtnObj.AddComponent<RectTransform>();
        closeRect.anchorMin = new Vector2(0.5f, 0f);
        closeRect.anchorMax = new Vector2(0.5f, 0f);
        closeRect.pivot = new Vector2(0.5f, 0f);
        closeRect.anchoredPosition = new Vector2(0f, 30f);
        closeRect.sizeDelta = new Vector2(150f, 40f);

        UnityEngine.UI.Image closeImg = closeBtnObj.AddComponent<UnityEngine.UI.Image>();
        closeImg.color = new Color(0.2f, 0.6f, 0.2f, 1f);
        UnityEngine.UI.Button closeBtn = closeBtnObj.AddComponent<UnityEngine.UI.Button>();

        GameObject closeTextObj = new GameObject("Text");
        closeTextObj.transform.SetParent(closeBtnObj.transform, false);
        RectTransform closeTextRect = closeTextObj.AddComponent<RectTransform>();
        closeTextRect.anchorMin = Vector2.zero;
        closeTextRect.anchorMax = Vector2.one;
        closeTextRect.offsetMin = Vector2.zero;
        closeTextRect.offsetMax = Vector2.zero;

        UnityEngine.UI.Text closeTxt = closeTextObj.AddComponent<UnityEngine.UI.Text>();
        closeTxt.text = "SELESAI";
        closeTxt.font = font;
        closeTxt.fontSize = 16;
        closeTxt.fontStyle = FontStyle.Bold;
        closeTxt.color = Color.white;
        closeTxt.alignment = TextAnchor.MiddleCenter;

        closeBtn.onClick.AddListener(() => { summaryPanelObj.SetActive(false); });

        summaryPanelObj.SetActive(false); // Hide by default
        quizManager.summaryPanel = summaryPanelObj; // Wire to quiz manager
    }
    private static Lab3TutorialManager SetupTutorialOverlay(Vector3 experimentOffset)
    {
        GameObject tutorialManagerObj = new GameObject("TutorialManager");
        Lab3TutorialManager tutManager = tutorialManagerObj.AddComponent<Lab3TutorialManager>();

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        Material fontMat = font != null ? font.material : null;

        // Step 1: Salt
        tutManager.step1_Salt = CreateTutorialText("Tut_Step1", "1. Masukkan Garam", experimentOffset + new Vector3(0.85f, 0.25f, -0.2f), font, fontMat);
        
        // Step 2: Goggles
        tutManager.step2_Goggles = CreateTutorialText("Tut_Step2", "2. Pakai Gogal", experimentOffset + new Vector3(0.55f, 0.2f, -0.25f), font, fontMat);

        // Step 3: Gas
        tutManager.step3_Gas = CreateTutorialText("Tut_Step3", "3. Buka Gas", experimentOffset + new Vector3(-0.6f, 0.2f, -1.0f), font, fontMat);

        // Step 4: Start
        tutManager.step4_Start = CreateTutorialText("Tut_Step4", "4. Mula Pemanasan", experimentOffset + new Vector3(-0.6f, 0.1f, -1.0f), font, fontMat);

        return tutManager;
    }

    private static GameObject CreateTutorialText(string name, string text, Vector3 position, Font font, Material fontMat)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.position = position;
        
        // Always face the camera generally
        textObj.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        textObj.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);

        TextMesh tm = textObj.AddComponent<TextMesh>();
        tm.text = text;
        tm.fontSize = 60;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        tm.color = Color.black;
        tm.fontStyle = FontStyle.Bold;

        if (font != null)
        {
            tm.font = font;
            textObj.GetComponent<MeshRenderer>().sharedMaterial = fontMat;
        }

        // Add a look-at camera script dynamically
        textObj.AddComponent<LookAtCamera3D>();

        return textObj;
    }
}
#endif

