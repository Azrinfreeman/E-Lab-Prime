#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class Lab4_3D_Setup : EditorWindow
{
    [MenuItem("E-Lab Tools/Generate 3D Lab 4 (Water Evaporation)")]
    public static void ShowWindow()
    {
        GenerateLab4_3D();
    }

    private static void GenerateLab4_3D()
    {
        if (EditorSceneManager.GetActiveScene().name != "Lab4_3D")
        {
            EditorSceneManager.OpenScene("Assets/Scenes/Lab4_3D.unity", OpenSceneMode.Single);
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
                    t.text = "AMALI 4 PENYEJATAN AIR";
                    t.color = Color.white;
                }
                
                var tmpUIs = titleObj.GetComponentsInChildren<TMPro.TextMeshProUGUI>(true);
                foreach (var t in tmpUIs) 
                {
                    t.text = "AMALI 4 PENYEJATAN AIR";
                    t.color = Color.white;
                }
                
                var txts = titleObj.GetComponentsInChildren<UnityEngine.UI.Text>(true);
                foreach (var t in txts) 
                {
                    t.text = "AMALI 4 PENYEJATAN AIR";
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

            // Create a dramatic overhead desk light
            Transform oldDeskLight = env.transform.Find("Overhead_DeskLight");
            if (oldDeskLight != null) DestroyImmediate(oldDeskLight.gameObject);

            GameObject deskLightObj = new GameObject("Overhead_DeskLight");
            deskLightObj.transform.SetParent(env.transform);
            deskLightObj.transform.position = experimentOffset + new Vector3(0f, 3.5f, 0.5f);
            deskLightObj.transform.localRotation = Quaternion.Euler(75f, 0f, 0f);
            
            Light deskLight = deskLightObj.AddComponent<Light>();
            deskLight.type = LightType.Spot;
            deskLight.intensity = 3.5f;
            deskLight.range = 10f;
            deskLight.spotAngle = 85f;
            deskLight.innerSpotAngle = 50f;
            deskLight.color = new Color(0.98f, 0.99f, 1f);
            deskLight.shadows = LightShadows.Soft;

            // Adjust Main Camera to look at the table setup
            if (Camera.main != null)
            {
                Camera.main.transform.position = experimentOffset + new Vector3(0, 0.95f, -1.8f);
                Camera.main.transform.rotation = Quaternion.Euler(22f, 0f, 0f);
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

        // 0.5 Rescue the Right UI panel from the tablet before destroying the tablet
        GameObject existingRightPanel = null;
        Transform[] allTransforms = Object.FindObjectsByType<Transform>(FindObjectsInactive.Include);
        foreach (Transform t in allTransforms)
        {
            if (t.name == "Right" && (t.parent == null || t.parent.name == "TabletCanvas" || t.parent.name == "bg"))
            {
                existingRightPanel = t.gameObject;
                break;
            }
        }
        if (existingRightPanel != null)
        {
            GameObject canvasForRescue = GameObject.Find("Canvas");
            if (canvasForRescue != null)
            {
                Transform bgTrans = canvasForRescue.transform.Find("bg");
                if (bgTrans != null)
                {
                    existingRightPanel.transform.SetParent(bgTrans, false);
                    existingRightPanel.SetActive(true);
                }
            }
        }

        // 1. Clean up existing generated objects to ensure clean regeneration
        DestroyIfExists("Container_Group");
        DestroyIfExists("Beaker_3D");
        DestroyIfExists("Water_Pivot");
        DestroyIfExists("Lamp_3D");
        DestroyIfExists("Pemanas_3D");
        DestroyIfExists("Fan_3D");
        DestroyIfExists("HologramDataBoard");
        DestroyIfExists("Lab4_Notebook");
        DestroyIfExists("Lab4_GridMat");
        DestroyIfExists("RotateCameraButton");
        DestroyIfExists("Global_PostProcessing");
        DestroyIfExists("Particle_Wap_3D");
        DestroyIfExists("Particle_Angin_3D");
        DestroyIfExists("Lab4_3DTablet");
        DestroyIfExists("Lab4_NotebookPanel");
        DestroyIfExists("Lab4_QuizPanel");
        DestroyIfExists("Lab4_SummaryPanel");
        DestroyIfExists("QuizManagerLab4");
        DestroyIfExists("Lab4TutorialManager");
        DestroyIfExists("Tut_Step1");
        DestroyIfExists("Tut_Step2");
        DestroyIfExists("TableSocketBox");
        DestroyIfExists("AmbientAudio_3D");
        DestroyIfExists("HeaterPowerSwitch_Group");
        DestroyIfExists("HeaterCable");
        DestroyIfExists("FanCable");
        DestroyIfExists("Thermometer_3D");
        DestroyIfExists("Particle_Haze_3D");

        // 9. Main 3D Manager Setup (moved up to make managerScript available early)
        GameObject manager2DObj = GameObject.Find("EvaporationManager");
        EvaporationManager manager2D = null;
        if (manager2DObj != null)
        {
            manager2D = manager2DObj.GetComponent<EvaporationManager>();
        }

        GameObject manager3DObj = manager2DObj != null ? manager2DObj : new GameObject("EvaporationManager");
        manager3DObj.transform.position = new Vector3(-3.00f, 1.55f, 2.00f);
        if (manager2D != null)
        {
            // Remove 2D component to avoid updates running in parallel
            DestroyImmediate(manager2D);
        }

        EvaporationManager3D managerScript = manager3DObj.GetComponent<EvaporationManager3D>();
        if (managerScript == null)
        {
            managerScript = manager3DObj.AddComponent<EvaporationManager3D>();
        }

        // 2. Create or Load Materials properly as Assets
        Material glassMat = GetOrCreateMaterial("GlassMat_Lab4", new Color(0.9f, 0.93f, 0.96f, 0.24f), true);
        Material waterMat = GetOrCreateMaterial("WaterMat_Lab4", new Color(0.12f, 0.58f, 0.85f, 0.82f), true);
        Material metalMat = GetOrCreateMaterial("MetalMat_Lab4", new Color(0.18f, 0.18f, 0.2f, 1f), false);
        if (metalMat.HasProperty("_Metallic")) metalMat.SetFloat("_Metallic", 0.9f);
        if (metalMat.HasProperty("_Smoothness")) metalMat.SetFloat("_Smoothness", 0.8f);
        
        Material bladeMat = GetOrCreateMaterial("BladeMat_Lab4", new Color(0.85f, 0.85f, 0.85f, 1f), false);
        
        Material bulbMat = GetOrCreateMaterial("BulbMat_Lab4", new Color(0.2f, 0.2f, 0.2f, 1f), false);
        bulbMat.EnableKeyword("_EMISSION");
        bulbMat.SetColor("_EmissionColor", Color.black); // Start unlit

        // Adjust transparent sorting
        waterMat.renderQueue = 3000;
        glassMat.renderQueue = 3002;

        AssetDatabase.SaveAssets();

        // 3. Create 3D Container Group sitting on hot plate
        GameObject containerGroup = new GameObject("Container_Group");
        containerGroup.transform.position = experimentOffset;

        Material condensationMat = GetOrCreateMaterial("CondensationMat_Lab4", new Color(0.95f, 0.96f, 0.98f, 0f), true);
        Material redLiquidMat = GetOrCreateMaterial("RedLiquidMat_Lab4", new Color(1.0f, 0.1f, 0.1f, 1f), false);
        redLiquidMat.EnableKeyword("_EMISSION");
        redLiquidMat.SetColor("_EmissionColor", new Color(0.6f, 0f, 0f));

        // 3.1 Graduated Cylinder
        BeakerCondensation3D cylinderCond = null;
        Thermometer3D cylinderThermo = null;
        GameObject cylinderObj = CreateContainerGroup(
            "GraduatedCylinder", 
            containerGroup.transform, 
            new Vector3(0f, 0f, 0f), 
            new Vector3(0.15f, 0.25f, 0.15f), // Cylinder Scale (height = 0.5)
            new Vector3(0.15f, 1.0f, 0.15f),  // Water Scale
            new Vector3(0.92f, 0.18f, 0.92f), // Water Volume Cylinder Scale (height = 0.36)
            new Vector3(0f, 0.18f, 0f),       // Water Volume Local Pos
            new Vector3(0.012f, 0.32f, -0.012f), // Thermometer Local Pos (raised to prevent bulb clipping in hotplate)
            Quaternion.Euler(-6f, 0f, -6f),   // Thermometer Local Rotation (steeper lean)
            glassMat, waterMat, condensationMat, redLiquidMat,
            out cylinderCond, out cylinderThermo
        );

        // 3.2 Standard Beaker
        BeakerCondensation3D beakerCond = null;
        Thermometer3D beakerThermo = null;
        GameObject beakerObj = CreateContainerGroup(
            "StandardBeaker", 
            containerGroup.transform, 
            new Vector3(0f, 0f, 0f), 
            new Vector3(0.4f, 0.28f, 0.4f),  // Beaker Scale (height = 0.56)
            new Vector3(0.4f, 1.0f, 0.4f),   // Water Scale
            new Vector3(0.92f, 0.2f, 0.92f), // Water Volume Scale (height = 0.4)
            new Vector3(0f, 0.2f, 0f),       // Water Volume Local Pos
            new Vector3(0.05f, 0.32f, -0.05f), // Thermometer Local Pos (raised slightly to sit correctly in water)
            Quaternion.Euler(-12f, 0f, -12f), // Thermometer Local Rotation
            glassMat, waterMat, condensationMat, redLiquidMat,
            out beakerCond, out beakerThermo
        );

        // 3.3 Petri Dish
        BeakerCondensation3D petriCond = null;
        Thermometer3D petriThermo = null;
        GameObject petriObj = CreateContainerGroup(
            "PetriDish", 
            containerGroup.transform, 
            new Vector3(0f, 0f, 0f), 
            new Vector3(0.40f, 0.05f, 0.40f), // Petri Scale (height = 0.10, Y scale = 0.05) - resized from 0.65 to fit on plate and not block the switch
            new Vector3(0.40f, 1.0f, 0.40f),  // Water Scale
            new Vector3(0.96f, 0.03f, 0.96f), // Water Volume Scale (height = 0.06)
            new Vector3(0f, 0.03f, 0f),       // Water Volume Local Pos
            new Vector3(0.08f, 0.24f, -0.08f), // Thermometer Local Pos (leaning flatter but bulb sitting correctly in water)
            Quaternion.Euler(-32f, 0f, -32f),  // Thermometer Local Rotation
            glassMat, waterMat, condensationMat, redLiquidMat,
            out petriCond, out petriThermo
        );

        // 5. Create 3D Hot Plate (Pemanas) under beaker
        GameObject pemanas = new GameObject("Pemanas_3D");
        pemanas.transform.position = experimentOffset;

        // Base Board (Wide bottom stand)
        GameObject heaterBase = GameObject.CreatePrimitive(PrimitiveType.Cube);
        heaterBase.name = "HeaterBase";
        heaterBase.transform.SetParent(pemanas.transform, false);
        heaterBase.transform.localPosition = new Vector3(0f, 0.015f, 0f);
        heaterBase.transform.localScale = new Vector3(0.72f, 0.02f, 0.5f);
        Material darkMetal = GetOrCreateMaterial("DarkMetalMat_Lab4", new Color(0.15f, 0.17f, 0.2f, 1f), false);
        if (darkMetal.HasProperty("_Metallic")) darkMetal.SetFloat("_Metallic", 0.6f);
        if (darkMetal.HasProperty("_Smoothness")) darkMetal.SetFloat("_Smoothness", 0.5f);
        heaterBase.GetComponent<MeshRenderer>().sharedMaterial = darkMetal;
        DestroyImmediate(heaterBase.GetComponent<Collider>());

        // 4 Bottom Legs under base plate
        for (int i = 0; i < 4; i++)
        {
            GameObject leg = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leg.name = "BaseLeg_" + i;
            leg.transform.SetParent(pemanas.transform, false);
            float signX = (i % 2 == 0) ? 1f : -1f;
            float signZ = (i / 2 == 0) ? 1f : -1f;
            leg.transform.localPosition = new Vector3(signX * 0.33f, 0.005f, signZ * 0.22f);
            leg.transform.localScale = new Vector3(0.04f, 0.01f, 0.04f);
            leg.GetComponent<MeshRenderer>().sharedMaterial = darkMetal;
            DestroyImmediate(leg.GetComponent<Collider>());
        }

        // 4 Rubber Feet supporting the chassis on the base board
        Material rubberFeetMat = GetOrCreateMaterial("RubberFeetMat_Lab4", new Color(0.08f, 0.08f, 0.08f, 1f), false);
        for (int i = 0; i < 4; i++)
        {
            GameObject chassisFoot = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            chassisFoot.name = "ChassisFoot_" + i;
            chassisFoot.transform.SetParent(pemanas.transform, false);
            float signX = (i % 2 == 0) ? 1f : -1f;
            float signZ = (i / 2 == 0) ? 1f : -1f;
            chassisFoot.transform.localPosition = new Vector3(signX * 0.25f, 0.03f, signZ * 0.21f);
            chassisFoot.transform.localScale = new Vector3(0.03f, 0.005f, 0.03f); // cylinder height is 2x, so thickness = 0.01
            chassisFoot.GetComponent<MeshRenderer>().sharedMaterial = rubberFeetMat;
            DestroyImmediate(chassisFoot.GetComponent<Collider>());
        }

        // Main Body (Chassis) - sits on rubber feet
        GameObject heaterBody = GameObject.CreatePrimitive(PrimitiveType.Cube);
        heaterBody.name = "HeaterBody";
        heaterBody.transform.SetParent(pemanas.transform, false);
        heaterBody.transform.localPosition = new Vector3(0f, 0.06f, 0f);
        heaterBody.transform.localScale = new Vector3(0.58f, 0.05f, 0.44f);
        Material chassisMat = GetOrCreateMaterial("ChassisMat_Lab4", new Color(0.22f, 0.25f, 0.3f, 1f), false); // premium slate blue/grey paint
        if (chassisMat.HasProperty("_Metallic")) chassisMat.SetFloat("_Metallic", 0.4f);
        if (chassisMat.HasProperty("_Smoothness")) chassisMat.SetFloat("_Smoothness", 0.6f);
        heaterBody.GetComponent<MeshRenderer>().sharedMaterial = chassisMat;
        DestroyImmediate(heaterBody.GetComponent<Collider>());

        // Silver Metal Rim (pan) on top of the chassis
        GameObject heaterRim = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        heaterRim.name = "HeaterRim";
        heaterRim.transform.SetParent(pemanas.transform, false);
        heaterRim.transform.localPosition = new Vector3(0f, 0.089f, 0f);
        heaterRim.transform.localScale = new Vector3(0.40f, 0.004f, 0.40f); // thickness = 0.008
        Material rimMat = GetOrCreateMaterial("HeaterRimMat_Lab4", new Color(0.85f, 0.87f, 0.9f, 1f), false);
        if (rimMat.HasProperty("_Metallic")) rimMat.SetFloat("_Metallic", 0.9f);
        if (rimMat.HasProperty("_Smoothness")) rimMat.SetFloat("_Smoothness", 0.85f);
        heaterRim.GetComponent<MeshRenderer>().sharedMaterial = rimMat;
        DestroyImmediate(heaterRim.GetComponent<Collider>());

        // Heating Plate (Element) inside the rim
        GameObject heaterPlate = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        heaterPlate.name = "HeaterPlate";
        heaterPlate.transform.SetParent(pemanas.transform, false);
        heaterPlate.transform.localPosition = new Vector3(0f, 0.093f, 0f);
        heaterPlate.transform.localScale = new Vector3(0.36f, 0.002f, 0.36f); // thickness = 0.004
        Material heaterPlateMat = GetOrCreateMaterial("HeaterPlateMat_Lab4", new Color(0.18f, 0.18f, 0.18f, 1f), false);
        heaterPlateMat.EnableKeyword("_EMISSION");
        heaterPlateMat.SetColor("_EmissionColor", Color.black);
        heaterPlate.GetComponent<MeshRenderer>().sharedMaterial = heaterPlateMat;
        DestroyImmediate(heaterPlate.GetComponent<Collider>());

        // Knob Parent (allows rotation around local Z axis without overriding cylinder rotation)
        GameObject heaterKnobParent = new GameObject("HeaterKnob");
        heaterKnobParent.transform.SetParent(pemanas.transform, false);
        heaterKnobParent.transform.localPosition = new Vector3(0.18f, 0.06f, -0.222f); // on the front face of chassis

        // Knob Model (cylinder rotated 90 around X)
        GameObject heaterKnobModel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        heaterKnobModel.name = "KnobModel";
        heaterKnobModel.transform.SetParent(heaterKnobParent.transform, false);
        heaterKnobModel.transform.localPosition = Vector3.zero;
        heaterKnobModel.transform.localScale = new Vector3(0.06f, 0.0075f, 0.06f); // thickness = 0.015
        heaterKnobModel.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        Material knobMat = GetOrCreateMaterial("KnobMat_Lab4", new Color(0.1f, 0.1f, 0.1f, 1f), false);
        heaterKnobModel.GetComponent<MeshRenderer>().sharedMaterial = knobMat;
        DestroyImmediate(heaterKnobModel.GetComponent<Collider>());

        // Indicator line on the knob front face
        GameObject knobIndicator = GameObject.CreatePrimitive(PrimitiveType.Cube);
        knobIndicator.name = "KnobIndicator";
        knobIndicator.transform.SetParent(heaterKnobParent.transform, false);
        knobIndicator.transform.localPosition = new Vector3(0f, 0.02f, -0.0085f); // 12 o'clock, in front of knob cylinder
        knobIndicator.transform.localScale = new Vector3(0.008f, 0.02f, 0.008f);
        Material indicatorMat = GetOrCreateMaterial("IndicatorMat_Lab4", new Color(0.9f, 0.9f, 0.9f, 1f), false);
        knobIndicator.GetComponent<MeshRenderer>().sharedMaterial = indicatorMat;
        DestroyImmediate(knobIndicator.GetComponent<Collider>());

        // Dial Labels 0 to 5 around the knob
        Font dialFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        float labelRadius = 0.045f;
        for (int i = 0; i <= 5; i++)
        {
            GameObject dialLabel = new GameObject("DialLabel_" + i);
            dialLabel.transform.SetParent(pemanas.transform, false);
            
            float angle = i * 54f; // 0 to 270 degrees in 5 steps (54 deg each)
            float angleRad = angle * Mathf.Deg2Rad;
            
            float lx = labelRadius * Mathf.Sin(angleRad);
            float ly = labelRadius * Mathf.Cos(angleRad);
            
            dialLabel.transform.localPosition = new Vector3(0.18f + lx, 0.06f + ly, -0.224f); // slightly in front of chassis face
            dialLabel.transform.localScale = new Vector3(0.002f, 0.002f, 0.002f); // small text
            
            TextMesh tm = dialLabel.AddComponent<TextMesh>();
            tm.text = i.ToString();
            tm.fontSize = 24;
            tm.fontStyle = FontStyle.Bold;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
            tm.color = new Color(0.85f, 0.85f, 0.85f, 1f); // cool silver/white
            tm.font = dialFont;
            if (dialFont != null) dialLabel.GetComponent<MeshRenderer>().sharedMaterial = dialFont.material;
        }

        // LED Indicator (Sphere on the left)
        GameObject led = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        led.name = "LED";
        led.transform.SetParent(pemanas.transform, false);
        led.transform.localPosition = new Vector3(-0.18f, 0.06f, -0.222f); // on the front face of chassis
        led.transform.localScale = new Vector3(0.025f, 0.025f, 0.025f);
        Material ledMat = GetOrCreateMaterial("LEDMat_Lab4", new Color(0.2f, 0f, 0f, 1f), false); // dim red when off
        ledMat.EnableKeyword("_EMISSION");
        ledMat.SetColor("_EmissionColor", Color.black);
        led.GetComponent<MeshRenderer>().sharedMaterial = ledMat;
        DestroyImmediate(led.GetComponent<Collider>());

        // LED Label "On" next to the LED
        GameObject ledTextObj = new GameObject("LED_Label");
        ledTextObj.transform.SetParent(pemanas.transform, false);
        ledTextObj.transform.localPosition = new Vector3(-0.145f, 0.06f, -0.222f); // slightly to the right of LED
        ledTextObj.transform.localScale = new Vector3(0.003f, 0.003f, 0.003f); // tiny text
        
        TextMesh ledTM = ledTextObj.AddComponent<TextMesh>();
        ledTM.text = "On";
        ledTM.fontSize = 24;
        ledTM.fontStyle = FontStyle.Bold;
        ledTM.anchor = TextAnchor.MiddleLeft;
        ledTM.alignment = TextAlignment.Left;
        ledTM.color = Color.white;
        
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        ledTM.font = font;
        if (font != null) ledTextObj.GetComponent<MeshRenderer>().sharedMaterial = font.material;

        // 5.5 Heater Red Power Switch on the Front Chassis
        GameObject switchGroup = new GameObject("HeaterPowerSwitch_Group");
        switchGroup.transform.SetParent(pemanas.transform, false);
        switchGroup.transform.localPosition = new Vector3(-0.06f, 0.06f, -0.222f); // Sits on the front chassis

        // Black bezel backing
        GameObject bezel = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bezel.name = "Bezel";
        bezel.transform.SetParent(switchGroup.transform, false);
        bezel.transform.localPosition = Vector3.zero;
        bezel.transform.localScale = new Vector3(0.045f, 0.045f, 0.01f);
        Material bezelMat = GetOrCreateMaterial("SwitchBezelMat_Lab4", new Color(0.08f, 0.08f, 0.08f, 1f), false);
        bezel.GetComponent<MeshRenderer>().sharedMaterial = bezelMat;
        DestroyImmediate(bezel.GetComponent<Collider>());

        // Red rocker lever (tilted)
        GameObject leverPivotObj = new GameObject("LeverPivot");
        leverPivotObj.transform.SetParent(switchGroup.transform, false);
        leverPivotObj.transform.localPosition = new Vector3(0f, 0f, -0.005f); // sits slightly forward

        GameObject lever = GameObject.CreatePrimitive(PrimitiveType.Cube);
        lever.name = "Lever";
        lever.transform.SetParent(leverPivotObj.transform, false);
        lever.transform.localPosition = Vector3.zero;
        lever.transform.localScale = new Vector3(0.03f, 0.035f, 0.015f);
        Material leverMat = GetOrCreateMaterial("SwitchLeverMat_Lab4", new Color(0.9f, 0.1f, 0.1f, 1f), false); // Bright red rocker
        if (leverMat.HasProperty("_Metallic")) leverMat.SetFloat("_Metallic", 0.1f);
        if (leverMat.HasProperty("_Smoothness")) leverMat.SetFloat("_Smoothness", 0.6f);
        lever.GetComponent<MeshRenderer>().sharedMaterial = leverMat;
        DestroyImmediate(lever.GetComponent<Collider>());

        // Power switch component
        var switchScript = switchGroup.AddComponent<HeaterPowerSwitch3D>();
        switchScript.manager = managerScript;
        switchScript.switchLever = leverPivotObj.transform;
        switchScript.offAngle = -15f;
        switchScript.onAngle = 15f;
        switchScript.clickSFX = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/TickSound.wav");
        if (switchScript.clickSFX == null)
        {
            switchScript.clickSFX = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/sfx/snap.wav");
        }

        // Add custom click box collider
        BoxCollider switchCol = switchGroup.AddComponent<BoxCollider>();
        switchCol.center = Vector3.zero;
        switchCol.size = new Vector3(0.05f, 0.05f, 0.03f);

        // Add tooltip
        AddTooltip(switchGroup, "Suis Kuasa Pemanas");

        // Point Light for Glow
        GameObject plateLightObj = new GameObject("HeaterLight");
        plateLightObj.transform.SetParent(pemanas.transform, false);
        plateLightObj.transform.localPosition = new Vector3(0f, 0.15f, 0f); // shines upwards inside beaker
        
        Light plateLight = plateLightObj.AddComponent<Light>();
        plateLight.type = LightType.Point;
        plateLight.intensity = 0f;
        plateLight.range = 1.0f;
        plateLight.color = new Color(1.0f, 0.45f, 0.1f);
        plateLight.shadows = LightShadows.None;

        // 6. Create Table Fan on the left blowing towards beaker
        GameObject fan = new GameObject("Fan_3D");
        fan.transform.position = experimentOffset + new Vector3(-0.75f, 0.15f, 0f);

        // Fan Base
        GameObject fanBase = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        fanBase.name = "FanBase";
        fanBase.transform.SetParent(fan.transform, false);
        fanBase.transform.localPosition = new Vector3(0f, -0.135f, 0f);
        fanBase.transform.localScale = new Vector3(0.2f, 0.015f, 0.2f);
        fanBase.GetComponent<MeshRenderer>().sharedMaterial = metalMat;
        DestroyImmediate(fanBase.GetComponent<Collider>());

        // Fan Post / Neck
        GameObject fanPost = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        fanPost.name = "FanPost";
        fanPost.transform.SetParent(fan.transform, false);
        fanPost.transform.localPosition = new Vector3(0f, -0.05f, 0f);
        fanPost.transform.localScale = new Vector3(0.02f, 0.1f, 0.02f);
        fanPost.GetComponent<MeshRenderer>().sharedMaterial = metalMat;
        DestroyImmediate(fanPost.GetComponent<Collider>());

        // Motor housing (Behind blades)
        GameObject fanMotor = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        fanMotor.name = "FanMotor";
        fanMotor.transform.SetParent(fan.transform, false);
        fanMotor.transform.localPosition = new Vector3(0f, 0.08f, 0f);
        fanMotor.transform.localScale = new Vector3(0.09f, 0.06f, 0.09f);
        fanMotor.transform.localEulerAngles = new Vector3(0f, 0f, 90f);
        fanMotor.GetComponent<MeshRenderer>().sharedMaterial = metalMat;
        DestroyImmediate(fanMotor.GetComponent<Collider>());

        // Fan blades pivot (Rotates on X axis, pointing to right)
        GameObject bladesPivot = new GameObject("BladesPivot");
        bladesPivot.transform.SetParent(fan.transform, false);
        bladesPivot.transform.localPosition = new Vector3(0.07f, 0.08f, 0f); // shifted slightly right
        bladesPivot.transform.localRotation = Quaternion.identity;

        // Hub (Center cap)
        GameObject fanHub = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        fanHub.name = "Hub";
        fanHub.transform.SetParent(bladesPivot.transform, false);
        fanHub.transform.localPosition = Vector3.zero;
        fanHub.transform.localScale = new Vector3(0.04f, 0.01f, 0.04f);
        fanHub.transform.localEulerAngles = new Vector3(0f, 0f, 90f);
        fanHub.GetComponent<MeshRenderer>().sharedMaterial = bladeMat;
        DestroyImmediate(fanHub.GetComponent<Collider>());

        // Create 3 blades rotated by 120 deg
        for (int i = 0; i < 3; i++)
        {
            GameObject blade = GameObject.CreatePrimitive(PrimitiveType.Cube);
            blade.name = "Blade_" + i;
            blade.transform.SetParent(bladesPivot.transform, false);
            blade.transform.localScale = new Vector3(0.005f, 0.16f, 0.035f);
            
            // Offset blade center along length and rotate around X (roll)
            float angle = i * 120f;
            blade.transform.localRotation = Quaternion.Euler(angle, 0f, 15f); // 15deg pitch
            blade.transform.localPosition = Quaternion.Euler(angle, 0f, 0f) * new Vector3(0f, 0.08f, 0f);
            
            blade.GetComponent<MeshRenderer>().sharedMaterial = bladeMat;
            DestroyImmediate(blade.GetComponent<Collider>());
        }

        // Fan grill (Outline wireframe look)
        GameObject fanGrill = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        fanGrill.name = "FanGrill";
        fanGrill.transform.SetParent(fan.transform, false);
        fanGrill.transform.localPosition = new Vector3(0.075f, 0.08f, 0f);
        fanGrill.transform.localScale = new Vector3(0.21f, 0.005f, 0.21f);
        fanGrill.transform.localEulerAngles = new Vector3(0f, 0f, 90f);
        Material grillMat = GetOrCreateMaterial("GrillMat_Lab4", new Color(0.1f, 0.1f, 0.1f, 0.2f), true);
        fanGrill.GetComponent<MeshRenderer>().sharedMaterial = grillMat;
        DestroyImmediate(fanGrill.GetComponent<Collider>());

        // 7. Particle Systems for Steam (Wap) and Wind (Angin)
        GameObject particleWapObj = new GameObject("Particle_Wap_3D");
        particleWapObj.transform.position = experimentOffset + new Vector3(0f, 0.51f, 0f); // steam starts near water surface
        particleWapObj.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f); // rises vertically
        
        ParticleSystem pWap = particleWapObj.AddComponent<ParticleSystem>();
        ConfigureSteamParticleSystem(pWap);

        GameObject particleAnginObj = new GameObject("Particle_Angin_3D");
        particleAnginObj.transform.position = fan.transform.position + new Vector3(0.1f, 0.08f, 0f); // wind starts at fan center
        particleAnginObj.transform.localRotation = Quaternion.Euler(0f, 90f, 0f); // blows right (+X) towards beaker
        
        ParticleSystem pAngin = particleAnginObj.AddComponent<ParticleSystem>();
        ConfigureWindParticleSystem(pAngin);

        // 7.5 Create Heat Haze Particle System
        GameObject particleHazeObj = new GameObject("Particle_Haze_3D");
        particleHazeObj.transform.position = experimentOffset + new Vector3(0f, 0.097f, 0f); // heat haze starts at hot plate surface Y
        particleHazeObj.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f); // rises vertically
        
        ParticleSystem pHaze = particleHazeObj.AddComponent<ParticleSystem>();
        ConfigureHazeParticleSystem(pHaze);

        // 8. Environmental Props (Grid Mat & Notebook)
        Material gridMatMat = GetOrCreateMaterial("GridMatMat_Lab4", new Color(0.1f, 0.35f, 0.3f, 1f), false);
        GameObject gridMat = GameObject.CreatePrimitive(PrimitiveType.Cube);
        gridMat.name = "Lab4_GridMat";
        gridMat.transform.position = experimentOffset + new Vector3(0f, 0.004f, 0f);
        gridMat.transform.localScale = new Vector3(2.0f, 0.008f, 1.0f);
        gridMat.GetComponent<MeshRenderer>().sharedMaterial = gridMatMat;
        DestroyImmediate(gridMat.GetComponent<Collider>());

        // Floating Hologram Data Board
        Material hologramGlassMat = GetOrCreateMaterial("HologramGlassMat_Lab4", new Color(0f, 0.15f, 0.3f, 0.85f), true);
        Material hologramBorderMat = GetOrCreateMaterial("HologramBorderMat_Lab4", new Color(0f, 0.9f, 1f, 1f), false);
        GameObject hologramBoard = CreateHologramBoard(experimentOffset, hologramGlassMat, hologramBorderMat, out TextMesh holoTM);

        // Notebook
        InteractiveNotebookLab4 notebookScript = SetupNotebookProp(experimentOffset);

        // 8.5 Construct 3D Tablet on the Desk (Diegetic UI)
        GameObject tabletObj = new GameObject("Lab4_3DTablet");
        GameObject mainCam = GameObject.Find("Main Camera");
        if (mainCam != null)
        {
            mainCam.GetComponent<Camera>().nearClipPlane = 0.05f;
            tabletObj.transform.SetParent(mainCam.transform, false);
            tabletObj.transform.localPosition = new Vector3(0.2004f, -0.0798f, 0.3001f); // Anchored at bottom-right corner of camera view, pushed slightly more right/down
            tabletObj.transform.localRotation = Quaternion.Euler(-70f, 0f, 15f); // Angled naturally facing the camera
            tabletObj.transform.localScale = new Vector3(0.2943f, 0.2943f, 0.2943f); // Scaled up slightly as requested (from 0.20f)
        }
        else
        {
            tabletObj.transform.position = experimentOffset + new Vector3(0.6f, 0.05f, -0.4f);
            tabletObj.transform.rotation = Quaternion.Euler(0f, 330f, 0f);
        }

        // Casing
        GameObject casing = GameObject.CreatePrimitive(PrimitiveType.Cube);
        casing.name = "TabletCasing";
        casing.transform.SetParent(tabletObj.transform, false);
        casing.transform.localScale = new Vector3(0.33f, 0.012f, 0.44f);
        casing.GetComponent<MeshRenderer>().sharedMaterial = darkMetal; // Sleek dark metal casing
        DestroyImmediate(casing.GetComponent<Collider>());

        // Screen
        GameObject screen = GameObject.CreatePrimitive(PrimitiveType.Cube);
        screen.name = "TabletScreen";
        screen.transform.SetParent(tabletObj.transform, false);
        screen.transform.localPosition = new Vector3(0f, 0.007f, 0f);
        screen.transform.localScale = new Vector3(0.30f, 0.002f, 0.41f);
        Material screenMat = GetOrCreateMaterial("TabletScreenMat_Lab4", new Color(0.02f, 0.06f, 0.12f, 1f), false); // Cool sci-fi dark blue screen
        screen.GetComponent<MeshRenderer>().sharedMaterial = screenMat;
        DestroyImmediate(screen.GetComponent<Collider>());

        // World Space Canvas
        GameObject tabletCanvasObj = new GameObject("TabletCanvas");
        tabletCanvasObj.transform.SetParent(tabletObj.transform, false);
        tabletCanvasObj.transform.localPosition = new Vector3(0f, 0.009f, 0f); // Position above screen to prevent occlusion
        tabletCanvasObj.transform.localRotation = Quaternion.Euler(90f, 0f, 0f); // Lie flat facing up
        tabletCanvasObj.transform.localScale = new Vector3(0.0005f, 0.0005f, 0.0005f); // Scale pixel units to meters (1px = 0.5mm)

        Canvas canvas = tabletCanvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = Camera.main;

        RectTransform canvasRect = tabletCanvasObj.GetComponent<RectTransform>();
        if (canvasRect != null)
        {
            canvasRect.sizeDelta = new Vector2(600f, 820f); // Match physical screen dimensions
        }

        tabletCanvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        // Reparent and style the 2D Right Panel controls
        GameObject rightPanel = null;
        GameObject canvasObj = GameObject.Find("Canvas");
        if (canvasObj != null)
        {
            Transform rightTrans = canvasObj.transform.Find("bg/Right");
            if (rightTrans != null)
            {
                rightPanel = rightTrans.gameObject;
            }
        }
        if (rightPanel != null)
        {
            rightPanel.transform.SetParent(tabletCanvasObj.transform, false);
            RectTransform rightRT = rightPanel.GetComponent<RectTransform>();
            rightRT.anchorMin = new Vector2(0.5f, 0.5f);
            rightRT.anchorMax = new Vector2(0.5f, 0.5f);
            rightRT.pivot = new Vector2(0.5f, 0.5f);
            rightRT.anchoredPosition = Vector2.zero;
            rightRT.localScale = Vector3.one;
            rightRT.sizeDelta = new Vector2(580f, 810f);

            // Sleek glassmorphism style for background image
            var panelImg = rightPanel.GetComponent<UnityEngine.UI.Image>();
            if (panelImg != null)
            {
                panelImg.color = new Color(0.04f, 0.12f, 0.22f, 0.85f); // glassmorphic blue
            }
            var panelOutline = rightPanel.GetComponent<UnityEngine.UI.Outline>();
            if (panelOutline == null)
            {
                panelOutline = rightPanel.AddComponent<UnityEngine.UI.Outline>();
            }
            panelOutline.effectColor = new Color(0.15f, 0.6f, 0.95f, 0.5f);
            panelOutline.effectDistance = new Vector2(2f, 2f);

            // Style Title (1) Header
            GameObject titleHeader = rightPanel.transform.Find("Title (1)")?.gameObject;
            if (titleHeader != null)
            {
                var headerImg = titleHeader.GetComponent<UnityEngine.UI.Image>();
                if (headerImg != null)
                {
                    headerImg.color = new Color(0.08f, 0.25f, 0.45f, 0.9f);
                }
            }

            // Set all text/label fonts to white in rightPanel to prevent any black font rendering
            var allTMPs = rightPanel.GetComponentsInChildren<TMPro.TextMeshProUGUI>(true);
            foreach (var tmp in allTMPs)
            {
                tmp.color = Color.white;
            }
            var allUI = rightPanel.GetComponentsInChildren<UnityEngine.UI.Text>(true);
            foreach (var txt in allUI)
            {
                txt.color = Color.white;
            }
        }

        // Hide old 2D background/overlays
        GameObject bg2D = GameObject.Find("Canvas/bg");
        if (bg2D != null)
        {
            bg2D.SetActive(false);
        }

        // 8.75 Construct 3D Power Socket Box & Cables
        GameObject socketBox = GameObject.CreatePrimitive(PrimitiveType.Cube);
        socketBox.name = "TableSocketBox";
        socketBox.transform.position = experimentOffset + new Vector3(-0.3f, 0.03f, 0.4f);
        socketBox.transform.localScale = new Vector3(0.16f, 0.06f, 0.1f);
        Material socketMat = GetOrCreateMaterial("SocketBoxMat_Lab4", new Color(0.2f, 0.2f, 0.2f, 1f), false);
        socketBox.GetComponent<MeshRenderer>().sharedMaterial = socketMat;
        DestroyImmediate(socketBox.GetComponent<Collider>());

        // Socket faceplates
        for (int i = 0; i < 2; i++)
        {
            GameObject socketFace = GameObject.CreatePrimitive(PrimitiveType.Cube);
            socketFace.name = "SocketFace_" + i;
            socketFace.transform.SetParent(socketBox.transform, false);
            float offset = (i == 0) ? -0.04f : 0.04f;
            socketFace.transform.localPosition = new Vector3(offset, 0f, -0.051f);
            socketFace.transform.localScale = new Vector3(0.05f, 0.04f, 0.005f);
            Material faceMat = GetOrCreateMaterial("SocketFaceMat_Lab4", new Color(0.9f, 0.9f, 0.9f, 1f), false);
            socketFace.GetComponent<MeshRenderer>().sharedMaterial = faceMat;
            DestroyImmediate(socketFace.GetComponent<Collider>());

            // Small black square holes for sockets
            GameObject socketHole = GameObject.CreatePrimitive(PrimitiveType.Cube);
            socketHole.name = "SocketHole_" + i;
            socketHole.transform.SetParent(socketFace.transform, false);
            socketHole.transform.localPosition = new Vector3(0f, 0f, -0.6f);
            socketHole.transform.localScale = new Vector3(0.4f, 0.4f, 0.2f);
            Material blackHoleMat = GetOrCreateMaterial("SocketHoleMat_Lab4", Color.black, false);
            socketHole.GetComponent<MeshRenderer>().sharedMaterial = blackHoleMat;
            DestroyImmediate(socketHole.GetComponent<Collider>());
        }

        Vector3 heaterStart = experimentOffset + new Vector3(-0.26f, 0.03f, 0.349f);
        Vector3 fanStart = experimentOffset + new Vector3(-0.34f, 0.03f, 0.349f);
        Vector3 heaterEnd = experimentOffset + new Vector3(0f, 0.05f, 0.22f);
        Vector3 fanEnd = experimentOffset + new Vector3(-0.75f, 0.015f, 0.1f);

        Transform envTrans = env != null ? env.transform : null;
        WirePulseAnimator heaterCable = CreatePowerCable("HeaterCable", heaterStart, heaterEnd, experimentOffset.y, envTrans);
        WirePulseAnimator fanCable = CreatePowerCable("FanCable", fanStart, fanEnd, experimentOffset.y, envTrans);

        // Main 3D Manager References

        // Link references
        managerScript.cylinderGroup = cylinderObj;
        managerScript.beakerGroup = beakerObj;
        managerScript.petriGroup = petriObj;
        managerScript.thermometers = new Thermometer3D[] { cylinderThermo, beakerThermo, petriThermo };
        managerScript.thermometer = beakerThermo; // default fallback

        managerScript.fanBladesPivot = bladesPivot.transform;
        managerScript.fanRotationAxis = new Vector3(1f, 0f, 0f); // Rotates on X axis!
        managerScript.heatLampLight = plateLight;
        managerScript.heatLampBulb = heaterPlate.GetComponent<Renderer>();
        managerScript.heaterKnob = heaterKnobParent.transform;
        managerScript.ledIndicator = led.GetComponent<Renderer>();
        managerScript.maxLightIntensity = 4.0f;
        managerScript.particleWap = pWap;
        managerScript.particleAngin = pAngin;
        managerScript.hologramText = holoTM;
        managerScript.heaterCableAnimator = heaterCable;
        managerScript.fanCableAnimator = fanCable;
        managerScript.particleHaze = pHaze;

        // 9.5 Ambient & Mechanical Audio Setup
        GameObject ambientAudio = new GameObject("AmbientAudio_3D");
        AudioSource bgm = ambientAudio.AddComponent<AudioSource>();
        AudioClip humClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/RoomHum.wav");
        if (humClip != null)
        {
            bgm.clip = humClip;
            bgm.loop = true;
            bgm.volume = 0.015f; // Distantly soft ambient hum
            bgm.spatialBlend = 0f; // 2D sound
            bgm.playOnAwake = true;
            bgm.Play();
        }

        AudioSource clickSource = heaterKnobParent.AddComponent<AudioSource>();
        AudioClip tickClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/TickSound.wav");
        if (tickClip != null)
        {
            clickSource.clip = tickClip;
            clickSource.loop = false;
            clickSource.volume = 0.25f;
            clickSource.spatialBlend = 0.8f; // 3D sound near dial
            clickSource.playOnAwake = false;
        }
        managerScript.dialClickAudio = clickSource;
        managerScript.powerSwitch = switchScript;

        // Link Graph Reference
        if (notebookScript != null)
        {
            managerScript.graph3D = notebookScript.transform.Find("GraphContainer")?.GetComponent<EvaporationGraph3D>();
        }

        // Bind UI references dynamically from rightPanel (which has been moved to Tablet)
        Slider sSuhu = rightPanel != null ? rightPanel.transform.Find("GameObject/SuhuSlider")?.GetComponent<Slider>() : null;
        Slider sAngin = rightPanel != null ? rightPanel.transform.Find("GameObject/AnginSlider")?.GetComponent<Slider>() : null;
        Slider sArea = rightPanel != null ? rightPanel.transform.Find("GameObject/AreaSlider")?.GetComponent<Slider>() : null;
        Slider sHumidity = rightPanel != null ? rightPanel.transform.Find("GameObject/HumiditySlider")?.GetComponent<Slider>() : null;
        
        TMPro.TextMeshProUGUI evaporationRateText = rightPanel != null ? rightPanel.transform.Find("Bottom/KadarPenyejatan")?.GetComponentInChildren<TMPro.TextMeshProUGUI>() : null;

        managerScript.tempSlider = sSuhu;
        managerScript.windSlider = sAngin;
        managerScript.areaSlider = sArea;
        managerScript.humiditySlider = sHumidity;
        managerScript.evaporationRateText = evaporationRateText;

        // Reconnect UI Buttons
        Button startBtn = rightPanel != null ? rightPanel.transform.Find("Bottom/Button")?.GetComponent<Button>() : null;
        if (startBtn != null)
        {
            // Clear broken persistent listeners in the editor
            while (startBtn.onClick.GetPersistentEventCount() > 0)
            {
                UnityEditor.Events.UnityEventTools.RemovePersistentListener(startBtn.onClick, 0);
            }
            startBtn.onClick.RemoveAllListeners();
            startBtn.onClick.AddListener(managerScript.StartExperiment);
        }

        Button resetBtn = rightPanel != null ? rightPanel.transform.Find("Bottom/ResetBtn")?.GetComponent<Button>() : null;
        if (resetBtn != null)
        {
            // Clear broken persistent listeners in the editor
            while (resetBtn.onClick.GetPersistentEventCount() > 0)
            {
                UnityEditor.Events.UnityEventTools.RemovePersistentListener(resetBtn.onClick, 0);
            }
            resetBtn.onClick.RemoveAllListeners();
            resetBtn.onClick.AddListener(managerScript.ResetEksperimen);
        }

        // Setup Camera dragging
        SetupCameraDragButton();

        // Setup Tooltips
        if (cylinderObj != null)
        {
            Transform m = cylinderObj.transform.Find("GraduatedCylinder_Model");
            if (m != null) AddTooltip(m.gameObject, "Silinder Penyukat");
            Transform t = cylinderObj.transform.Find("Thermometer_3D");
            if (t != null) AddTooltip(t.gameObject, "Termometer");
        }
        if (beakerObj != null)
        {
            Transform m = beakerObj.transform.Find("StandardBeaker_Model");
            if (m != null) AddTooltip(m.gameObject, "Bikar Penyejatan");
            Transform t = beakerObj.transform.Find("Thermometer_3D");
            if (t != null) AddTooltip(t.gameObject, "Termometer");
        }
        if (petriObj != null)
        {
            Transform m = petriObj.transform.Find("PetriDish_Model");
            if (m != null) AddTooltip(m.gameObject, "Piring Petri");
            Transform t = petriObj.transform.Find("Thermometer_3D");
            if (t != null) AddTooltip(t.gameObject, "Termometer");
        }
        AddTooltip(heaterBody, "Pemanas Air (Hot Plate)");
        AddTooltip(fanMotor, "Kipas (Kelajuan Angin)");
        AddTooltip(hologramBoard, "Paparan Kadar Penyejatan");
        GameObject notebookObj = GameObject.Find("Lab4_Notebook");
        if (notebookObj != null) AddTooltip(notebookObj, "Buku Laporan");

        // Cinematic Post-processing
        
        // Setup Interactive UIs (Quiz, Notebook Panel)
        SetupInteractiveUIs(notebookScript);

        // Setup Tutorial
        Lab4TutorialManager tutManager = SetupTutorialOverlay(experimentOffset);
        managerScript.tutorialManager = tutManager;
        notebookScript.tutorialManager = tutManager;
        notebookScript.evaporationManager = managerScript;

        SetupPostProcessing();

        // Mark scene dirty and save
        var activeScene = EditorSceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(activeScene);
        EditorSceneManager.SaveScene(activeScene);

        Debug.Log("3D Lab 4 generated and converted successfully!");
    }

    private static void AddTooltip(GameObject obj, string tooltipText)
    {
        if (obj == null) return;
        if (obj.GetComponent<Collider>() == null)
        {
            BoxCollider col = obj.AddComponent<BoxCollider>();
            Bounds bounds = new Bounds(obj.transform.position, Vector3.zero);
            Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
            foreach (Renderer r in renderers)
            {
                if (r is ParticleSystemRenderer) continue;
                bounds.Encapsulate(r.bounds);
            }
            col.center = obj.transform.InverseTransformPoint(bounds.center);
            col.size = bounds.size;
        }

        TooltipHover3D tooltip = obj.GetComponent<TooltipHover3D>();
        if (tooltip == null)
        {
            tooltip = obj.AddComponent<TooltipHover3D>();
        }
        tooltip.tooltipText = tooltipText;
        EditorUtility.SetDirty(obj);
    }

    private static void ConfigureHazeParticleSystem(ParticleSystem ps)
    {
        var main = ps.main;
        main.duration = 1f;
        main.loop = true;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.6f, 1.2f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.4f, 0.8f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.18f);
        // Light warm shimmer color with extremely low alpha
        main.startColor = new Color(1.0f, 0.65f, 0.35f, 0.03f);
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = ps.emission;
        emission.rateOverTime = 0f; // controlled dynamically

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(0.3f, 0.3f, 0.01f); // fits heater surface

        var size = ps.sizeOverLifetime;
        size.enabled = true;
        size.size = new ParticleSystem.MinMaxCurve(1.0f, 2.2f); // expands as it rises

        var color = ps.colorOverLifetime;
        color.enabled = true;
        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] { new GradientColorKey(new Color(1f, 0.7f, 0.4f), 0f), new GradientColorKey(new Color(1f, 0.5f, 0.2f), 1f) },
            new GradientAlphaKey[] { new GradientAlphaKey(0.04f, 0f), new GradientAlphaKey(0.06f, 0.2f), new GradientAlphaKey(0f, 1f) }
        );
        color.color = grad;

        // Satisfy the shimmer effect using Noise module
        var noise = ps.noise;
        noise.enabled = true;
        noise.strength = 0.6f;
        noise.frequency = 2.0f;
        noise.scrollSpeed = 1.5f;
        noise.damping = true;

        ParticleSystemRenderer psr = ps.GetComponent<ParticleSystemRenderer>();
        psr.sharedMaterial = AssetDatabase.GetBuiltinExtraResource<Material>("Default-ParticleSystem.mat");
        psr.renderMode = ParticleSystemRenderMode.Billboard;
    }

    private static void ConfigureSteamParticleSystem(ParticleSystem ps)
    {
        var main = ps.main;
        main.duration = 1f;
        main.loop = true;
        main.startLifetime = new ParticleSystem.MinMaxCurve(1.5f, 2.5f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.15f, 0.3f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.04f, 0.08f);
        main.startColor = new Color(0.9f, 0.95f, 1f, 0.35f); // Soft whitish steam
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = ps.emission;
        emission.rateOverTime = 0f; // controlled dynamically by manager

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(0.2f, 0.2f, 0.01f); // fits water surface area

        var size = ps.sizeOverLifetime;
        size.enabled = true;
        size.size = new ParticleSystem.MinMaxCurve(1.0f, 2.0f); // expands as it rises

        var color = ps.colorOverLifetime;
        color.enabled = true;
        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new GradientAlphaKey[] { new GradientAlphaKey(0.4f, 0f), new GradientAlphaKey(0.8f, 0.2f), new GradientAlphaKey(0f, 1f) } // fade out
        );
        color.color = grad;

        ParticleSystemRenderer psr = ps.GetComponent<ParticleSystemRenderer>();
        psr.sharedMaterial = AssetDatabase.GetBuiltinExtraResource<Material>("Default-ParticleSystem.mat");
        psr.renderMode = ParticleSystemRenderMode.Billboard;
    }

    private static void ConfigureWindParticleSystem(ParticleSystem ps)
    {
        var main = ps.main;
        main.duration = 1f;
        main.loop = true;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.4f, 0.7f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(2.0f, 3.5f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.01f, 0.03f);
        main.startColor = new Color(0.85f, 0.95f, 1f, 0.25f);
        main.simulationSpace = ParticleSystemSimulationSpace.Local;

        var emission = ps.emission;
        emission.rateOverTime = 0f; // controlled dynamically by manager

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 15f; // cone dispersion
        shape.radius = 0.08f;

        var velocity = ps.velocityOverLifetime;
        velocity.enabled = false;

        ParticleSystemRenderer psr = ps.GetComponent<ParticleSystemRenderer>();
        psr.sharedMaterial = AssetDatabase.GetBuiltinExtraResource<Material>("Default-ParticleSystem.mat");
        psr.renderMode = ParticleSystemRenderMode.Stretch; // stretched particle vectors representing wind lines
        psr.lengthScale = 3f;
    }

    private static GameObject CreateHologramBoard(Vector3 experimentOffset, Material glassMat, Material borderMat, out TextMesh holoTM)
    {
        GameObject hologramBoard = new GameObject("HologramDataBoard");
        hologramBoard.transform.position = experimentOffset + new Vector3(-1.0f, 1.05f, 0.25f);
        hologramBoard.transform.localScale = new Vector3(0.55f, 0.55f, 0.55f);

        // Make it face the camera position initially during setup
        Vector3 camPos = experimentOffset + new Vector3(0f, 0.95f, -1.8f);
        hologramBoard.transform.LookAt(hologramBoard.transform.position + (hologramBoard.transform.position - camPos).normalized);

        // Add dynamic look-at component so it updates as camera is rotated/moved
        hologramBoard.AddComponent<LookAtCamera3D>();

        // Glass panel
        GameObject glassPlate = GameObject.CreatePrimitive(PrimitiveType.Cube);
        glassPlate.name = "HologramGlassPlate";
        glassPlate.transform.SetParent(hologramBoard.transform, false);
        glassPlate.transform.localScale = new Vector3(0.9f, 0.6f, 0.01f);
        glassPlate.GetComponent<MeshRenderer>().sharedMaterial = glassMat;
        DestroyImmediate(glassPlate.GetComponent<Collider>());

        // Border Top
        GameObject borderTop = GameObject.CreatePrimitive(PrimitiveType.Cube);
        borderTop.name = "BorderTop";
        borderTop.transform.SetParent(hologramBoard.transform, false);
        borderTop.transform.localPosition = new Vector3(0f, 0.31f, 0f);
        borderTop.transform.localScale = new Vector3(0.94f, 0.02f, 0.02f);
        borderTop.GetComponent<MeshRenderer>().sharedMaterial = borderMat;
        DestroyImmediate(borderTop.GetComponent<Collider>());

        // Border Bottom
        GameObject borderBottom = GameObject.CreatePrimitive(PrimitiveType.Cube);
        borderBottom.name = "BorderBottom";
        borderBottom.transform.SetParent(hologramBoard.transform, false);
        borderBottom.transform.localPosition = new Vector3(0f, -0.31f, 0f);
        borderBottom.transform.localScale = new Vector3(0.94f, 0.02f, 0.02f);
        borderBottom.GetComponent<MeshRenderer>().sharedMaterial = borderMat;
        DestroyImmediate(borderBottom.GetComponent<Collider>());

        // Border Left
        GameObject borderLeft = GameObject.CreatePrimitive(PrimitiveType.Cube);
        borderLeft.name = "BorderLeft";
        borderLeft.transform.SetParent(hologramBoard.transform, false);
        borderLeft.transform.localPosition = new Vector3(-0.46f, 0f, 0f);
        borderLeft.transform.localScale = new Vector3(0.02f, 0.64f, 0.02f);
        borderLeft.GetComponent<MeshRenderer>().sharedMaterial = borderMat;
        DestroyImmediate(borderLeft.GetComponent<Collider>());

        // Border Right
        GameObject borderRight = GameObject.CreatePrimitive(PrimitiveType.Cube);
        borderRight.name = "BorderRight";
        borderRight.transform.SetParent(hologramBoard.transform, false);
        borderRight.transform.localPosition = new Vector3(0.46f, 0f, 0f);
        borderRight.transform.localScale = new Vector3(0.02f, 0.64f, 0.02f);
        borderRight.GetComponent<MeshRenderer>().sharedMaterial = borderMat;
        DestroyImmediate(borderRight.GetComponent<Collider>());

        // Hologram Text
        GameObject holoTextObj = new GameObject("HologramText");
        holoTextObj.transform.SetParent(hologramBoard.transform, false);
        holoTextObj.transform.localPosition = new Vector3(0f, 0f, -0.015f);
        holoTextObj.transform.localScale = new Vector3(0.12f, 0.12f, 0.12f);

        holoTM = holoTextObj.AddComponent<TextMesh>();
        holoTM.text = "Kadar Penyejatan:\n0.000 unit/s";
        holoTM.characterSize = 0.1f;
        holoTM.fontSize = 64;
        holoTM.fontStyle = FontStyle.Bold;
        holoTM.anchor = TextAnchor.MiddleCenter;
        holoTM.alignment = TextAlignment.Center;
        holoTM.color = Color.white;

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        holoTM.font = font;
        if (font != null) holoTextObj.GetComponent<MeshRenderer>().sharedMaterial = font.material;

        return hologramBoard;
    }

    private static InteractiveNotebookLab4 SetupNotebookProp(Vector3 experimentOffset)
    {
        Material notebookCoverMat = GetOrCreateMaterial("NotebookCoverMat_Lab4", new Color(0.08f, 0.08f, 0.12f, 1f), false);
        Material notebookPageMat = GetOrCreateMaterial("NotebookPageMat_Lab4", new Color(0.6f, 0.58f, 0.5f, 1f), false);
        
        GameObject notebook = new GameObject("Lab4_Notebook");
        notebook.transform.position = experimentOffset + new Vector3(0.7f, 0.005f, -0.2f);
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
        leftTM.text = "AMALI 4: PENYEJATAN AIR\n\nTujuan:\nMengkaji faktor-faktor\nyang mempengaruhi kadar\npenyejatan air.\n\nHipotesis:\nKadar penyejatan meningkat\ndengan suhu tinggi, angin,\nluas permukaan besar,\natau kelembapan rendah.";
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

        // Right Page Header (positioned near the top of the page)
        GameObject rightTextObj = new GameObject("RightPageText");
        rightTextObj.transform.SetParent(notebook.transform, false);
        rightTextObj.transform.localPosition = new Vector3(0.08f, 0.01f, 0.14f); // Shifted to top
        rightTextObj.transform.localEulerAngles = new Vector3(90f, 0f, 0f);
        rightTextObj.transform.localScale = new Vector3(0.0022f, 0.0022f, 0.0022f);
        TextMesh rightTM = rightTextObj.AddComponent<TextMesh>();
        rightTM.text = "PEMERHATIAN & GRAF";
        rightTM.fontSize = 28;
        rightTM.fontStyle = FontStyle.Bold;
        rightTM.anchor = TextAnchor.MiddleCenter;
        rightTM.alignment = TextAlignment.Center;
        rightTM.color = Color.black;
        rightTM.font = font;
        if (font != null) rightTextObj.GetComponent<MeshRenderer>().sharedMaterial = font.material;

        // Uniform Scale Graph Container (sits flat on the right page)
        GameObject graphContainer = new GameObject("GraphContainer");
        graphContainer.transform.SetParent(notebook.transform, false);
        graphContainer.transform.localPosition = new Vector3(0.08f, 0.006f, 0f);
        graphContainer.transform.localRotation = Quaternion.Euler(0f, 0f, 2f); // slants with page
        graphContainer.transform.localScale = Vector3.one; // uniform 1:1 scale
        graphContainer.AddComponent<EvaporationGraph3D>();

        // Pen lying on book
        Material penBodyMat = GetOrCreateMaterial("PenBodyMat_Lab4", new Color(0.1f, 0.2f, 0.6f, 1f), false);
        Material penTipMat = GetOrCreateMaterial("PenTipMat_Lab4", new Color(0.2f, 0.2f, 0.2f, 1f), false);
        
        GameObject pen = new GameObject("Pen");
        pen.transform.SetParent(notebook.transform, false);
        pen.transform.localPosition = new Vector3(0.06f, 0.012f, -0.12f);
        pen.transform.localEulerAngles = new Vector3(0f, 25f, 90f);

        GameObject penBody = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        penBody.name = "PenBody";
        penBody.transform.SetParent(pen.transform, false);
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

        BoxCollider noteCol = notebook.AddComponent<BoxCollider>();
        noteCol.center = new Vector3(0f, 0.01f, 0f);
        noteCol.size = new Vector3(0.35f, 0.02f, 0.45f);

        InteractiveNotebookLab4 notebookScript = notebook.AddComponent<InteractiveNotebookLab4>();
        notebookScript.leftPageTextMesh = leftTM;
        notebookScript.rightPageTextMesh = rightTM;
        return notebookScript;
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
            vp.name = "Lab4_VolumeProfile";
            AssetDatabase.CreateAsset(vp, "Assets/Materials/Lab4_VolumeProfile.asset");

            // Bloom for cyber glow effects
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

    private static void CreateBeakerOutline(GameObject beaker)
    {
        // Top Outline
        GameObject outlineTop = new GameObject("Outline_Top");
        outlineTop.transform.SetParent(beaker.transform, false);
        
        LineRenderer lrTop = outlineTop.AddComponent<LineRenderer>();
        lrTop.useWorldSpace = false;
        lrTop.loop = true;
        lrTop.startWidth = 0.02f;
        lrTop.endWidth = 0.02f;
        
        // Find URP/Unlit shader as fallback if Sprites/Default is missing
        Shader shader = Shader.Find("Sprites/Default");
        if (shader == null) shader = Shader.Find("Universal Render Pipeline/Unlit");
        Material outlineMat = new Material(shader);
        
        lrTop.sharedMaterial = outlineMat;
        lrTop.startColor = new Color(0.1f, 0.1f, 0.1f, 0.8f);
        lrTop.endColor = new Color(0.1f, 0.1f, 0.1f, 0.8f);

        int segments = 32;
        lrTop.positionCount = segments;
        for (int i = 0; i < segments; i++)
        {
            float angle = (i * Mathf.PI * 2f) / segments;
            lrTop.SetPosition(i, new Vector3(Mathf.Cos(angle) * 0.5f, 1.0f, Mathf.Sin(angle) * 0.5f));
        }

        // Bottom Outline
        GameObject outlineBottom = new GameObject("Outline_Bottom");
        outlineBottom.transform.SetParent(beaker.transform, false);
        
        LineRenderer lrBottom = outlineBottom.AddComponent<LineRenderer>();
        lrBottom.useWorldSpace = false;
        lrBottom.loop = true;
        lrBottom.startWidth = 0.02f;
        lrBottom.endWidth = 0.02f;
        lrBottom.sharedMaterial = outlineMat;
        lrBottom.startColor = new Color(0.1f, 0.1f, 0.1f, 0.8f);
        lrBottom.endColor = new Color(0.1f, 0.1f, 0.1f, 0.8f);

        lrBottom.positionCount = segments;
        for (int i = 0; i < segments; i++)
        {
            float angle = (i * Mathf.PI * 2f) / segments;
            lrBottom.SetPosition(i, new Vector3(Mathf.Cos(angle) * 0.5f, -1.0f, Mathf.Sin(angle) * 0.5f));
        }
    }

    private static WirePulseAnimator CreatePowerCable(string name, Vector3 start, Vector3 end, float tableY, Transform parent)
    {
        GameObject cableObj = GameObject.Find(name);
        if (cableObj == null) cableObj = new GameObject(name);
        if (parent != null) cableObj.transform.SetParent(parent);

        LineRenderer lr = cableObj.GetComponent<LineRenderer>();
        if (lr == null) lr = cableObj.AddComponent<LineRenderer>();
        
        lr.useWorldSpace = true;
        lr.startWidth = 0.012f;
        lr.endWidth = 0.012f;
        
        Material cableMat = new Material(Shader.Find("Sprites/Default"));
        if (cableMat.shader == null)
        {
            cableMat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        }
        lr.sharedMaterial = cableMat;

        Color baseColor = new Color(0.15f, 0.15f, 0.15f, 1f);
        lr.startColor = baseColor;
        lr.endColor = baseColor;

        // Set static gradient first to prevent white wires
        Gradient staticGrad = new Gradient();
        staticGrad.SetKeys(
            new GradientColorKey[] { new GradientColorKey(baseColor, 0f), new GradientColorKey(baseColor, 1f) },
            new GradientAlphaKey[] { new GradientAlphaKey(baseColor.a, 0f), new GradientAlphaKey(baseColor.a, 1f) }
        );
        lr.colorGradient = staticGrad;

        // Compute Bezier points
        float dist = Vector3.Distance(start, end);
        float sag = dist * 0.4f; // sag depth
        Vector3 controlPoint = Vector3.Lerp(start, end, 0.5f);
        controlPoint.y -= sag;

        int segments = 25;
        lr.positionCount = segments;
        for (int i = 0; i < segments; i++)
        {
            float t = (float)i / (segments - 1);
            // Quadratic Bezier
            Vector3 pos = Mathf.Pow(1f - t, 2f) * start + 
                          2f * (1f - t) * t * controlPoint + 
                          Mathf.Pow(t, 2f) * end;

            // Clamp to table top height to prevent clipping/z-fighting
            if (pos.y < tableY + 0.005f)
            {
                pos.y = tableY + 0.005f;
            }

            lr.SetPosition(i, pos);
        }

        WirePulseAnimator animator = cableObj.GetComponent<WirePulseAnimator>();
        if (animator == null) animator = cableObj.AddComponent<WirePulseAnimator>();

        animator.baseColor = baseColor;
        animator.pulseColor = new Color(0f, 0.8f, 1f, 1f);
        animator.speed = 2.0f;
        animator.pulseWidth = 0.25f;
        animator.SetActive(false);

        return animator;
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

    private static Material GetOrCreateMaterial(string name, Color color, bool isTransparent)
    {
        string path = "Assets/Materials/" + name + ".mat";
        
        if (!AssetDatabase.IsValidFolder("Assets/Materials"))
        {
            AssetDatabase.CreateFolder("Assets", "Materials");
        }

        Shader shader = null;
        if (name.Contains("Glass") || name.Contains("Water") || name.Contains("Line") || name.Contains("Grill") || name.Contains("Condensation"))
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
                    mat.SetInt("_ZWrite", 0);
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
                    mat.SetInt("_ZWrite", 0);
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
                mat.SetInt("_ZWrite", 0);
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
                mat.SetInt("_ZWrite", 0);
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

    private static void SetupInteractiveUIs(InteractiveNotebookLab4 notebookScript)
    {
        GameObject canvasObj = GameObject.Find("Canvas");
        if (canvasObj == null) return;
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        // --- Notebook UI World Space Canvas (Placed on Left Page) ---
        GameObject notebookObj = notebookScript.gameObject;
        
        GameObject nbPanel = new GameObject("Lab4_NotebookPanel");
        nbPanel.transform.SetParent(notebookObj.transform, false);
        nbPanel.transform.localPosition = new Vector3(-0.08f, 0.015f, -0.12f); // Raised from 0.007f to 0.015f to prevent clipping
        nbPanel.transform.localRotation = Quaternion.Euler(90f, 0f, -2f); // slants with LeftPage
        nbPanel.transform.localScale = new Vector3(0.0003f, 0.0003f, 0.0003f); // scale pixels to meters

        Canvas nbCanvas = nbPanel.AddComponent<Canvas>();
        nbCanvas.renderMode = RenderMode.WorldSpace;
        nbCanvas.worldCamera = Camera.main;
        nbPanel.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        RectTransform nbRect = nbPanel.GetComponent<RectTransform>();
        nbRect.sizeDelta = new Vector2(450f, 100f);

        // Submit Button (Log Pemerhatian)
        GameObject submitBtnObj = new GameObject("SubmitBtn");
        submitBtnObj.transform.SetParent(nbPanel.transform, false);
        RectTransform submitRect = submitBtnObj.AddComponent<RectTransform>();
        submitRect.sizeDelta = new Vector2(200f, 70f);
        submitRect.anchoredPosition = new Vector2(-110f, 0f);
        
        UnityEngine.UI.Image submitImg = submitBtnObj.AddComponent<UnityEngine.UI.Image>();
        submitImg.color = new Color(0.12f, 0.6f, 0.3f, 1.0f); // Forest Green
        
        UnityEngine.UI.Button submitBtn = submitBtnObj.AddComponent<UnityEngine.UI.Button>();
        notebookScript.submitButton = submitBtn;

        GameObject submitTxtObj = new GameObject("Text");
        submitTxtObj.transform.SetParent(submitBtnObj.transform, false);
        RectTransform submitTxtRect = submitTxtObj.AddComponent<RectTransform>();
        submitTxtRect.sizeDelta = new Vector2(200f, 70f);
        
        TMPro.TextMeshProUGUI submitTxt = submitTxtObj.AddComponent<TMPro.TextMeshProUGUI>();
        submitTxt.text = "Log Data";
        submitTxt.fontSize = 28;
        submitTxt.fontStyle = TMPro.FontStyles.Bold;
        submitTxt.color = Color.white;
        submitTxt.alignment = TMPro.TextAlignmentOptions.Center;

        // Close Button (Kembali)
        GameObject closeBtnObj = new GameObject("CloseBtn");
        closeBtnObj.transform.SetParent(nbPanel.transform, false);
        RectTransform closeRect = closeBtnObj.AddComponent<RectTransform>();
        closeRect.sizeDelta = new Vector2(200f, 70f);
        closeRect.anchoredPosition = new Vector2(110f, 0f);
        
        UnityEngine.UI.Image closeImg = closeBtnObj.AddComponent<UnityEngine.UI.Image>();
        closeImg.color = new Color(0.75f, 0.2f, 0.2f, 1.0f); // Slate Red
        
        UnityEngine.UI.Button closeBtn = closeBtnObj.AddComponent<UnityEngine.UI.Button>();
        notebookScript.closeButton = closeBtn;

        GameObject closeTxtObj = new GameObject("Text");
        closeTxtObj.transform.SetParent(closeBtnObj.transform, false);
        RectTransform closeTxtRect = closeTxtObj.AddComponent<RectTransform>();
        closeTxtRect.sizeDelta = new Vector2(200f, 70f);
        
        TMPro.TextMeshProUGUI closeTxt = closeTxtObj.AddComponent<TMPro.TextMeshProUGUI>();
        closeTxt.text = "Kembali";
        closeTxt.fontSize = 28;
        closeTxt.fontStyle = TMPro.FontStyles.Bold;
        closeTxt.color = Color.white;
        closeTxt.alignment = TMPro.TextAlignmentOptions.Center;

        notebookScript.feedbackText = null; // No screen space feedback text needed
        notebookScript.uiPanel = nbPanel;
        nbPanel.SetActive(false);

        // --- Quiz UI Panel ---
        GameObject quizManagerObj = new GameObject("QuizManagerLab4");
        QuizManagerLab4 quizManager = quizManagerObj.AddComponent<QuizManagerLab4>();
        notebookScript.quizManager = quizManager;

        GameObject quizPanel = new GameObject("Lab4_QuizPanel");
        quizPanel.transform.SetParent(canvasObj.transform, false);
        RectTransform qRect = quizPanel.AddComponent<RectTransform>();
        qRect.anchorMin = new Vector2(0.5f, 0.5f);
        qRect.anchorMax = new Vector2(0.5f, 0.5f);
        qRect.sizeDelta = new Vector2(600f, 450f);
        UnityEngine.UI.Image qImg = quizPanel.AddComponent<UnityEngine.UI.Image>();
        qImg.color = new Color(0.1f, 0.2f, 0.3f, 0.95f);
        quizManager.quizPanel = quizPanel;

        GameObject qTextObj = new GameObject("QuizQuestion");
        qTextObj.transform.SetParent(quizPanel.transform, false);
        RectTransform qTextRect = qTextObj.AddComponent<RectTransform>();
        qTextRect.sizeDelta = new Vector2(560f, 100f);
        qTextRect.anchoredPosition = new Vector2(0, 120);
        TMPro.TextMeshProUGUI qText = qTextObj.AddComponent<TMPro.TextMeshProUGUI>();
        qText.text = "Faktor manakah yang TIDAK meningkatkan kadar penyejatan air?";
        qText.fontSize = 24;
        qText.color = Color.white;
        qText.alignment = TMPro.TextAlignmentOptions.Center;

        GameObject feedbackObj = new GameObject("FeedbackText");
        feedbackObj.transform.SetParent(quizPanel.transform, false);
        RectTransform fTextRect = feedbackObj.AddComponent<RectTransform>();
        fTextRect.sizeDelta = new Vector2(560f, 75f);
        fTextRect.anchoredPosition = new Vector2(0, -165);
        TMPro.TextMeshProUGUI fText = feedbackObj.AddComponent<TMPro.TextMeshProUGUI>();
        fText.fontSize = 18;
        fText.alignment = TMPro.TextAlignmentOptions.Center;
        quizManager.feedbackText = fText;

        // Answers
        string[] answers = { "Suhu Tinggi", "Luas Permukaan Besar", "Kelajuan Angin Tinggi", "Kelembapan Tinggi" };
        bool[] isCorrect = { false, false, false, true };
        for(int i=0; i<4; i++) {
            GameObject ansBtnObj = new GameObject("Ans" + i);
            ansBtnObj.transform.SetParent(quizPanel.transform, false);
            RectTransform ansRect = ansBtnObj.AddComponent<RectTransform>();
            ansRect.sizeDelta = new Vector2(400f, 40f);
            ansRect.anchoredPosition = new Vector2(0, 50 - (i * 50));
            UnityEngine.UI.Image ansImg = ansBtnObj.AddComponent<UnityEngine.UI.Image>();
            ansImg.color = new Color(0.2f, 0.3f, 0.4f);
            UnityEngine.UI.Button ansBtn = ansBtnObj.AddComponent<UnityEngine.UI.Button>();
            
            int index = i;
            ansBtn.onClick.AddListener(() => { quizManager.CheckAnswer(isCorrect[index]); });

            GameObject ansTxtObj = new GameObject("Text");
            ansTxtObj.transform.SetParent(ansBtnObj.transform, false);
            RectTransform ansTxtRect = ansTxtObj.AddComponent<RectTransform>();
            ansTxtRect.sizeDelta = new Vector2(400f, 40f);
            TMPro.TextMeshProUGUI ansTxt = ansTxtObj.AddComponent<TMPro.TextMeshProUGUI>();
            ansTxt.text = answers[i];
            ansTxt.fontSize = 20;
            ansTxt.color = Color.white;
            ansTxt.alignment = TMPro.TextAlignmentOptions.Center;
        }

        quizPanel.SetActive(false);

        // --- Summary Panel ---
        GameObject sumPanel = new GameObject("Lab4_SummaryPanel");
        sumPanel.transform.SetParent(canvasObj.transform, false);
        RectTransform sumRect = sumPanel.AddComponent<RectTransform>();
        sumRect.anchorMin = new Vector2(0.5f, 0.5f);
        sumRect.anchorMax = new Vector2(0.5f, 0.5f);
        sumRect.sizeDelta = new Vector2(500f, 300f);
        UnityEngine.UI.Image sumImg = sumPanel.AddComponent<UnityEngine.UI.Image>();
        sumImg.color = new Color(0.95f, 0.95f, 0.95f, 1f);
        quizManager.summaryPanel = sumPanel;

        GameObject sumTextObj = new GameObject("SummaryText");
        sumTextObj.transform.SetParent(sumPanel.transform, false);
        RectTransform sumTextRect = sumTextObj.AddComponent<RectTransform>();
        sumTextRect.sizeDelta = new Vector2(460f, 200f);
        sumTextRect.anchoredPosition = new Vector2(0, 20);
        UnityEngine.UI.Text sumText = sumTextObj.AddComponent<UnityEngine.UI.Text>();
        sumText.font = font;
        sumText.fontSize = 22;
        sumText.color = Color.black;
        sumText.alignment = TextAnchor.MiddleCenter;
        sumText.text = "TAHNIAH!\nEksperimen Penyejatan Air Selesai.\n\nAnda telah berjaya memahami\nfaktor-faktor yang mempengaruhi\nkadar penyejatan air.";

        GameObject sumBtnObj = new GameObject("CloseBtn");
        sumBtnObj.transform.SetParent(sumPanel.transform, false);
        RectTransform sumBtnRect = sumBtnObj.AddComponent<RectTransform>();
        sumBtnRect.sizeDelta = new Vector2(150f, 40f);
        sumBtnRect.anchoredPosition = new Vector2(0, -100);
        UnityEngine.UI.Image sumBtnImg = sumBtnObj.AddComponent<UnityEngine.UI.Image>();
        sumBtnImg.color = new Color(0.8f, 0.2f, 0.2f);
        UnityEngine.UI.Button sumBtn = sumBtnObj.AddComponent<UnityEngine.UI.Button>();
        sumBtn.onClick.AddListener(() => { sumPanel.SetActive(false); });

        GameObject sumBtnTxtObj = new GameObject("Text");
        sumBtnTxtObj.transform.SetParent(sumBtnObj.transform, false);
        RectTransform sumBtnTxtRect = sumBtnTxtObj.AddComponent<RectTransform>();
        sumBtnTxtRect.sizeDelta = new Vector2(150f, 40f);
        UnityEngine.UI.Text sumBtnTxt = sumBtnTxtObj.AddComponent<UnityEngine.UI.Text>();
        sumBtnTxt.font = font;
        sumBtnTxt.fontSize = 18;
        sumBtnTxt.color = Color.white;
        sumBtnTxt.alignment = TextAnchor.MiddleCenter;
        sumBtnTxt.text = "Tutup";

        sumPanel.SetActive(false);
    }

    private static Lab4TutorialManager SetupTutorialOverlay(Vector3 experimentOffset)
    {
        GameObject tutorialManagerObj = new GameObject("Lab4TutorialManager");
        Lab4TutorialManager tutManager = tutorialManagerObj.AddComponent<Lab4TutorialManager>();

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        Material fontMat = font != null ? font.material : null;

        // Step 1: Evaporate
        GameObject step1 = new GameObject("Tut_Step1");
        step1.transform.position = experimentOffset + new Vector3(-0.749f, 0.2615f, -0.68f);
        step1.transform.localScale = new Vector3(0.3901f, 0.3901f, 0.3901f);
        TextMesh tm1 = step1.AddComponent<TextMesh>();
        tm1.text = "1. Hidupkan kipas & pemanas\nuntuk sejatkan air";
        tm1.characterSize = 0.02f;
        tm1.fontSize = 64;
        tm1.color = new Color(0.1f, 0.15f, 0.3f); // Dark navy blue for better contrast
        if (font != null) { tm1.font = font; step1.GetComponent<MeshRenderer>().sharedMaterial = fontMat; }
        step1.AddComponent<LookAtCamera3D>();

        // Step 2: Read Book
        GameObject step2 = new GameObject("Tut_Step2");
        step2.transform.position = experimentOffset + new Vector3(0.9558f, 0.115f, 0.739f);
        step2.transform.localScale = new Vector3(0.6182f, 0.6182f, 0.6182f);
        TextMesh tm2 = step2.AddComponent<TextMesh>();
        tm2.text = "2. Klik Buku Nota untuk\nmerekod data & Kuiz";
        tm2.characterSize = 0.02f;
        tm2.fontSize = 64;
        tm2.color = new Color(0.1f, 0.15f, 0.3f);
        if (font != null) { tm2.font = font; step2.GetComponent<MeshRenderer>().sharedMaterial = fontMat; }
        step2.AddComponent<LookAtCamera3D>();

        tutManager.step1_Evaporate = step1;
        tutManager.step2_ReadBook = step2;

        return tutManager;
    }

    private static Thermometer3D CreateThermometer(Transform parent, Vector3 localPos, Quaternion localRot, Material glassMat, Material redLiquidMat)
    {
        GameObject thermometerObj = new GameObject("Thermometer_3D");
        thermometerObj.transform.SetParent(parent, false);
        thermometerObj.transform.localPosition = localPos;
        thermometerObj.transform.localRotation = localRot;

        // Glass Tube
        GameObject glassTube = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        glassTube.name = "GlassTube";
        glassTube.transform.SetParent(thermometerObj.transform, false);
        glassTube.transform.localPosition = Vector3.zero;
        glassTube.transform.localScale = new Vector3(0.015f, 0.18f, 0.015f); // diameter 0.015m, height 0.36m
        glassTube.GetComponent<MeshRenderer>().sharedMaterial = glassMat;
        DestroyImmediate(glassTube.GetComponent<Collider>());

        // Backing Scale
        GameObject backingScale = GameObject.CreatePrimitive(PrimitiveType.Cube);
        backingScale.name = "BackingScale";
        backingScale.transform.SetParent(thermometerObj.transform, false);
        backingScale.transform.localPosition = new Vector3(0f, 0f, 0.003f); // shifted slightly back
        backingScale.transform.localScale = new Vector3(0.008f, 0.34f, 0.002f);
        Material backingMat = GetOrCreateMaterial("ThermometerBackingMat_Lab4", new Color(0.95f, 0.95f, 0.95f, 1f), false);
        backingScale.GetComponent<MeshRenderer>().sharedMaterial = backingMat;
        DestroyImmediate(backingScale.GetComponent<Collider>());

        // Tick Marks along the scale
        Material tickMat = GetOrCreateMaterial("TickMat_Lab4", Color.black, false);
        for (int i = 0; i <= 10; i++)
        {
            GameObject tick = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tick.name = "Tick_" + i;
            tick.transform.SetParent(backingScale.transform, false);
            
            float localY = -0.4f + (i * 0.8f / 10f); // ranges from -0.4 to 0.4 locally
            tick.transform.localPosition = new Vector3(0f, localY, -0.51f); // slightly in front
            
            bool isMajor = (i % 5 == 0);
            float tickWidth = isMajor ? 0.6f : 0.3f;
            tick.transform.localScale = new Vector3(tickWidth, 0.015f, 0.1f);
            tick.GetComponent<MeshRenderer>().sharedMaterial = tickMat;
            DestroyImmediate(tick.GetComponent<Collider>());
        }

        // Labels
        Font thermoFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        
        GameObject label0 = new GameObject("Label_0");
        label0.transform.SetParent(backingScale.transform, false);
        label0.transform.localPosition = new Vector3(0.5f, -0.4f, -0.6f);
        label0.transform.localScale = new Vector3(0.04f, 0.04f, 0.04f);
        TextMesh tm0 = label0.AddComponent<TextMesh>();
        tm0.text = "0";
        tm0.fontSize = 24;
        tm0.color = Color.black;
        tm0.font = thermoFont;
        tm0.anchor = TextAnchor.MiddleLeft;
        if (thermoFont != null) label0.GetComponent<MeshRenderer>().sharedMaterial = thermoFont.material;

        GameObject label100 = new GameObject("Label_100");
        label100.transform.SetParent(backingScale.transform, false);
        label100.transform.localPosition = new Vector3(0.5f, 0.4f, -0.6f);
        label100.transform.localScale = new Vector3(0.04f, 0.04f, 0.04f);
        TextMesh tm100 = label100.AddComponent<TextMesh>();
        tm100.text = "100";
        tm100.fontSize = 24;
        tm100.color = Color.black;
        tm100.font = thermoFont;
        tm100.anchor = TextAnchor.MiddleLeft;
        if (thermoFont != null) label100.GetComponent<MeshRenderer>().sharedMaterial = thermoFont.material;

        // Bulb
        GameObject redBulb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        redBulb.name = "RedBulb";
        redBulb.transform.SetParent(thermometerObj.transform, false);
        redBulb.transform.localPosition = new Vector3(0f, -0.17f, 0f);
        redBulb.transform.localScale = new Vector3(0.022f, 0.022f, 0.022f); // slightly wider bulb
        redBulb.GetComponent<MeshRenderer>().sharedMaterial = redLiquidMat;
        DestroyImmediate(redBulb.GetComponent<Collider>());

        // Liquid Column
        GameObject liquidPivot = new GameObject("RedLiquidPivot");
        liquidPivot.transform.SetParent(thermometerObj.transform, false);
        liquidPivot.transform.localPosition = new Vector3(0f, -0.17f, 0f); // Pivot at the bulb level
        liquidPivot.transform.localRotation = Quaternion.identity;
        liquidPivot.transform.localScale = new Vector3(1f, 0.05f, 1f); // Start at minimum

        GameObject liquidColumn = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        liquidColumn.name = "RedLiquidColumn";
        liquidColumn.transform.SetParent(liquidPivot.transform, false);
        liquidColumn.transform.localPosition = new Vector3(0f, 0.5f, 0f);
        liquidColumn.transform.localScale = new Vector3(0.006f, 0.5f, 0.006f);
        liquidColumn.GetComponent<MeshRenderer>().sharedMaterial = redLiquidMat;
        DestroyImmediate(liquidColumn.GetComponent<Collider>());

        Thermometer3D script = thermometerObj.AddComponent<Thermometer3D>();
        script.liquidPivot = liquidPivot.transform;
        script.minScaleY = 0.02f;
        script.maxScaleY = 0.33f;
        script.responseSpeed = 0.5f;

        return script;
    }

    private static GameObject CreateContainerGroup(string groupName, Transform parent, Vector3 localPos, Vector3 containerScale, Vector3 waterScale, Vector3 waterVolScale, Vector3 waterVolLocalPos, Vector3 thermoLocalPos, Quaternion thermoLocalRot, Material glassMat, Material waterMat, Material condensationMat, Material redLiquidMat, out BeakerCondensation3D condScript, out Thermometer3D thermoScript)
    {
        GameObject group = new GameObject(groupName);
        group.transform.SetParent(parent, false);
        group.transform.localPosition = localPos;

        // Container Shell
        GameObject model = new GameObject(groupName + "_Model");
        model.transform.SetParent(group.transform, false);
        model.transform.localPosition = new Vector3(0f, 0.093f + containerScale.y, 0f); // Sits on plate (hot plate surface Y = 0.093f)
        model.transform.localScale = containerScale;
        model.AddComponent<MeshFilter>().sharedMesh = GenerateOpenBeakerMesh();
        model.AddComponent<MeshRenderer>().sharedMaterial = glassMat;
        CreateBeakerOutline(model);

        // Add tick marks to graduated cylinder to make it look realistic
        if (groupName.Contains("Cylinder"))
        {
            Material tickMat = GetOrCreateMaterial("TickMat_Lab4", Color.black, false);
            for (int i = 1; i <= 9; i++)
            {
                GameObject tick = GameObject.CreatePrimitive(PrimitiveType.Cube);
                tick.name = "GradTick_" + i;
                tick.transform.SetParent(model.transform, false);
                float ly = -0.8f + (i * 1.6f / 10f);
                tick.transform.localPosition = new Vector3(-0.48f, ly, -0.15f);
                tick.transform.localScale = new Vector3(0.06f, 0.015f, 0.1f);
                tick.GetComponent<MeshRenderer>().sharedMaterial = tickMat;
                DestroyImmediate(tick.GetComponent<Collider>());
            }
        }

        // Condensation
        GameObject condensationObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        condensationObj.name = "BeakerCondensation";
        condensationObj.transform.SetParent(model.transform, false);
        condensationObj.transform.localPosition = Vector3.zero;
        condensationObj.transform.localScale = new Vector3(0.98f, 0.98f, 0.98f);
        condensationObj.GetComponent<MeshRenderer>().sharedMaterial = condensationMat;
        DestroyImmediate(condensationObj.GetComponent<Collider>());

        condScript = condensationObj.AddComponent<BeakerCondensation3D>();
        condScript.condensationRenderer = condensationObj.GetComponent<Renderer>();
        condScript.beakerTransform = model.transform;

        // Thermometer
        thermoScript = CreateThermometer(group.transform, thermoLocalPos, thermoLocalRot, glassMat, redLiquidMat);

        // Water Pivot
        GameObject waterPivot = new GameObject("Water_Pivot");
        waterPivot.transform.SetParent(group.transform, false);
        waterPivot.transform.localPosition = new Vector3(0f, 0.097f, 0f);
        waterPivot.transform.localScale = waterScale;

        GameObject waterVolume = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        waterVolume.name = "WaterVolume";
        waterVolume.transform.SetParent(waterPivot.transform, false);
        waterVolume.transform.localPosition = waterVolLocalPos;
        waterVolume.transform.localScale = waterVolScale;
        waterVolume.GetComponent<MeshRenderer>().sharedMaterial = waterMat;
        DestroyImmediate(waterVolume.GetComponent<Collider>());

        return group;
    }
}
#endif
