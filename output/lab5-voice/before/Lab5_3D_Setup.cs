#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class Lab5_3D_Setup : EditorWindow
{
    [MenuItem("E-Lab Tools/Generate 3D Lab 5 (Kadar Keterlarutan)")]
    public static void ShowWindow()
    {
        GenerateLab5_3D();
    }

    private static void GenerateLab5_3D()
    {
        if (EditorSceneManager.GetActiveScene().name != "Lab5_3D")
        {
            EditorSceneManager.OpenScene("Assets/Scenes/Lab5_3D.unity", OpenSceneMode.Single);
        }

        DestroyDuplicateLights();

        // 1. Copy Environment
        Vector3 experimentOffset = Vector3.zero;
        GameObject env = CopyEnvironmentFromLab1();
        if (env != null)
        {
            string[] lab1Objects = { "Beaker_1", "Beaker_2", "Liquid_1", "Liquid_2", "BurnerBase", "BurnerTube" };
            foreach (string objName in lab1Objects)
            {
                Transform oldObj = env.transform.Find(objName);
                if (oldObj != null) DestroyImmediate(oldObj.gameObject);
            }

            GameObject titleObj = GameObject.Find("Whiteboard_Title");
            if (titleObj != null)
            {
                var tmps = titleObj.GetComponentsInChildren<TMPro.TextMeshPro>(true);
                foreach (var t in tmps) 
                {
                    t.text = "AMALI 5 KADAR KETERLARUTAN";
                    t.color = Color.white;
                }
                
                var tmpUIs = titleObj.GetComponentsInChildren<TMPro.TextMeshProUGUI>(true);
                foreach (var t in tmpUIs) 
                {
                    t.text = "AMALI 5 KADAR KETERLARUTAN";
                    t.color = Color.white;
                }
            }

            MeshRenderer[] envRenderers = env.GetComponentsInChildren<MeshRenderer>(true);
            foreach (MeshRenderer r in envRenderers)
            {
                string rName = r.gameObject.name.ToLower();
                if (rName.Contains("whiteboard") || rName.Contains("board"))
                {
                    if (r.sharedMaterial != null)
                    {
                        Material darkMat = new Material(r.sharedMaterial);
                        darkMat.color = new Color(0.25f, 0.28f, 0.32f, 1f);
                        r.material = darkMat;
                    }
                }
            }

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
                        if (woodMat != null) r.sharedMaterial = woodMat;
                    }
                    else if (objName.Contains("desk") || objName.Contains("table") || matName.Contains("epoxy") || matName.Contains("wood"))
                    {
                        bool isTop = objName.Contains("top") || objName.Contains("surface") || matName.Contains("epoxy") || objName == "desk_1";
                        if (isTop && deskMat != null) r.sharedMaterial = deskMat;
                        else if (woodMat != null) r.sharedMaterial = woodMat;
                    }
                }
            }

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

            if (Camera.main != null)
            {
                Camera.main.transform.position = experimentOffset + new Vector3(0, 0.95f, -1.8f);
                Camera.main.transform.rotation = Quaternion.Euler(22f, 0f, 0f);
                Camera.main.nearClipPlane = 0.05f;
                if (Camera.main.gameObject.GetComponent<AudioListener>() == null)
                {
                    Camera.main.gameObject.AddComponent<AudioListener>();
                }
            }
        }

        // 1.5 Rescue the Left UI panel from the tablet before destroying the tablet, or restore it if missing
        GameObject existingLeftPanel = null;
        Transform[] allTransforms = Object.FindObjectsByType<Transform>(FindObjectsInactive.Include);
        foreach (Transform t in allTransforms)
        {
            if (t.name == "Left" && (t.parent == null || t.parent.name == "TabletCanvas" || t.parent.name == "bg"))
            {
                existingLeftPanel = t.gameObject;
                break;
            }
        }

        if (existingLeftPanel != null)
        {
            GameObject canvasForRescue = GameObject.Find("Canvas");
            if (canvasForRescue != null)
            {
                Transform bgTrans = canvasForRescue.transform.Find("bg");
                if (bgTrans != null)
                {
                    existingLeftPanel.transform.SetParent(bgTrans, false);
                    existingLeftPanel.SetActive(true);
                }
            }
        }
        else
        {
            // If it is completely missing (destroyed on a previous run), attempt to restore it additively from the 2D scene
            Debug.Log("Left panel not found, attempting to restore from original Lab5 scene...");
            string originalScenePath = "Assets/Scenes/Lab5_0.unity";
            if (!System.IO.File.Exists(originalScenePath)) originalScenePath = "Assets/Scenes/Lab5.unity";
            
            if (System.IO.File.Exists(originalScenePath))
            {
                var lab5Scene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene(originalScenePath, UnityEditor.SceneManagement.OpenSceneMode.Additive);
                GameObject originalLeft = null;
                foreach (var root in lab5Scene.GetRootGameObjects())
                {
                    if (root.name == "Canvas")
                    {
                        Transform bg = root.transform.Find("bg");
                        if (bg != null)
                        {
                            Transform left = bg.Find("Left");
                            if (left != null)
                            {
                                originalLeft = left.gameObject;
                                break;
                            }
                        }
                    }
                }
                
                if (originalLeft != null)
                {
                    GameObject canvasForRescue = GameObject.Find("Canvas");
                    if (canvasForRescue != null)
                    {
                        Transform bgTrans = canvasForRescue.transform.Find("bg");
                        if (bgTrans != null)
                        {
                            existingLeftPanel = UnityEngine.Object.Instantiate(originalLeft);
                            existingLeftPanel.name = "Left";
                            existingLeftPanel.transform.SetParent(bgTrans, false);
                            existingLeftPanel.SetActive(true);
                        }
                    }
                }
                UnityEditor.SceneManagement.EditorSceneManager.CloseScene(lab5Scene, true);
            }
        }

        // 2. Clean up previous setups
        DestroyIfExists("SolubilityManager_3D");
        DestroyIfExists("Beaker_3D");
        DestroyIfExists("Pemanas_3D");
        DestroyIfExists("Lab5_3DTablet");
        DestroyIfExists("RotateCameraButton");
        DestroyIfExists("Lab5_GridMat");
        DestroyIfExists("HologramDataBoard");
        DestroyIfExists("Lab5_Notebook");
        DestroyIfExists("Lab5_NotebookPanel");
        DestroyIfExists("Lab5_QuizPanel");
        DestroyIfExists("Lab5_SummaryPanel");
        DestroyIfExists("QuizManagerLab5");
        DestroyIfExists("TableSocketBox");
        DestroyIfExists("HeaterCable");
        DestroyIfExists("Thermometer_3D");
        DestroyIfExists("Lab5_TutorialManager");
        DestroyIfExists("Global_PostProcessing");

        // 3. Materials
        Material glassMat = GetOrCreateMaterial("GlassMat_Lab5", new Color(0.9f, 0.93f, 0.96f, 0.24f), true);
        Material waterMat = GetOrCreateMaterial("WaterMat_Lab5", new Color(0.12f, 0.58f, 0.85f, 0.82f), true);
        Material metalMat = GetOrCreateMaterial("MetalMat_Lab5", new Color(0.18f, 0.18f, 0.2f, 1f), false);
        if (metalMat.HasProperty("_Metallic")) metalMat.SetFloat("_Metallic", 0.9f);
        if (metalMat.HasProperty("_Smoothness")) metalMat.SetFloat("_Smoothness", 0.8f);

        // 4. Setup 3D Hot Plate (Pemanas)
        GameObject pemanas = new GameObject("Pemanas_3D");
        pemanas.transform.position = experimentOffset;

        // Base Board
        GameObject heaterBase = GameObject.CreatePrimitive(PrimitiveType.Cube);
        heaterBase.name = "HeaterBase";
        heaterBase.transform.SetParent(pemanas.transform, false);
        heaterBase.transform.localPosition = new Vector3(0f, 0.015f, 0f);
        heaterBase.transform.localScale = new Vector3(0.72f, 0.02f, 0.5f);
        Material darkMetal = GetOrCreateMaterial("DarkMetalMat_Lab5", new Color(0.15f, 0.17f, 0.2f, 1f), false);
        if (darkMetal.HasProperty("_Metallic")) darkMetal.SetFloat("_Metallic", 0.6f);
        if (darkMetal.HasProperty("_Smoothness")) darkMetal.SetFloat("_Smoothness", 0.5f);
        heaterBase.GetComponent<MeshRenderer>().sharedMaterial = darkMetal;
        DestroyImmediate(heaterBase.GetComponent<Collider>());

        // 4 Legs
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

        // Rubber Feet
        Material rubberFeetMat = GetOrCreateMaterial("RubberFeetMat_Lab5", new Color(0.08f, 0.08f, 0.08f, 1f), false);
        for (int i = 0; i < 4; i++)
        {
            GameObject chassisFoot = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            chassisFoot.name = "ChassisFoot_" + i;
            chassisFoot.transform.SetParent(pemanas.transform, false);
            float signX = (i % 2 == 0) ? 1f : -1f;
            float signZ = (i / 2 == 0) ? 1f : -1f;
            chassisFoot.transform.localPosition = new Vector3(signX * 0.25f, 0.03f, signZ * 0.21f);
            chassisFoot.transform.localScale = new Vector3(0.03f, 0.005f, 0.03f);
            chassisFoot.GetComponent<MeshRenderer>().sharedMaterial = rubberFeetMat;
            DestroyImmediate(chassisFoot.GetComponent<Collider>());
        }

        // Main Body (Chassis)
        GameObject heaterBody = GameObject.CreatePrimitive(PrimitiveType.Cube);
        heaterBody.name = "HeaterBody";
        heaterBody.transform.SetParent(pemanas.transform, false);
        heaterBody.transform.localPosition = new Vector3(0f, 0.06f, 0f);
        heaterBody.transform.localScale = new Vector3(0.58f, 0.05f, 0.44f);
        Material chassisMat = GetOrCreateMaterial("ChassisMat_Lab5", new Color(0.22f, 0.25f, 0.3f, 1f), false);
        if (chassisMat.HasProperty("_Metallic")) chassisMat.SetFloat("_Metallic", 0.4f);
        if (chassisMat.HasProperty("_Smoothness")) chassisMat.SetFloat("_Smoothness", 0.6f);
        heaterBody.GetComponent<MeshRenderer>().sharedMaterial = chassisMat;
        DestroyImmediate(heaterBody.GetComponent<Collider>());

        // Rim
        GameObject heaterRim = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        heaterRim.name = "HeaterRim";
        heaterRim.transform.SetParent(pemanas.transform, false);
        heaterRim.transform.localPosition = new Vector3(0f, 0.089f, 0f);
        heaterRim.transform.localScale = new Vector3(0.40f, 0.004f, 0.40f);
        Material rimMat = GetOrCreateMaterial("HeaterRimMat_Lab5", new Color(0.85f, 0.87f, 0.9f, 1f), false);
        if (rimMat.HasProperty("_Metallic")) rimMat.SetFloat("_Metallic", 0.9f);
        if (rimMat.HasProperty("_Smoothness")) rimMat.SetFloat("_Smoothness", 0.85f);
        heaterRim.GetComponent<MeshRenderer>().sharedMaterial = rimMat;
        DestroyImmediate(heaterRim.GetComponent<Collider>());

        // Heating Element
        GameObject heaterPlate = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        heaterPlate.name = "HeaterPlate";
        heaterPlate.transform.SetParent(pemanas.transform, false);
        heaterPlate.transform.localPosition = new Vector3(0f, 0.093f, 0f);
        heaterPlate.transform.localScale = new Vector3(0.36f, 0.002f, 0.36f);
        Material heaterPlateMat = GetOrCreateMaterial("HeaterPlateMat_Lab5", new Color(0.18f, 0.18f, 0.18f, 1f), false);
        heaterPlateMat.EnableKeyword("_EMISSION");
        heaterPlateMat.SetColor("_EmissionColor", Color.black);
        EditorUtility.SetDirty(heaterPlateMat);
        heaterPlate.GetComponent<MeshRenderer>().sharedMaterial = heaterPlateMat;
        DestroyImmediate(heaterPlate.GetComponent<Collider>());

        // Knob
        GameObject heaterKnobParent = new GameObject("HeaterKnob");
        heaterKnobParent.transform.SetParent(pemanas.transform, false);
        heaterKnobParent.transform.localPosition = new Vector3(0.18f, 0.06f, -0.222f);

        GameObject heaterKnobModel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        heaterKnobModel.name = "KnobModel";
        heaterKnobModel.transform.SetParent(heaterKnobParent.transform, false);
        heaterKnobModel.transform.localPosition = Vector3.zero;
        heaterKnobModel.transform.localScale = new Vector3(0.06f, 0.0075f, 0.06f);
        heaterKnobModel.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        Material knobMat = GetOrCreateMaterial("KnobMat_Lab5", new Color(0.1f, 0.1f, 0.1f, 1f), false);
        heaterKnobModel.GetComponent<MeshRenderer>().sharedMaterial = knobMat;
        DestroyImmediate(heaterKnobModel.GetComponent<Collider>());

        GameObject knobIndicator = GameObject.CreatePrimitive(PrimitiveType.Cube);
        knobIndicator.name = "KnobIndicator";
        knobIndicator.transform.SetParent(heaterKnobParent.transform, false);
        knobIndicator.transform.localPosition = new Vector3(0f, 0.02f, -0.0085f);
        knobIndicator.transform.localScale = new Vector3(0.008f, 0.02f, 0.008f);
        Material indicatorMat = GetOrCreateMaterial("IndicatorMat_Lab5", new Color(0.9f, 0.9f, 0.9f, 1f), false);
        knobIndicator.GetComponent<MeshRenderer>().sharedMaterial = indicatorMat;
        DestroyImmediate(knobIndicator.GetComponent<Collider>());

        // LED Indicator
        GameObject led = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        led.name = "LED";
        led.transform.SetParent(pemanas.transform, false);
        led.transform.localPosition = new Vector3(-0.18f, 0.06f, -0.222f);
        led.transform.localScale = new Vector3(0.025f, 0.025f, 0.025f);
        Material ledMat = GetOrCreateMaterial("LEDMat_Lab5", new Color(0.2f, 0f, 0f, 1f), false);
        ledMat.EnableKeyword("_EMISSION");
        ledMat.SetColor("_EmissionColor", Color.black);
        led.GetComponent<MeshRenderer>().sharedMaterial = ledMat;
        DestroyImmediate(led.GetComponent<Collider>());

        // 3D Power Switch
        GameObject switchGroup = new GameObject("HeaterPowerSwitch_Group");
        switchGroup.transform.SetParent(pemanas.transform, false);
        switchGroup.transform.localPosition = new Vector3(-0.06f, 0.06f, -0.222f);

        GameObject bezel = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bezel.name = "Bezel";
        bezel.transform.SetParent(switchGroup.transform, false);
        bezel.transform.localPosition = Vector3.zero;
        bezel.transform.localScale = new Vector3(0.045f, 0.045f, 0.01f);
        Material bezelMat = GetOrCreateMaterial("SwitchBezelMat_Lab5", new Color(0.08f, 0.08f, 0.08f, 1f), false);
        bezel.GetComponent<MeshRenderer>().sharedMaterial = bezelMat;
        DestroyImmediate(bezel.GetComponent<Collider>());

        GameObject leverPivotObj = new GameObject("LeverPivot");
        leverPivotObj.transform.SetParent(switchGroup.transform, false);
        leverPivotObj.transform.localPosition = new Vector3(0f, 0f, -0.005f);

        GameObject lever = GameObject.CreatePrimitive(PrimitiveType.Cube);
        lever.name = "Lever";
        lever.transform.SetParent(leverPivotObj.transform, false);
        lever.transform.localPosition = Vector3.zero;
        lever.transform.localScale = new Vector3(0.03f, 0.035f, 0.015f);
        Material leverMat = GetOrCreateMaterial("SwitchLeverMat_Lab5", new Color(0.9f, 0.1f, 0.1f, 1f), false);
        lever.GetComponent<MeshRenderer>().sharedMaterial = leverMat;
        DestroyImmediate(lever.GetComponent<Collider>());

        BoxCollider switchCol = switchGroup.AddComponent<BoxCollider>();
        switchCol.center = Vector3.zero;
        switchCol.size = new Vector3(0.05f, 0.05f, 0.03f);

        // 5. Setup Beaker and Water
        GameObject beaker = new GameObject("Beaker_3D");
        beaker.transform.position = experimentOffset + new Vector3(0f, 0.093f, 0f);
        beaker.transform.localScale = new Vector3(0.35f, 0.25f, 0.35f); 
        beaker.AddComponent<MeshFilter>().sharedMesh = GenerateOpenBeakerMesh();
        beaker.AddComponent<MeshRenderer>().sharedMaterial = glassMat;
        CreateBeakerOutline(beaker);

        GameObject water = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        water.name = "Water_3D";
        water.transform.SetParent(beaker.transform, false);
        water.transform.localPosition = new Vector3(0f, 0.45f, 0f);
        water.transform.localScale = new Vector3(0.92f, 0.45f, 0.92f);
        water.GetComponent<MeshRenderer>().sharedMaterial = waterMat;
        DestroyImmediate(water.GetComponent<Collider>());

        // 6. Setup Bubbles
        GameObject bubbles = new GameObject("BubblesParticle");
        bubbles.transform.SetParent(water.transform, false);
        bubbles.transform.localPosition = new Vector3(0f, -0.45f, 0f);
        bubbles.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
        
        ParticleSystem ps = bubbles.AddComponent<ParticleSystem>();
        var psMain = ps.main;
        psMain.startSize = new ParticleSystem.MinMaxCurve(0.005f, 0.018f);
        psMain.startSpeed = new ParticleSystem.MinMaxCurve(0.08f, 0.16f);
        psMain.startLifetime = new ParticleSystem.MinMaxCurve(0.8f, 1.4f);
        psMain.startColor = new Color(1f, 1f, 1f, 0.35f);
        psMain.simulationSpace = ParticleSystemSimulationSpace.World;
        psMain.scalingMode = ParticleSystemScalingMode.Shape;
        
        var psEm = ps.emission;
        psEm.rateOverTime = 0;
        
        var psShape = ps.shape;
        psShape.shapeType = ParticleSystemShapeType.Cone;
        psShape.radius = 0.12f;
        psShape.angle = 3f;
        
        var psRender = ps.GetComponent<ParticleSystemRenderer>();
        psRender.sharedMaterial = AssetDatabase.GetBuiltinExtraResource<Material>("Default-ParticleSystem.mat");
        psRender.renderMode = ParticleSystemRenderMode.Billboard;

        // 6.5 Setup Steam Particles
        GameObject steamObj = new GameObject("SteamParticle");
        steamObj.transform.SetParent(beaker.transform, false);
        steamObj.transform.localPosition = new Vector3(0f, 0.85f, 0f);
        steamObj.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
        
        ParticleSystem steamPS = steamObj.AddComponent<ParticleSystem>();
        var steamMain = steamPS.main;
        steamMain.startSize = new ParticleSystem.MinMaxCurve(0.04f, 0.08f);
        steamMain.startSpeed = new ParticleSystem.MinMaxCurve(0.12f, 0.25f);
        steamMain.startLifetime = new ParticleSystem.MinMaxCurve(1.5f, 2.5f);
        steamMain.startColor = new Color(0.95f, 0.97f, 1f, 0.25f); // Soft realistic steam
        steamMain.simulationSpace = ParticleSystemSimulationSpace.World;
        steamMain.scalingMode = ParticleSystemScalingMode.Shape;
        
        var steamEm = steamPS.emission;
        steamEm.rateOverTime = 0;
        
        var steamShape = steamPS.shape;
        steamShape.shapeType = ParticleSystemShapeType.Circle;
        steamShape.radius = 0.14f; // Fits cleanly inside beaker opening without spilling out
        
        var steamSize = steamPS.sizeOverLifetime;
        steamSize.enabled = true;
        steamSize.size = new ParticleSystem.MinMaxCurve(1.0f, 2.0f); // Expands naturally as vapor rises
        
        var steamColor = steamPS.colorOverLifetime;
        steamColor.enabled = true;
        Gradient steamGrad = new Gradient();
        steamGrad.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new GradientAlphaKey[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(0.2f, 0.2f), new GradientAlphaKey(0f, 1f) } // Fades in and out smoothly
        );
        steamColor.color = steamGrad;
        
        var steamRender = steamPS.GetComponent<ParticleSystemRenderer>();
        steamRender.sharedMaterial = AssetDatabase.GetBuiltinExtraResource<Material>("Default-ParticleSystem.mat");
        steamRender.renderMode = ParticleSystemRenderMode.Billboard;

        // 6.6 Setup Heat Haze Shimmer above Hot Plate
        GameObject hazeObj = new GameObject("HeatHazeParticle");
        hazeObj.transform.SetParent(pemanas.transform, false);
        hazeObj.transform.localPosition = new Vector3(0f, 0.095f, 0f); // Starts directly at top surface of heater plate
        hazeObj.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);

        ParticleSystem hazePS = hazeObj.AddComponent<ParticleSystem>();
        var hazeMain = hazePS.main;
        hazeMain.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.16f);
        hazeMain.startSpeed = new ParticleSystem.MinMaxCurve(0.2f, 0.4f);
        hazeMain.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 0.9f);
        hazeMain.startColor = new Color(1.0f, 0.65f, 0.35f, 0.02f); // Extremely low alpha warm tint to avoid white smoke appearance
        hazeMain.simulationSpace = ParticleSystemSimulationSpace.World;
        
        var hazeEm = hazePS.emission;
        hazeEm.rateOverTime = 0;
        
        var hazeShape = hazePS.shape;
        hazeShape.shapeType = ParticleSystemShapeType.Circle;
        hazeShape.radius = 0.15f; // Strictly confined to heater plate cylinder (radius 0.18) so particles NEVER spill onto desk or socket box!
        
        var hazeSize = hazePS.sizeOverLifetime;
        hazeSize.enabled = true;
        hazeSize.size = new ParticleSystem.MinMaxCurve(1.0f, 1.8f);
        
        var hazeColor = hazePS.colorOverLifetime;
        hazeColor.enabled = true;
        Gradient hazeGrad = new Gradient();
        hazeGrad.SetKeys(
            new GradientColorKey[] { new GradientColorKey(new Color(1f, 0.7f, 0.4f), 0f), new GradientColorKey(new Color(1f, 0.5f, 0.2f), 1f) },
            new GradientAlphaKey[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(0.03f, 0.3f), new GradientAlphaKey(0f, 1f) } // Delicate transparent fade
        );
        hazeColor.color = hazeGrad;
        
        var hazeNoise = hazePS.noise;
        hazeNoise.enabled = true;
        hazeNoise.strength = 0.3f;
        hazeNoise.frequency = 2.0f;
        hazeNoise.scrollSpeed = 1.5f;
        
        var hazeRender = hazePS.GetComponent<ParticleSystemRenderer>();
        hazeRender.sharedMaterial = AssetDatabase.GetBuiltinExtraResource<Material>("Default-ParticleSystem.mat");
        hazeRender.renderMode = ParticleSystemRenderMode.Billboard;

        // 7. Setup Sugar Models
        Material sugarMat = GetOrCreateMaterial("SugarMat_Lab5", new Color(0.95f, 0.95f, 0.95f, 1f), true);

        GameObject sugarCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        sugarCube.name = "SugarCube";
        sugarCube.transform.SetParent(water.transform, false);
        sugarCube.transform.localPosition = new Vector3(0f, -0.3f, 0f);
        sugarCube.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
        sugarCube.GetComponent<MeshRenderer>().sharedMaterial = sugarMat;
        DestroyImmediate(sugarCube.GetComponent<Collider>());

        GameObject sugarFine = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sugarFine.name = "SugarFine";
        sugarFine.transform.SetParent(water.transform, false);
        sugarFine.transform.localPosition = new Vector3(0f, -0.45f, 0f);
        sugarFine.transform.localScale = new Vector3(0.5f, 0.1f, 0.5f);
        sugarFine.GetComponent<MeshRenderer>().sharedMaterial = sugarMat;
        sugarFine.SetActive(false);
        DestroyImmediate(sugarFine.GetComponent<Collider>());

        // 8. Setup Glass Rod (Stirrer)
        GameObject rod = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        rod.name = "GlassRod_3D";
        rod.transform.SetParent(beaker.transform, false);
        rod.transform.localPosition = new Vector3(0.1f, 1.0f, -0.1f);
        rod.transform.localRotation = Quaternion.Euler(15f, 0f, 0f);
        rod.transform.localScale = new Vector3(0.04f, 0.8f, 0.04f);
        rod.GetComponent<MeshRenderer>().sharedMaterial = glassMat;
        DestroyImmediate(rod.GetComponent<Collider>());

        // 8.5 Grid Mat under hot plate
        Material gridMatMat = GetOrCreateMaterial("GridMatMat_Lab5", new Color(0.1f, 0.35f, 0.3f, 1f), false);
        GameObject gridMat = GameObject.CreatePrimitive(PrimitiveType.Cube);
        gridMat.name = "Lab5_GridMat";
        gridMat.transform.position = experimentOffset + new Vector3(0f, 0.004f, 0f);
        gridMat.transform.localScale = new Vector3(2.0f, 0.008f, 1.0f);
        gridMat.GetComponent<MeshRenderer>().sharedMaterial = gridMatMat;
        DestroyImmediate(gridMat.GetComponent<Collider>());

        // 8.75 Power Socket & Cable
        GameObject socketBox = GameObject.CreatePrimitive(PrimitiveType.Cube);
        socketBox.name = "TableSocketBox";
        socketBox.transform.position = experimentOffset + new Vector3(-0.3f, 0.03f, 0.4f);
        socketBox.transform.localScale = new Vector3(0.16f, 0.06f, 0.1f);
        Material socketMat = GetOrCreateMaterial("SocketBoxMat_Lab5", new Color(0.2f, 0.2f, 0.2f, 1f), false);
        socketBox.GetComponent<MeshRenderer>().sharedMaterial = socketMat;
        DestroyImmediate(socketBox.GetComponent<Collider>());

        Vector3 heaterStart = experimentOffset + new Vector3(-0.26f, 0.03f, 0.349f);
        Vector3 heaterEnd = experimentOffset + new Vector3(0f, 0.05f, 0.22f);
        Transform envTrans = env != null ? env.transform : null;
        WirePulseAnimator heaterCable = CreatePowerCable("HeaterCable", heaterStart, heaterEnd, experimentOffset.y, envTrans);

        // 8.5 Setup 3D Thermometer Prop
        GameObject thermometer = new GameObject("Thermometer_3D");
        thermometer.transform.position = experimentOffset + new Vector3(-0.08f, 0.13f, 0.06f);
        thermometer.transform.rotation = Quaternion.Euler(14f, 135f, -10f);
        
        GameObject thermBody = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        thermBody.name = "Body";
        thermBody.transform.SetParent(thermometer.transform, false);
        thermBody.transform.localPosition = new Vector3(0f, 0.12f, 0f);
        thermBody.transform.localScale = new Vector3(0.012f, 0.12f, 0.012f);
        thermBody.GetComponent<MeshRenderer>().sharedMaterial = glassMat;
        DestroyImmediate(thermBody.GetComponent<Collider>());

        GameObject thermBulb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        thermBulb.name = "Bulb";
        thermBulb.transform.SetParent(thermometer.transform, false);
        thermBulb.transform.localPosition = new Vector3(0f, 0f, 0f);
        thermBulb.transform.localScale = new Vector3(0.022f, 0.022f, 0.022f);
        Material redMat = GetOrCreateMaterial("ThermometerRedMat_Lab5", new Color(0.8f, 0.1f, 0.1f, 1f), false);
        thermBulb.GetComponent<MeshRenderer>().sharedMaterial = redMat;
        DestroyImmediate(thermBulb.GetComponent<Collider>());

        GameObject thermMercury = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        thermMercury.name = "MercuryColumn";
        thermMercury.transform.SetParent(thermometer.transform, false);
        thermMercury.transform.localPosition = new Vector3(0f, 0.01f, 0f);
        thermMercury.transform.localScale = new Vector3(0.005f, 0.01f, 0.005f);
        thermMercury.GetComponent<MeshRenderer>().sharedMaterial = redMat;
        DestroyImmediate(thermMercury.GetComponent<Collider>());

        // 9. Setup Solubility Manager
        GameObject managerObj = GameObject.Find("ExperimentManager");
        SolubilityManager manager2D = null;
        if (managerObj != null)
        {
            manager2D = managerObj.GetComponent<SolubilityManager>();
        }
        if (manager2D != null)
        {
            DestroyImmediate(manager2D);
        }
        if (managerObj == null)
        {
            managerObj = new GameObject("ExperimentManager");
        }
        
        SolubilityManager3D manager = managerObj.GetComponent<SolubilityManager3D>();
        if (manager == null)
        {
            manager = managerObj.AddComponent<SolubilityManager3D>();
        }
        manager.rodVisual = rod.transform;
        manager.sugarCubeVisual = sugarCube.transform;
        manager.sugarFineVisual = sugarFine.transform;
        manager.heatingPlateRenderer = heaterPlate.GetComponent<MeshRenderer>();
        manager.particleBuih = ps;
        manager.particleSteam = steamPS;
        manager.particleHaze = hazePS;
        manager.waterRenderer = water.GetComponent<MeshRenderer>();
        manager.ledIndicator = led.GetComponent<Renderer>();
        manager.heaterKnob = heaterKnobParent.transform;

        var thermScript = thermometer.AddComponent<SolubilityThermometer3D>();
        thermScript.manager = manager;
        thermScript.mercuryColumn = thermMercury.transform;
        thermScript.mercuryRenderer = thermMercury.GetComponent<MeshRenderer>();

        SolubilityAudio3D audioScript = managerObj.GetComponent<SolubilityAudio3D>();
        if (audioScript == null)
        {
            audioScript = managerObj.AddComponent<SolubilityAudio3D>();
        }

        // Power Switch Link
        var switchScript = switchGroup.AddComponent<SolubilityHeaterPowerSwitch3D>();
        switchScript.manager = manager;
        switchScript.switchLever = leverPivotObj.transform;
        switchScript.offAngle = -15f;
        switchScript.onAngle = 15f;
        switchScript.clickSFX = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/TickSound.wav");
        if (switchScript.clickSFX == null)
        {
            switchScript.clickSFX = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/sfx/snap.wav");
        }
        manager.powerSwitch = switchScript;

        // 10. Setup Diegetic Tablet UI (Identical to Lab 4)
        GameObject tabletObj = new GameObject("Lab5_3DTablet");
        GameObject mainCam = GameObject.Find("Main Camera");
        if (mainCam != null)
        {
            mainCam.GetComponent<Camera>().nearClipPlane = 0.05f;
            if (mainCam.GetComponent<AudioListener>() == null)
            {
                mainCam.AddComponent<AudioListener>();
            }
            tabletObj.transform.SetParent(mainCam.transform, false);
            tabletObj.transform.localPosition = new Vector3(0.2004f, -0.0798f, 0.3001f); 
            tabletObj.transform.localRotation = Quaternion.Euler(-70f, 0f, 15f); 
            tabletObj.transform.localScale = new Vector3(0.2943f, 0.2943f, 0.2943f); 
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
        casing.GetComponent<MeshRenderer>().sharedMaterial = darkMetal;
        DestroyImmediate(casing.GetComponent<Collider>());

        // Screen
        GameObject screen = GameObject.CreatePrimitive(PrimitiveType.Cube);
        screen.name = "TabletScreen";
        screen.transform.SetParent(tabletObj.transform, false);
        screen.transform.localPosition = new Vector3(0f, 0.007f, 0f);
        screen.transform.localScale = new Vector3(0.30f, 0.002f, 0.41f);
        Material screenMat = GetOrCreateMaterial("TabletScreenMat_Lab5", new Color(0.02f, 0.06f, 0.12f, 1f), false);
        screen.GetComponent<MeshRenderer>().sharedMaterial = screenMat;
        DestroyImmediate(screen.GetComponent<Collider>());

        // World Space Canvas
        GameObject tabletCanvasObj = new GameObject("TabletCanvas");
        tabletCanvasObj.transform.SetParent(tabletObj.transform, false);
        tabletCanvasObj.transform.localPosition = new Vector3(0f, 0.009f, 0f); 
        tabletCanvasObj.transform.localRotation = Quaternion.Euler(90f, 0f, 0f); 
        tabletCanvasObj.transform.localScale = new Vector3(0.0005f, 0.0005f, 0.0005f); 

        Canvas canvas = tabletCanvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = Camera.main;

        RectTransform canvasRect = tabletCanvasObj.GetComponent<RectTransform>();
        if (canvasRect != null)
        {
            canvasRect.sizeDelta = new Vector2(600f, 820f); 
        }
        tabletCanvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        // Reparent 2D Left Panel to the Tablet Screen Canvas
        GameObject leftPanel = null;
        GameObject canvasObj = GameObject.Find("Canvas");
        if (canvasObj != null)
        {
            Transform leftTrans = canvasObj.transform.Find("bg/Left");
            if (leftTrans != null)
            {
                leftPanel = leftTrans.gameObject;
            }
        }
        if (leftPanel != null)
        {
            leftPanel.transform.SetParent(tabletCanvasObj.transform, false);
            leftPanel.name = "Left";
            RectTransform leftRT = leftPanel.GetComponent<RectTransform>();
            leftRT.anchorMin = new Vector2(0.5f, 0.5f);
            leftRT.anchorMax = new Vector2(0.5f, 0.5f);
            leftRT.pivot = new Vector2(0.5f, 0.5f);
            leftRT.anchoredPosition = Vector2.zero;
            leftRT.localScale = Vector3.one;
            leftRT.sizeDelta = new Vector2(580f, 810f);

            // Sleek glassmorphism look
            var panelImg = leftPanel.GetComponent<UnityEngine.UI.Image>();
            if (panelImg != null)
            {
                panelImg.color = new Color(0.04f, 0.12f, 0.22f, 0.85f); 
            }
            var panelOutline = leftPanel.GetComponent<UnityEngine.UI.Outline>();
            if (panelOutline == null)
            {
                panelOutline = leftPanel.AddComponent<UnityEngine.UI.Outline>();
            }
            panelOutline.effectColor = new Color(0.15f, 0.6f, 0.95f, 0.5f);
            panelOutline.effectDistance = new Vector2(2f, 2f);

            GameObject titleHeader = leftPanel.transform.Find("Title (1)")?.gameObject;
            if (titleHeader != null)
            {
                var headerImg = titleHeader.GetComponent<UnityEngine.UI.Image>();
                if (headerImg != null)
                {
                    headerImg.color = new Color(0.08f, 0.25f, 0.45f, 0.9f);
                }
            }

            // White text fonts
            var allTMPs = leftPanel.GetComponentsInChildren<TMPro.TextMeshProUGUI>(true);
            foreach (var tmp in allTMPs) tmp.color = Color.white;
            var allUI = leftPanel.GetComponentsInChildren<UnityEngine.UI.Text>(true);
            foreach (var txt in allUI) txt.color = Color.white;
        }

        // Hide 2D background overlay
        GameObject bg2D = GameObject.Find("Canvas/bg");
        if (bg2D != null)
        {
            bg2D.SetActive(false);
        }

        // 11. Hologram Data Board (plots solubility rate)
        Material hologramGlassMat = GetOrCreateMaterial("HologramGlassMat_Lab5", new Color(0f, 0.15f, 0.3f, 0.85f), true);
        Material hologramBorderMat = GetOrCreateMaterial("HologramBorderMat_Lab5", new Color(0f, 0.9f, 1f, 1f), false);
        GameObject hologramBoard = CreateHologramBoard(experimentOffset, hologramGlassMat, hologramBorderMat, out TextMesh holoTM);
        manager.hologramText = holoTM;

        // 12. Open Report Notebook (Buku Laporan)
        InteractiveNotebookLab5 notebookScript = SetupNotebookProp(experimentOffset);

        // Link manager references
        if (leftPanel != null)
        {
            manager.tempSlider = leftPanel.transform.Find("GameObject/SuhuSlider")?.GetComponent<UnityEngine.UI.Slider>();
            manager.stirSlider = leftPanel.transform.Find("GameObject/KadarKacauSlider")?.GetComponent<UnityEngine.UI.Slider>();
            manager.sizeSlider = leftPanel.transform.Find("GameObject/SizeSlider")?.GetComponent<UnityEngine.UI.Slider>();
            manager.solubilityRateText = leftPanel.transform.Find("Bottom/KadarPenyejatan/Text (TMP)")?.GetComponent<TMPro.TextMeshProUGUI>();

            if (manager.tempSlider != null) manager.tempSlider.value = 0f;
            if (manager.stirSlider != null) manager.stirSlider.value = 0f;
            if (manager.sizeSlider != null) manager.sizeSlider.value = 0f;
        }
        manager.graph3D = notebookScript.transform.Find("GraphContainer")?.GetComponent<SolubilityGraph3D>();

        // 13. Setup Interactive UI Panels (Notebook submit, Quiz and Summary)
        SetupInteractiveUIs(notebookScript);

        // 14. Setup Camera Drag Area and POV Switch Button
        SetupCameraDragButton();

        // 15. Setup Tooltips
        AddTooltip(beaker, "Bikar Larutan");
        AddTooltip(rod, "Rod Pengacau");
        AddTooltip(heaterBody, "Pemanas Air (Hot Plate)");
        AddTooltip(notebookScript.gameObject, "Buku Laporan");
        AddTooltip(hologramBoard, "Paparan Kadar Keterlarutan");

        // 16. Cinematic Post processing
        SetupPostProcessing();

        // 17. Cinematic Intro Camera & Tutorial Hints
        if (Camera.main != null)
        {
            var introCam = Camera.main.gameObject.GetComponent<Lab5IntroCamera>();
            if (introCam == null) introCam = Camera.main.gameObject.AddComponent<Lab5IntroCamera>();
        }

        GameObject tutObj = new GameObject("Lab5_TutorialManager");
        tutObj.AddComponent<Lab5TutorialManager>();

        // Save
        var activeScene = EditorSceneManager.GetActiveScene();
        if (KidFriendlySceneTheme.Includes(activeScene)) KidFriendlySceneColourSetup.Apply(activeScene, false);
        EditorSceneManager.MarkSceneDirty(activeScene);
        EditorSceneManager.SaveScene(activeScene);
        AssetDatabase.SaveAssets();

        Debug.Log("3D Lab 5 generated successfully!");
    }

    private static void SetupInteractiveUIs(InteractiveNotebookLab5 notebookScript)
    {
        GameObject canvasObj = GameObject.Find("Canvas");
        if (canvasObj == null) return;
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        // --- Notebook UI World Space Canvas (Placed on Left Page) ---
        GameObject notebookObj = notebookScript.gameObject;
        
        GameObject nbPanel = new GameObject("Lab5_NotebookPanel");
        nbPanel.transform.SetParent(notebookObj.transform, false);
        nbPanel.transform.localPosition = new Vector3(-0.08f, 0.015f, -0.12f); 
        nbPanel.transform.localRotation = Quaternion.Euler(90f, 0f, -2f); 
        nbPanel.transform.localScale = new Vector3(0.0003f, 0.0003f, 0.0003f); 

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
        submitImg.color = new Color(0.12f, 0.6f, 0.3f, 1.0f); 
        
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
        closeImg.color = new Color(0.75f, 0.2f, 0.2f, 1.0f); 
        
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

        notebookScript.uiPanel = nbPanel;
        nbPanel.SetActive(false);

        // --- Quiz UI Panel ---
        GameObject quizManagerObj = new GameObject("QuizManagerLab5");
        QuizManagerLab5 quizManager = quizManagerObj.AddComponent<QuizManagerLab5>();
        notebookScript.quizManager = quizManager;

        GameObject quizPanel = new GameObject("Lab5_QuizPanel");
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
        qText.text = "Faktor manakah yang mempercepatkan kadar keterlarutan gula dalam air?";
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
        string[] answers = { "Merendahkan suhu air", "Menggunakan gula ketul berbanding gula halus", "Membiarkan larutan tanpa dikacau", "Meningkatkan suhu air dan kadar kacauan" };
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
        GameObject sumPanel = new GameObject("Lab5_SummaryPanel");
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
        sumText.text = "TAHNIAH!\nEksperimen Kadar Keterlarutan Selesai.\n\nAnda telah berjaya memahami\nfaktor-faktor yang mempengaruhi\nkadar keterlarutan gula.";

        GameObject sumBtnObj = new GameObject("CloseBtn");
        sumBtnObj.transform.SetParent(sumPanel.transform, false);
        RectTransform sumBtnRect = sumBtnObj.AddComponent<RectTransform>();
        sumBtnRect.sizeDelta = new Vector2(150f, 40f);
        sumBtnRect.anchoredPosition = new Vector2(0, -100);
        UnityEngine.UI.Image sumBtnImg = sumBtnObj.AddComponent<UnityEngine.UI.Image>();
        sumBtnImg.color = new Color(0.8f, 0.2f, 0.2f);
        UnityEngine.UI.Button sumBtn = sumBtnObj.AddComponent<UnityEngine.UI.Button>();

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

    private static void SetupCameraDragButton()
    {
        GameObject canvasObj = GameObject.Find("Canvas");
        if (canvasObj == null) return;

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
        dragScript.targetCamera = Camera.main != null ? Camera.main.transform : null;
    }

    private static InteractiveNotebookLab5 SetupNotebookProp(Vector3 experimentOffset)
    {
        Material notebookCoverMat = GetOrCreateMaterial("NotebookCoverMat_Lab5", new Color(0.08f, 0.08f, 0.12f, 1f), false);
        Material notebookPageMat = GetOrCreateMaterial("NotebookPageMat_Lab5", new Color(0.6f, 0.58f, 0.5f, 1f), false);
        
        GameObject notebook = new GameObject("Lab5_Notebook");
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
        rightTextObj.transform.localPosition = new Vector3(0.08f, 0.01f, 0.14f); 
        rightTextObj.transform.localEulerAngles = new Vector3(90f, 0f, 0f);
        rightTextObj.transform.localScale = new Vector3(0.0022f, 0.0022f, 0.0022f);
        TextMesh rightTM = rightTextObj.AddComponent<TextMesh>();
        rightTM.fontSize = 28;
        rightTM.fontStyle = FontStyle.Bold;
        rightTM.anchor = TextAnchor.MiddleCenter;
        rightTM.alignment = TextAlignment.Center;
        rightTM.color = Color.black;
        rightTM.font = font;
        if (font != null) rightTextObj.GetComponent<MeshRenderer>().sharedMaterial = font.material;

        // 3D Graph Container
        GameObject graphContainer = new GameObject("GraphContainer");
        graphContainer.transform.SetParent(notebook.transform, false);
        graphContainer.transform.localPosition = new Vector3(0.08f, 0.006f, 0f);
        graphContainer.transform.localRotation = Quaternion.Euler(0f, 0f, 2f); 
        graphContainer.transform.localScale = Vector3.one; 
        graphContainer.AddComponent<SolubilityGraph3D>();

        // Pen
        Material penBodyMat = GetOrCreateMaterial("PenBodyMat_Lab5", new Color(0.1f, 0.2f, 0.6f, 1f), false);
        Material penTipMat = GetOrCreateMaterial("PenTipMat_Lab5", new Color(0.2f, 0.2f, 0.2f, 1f), false);
        
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

        InteractiveNotebookLab5 notebookScript = notebook.AddComponent<InteractiveNotebookLab5>();
        notebookScript.leftPageTextMesh = leftTM;
        notebookScript.rightPageTextMesh = rightTM;
        return notebookScript;
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
            vp.name = "Lab5_VolumeProfile";
            AssetDatabase.CreateAsset(vp, "Assets/Materials/Lab5_VolumeProfile.asset");

            Bloom bloom = vp.Add<Bloom>();
            bloom.active = true;
            bloom.threshold.Override(1.1f);
            bloom.intensity.Override(0.8f);
            bloom.scatter.Override(0.5f);
            bloom.tint.Override(new Color(0.6f, 0.85f, 1f));
            AssetDatabase.AddObjectToAsset(bloom, vp);

            ColorAdjustments ca = vp.Add<ColorAdjustments>();
            ca.active = true;
            ca.postExposure.Override(0.15f);
            ca.contrast.Override(15f);
            ca.saturation.Override(25f);
            AssetDatabase.AddObjectToAsset(ca, vp);

            Tonemapping tm = vp.Add<Tonemapping>();
            tm.active = true;
            tm.mode.Override(TonemappingMode.ACES);
            AssetDatabase.AddObjectToAsset(tm, vp);

            AssetDatabase.SaveAssets();
            v.sharedProfile = vp;
        }

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
        GameObject outlineTop = new GameObject("Outline_Top");
        outlineTop.transform.SetParent(beaker.transform, false);
        
        LineRenderer lrTop = outlineTop.AddComponent<LineRenderer>();
        lrTop.useWorldSpace = false;
        lrTop.loop = true;
        lrTop.startWidth = 0.02f;
        lrTop.endWidth = 0.02f;
        
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

        Gradient staticGrad = new Gradient();
        staticGrad.SetKeys(
            new GradientColorKey[] { new GradientColorKey(baseColor, 0f), new GradientColorKey(baseColor, 1f) },
            new GradientAlphaKey[] { new GradientAlphaKey(baseColor.a, 0f), new GradientAlphaKey(baseColor.a, 1f) }
        );
        lr.colorGradient = staticGrad;

        float dist = Vector3.Distance(start, end);
        float sag = dist * 0.4f;
        Vector3 controlPoint = Vector3.Lerp(start, end, 0.5f);
        controlPoint.y -= sag;

        int segments = 25;
        lr.positionCount = segments;
        for (int i = 0; i < segments; i++)
        {
            float t = (float)i / (segments - 1);
            Vector3 pos = Mathf.Pow(1f - t, 2f) * start + 
                          2f * (1f - t) * t * controlPoint + 
                          Mathf.Pow(t, 2f) * end;

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

    private static GameObject CreateHologramBoard(Vector3 experimentOffset, Material glassMat, Material borderMat, out TextMesh holoTM)
    {
        GameObject hologramBoard = new GameObject("HologramDataBoard");
        hologramBoard.transform.position = experimentOffset + new Vector3(-1.0f, 1.05f, 0.25f);
        hologramBoard.transform.localScale = new Vector3(0.55f, 0.55f, 0.55f);

        Vector3 camPos = experimentOffset + new Vector3(0f, 0.95f, -1.8f);
        hologramBoard.transform.LookAt(hologramBoard.transform.position + (hologramBoard.transform.position - camPos).normalized);

        hologramBoard.AddComponent<LookAtCamera3D>();

        GameObject glassPlate = GameObject.CreatePrimitive(PrimitiveType.Cube);
        glassPlate.name = "HologramGlassPlate";
        glassPlate.transform.SetParent(hologramBoard.transform, false);
        glassPlate.transform.localScale = new Vector3(0.9f, 0.6f, 0.01f);
        glassPlate.GetComponent<MeshRenderer>().sharedMaterial = glassMat;
        DestroyImmediate(glassPlate.GetComponent<Collider>());

        GameObject borderTop = GameObject.CreatePrimitive(PrimitiveType.Cube);
        borderTop.name = "BorderTop";
        borderTop.transform.SetParent(hologramBoard.transform, false);
        borderTop.transform.localPosition = new Vector3(0f, 0.31f, 0f);
        borderTop.transform.localScale = new Vector3(0.94f, 0.02f, 0.02f);
        borderTop.GetComponent<MeshRenderer>().sharedMaterial = borderMat;
        DestroyImmediate(borderTop.GetComponent<Collider>());

        GameObject borderBottom = GameObject.CreatePrimitive(PrimitiveType.Cube);
        borderBottom.name = "BorderBottom";
        borderBottom.transform.SetParent(hologramBoard.transform, false);
        borderBottom.transform.localPosition = new Vector3(0f, -0.31f, 0f);
        borderBottom.transform.localScale = new Vector3(0.94f, 0.02f, 0.02f);
        borderBottom.GetComponent<MeshRenderer>().sharedMaterial = borderMat;
        DestroyImmediate(borderBottom.GetComponent<Collider>());

        GameObject borderLeft = GameObject.CreatePrimitive(PrimitiveType.Cube);
        borderLeft.name = "BorderLeft";
        borderLeft.transform.SetParent(hologramBoard.transform, false);
        borderLeft.transform.localPosition = new Vector3(-0.46f, 0f, 0f);
        borderLeft.transform.localScale = new Vector3(0.02f, 0.64f, 0.02f);
        borderLeft.GetComponent<MeshRenderer>().sharedMaterial = borderMat;
        DestroyImmediate(borderLeft.GetComponent<Collider>());

        GameObject borderRight = GameObject.CreatePrimitive(PrimitiveType.Cube);
        borderRight.name = "BorderRight";
        borderRight.transform.SetParent(hologramBoard.transform, false);
        borderRight.transform.localPosition = new Vector3(0.46f, 0f, 0f);
        borderRight.transform.localScale = new Vector3(0.02f, 0.64f, 0.02f);
        borderRight.GetComponent<MeshRenderer>().sharedMaterial = borderMat;
        DestroyImmediate(borderRight.GetComponent<Collider>());

        GameObject holoTextObj = new GameObject("HologramText");
        holoTextObj.transform.SetParent(hologramBoard.transform, false);
        holoTextObj.transform.localPosition = new Vector3(0f, 0f, -0.015f);
        holoTextObj.transform.localScale = new Vector3(0.12f, 0.12f, 0.12f);

        holoTM = holoTextObj.AddComponent<TextMesh>();
        holoTM.text = "Kadar Keterlarutan:\n0.000 unit/s";
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

    private static Material GetOrCreateMaterial(string name, Color color, bool isTransparent)
    {
        string path = "Assets/Materials/" + name + ".mat";
        
        if (!AssetDatabase.IsValidFolder("Assets/Materials"))
        {
            AssetDatabase.CreateFolder("Assets", "Materials");
        }

        Shader shader = Shader.Find("Universal Render Pipeline/Simple Lit");
        if (shader == null) shader = Shader.Find("Universal Render Pipeline/Lit");
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
            EditorUtility.SetDirty(mat);
            AssetDatabase.SaveAssets();
            return mat;
        }

        mat = new Material(shader);
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
                if (rootObj.name == "Classroom Environment" || (rootObj.GetComponent<Light>() != null && rootObj.name == "Directional Light"))
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
}
#endif
