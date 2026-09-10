#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class Lab2_3D_Setup : EditorWindow
{
    [MenuItem("E-Lab Tools/Generate 3D Lab 2 (Electrolysis)")]
    public static void ShowWindow()
    {
        GenerateLab2_3D();
    }

    private static void GenerateLab2_3D()
    {
        if (UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene().name != "Lab2_3D")
        {
            UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/Lab2_3D.unity", OpenSceneMode.Single);
        }

        DestroyDuplicateLights();
        
        // 0. Import Environment from Lab 1 and calculate table offset
        Vector3 experimentOffset = Vector3.zero;
        GameObject env = CopyEnvironmentFromLab1();
        if (env != null)
        {
            // Clean up Lab 1 specific objects that came with the environment
            string[] lab1Objects = { "Beaker_2", "Liquid_1", "Liquid_2", "BurnerBase", "BurnerTube" };
            foreach (string objName in lab1Objects)
            {
                Transform oldObj = env.transform.Find(objName);
                if (oldObj != null) DestroyImmediate(oldObj.gameObject);
            }

            // Ubah Tajuk Papan Putih (Cari pada objek dan anak-anaknya)
            GameObject titleObj = GameObject.Find("Whiteboard_Title");
            if (titleObj != null)
            {
                var tmps = titleObj.GetComponentsInChildren<TMPro.TextMeshPro>(true);
                foreach (var t in tmps) 
                {
                    t.text = "AMALI 2 ELEKTROLISIS";
                    t.color = Color.white;
                }
                
                var tmpUIs = titleObj.GetComponentsInChildren<TMPro.TextMeshProUGUI>(true);
                foreach (var t in tmpUIs) 
                {
                    t.text = "AMALI 2 ELEKTROLISIS";
                    t.color = Color.white;
                }
                
                var txts = titleObj.GetComponentsInChildren<UnityEngine.UI.Text>(true);
                foreach (var t in txts) 
                {
                    t.text = "AMALI 2 ELEKTROLISIS";
                    t.color = Color.white;
                }
            }

            // Darken the whiteboard to make clear glass pop against the background
            MeshRenderer[] envRenderers = env.GetComponentsInChildren<MeshRenderer>(true);
            foreach (MeshRenderer r in envRenderers)
            {
                string rName = r.gameObject.name.ToLower();
                if (rName.Contains("whiteboard") || rName.Contains("board"))
                {
                    if (r.sharedMaterial != null)
                    {
                        Material darkMat = new Material(r.sharedMaterial);
                        // Make whiteboard darker so it doesn't blow out under bright lights
                        darkMat.color = new Color(0.25f, 0.28f, 0.32f, 1f); // Dark slate grey
                        r.material = darkMat;
                    }
                }
            }

            // Set Directional Lights and Ambient Lighting for a fully bright, professional laboratory environment
            Light[] lights = Object.FindObjectsByType<Light>(FindObjectsInactive.Include);
            foreach(Light l in lights)
            {
                if (l.type == LightType.Directional)
                {
                    l.intensity = 1.6f; // Brighter daylight
                    l.color = new Color(0.98f, 0.99f, 1f); 
                    l.shadows = LightShadows.Soft; // Enable shadows
                    l.shadowStrength = 0.6f; // Softer shadows so it's less dark
                }
            }

            // Atmospheric Depth (Fog)
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = 0.015f; // Much less dense fog
            RenderSettings.fogColor = new Color(0.1f, 0.15f, 0.2f); // Lighter blue-grey fog
            if (Camera.main != null)
            {
                Camera.main.clearFlags = CameraClearFlags.SolidColor;
                Camera.main.backgroundColor = new Color(0.1f, 0.15f, 0.2f); // Lighter background
            }

            // Force the ambient lighting to be bright so shadows aren't pitch black
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.85f, 0.9f, 0.95f); // Very bright ambient fill light

            GameObject deskTop = GameObject.Find("Desk_1");
            if (deskTop == null) deskTop = GameObject.Find("TeacherDeskTop"); // Fallback
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
                            Material darkGreyMat = new Material(r.sharedMaterial);
                            if (darkGreyMat.HasProperty("_Color")) 
                            {
                                darkGreyMat.color = new Color(0.1f, 0.12f, 0.15f, 1f); // Dark blue-grey
                            }
                            if (darkGreyMat.HasProperty("_Metallic")) darkGreyMat.SetFloat("_Metallic", 0.8f);
                            if (darkGreyMat.HasProperty("_Glossiness")) darkGreyMat.SetFloat("_Glossiness", 0.75f);
                            if (darkGreyMat.HasProperty("_Smoothness")) darkGreyMat.SetFloat("_Smoothness", 0.75f);
                            
                            r.material = darkGreyMat;
                        }
                    }
                }
            }
            // Create a dramatic overhead desk light
            Transform oldDeskLight = env.transform.Find("Overhead_DeskLight");
            if (oldDeskLight != null) DestroyImmediate(oldDeskLight.gameObject);

            GameObject deskLightObj = new GameObject("Overhead_DeskLight");
            deskLightObj.transform.SetParent(env.transform);
            deskLightObj.transform.position = experimentOffset + new Vector3(0f, 3.5f, 0.5f); // Directly over the apparatus and slightly towards camera
            deskLightObj.transform.localRotation = Quaternion.Euler(75f, 0f, 0f); // Point mostly down, slightly towards back
            
            Light deskLight = deskLightObj.AddComponent<Light>();
            deskLight.type = LightType.Spot;
            deskLight.intensity = 3.5f; // Balanced illumination for the table
            deskLight.range = 10f;
            deskLight.spotAngle = 85f; // Wide enough to cover the desk props
            deskLight.innerSpotAngle = 50f;
            deskLight.color = new Color(0.98f, 0.99f, 1f); // Pure crisp light
            deskLight.shadows = LightShadows.Soft; // Realistic soft shadows

            // Adjust Main Camera to look realistically at the table setup
            if (Camera.main != null)
            {
                Camera.main.transform.position = experimentOffset + new Vector3(0, 1.2f, -2.5f);
                Camera.main.transform.LookAt(experimentOffset + new Vector3(0, 0.6f, 0));
                Camera.main.nearClipPlane = 0.05f; // Very small near clip plane so close UI elements don't disappear
                
                AudioSource camAudio = Camera.main.GetComponent<AudioSource>();
                if (camAudio != null)
                {
                    camAudio.volume = 0.02f; // Make ambient hum extremely subtle
                }
            }

            // Ensure Canvas is visible in 3D (Overlay mode)
            Canvas mainCanvas = Object.FindAnyObjectByType<Canvas>();
            if (mainCanvas != null)
            {
                mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            }
        }

        // Clean up existing generated objects to ensure clean regeneration
        DestroyIfExists("Beaker_3D");
        DestroyIfExists("BeakerGlowLight");
        DestroyIfExists("Lab2_SummaryPanel");
        DestroyIfExists("HologramDataBoard");
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

        // 1. Create or Load Materials properly as Assets to avoid Magenta/Missing Shader errors
        Material glassMat = GetOrCreateMaterial("GlassMat_Lab2", new Color(0.9f, 0.93f, 0.96f, 0.24f), true);
        Material waterMat = GetOrCreateMaterial("WaterMat_Lab2", new Color(0.12f, 0.58f, 0.85f, 0.82f), true);
        Material carbonMat = GetOrCreateMaterial("CarbonMat_Lab2", new Color(0.1f, 0.1f, 0.1f, 1f), false);
        Material instrumentMat = GetOrCreateMaterial("InstrumentMat_Lab2", new Color(0.75f, 0.8f, 0.85f, 1f), false);
        Material baseMat = GetOrCreateMaterial("BaseMat_Lab2", new Color(0.8f, 0.8f, 0.8f, 1f), false);
        Material batteryMat = GetOrCreateMaterial("BatteryMat_Lab2", new Color(0.9f, 0.2f, 0.2f, 1f), false);
        Material redMat = GetOrCreateMaterial("RedMat_Lab2", new Color(1f, 0f, 0f, 1f), false);
        
        // Distinct materials for test tubes so they stand out
        Material tubeGlassMat = GetOrCreateMaterial("TubeGlassMat_Lab2", new Color(0.85f, 0.85f, 0.9f, 0.1f), true);
        Material tubeWaterMat = GetOrCreateMaterial("TubeWaterMat_Lab2", new Color(0.02f, 0.25f, 0.75f, 0.85f), true);

        // Explicit Render Queues to fix transparent sorting and prevent color popping
        tubeWaterMat.renderQueue = 3000;  // Render inner water first
        tubeGlassMat.renderQueue = 3002;  // Then test tube glass
        waterMat.renderQueue = 3004;      // Then beaker water
        glassMat.renderQueue = 3006;      // Finally beaker glass

        EditorUtility.SetDirty(tubeWaterMat);
        EditorUtility.SetDirty(tubeGlassMat);
        EditorUtility.SetDirty(waterMat);
        EditorUtility.SetDirty(glassMat);
        AssetDatabase.SaveAssets();

        // Retort stand materials
        Material retortMetalMat = GetOrCreateMaterial("RetortMetalMat_Lab2", new Color(0.15f, 0.15f, 0.15f, 1f), false);
        Material retortRodMat = GetOrCreateMaterial("RetortRodMat_Lab2", new Color(0.7f, 0.7f, 0.7f, 1f), false);

        // 2. Base Container (Beaker) - Open Top Custom Mesh
        GameObject beaker = GameObject.Find("Beaker_3D");
        if (beaker == null)
        {
            beaker = new GameObject("Beaker_3D");
            MeshFilter mf = beaker.AddComponent<MeshFilter>();
            mf.sharedMesh = GenerateOpenBeakerMesh();
            MeshRenderer mr = beaker.AddComponent<MeshRenderer>();
            beaker.transform.position = experimentOffset + new Vector3(0, 0.485f, 0); // Pushed down slightly to prevent bottom Z-fighting
            beaker.transform.localScale = new Vector3(0.84f, 0.49f, 0.84f);
            mr.sharedMaterial = glassMat;
        }

        // Add distinct black structural outline to beaker
        CreateBeakerOutline(beaker);

        // 3. Main Water inside Beaker
        GameObject mainWater = GameObject.Find("MainWater_3D");
        if (mainWater == null)
        {
            mainWater = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            mainWater.name = "MainWater_3D";
            mainWater.transform.SetParent(beaker.transform);
            mainWater.transform.localPosition = new Vector3(0, -0.15f, 0); 
            mainWater.transform.localScale = new Vector3(0.95f, 0.8f, 0.95f);
            mainWater.GetComponent<MeshRenderer>().sharedMaterial = waterMat;
            DestroyImmediate(mainWater.GetComponent<Collider>());
            
            // Add Surface Details (Ripples/Highlights)
            GameObject surface = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            surface.name = "WaterSurface";
            surface.transform.SetParent(mainWater.transform, false);
            surface.transform.localPosition = new Vector3(0, 1.001f, 0); // Top of cylinder
            surface.transform.localScale = new Vector3(1f, 0.001f, 1f); // Thin disc
            DestroyImmediate(surface.GetComponent<Collider>());
            
            Material surfaceMat = new Material(waterMat);
            surfaceMat.name = "WaterSurfaceMat";
            surfaceMat.color = new Color(waterMat.color.r + 0.2f, waterMat.color.g + 0.2f, waterMat.color.b + 0.2f, 0.85f);
            surfaceMat.EnableKeyword("_EMISSION");
            surfaceMat.SetColor("_EmissionColor", new Color(0f, 0.3f, 0.6f));
            surface.GetComponent<MeshRenderer>().sharedMaterial = surfaceMat;
        }

        // Beaker Glow Light (Point light for water refraction/cyan glow)
        GameObject glowLight = GameObject.Find("BeakerGlowLight");
        if (glowLight == null)
        {
            glowLight = new GameObject("BeakerGlowLight");
            Light lightComp = glowLight.AddComponent<Light>();
            lightComp.type = LightType.Point;
            lightComp.color = new Color(0.1f, 0.6f, 1f); // Neon Cyan/Blue glow
            lightComp.range = 1.5f;
            lightComp.intensity = 2.5f;
        }
        glowLight.transform.position = experimentOffset + new Vector3(0f, 0.525f, 0f); // Positioned inside the beaker water

        // No Base Stand - we sit on the table

        // --- Left Side (Anode - Oxygen) ---
        GameObject anodeTube = GameObject.Find("TestTube_Anode (Oxygen)");
        if (anodeTube == null)
        {
            anodeTube = CreateTestTube("TestTube_Anode (Oxygen)", experimentOffset + new Vector3(-0.25f, 0.98f, 0), tubeGlassMat);
            anodeTube.transform.localScale = new Vector3(0.11f, 0.42f, 0.11f);
        }
        
        GameObject anodeWaterPivot = GameObject.Find("AnodeWaterPivot");
        if (anodeWaterPivot == null)
        {
            anodeWaterPivot = new GameObject("AnodeWaterPivot");
            anodeWaterPivot.transform.position = experimentOffset + new Vector3(-0.25f, 0.56f, 0);
            anodeWaterPivot.transform.localScale = new Vector3(0.11f, 0.7f, 0.11f);
            
            GameObject anodeWater = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            anodeWater.name = "WaterVolume";
            anodeWater.transform.SetParent(anodeWaterPivot.transform);
            anodeWater.transform.localPosition = new Vector3(0, 0.55f, 0); 
            anodeWater.transform.localScale = new Vector3(0.85f, 0.55f, 0.85f); 
            anodeWater.GetComponent<MeshRenderer>().sharedMaterial = tubeWaterMat;
            DestroyImmediate(anodeWater.GetComponent<Collider>());
        }

        GameObject anodeElectrode = GameObject.Find("Electrode_Anode");
        if (anodeElectrode == null)
        {
            anodeElectrode = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            anodeElectrode.name = "Electrode_Anode";
            anodeElectrode.transform.position = experimentOffset + new Vector3(-0.25f, 0.7f, 0); 
            anodeElectrode.transform.localScale = new Vector3(0.04f, 0.21f, 0.04f); 
            anodeElectrode.GetComponent<MeshRenderer>().sharedMaterial = carbonMat;
            DestroyImmediate(anodeElectrode.GetComponent<Collider>());
        }
        CreateBubbleParticles(anodeElectrode, "OxygenBubbles", 20f, new Color(1f, 1f, 1f, 0.95f)); // Oxygen bubbles

        // --- Right Side (Cathode - Hydrogen) ---
        GameObject cathodeTube = GameObject.Find("TestTube_Cathode (Hydrogen)");
        if (cathodeTube == null)
        {
            cathodeTube = CreateTestTube("TestTube_Cathode (Hydrogen)", experimentOffset + new Vector3(0.25f, 0.98f, 0), tubeGlassMat);
            cathodeTube.transform.localScale = new Vector3(0.11f, 0.42f, 0.11f);
        }
        
        GameObject cathodeWaterPivot = GameObject.Find("CathodeWaterPivot");
        if (cathodeWaterPivot == null)
        {
            cathodeWaterPivot = new GameObject("CathodeWaterPivot");
            cathodeWaterPivot.transform.position = experimentOffset + new Vector3(0.25f, 0.56f, 0);
            cathodeWaterPivot.transform.localScale = new Vector3(0.11f, 0.7f, 0.11f);
            
            GameObject cathodeWater = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cathodeWater.name = "WaterVolume";
            cathodeWater.transform.SetParent(cathodeWaterPivot.transform);
            cathodeWater.transform.localPosition = new Vector3(0, 0.5f, 0);
            cathodeWater.transform.localScale = new Vector3(0.85f, 0.5f, 0.85f);
            cathodeWater.GetComponent<MeshRenderer>().sharedMaterial = tubeWaterMat;
            DestroyImmediate(cathodeWater.GetComponent<Collider>());
        }

        GameObject cathodeElectrode = GameObject.Find("Electrode_Cathode");
        if (cathodeElectrode == null)
        {
            cathodeElectrode = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cathodeElectrode.name = "Electrode_Cathode";
            cathodeElectrode.transform.position = experimentOffset + new Vector3(0.25f, 0.7f, 0);
            cathodeElectrode.transform.localScale = new Vector3(0.04f, 0.21f, 0.04f);
            cathodeElectrode.GetComponent<MeshRenderer>().sharedMaterial = carbonMat;
            DestroyImmediate(cathodeElectrode.GetComponent<Collider>());
        }
        CreateBubbleParticles(cathodeElectrode, "HydrogenBubbles", 40f, new Color(1f, 1f, 1f, 0.95f)); // Hydrogen produces double gas!

        // 5. Battery
        GameObject battery = GameObject.Find("Battery_9V_3D");
        if (battery == null)
        {
            battery = GameObject.CreatePrimitive(PrimitiveType.Cube);
            battery.name = "Battery_9V_3D";
            battery.transform.localScale = new Vector3(0.32f, 0.48f, 0.2f);
            battery.GetComponent<MeshRenderer>().sharedMaterial = batteryMat;
            
            GameObject termPos = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            termPos.name = "Terminal_Pos";
            termPos.transform.SetParent(battery.transform);
            termPos.transform.localPosition = new Vector3(-0.2f, 0.55f, 0);
            termPos.transform.localScale = new Vector3(0.2f, 0.1f, 0.3f);
            termPos.GetComponent<MeshRenderer>().sharedMaterial = carbonMat;

            GameObject termNeg = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            termNeg.name = "Terminal_Neg";
            termNeg.transform.SetParent(battery.transform);
            termNeg.transform.localPosition = new Vector3(0.2f, 0.55f, 0);
            termNeg.transform.localScale = new Vector3(0.2f, 0.1f, 0.3f);
            termNeg.GetComponent<MeshRenderer>().sharedMaterial = carbonMat;
        }
        // Reposition battery to the left side
        battery.transform.position = experimentOffset + new Vector3(-1.6f, 0.238f, 0); // Pushed down by 2mm to prevent Z-fighting

        // 5.5 Ammeter
        GameObject ammeter = GameObject.Find("Ammeter_3D");
        if (ammeter == null)
        {
            ammeter = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ammeter.name = "Ammeter_3D";
            ammeter.transform.localScale = new Vector3(0.25f, 0.12f, 0.25f);
            ammeter.GetComponent<MeshRenderer>().sharedMaterial = instrumentMat; 
            
            // Dial Texture Material
            Material dialMat = null;
            string[] guids = AssetDatabase.FindAssets("AmmeterDialMat t:Material");
            if (guids.Length > 0) dialMat = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guids[0]));
            if (dialMat == null) dialMat = new Material(Shader.Find("Standard")) { color = Color.white };

            // Create Slanted Panel
            GameObject panel = GameObject.CreatePrimitive(PrimitiveType.Cube);
            panel.name = "DialPanel";
            panel.transform.SetParent(ammeter.transform);
            panel.transform.localPosition = new Vector3(0f, 0.6f, 0f);
            panel.transform.localEulerAngles = new Vector3(15f, 0f, 0f);
            panel.transform.localScale = new Vector3(0.9f, 0.2f, 0.9f);
            panel.GetComponent<MeshRenderer>().sharedMaterial = instrumentMat;

            // Create Dial Face
            GameObject dialFace = GameObject.CreatePrimitive(PrimitiveType.Quad);
            dialFace.name = "DialFace";
            dialFace.transform.SetParent(panel.transform);
            dialFace.transform.localPosition = new Vector3(0f, 0.51f, 0.1f);
            dialFace.transform.localEulerAngles = new Vector3(90f, 0f, 0f);
            dialFace.transform.localScale = new Vector3(0.8f, 0.8f, 1f);
            dialFace.GetComponent<MeshRenderer>().sharedMaterial = dialMat;
            Object.DestroyImmediate(dialFace.GetComponent<Collider>());

            // Create Glass Cover
            GameObject glass = GameObject.CreatePrimitive(PrimitiveType.Cube);
            glass.name = "GlassCover";
            glass.transform.SetParent(panel.transform);
            glass.transform.localPosition = new Vector3(0f, 0.6f, 0.1f);
            glass.transform.localEulerAngles = Vector3.zero;
            glass.transform.localScale = new Vector3(0.85f, 0.05f, 0.85f);
            glass.GetComponent<MeshRenderer>().sharedMaterial = glassMat;
            Object.DestroyImmediate(glass.GetComponent<Collider>());

            GameObject needle = GameObject.CreatePrimitive(PrimitiveType.Cube);
            needle.name = "AmmeterNeedle";
            needle.transform.SetParent(panel.transform);
            needle.transform.localPosition = new Vector3(0f, 0.52f, 0f); 
            needle.transform.localScale = new Vector3(0.04f, 0.05f, 0.4f);
            needle.GetComponent<MeshRenderer>().sharedMaterial = redMat; 
            
            AmmeterNeedle3D needleScript = ammeter.AddComponent<AmmeterNeedle3D>();
            needleScript.needlePivot = needle.transform;
            needleScript.zeroAngle = 45f;
            needleScript.targetAngle = -45f;
            EditorUtility.SetDirty(needleScript);
            
            GameObject textObj = new GameObject("AmmeterText");
            textObj.transform.SetParent(ammeter.transform);
            Vector3 parentScale = ammeter.transform.localScale;
            float targetWorldScale = 0.15f;
            textObj.transform.localScale = new Vector3(
                targetWorldScale / parentScale.x,
                targetWorldScale / parentScale.y,
                targetWorldScale / parentScale.z
            );
            textObj.transform.localPosition = new Vector3(0, 1.8f, 0.4f);
            textObj.transform.localEulerAngles = new Vector3(0, 0, 0);
            TextMesh tm = textObj.AddComponent<TextMesh>();
            tm.text = "Ammeter (A)";
            tm.characterSize = 0.1f;
            tm.fontSize = 72;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.color = new Color(0.9f, 0.9f, 0.9f, 1f);
        }
        else
        {
            // Update TextMesh properties on existing ammeter if it exists
            Transform ammeterTextTransform = ammeter.transform.Find("AmmeterText");
            if (ammeterTextTransform != null)
            {
                ammeterTextTransform.localEulerAngles = new Vector3(0, 0, 0);
                ammeterTextTransform.localPosition = new Vector3(0, 1.2f, -0.6f);
                Vector3 parentScale = ammeter.transform.localScale;
                float targetWorldScale = 0.15f;
                ammeterTextTransform.localScale = new Vector3(
                    targetWorldScale / parentScale.x,
                    targetWorldScale / parentScale.y,
                    targetWorldScale / parentScale.z
                );
                TextMesh tm = ammeterTextTransform.GetComponent<TextMesh>();
                if (tm != null)
                {
                    tm.characterSize = 0.1f;
                    tm.fontSize = 72;
                    tm.color = new Color(0.1f, 0.1f, 0.15f, 1f);
                }
            }
        }
        // Reposition ammeter further to the left side
        ammeter.transform.position = experimentOffset + new Vector3(-1.2f, 0.048f, -0.2f); // Pushed down by 2mm to prevent Z-fighting

        // 5.6 Physical Switch
        GameObject switch3D = GameObject.Find("PhysicalSwitch_3D");
        if (switch3D == null)
        {
            switch3D = GameObject.CreatePrimitive(PrimitiveType.Cube);
            switch3D.name = "PhysicalSwitch_3D";
            switch3D.transform.localScale = new Vector3(0.2f, 0.1f, 0.3f);
            switch3D.GetComponent<MeshRenderer>().sharedMaterial = instrumentMat; 
            
            GameObject lever = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            lever.name = "SwitchLever";
            lever.transform.SetParent(switch3D.transform);
            lever.transform.localPosition = new Vector3(0, 1.0f, -0.4f);
            lever.transform.localEulerAngles = new Vector3(-45, 0, 0);
            lever.transform.localScale = new Vector3(0.2f, 0.8f, 0.2f);
            lever.GetComponent<MeshRenderer>().sharedMaterial = redMat;
            
            PhysicalSwitch3D switchScript = switch3D.AddComponent<PhysicalSwitch3D>();
            switchScript.leverPivot = lever.transform;
            switchScript.offAngle = -45f;
            switchScript.onAngle = 0f; 
            EditorUtility.SetDirty(switchScript);
        }
        // Reposition switch further to the left side
        switch3D.transform.position = experimentOffset + new Vector3(-0.8f, 0.048f, -0.2f); // Pushed down by 2mm to prevent Z-fighting

        // 6. Cubic Bezier Wires (Rerouted professionally to the left side)
        Vector3 posTerminal = battery.transform.position + new Vector3(-0.06f, 0.26f, 0);
        Vector3 negTerminal = battery.transform.position + new Vector3(0.06f, 0.26f, 0);
        
        Vector3 ammeterPosTerm = ammeter.transform.position + new Vector3(-0.1f, 0.1f, 0);
        Vector3 ammeterNegTerm = ammeter.transform.position + new Vector3(0.1f, 0.1f, 0);

        Vector3 switchPosTerm = switch3D.transform.position + new Vector3(0, 0.1f, -0.1f);
        Vector3 switchNegTerm = switch3D.transform.position + new Vector3(0, 0.1f, 0.1f);

        Vector3 anodeConnect = anodeElectrode.transform.position + new Vector3(0, -0.21f, 0);
        Vector3 cathodeConnect = cathodeElectrode.transform.position + new Vector3(0, -0.21f, 0);

        // Wire 1: Battery Neg -> Cathode (Black)
        // Curves up over beaker rim, loops behind the tubes, enters cathode tube from bottom without clipping
        Vector3 cathodeC1 = negTerminal + new Vector3(0.84f, 1.974f, -0.2f);
        Vector3 cathodeC2 = cathodeConnect + new Vector3(-0.196f, 0.074f, -0.029f);
        CreateCubicBezierWire("Wire_Cathode (Black)", negTerminal, cathodeC1, cathodeC2, cathodeConnect, Color.black);

        // Wire 2: Battery Pos -> Ammeter Pos (Red)
        CreateCubicBezierWire("Wire_Bat_Ammeter (Red)", posTerminal, posTerminal + new Vector3(0.1f, 0.3f, -0.2f), ammeterPosTerm + new Vector3(-0.1f, 0.3f, 0f), ammeterPosTerm, Color.red);
        
        // Wire 3: Ammeter Neg -> Switch Pos (Red)
        CreateCubicBezierWire("Wire_Ammeter_Switch (Red)", ammeterNegTerm, ammeterNegTerm + new Vector3(0.1f, 0.2f, -0.1f), switchPosTerm + new Vector3(-0.1f, 0.2f, 0f), switchPosTerm, Color.red);
        
        // Wire 4: Switch Neg -> Anode (Red)
        // Curves up over beaker rim, loops along the left side, enters anode tube from bottom without clipping
        Vector3 anodeC1 = switchNegTerm + new Vector3(0.1f, 2.6f, -0.1f);
        Vector3 anodeC2 = anodeConnect + new Vector3(-0.298f, 0.074f, 0.171f); 
        CreateCubicBezierWire("Wire_Anode (Red)", switchNegTerm, anodeC1, anodeC2, anodeConnect, Color.red);

        // 6.5 Retort Stand & Clamps
        GameObject retortStand = GameObject.Find("RetortStand_3D");
        if (retortStand == null)
        {
            retortStand = new GameObject("RetortStand_3D");
        }
        retortStand.transform.position = experimentOffset;

        // Base
        GameObject baseObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        baseObj.name = "RetortBase";
        baseObj.transform.SetParent(retortStand.transform);
        baseObj.transform.localPosition = new Vector3(0f, 0.013f, 0.85f); // Pushed down by 2mm to prevent Z-fighting
        baseObj.transform.localScale = new Vector3(0.5f, 0.03f, 0.3f);
        baseObj.GetComponent<MeshRenderer>().sharedMaterial = retortMetalMat;
        DestroyImmediate(baseObj.GetComponent<Collider>());

        // Vertical Rod
        GameObject rodObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        rodObj.name = "VerticalRod";
        rodObj.transform.SetParent(retortStand.transform);
        rodObj.transform.localPosition = new Vector3(0f, 1.0f, 0.95f);
        rodObj.transform.localScale = new Vector3(0.03f, 1.0f, 0.03f);
        rodObj.GetComponent<MeshRenderer>().sharedMaterial = retortRodMat;
        DestroyImmediate(rodObj.GetComponent<Collider>());

        // Bosshead 1 (Joint between Rod and Arm)
        GameObject joint1Obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        joint1Obj.name = "Bosshead_RodArm";
        joint1Obj.transform.SetParent(retortStand.transform);
        joint1Obj.transform.localPosition = new Vector3(0f, 1.25f, 0.95f);
        joint1Obj.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
        joint1Obj.GetComponent<MeshRenderer>().sharedMaterial = retortMetalMat;
        DestroyImmediate(joint1Obj.GetComponent<Collider>());

        // Horizontal Arm
        GameObject armObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        armObj.name = "HorizontalArm";
        armObj.transform.SetParent(retortStand.transform);
        armObj.transform.localPosition = new Vector3(0f, 1.25f, 0.475f);
        armObj.transform.localScale = new Vector3(0.02f, 0.475f, 0.02f);
        armObj.transform.localEulerAngles = new Vector3(90f, 0f, 0f);
        armObj.GetComponent<MeshRenderer>().sharedMaterial = retortRodMat;
        DestroyImmediate(armObj.GetComponent<Collider>());

        // Bosshead 2 (Joint between Arm and Crossbar)
        GameObject joint2Obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        joint2Obj.name = "Bosshead_ArmCrossbar";
        joint2Obj.transform.SetParent(retortStand.transform);
        joint2Obj.transform.localPosition = new Vector3(0f, 1.25f, 0f);
        joint2Obj.transform.localScale = new Vector3(0.04f, 0.04f, 0.04f);
        joint2Obj.GetComponent<MeshRenderer>().sharedMaterial = retortMetalMat;
        DestroyImmediate(joint2Obj.GetComponent<Collider>());

        // Crossbar
        GameObject crossbarObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        crossbarObj.name = "Crossbar";
        crossbarObj.transform.SetParent(retortStand.transform);
        crossbarObj.transform.localPosition = new Vector3(0f, 1.25f, 0f);
        crossbarObj.transform.localScale = new Vector3(0.02f, 0.25f, 0.02f);
        crossbarObj.transform.localEulerAngles = new Vector3(0f, 0f, 90f);
        crossbarObj.GetComponent<MeshRenderer>().sharedMaterial = retortRodMat;
        DestroyImmediate(crossbarObj.GetComponent<Collider>());

        // Anode Clamp Ring (Left)
        GameObject anodeClamp = new GameObject("AnodeClamp");
        anodeClamp.transform.SetParent(retortStand.transform);
        anodeClamp.transform.localPosition = new Vector3(-0.25f, 1.25f, 0f);
        anodeClamp.transform.localRotation = Quaternion.identity;
        MeshFilter filterA = anodeClamp.AddComponent<MeshFilter>();
        filterA.sharedMesh = GenerateRingMesh(0.055f, 0.068f, 0.04f, 24);
        MeshRenderer rendererA = anodeClamp.AddComponent<MeshRenderer>();
        rendererA.sharedMaterial = retortMetalMat;

        // Cathode Clamp Ring (Right)
        GameObject cathodeClamp = new GameObject("CathodeClamp");
        cathodeClamp.transform.SetParent(retortStand.transform);
        cathodeClamp.transform.localPosition = new Vector3(0.25f, 1.25f, 0f);
        cathodeClamp.transform.localRotation = Quaternion.identity;
        MeshFilter filterC = cathodeClamp.AddComponent<MeshFilter>();
        filterC.sharedMesh = filterA.sharedMesh;
        MeshRenderer rendererC = cathodeClamp.AddComponent<MeshRenderer>();
        rendererC.sharedMaterial = retortMetalMat;

        // 6.8 Hologram Data Board (Floating Monitor Panel)
        // Darker, more opaque background for better text contrast
        Material hologramGlassMat = GetOrCreateMaterial("HologramGlassMat_Lab2", new Color(0f, 0.15f, 0.3f, 0.85f), true);
        Material hologramBorderMat = GetOrCreateMaterial("HologramBorderMat_Lab2", new Color(0f, 0.9f, 1f, 1f), false);

        GameObject hologramBoard = GameObject.Find("HologramDataBoard");
        if (hologramBoard == null)
        {
            hologramBoard = new GameObject("HologramDataBoard");
        }
        hologramBoard.transform.position = experimentOffset + new Vector3(-1.518f, 1.446f, -0.3f);
        hologramBoard.transform.rotation = Quaternion.Euler(354.742f, 325.394f, 352.063f); // Saved customized rotation
        hologramBoard.transform.localScale = new Vector3(0.617f, 0.617f, 0.617f); // Custom scaled down
        
        // Ensure it always faces the camera
        if (hologramBoard.GetComponent<HologramBillboard>() == null)
        {
            hologramBoard.AddComponent<HologramBillboard>();
        }

        // Transparent Glass Plate
        GameObject glassPlate = GameObject.Find("HologramGlassPlate");
        if (glassPlate != null) DestroyImmediate(glassPlate);
        glassPlate = GameObject.CreatePrimitive(PrimitiveType.Cube);
        glassPlate.name = "HologramGlassPlate";
        glassPlate.transform.SetParent(hologramBoard.transform);
        glassPlate.transform.localPosition = Vector3.zero;
        glassPlate.transform.localRotation = Quaternion.identity;
        glassPlate.transform.localScale = new Vector3(0.85f, 0.55f, 0.01f);
        glassPlate.GetComponent<MeshRenderer>().sharedMaterial = hologramGlassMat;
        DestroyImmediate(glassPlate.GetComponent<Collider>()); // Avoid blocking clicks

        // Borders
        float halfW = 0.85f * 0.5f;
        float halfH = 0.55f * 0.5f;
        float thick = 0.015f;

        // Top Border
        GameObject borderTop = GameObject.CreatePrimitive(PrimitiveType.Cube);
        borderTop.name = "BorderTop";
        borderTop.transform.SetParent(hologramBoard.transform);
        borderTop.transform.localPosition = new Vector3(0f, halfH + thick * 0.5f, 0f);
        borderTop.transform.localRotation = Quaternion.identity;
        borderTop.transform.localScale = new Vector3(0.85f + thick * 2f, thick, thick);
        borderTop.GetComponent<MeshRenderer>().sharedMaterial = hologramBorderMat;
        DestroyImmediate(borderTop.GetComponent<Collider>());

        // Bottom Border
        GameObject borderBottom = GameObject.CreatePrimitive(PrimitiveType.Cube);
        borderBottom.name = "BorderBottom";
        borderBottom.transform.SetParent(hologramBoard.transform);
        borderBottom.transform.localPosition = new Vector3(0f, -halfH - thick * 0.5f, 0f);
        borderBottom.transform.localRotation = Quaternion.identity;
        borderBottom.transform.localScale = new Vector3(0.85f + thick * 2f, thick, thick);
        borderBottom.GetComponent<MeshRenderer>().sharedMaterial = hologramBorderMat;
        DestroyImmediate(borderBottom.GetComponent<Collider>());

        // Left Border
        GameObject borderLeft = GameObject.CreatePrimitive(PrimitiveType.Cube);
        borderLeft.name = "BorderLeft";
        borderLeft.transform.SetParent(hologramBoard.transform);
        borderLeft.transform.localPosition = new Vector3(-halfW - thick * 0.5f, 0f, 0f);
        borderLeft.transform.localRotation = Quaternion.identity;
        borderLeft.transform.localScale = new Vector3(thick, 0.55f, thick);
        borderLeft.GetComponent<MeshRenderer>().sharedMaterial = hologramBorderMat;
        DestroyImmediate(borderLeft.GetComponent<Collider>());

        // Right Border
        GameObject borderRight = GameObject.CreatePrimitive(PrimitiveType.Cube);
        borderRight.name = "BorderRight";
        borderRight.transform.SetParent(hologramBoard.transform);
        borderRight.transform.localPosition = new Vector3(halfW + thick * 0.5f, 0f, 0f);
        borderRight.transform.localRotation = Quaternion.identity;
        borderRight.transform.localScale = new Vector3(thick, 0.55f, thick);
        borderRight.GetComponent<MeshRenderer>().sharedMaterial = hologramBorderMat;
        DestroyImmediate(borderRight.GetComponent<Collider>());

        // Hologram Text
        GameObject holoTextObj = GameObject.Find("HologramText");
        if (holoTextObj != null) DestroyImmediate(holoTextObj);
        holoTextObj = new GameObject("HologramText");
        holoTextObj.transform.SetParent(hologramBoard.transform);
        holoTextObj.transform.localPosition = new Vector3(0f, 0f, -0.015f); // Slightly in front
        holoTextObj.transform.localRotation = Quaternion.identity;
        holoTextObj.transform.localScale = new Vector3(0.12f, 0.12f, 0.12f);

        TextMesh holoTM = holoTextObj.AddComponent<TextMesh>();
        holoTM.text = "DATA GAS\nOksigen (O₂): 0.0%\nHidrogen (H₂): 0.0%";
        holoTM.characterSize = 0.1f;
        holoTM.fontSize = 72;
        holoTM.fontStyle = FontStyle.Bold;
        holoTM.anchor = TextAnchor.MiddleCenter;
        holoTM.alignment = TextAlignment.Center;
        holoTM.color = Color.white; // White text on dark cyan background is highly readable

        // === SECTION 6.9: ENVIRONMENTAL PROPS (Non-Interactive Atmosphere) ===

        // --- 6.9.1: Grid Cutting Mat ---
        Material gridMatMat = GetOrCreateMaterial("GridMatMat_Lab2", new Color(0.1f, 0.35f, 0.3f, 1f), false);
        Texture2D gridTex = CreateGridMatTexture();
        string gridTexPath = "Assets/Materials/Lab2_GridMatTex.png";
        byte[] pngData = gridTex.EncodeToPNG();
        System.IO.File.WriteAllBytes(gridTexPath, pngData);
        AssetDatabase.Refresh(); // Refresh so Unity detects the written PNG file
        AssetDatabase.ImportAsset(gridTexPath);
        Texture2D savedGridTex = AssetDatabase.LoadAssetAtPath<Texture2D>(gridTexPath);
        if (savedGridTex != null)
        {
            if (gridMatMat.HasProperty("_BaseMap"))
                gridMatMat.SetTexture("_BaseMap", savedGridTex);
            else if (gridMatMat.HasProperty("_MainTex"))
                gridMatMat.SetTexture("_MainTex", savedGridTex);
            EditorUtility.SetDirty(gridMatMat);
            AssetDatabase.SaveAssets(); // Save material changes to disk
        }

        GameObject gridMat = GameObject.CreatePrimitive(PrimitiveType.Cube);
        gridMat.name = "Lab2_GridMat";
        gridMat.transform.position = experimentOffset + new Vector3(-0.4f, 0.004f, 0.25f);
        gridMat.transform.localScale = new Vector3(2.5f, 0.008f, 1.4f);
        gridMat.GetComponent<MeshRenderer>().sharedMaterial = gridMatMat;
        DestroyImmediate(gridMat.GetComponent<Collider>());

        // --- 6.9.2: Open Scientific Notebook with Pen ---
        Material notebookCoverMat = GetOrCreateMaterial("NotebookCoverMat_Lab2", new Color(0.08f, 0.08f, 0.12f, 1f), false);
        if (notebookCoverMat.HasProperty("_Smoothness")) notebookCoverMat.SetFloat("_Smoothness", 0f);
        if (notebookCoverMat.HasProperty("_Glossiness")) notebookCoverMat.SetFloat("_Glossiness", 0f);
        if (notebookCoverMat.HasProperty("_Metallic")) notebookCoverMat.SetFloat("_Metallic", 0f);
        if (notebookCoverMat.HasProperty("_SpecularHighlights")) notebookCoverMat.SetFloat("_SpecularHighlights", 0f);
        if (notebookCoverMat.HasProperty("_SpecColor")) notebookCoverMat.SetColor("_SpecColor", new Color(0.04f, 0.04f, 0.04f, 1f));
        EditorUtility.SetDirty(notebookCoverMat);

        Material notebookPageMat = GetOrCreateMaterial("NotebookPageMat_Lab2", new Color(0.6f, 0.58f, 0.5f, 1f), false);
        if (notebookPageMat.HasProperty("_Smoothness")) notebookPageMat.SetFloat("_Smoothness", 0f);
        if (notebookPageMat.HasProperty("_Glossiness")) notebookPageMat.SetFloat("_Glossiness", 0f);
        if (notebookPageMat.HasProperty("_Metallic")) notebookPageMat.SetFloat("_Metallic", 0f);
        if (notebookPageMat.HasProperty("_SpecularHighlights")) notebookPageMat.SetFloat("_SpecularHighlights", 0f);
        if (notebookPageMat.HasProperty("_SpecColor")) notebookPageMat.SetColor("_SpecColor", new Color(0.04f, 0.04f, 0.04f, 1f));
        EditorUtility.SetDirty(notebookPageMat);
        AssetDatabase.SaveAssets();

        Material penBodyMat = GetOrCreateMaterial("PenBodyMat_Lab2", new Color(0.1f, 0.2f, 0.6f, 1f), false);
        Material penTipMat = GetOrCreateMaterial("PenTipMat_Lab2", new Color(0.2f, 0.2f, 0.2f, 1f), false);

        GameObject notebook = GameObject.Find("Lab2_Notebook");
        if (notebook == null)
        {
            notebook = new GameObject("Lab2_Notebook");
            notebook.transform.position = experimentOffset + new Vector3(0.850f, 0.005f, -0.600f);
            notebook.transform.rotation = Quaternion.Euler(0f, 345f, 0f);
        }

        // Cover (dark base under pages)
        GameObject cover = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cover.name = "NotebookCover";
        cover.transform.SetParent(notebook.transform);
        cover.transform.localPosition = new Vector3(0f, 0f, 0f);
        cover.transform.localScale = new Vector3(0.32f, 0.008f, 0.42f);
        cover.GetComponent<MeshRenderer>().sharedMaterial = notebookCoverMat;
        DestroyImmediate(cover.GetComponent<Collider>());

        // Left Page
        GameObject leftPage = GameObject.CreatePrimitive(PrimitiveType.Cube);
        leftPage.name = "LeftPage";
        leftPage.transform.SetParent(notebook.transform);
        leftPage.transform.localPosition = new Vector3(-0.08f, 0.005f, 0f);
        leftPage.transform.localScale = new Vector3(0.14f, 0.004f, 0.38f);
        leftPage.transform.localEulerAngles = new Vector3(0f, 0f, -2f);
        leftPage.GetComponent<MeshRenderer>().sharedMaterial = notebookPageMat;
        DestroyImmediate(leftPage.GetComponent<Collider>());

        // Left Page Text
        GameObject leftTextObj = new GameObject("LeftPageText");
        leftTextObj.transform.SetParent(notebook.transform);
        leftTextObj.transform.localPosition = new Vector3(-0.08f, 0.01f, 0f);
        leftTextObj.transform.localEulerAngles = new Vector3(90f, 0f, 0f);
        leftTextObj.transform.localScale = new Vector3(0.0022f, 0.0022f, 0.0022f);
        TextMesh leftTM = leftTextObj.AddComponent<TextMesh>();
        leftTM.text = "AMALI 2: ELEKTROLISIS\n\nTujuan:\nMengkaji elektrolisis air.\n\nHipotesis:\nGas hidrogen di katod,\ngas oksigen di anod.";
        leftTM.fontSize = 32;
        leftTM.fontStyle = FontStyle.Bold;
        leftTM.anchor = TextAnchor.MiddleCenter;
        leftTM.alignment = TextAlignment.Left;
        leftTM.color = Color.black;

        // Right Page
        GameObject rightPage = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rightPage.name = "RightPage";
        rightPage.transform.SetParent(notebook.transform);
        rightPage.transform.localPosition = new Vector3(0.08f, 0.005f, 0f);
        rightPage.transform.localScale = new Vector3(0.14f, 0.004f, 0.38f);
        rightPage.transform.localEulerAngles = new Vector3(0f, 0f, 2f);
        rightPage.GetComponent<MeshRenderer>().sharedMaterial = notebookPageMat;
        DestroyImmediate(rightPage.GetComponent<Collider>());

        // Right Page Text
        GameObject rightTextObj = new GameObject("RightPageText");
        rightTextObj.transform.SetParent(notebook.transform);
        rightTextObj.transform.localPosition = new Vector3(0.08f, 0.01f, 0f);
        rightTextObj.transform.localEulerAngles = new Vector3(90f, 0f, 0f);
        rightTextObj.transform.localScale = new Vector3(0.0022f, 0.0022f, 0.0022f);
        TextMesh rightTM = rightTextObj.AddComponent<TextMesh>();
        rightTM.text = "PERSAMAAN KIMIA\n\nAnod (+):\n2H2O -> O2 + 4H+ + 4e-\n\nKatod (-):\n4H2O + 4e- -> 2H2 + 4OH-\n\nNisbah H2 : O2 = 2 : 1";
        rightTM.fontSize = 32;
        rightTM.fontStyle = FontStyle.Bold;
        rightTM.anchor = TextAnchor.MiddleCenter;
        rightTM.alignment = TextAlignment.Left;
        rightTM.color = Color.black;

        // Pen lying on notebook
        GameObject pen = new GameObject("Pen");
        pen.transform.SetParent(notebook.transform);
        pen.transform.localPosition = new Vector3(0.06f, 0.012f, -0.12f);
        pen.transform.localEulerAngles = new Vector3(0f, 25f, 90f);

        GameObject penBody = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        penBody.name = "PenBody";
        penBody.transform.SetParent(pen.transform);
        penBody.transform.localPosition = Vector3.zero;
        penBody.transform.localScale = new Vector3(0.012f, 0.06f, 0.012f);
        penBody.GetComponent<MeshRenderer>().sharedMaterial = penBodyMat;
        DestroyImmediate(penBody.GetComponent<Collider>());

        GameObject penTip = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        penTip.name = "PenTip";
        penTip.transform.SetParent(pen.transform);
        penTip.transform.localPosition = new Vector3(0f, -0.065f, 0f);
        penTip.transform.localScale = new Vector3(0.006f, 0.012f, 0.006f);
        penTip.GetComponent<MeshRenderer>().sharedMaterial = penTipMat;
        DestroyImmediate(penTip.GetComponent<Collider>());

        // Pen clip (small metallic strip)
        Material penClipMat = GetOrCreateMaterial("PenClipMat_Lab2", new Color(0.75f, 0.75f, 0.8f, 1f), false);
        GameObject penClip = GameObject.CreatePrimitive(PrimitiveType.Cube);
        penClip.name = "PenClip";
        penClip.transform.SetParent(pen.transform);
        penClip.transform.localPosition = new Vector3(0.007f, 0.04f, 0f);
        penClip.transform.localScale = new Vector3(0.003f, 0.035f, 0.008f);
        penClip.GetComponent<MeshRenderer>().sharedMaterial = penClipMat;
        DestroyImmediate(penClip.GetComponent<Collider>());

        // --- 6.9.3: Chemical Bottles ---
        // Bottle 1: Dilute Sulfuric Acid (H2SO4) - Amber Glass, Right-Back
        CreateChemicalBottle("Lab2_ChemicalBottle_H2SO4",
            experimentOffset + new Vector3(0.950f, 0f, 0.700f),
            new Color(0.9f, 0.9f, 0.9f, 0.6f),
            "Asid Sulfurik Cair", "H2SO4", false);

        // Bottle 2: Distilled Water (H2O) - Clear Glass, Right-Back
        CreateChemicalBottle("Lab2_ChemicalBottle_H2O",
            experimentOffset + new Vector3(1.150f, 0f, 0.650f),
            new Color(0.95f, 0.98f, 1f, 0.5f),
            "Air Suling", "H2O", false);

        // Bottle 3: Chemical Indicator - Clear Glass, Left-Back (near battery)
        CreateChemicalBottle("Lab2_ChemicalBottle_Indicator",
            experimentOffset + new Vector3(-1.550f, 0f, 0.700f),
            new Color(0.8f, 0.6f, 0.2f, 0.8f),
            "Penunjuk Semesta", "pH Ind.", true);

        // 7. Attach Manager Script to a main object
        GameObject managerObj = GameObject.Find("ElectrolysisManager3D_Controller");
        if (managerObj == null) managerObj = new GameObject("ElectrolysisManager3D_Controller");
        
        ElectrolysisManager3D managerScript = managerObj.GetComponent<ElectrolysisManager3D>();
        if (managerScript == null) managerScript = managerObj.AddComponent<ElectrolysisManager3D>();
        
        managerScript.waterAnodePivot = anodeWaterPivot.transform;
        managerScript.waterCathodePivot = cathodeWaterPivot.transform;
        managerScript.hologramText = holoTM;

        Transform anodeBubbles = anodeElectrode.transform.Find("OxygenBubbles");
        if (anodeBubbles != null) managerScript.bubblesAnode = anodeBubbles.gameObject;
        
        Transform cathodeBubbles = cathodeElectrode.transform.Find("HydrogenBubbles");
        if (cathodeBubbles != null) managerScript.bubblesCathode = cathodeBubbles.gameObject;

        EditorUtility.SetDirty(managerScript);

        PhysicalSwitch3D ps = switch3D.GetComponent<PhysicalSwitch3D>();
        if (ps != null)
        {
            ps.manager = managerScript;
            ps.ammeter = ammeter.GetComponent<AmmeterNeedle3D>();
            EditorUtility.SetDirty(ps);
        }

        // Hide Old UI Button Robustly
        UnityEngine.UI.Button[] btns = Object.FindObjectsByType<UnityEngine.UI.Button>(FindObjectsInactive.Include);
        foreach (var b in btns)
        {
            UnityEngine.UI.Text t = b.GetComponentInChildren<UnityEngine.UI.Text>();
            TMPro.TextMeshProUGUI tmp = b.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            string txt = (t != null) ? t.text : ((tmp != null) ? tmp.text : "");
            if (txt.ToLower().Contains("switch") || txt.ToLower().Contains("turn on") || b.name.ToLower().Contains("switch"))
            {
                b.gameObject.SetActive(false);
            }
        }

        // 8. Add Camera Drag Rotation feature (Specific Button as requested)
        GameObject canvasObj = GameObject.Find("Canvas");
        if (canvasObj != null)
        {
            // Remove the old full-screen invisible drag area if it exists
            Transform oldDragArea = canvasObj.transform.Find("CameraDragArea");
            if (oldDragArea != null) Object.DestroyImmediate(oldDragArea.gameObject);
            
            Transform rotateBtn = canvasObj.transform.Find("RotateCameraButton");
            if (rotateBtn != null) Object.DestroyImmediate(rotateBtn.gameObject);
            
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
            rect.anchorMin = new Vector2(0, 0); // Bottom Left corner
            rect.anchorMax = new Vector2(0, 0);
            rect.pivot = new Vector2(0, 0);
            rect.anchoredPosition = new Vector2(20, 20); // Margin
            rect.sizeDelta = new Vector2(90, 90); // Square button
            
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
            
            // Attach the drag script to this specific button
            CameraDragArea dragScript = btnObj.AddComponent<CameraDragArea>();
            dragScript.rotationSpeed = 0.5f; // Slightly faster for button
            EditorUtility.SetDirty(dragScript);

            // 8.5 Construct 3D Tablet on the Desk (Diegetic UI)
            Transform oldPanel = canvasObj.transform.Find("CameraPOVPanel");
            if (oldPanel != null) Object.DestroyImmediate(oldPanel.gameObject);

            GameObject oldTablet = GameObject.Find("Lab2_3DTablet");
            if (oldTablet != null) Object.DestroyImmediate(oldTablet.gameObject);

            GameObject tabletObj = new GameObject("Lab2_3DTablet");
            GameObject mainCam = GameObject.Find("Main Camera");
            if (mainCam != null)
            {
                mainCam.GetComponent<Camera>().nearClipPlane = 0.05f;
                tabletObj.transform.SetParent(mainCam.transform, false);
                tabletObj.transform.localPosition = new Vector3(0.105f, -0.06f, 0.24f); // Anchored at bottom-right corner of camera view
                tabletObj.transform.localRotation = Quaternion.Euler(-70f, 0f, 15f); // Angled naturally facing the camera
                tabletObj.transform.localScale = new Vector3(0.21f, 0.21f, 0.21f); // Scaled down to not clip through scene objects
            }
            else
            {
                tabletObj.transform.position = experimentOffset + new Vector3(0.584f, 0.819f, -1.769f);
                tabletObj.transform.rotation = Quaternion.Euler(303.496f, 0f, 15f); // Lean it back          
            }

            // Casing
            GameObject casing = GameObject.CreatePrimitive(PrimitiveType.Cube);
            casing.name = "TabletCasing";
            casing.transform.SetParent(tabletObj.transform, false);
            casing.transform.localScale = new Vector3(0.46f, 0.012f, 0.34f);
            casing.GetComponent<MeshRenderer>().sharedMaterial = retortMetalMat; // Dark charcoal metallic casing
            Object.DestroyImmediate(casing.GetComponent<Collider>());

            // Screen
            GameObject screen = GameObject.CreatePrimitive(PrimitiveType.Cube);
            screen.name = "TabletScreen";
            screen.transform.SetParent(tabletObj.transform, false);
            screen.transform.localPosition = new Vector3(0f, 0.007f, 0f);
            screen.transform.localScale = new Vector3(0.43f, 0.002f, 0.31f);
            Material screenMat = GetOrCreateMaterial("TabletScreenMat", new Color(0.02f, 0.06f, 0.12f, 1f), false); // Cool sci-fi dark blue screen
            screen.GetComponent<MeshRenderer>().sharedMaterial = screenMat;
            Object.DestroyImmediate(screen.GetComponent<Collider>());

            // World Space Canvas
            GameObject tabletCanvasObj = new GameObject("TabletCanvas");
            tabletCanvasObj.transform.SetParent(tabletObj.transform, false);
            tabletCanvasObj.transform.localPosition = new Vector3(0f, 0.009f, 0f); // Position above screen mesh to prevent occlusion
            tabletCanvasObj.transform.localRotation = Quaternion.Euler(90f, 0f, 0f); // Lie flat facing up
            tabletCanvasObj.transform.localScale = new Vector3(0.001f, 0.001f, 0.001f); // Scale pixel units to meters (1px = 1mm)

            Canvas canvas = tabletCanvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = Camera.main;

            RectTransform canvasRect = tabletCanvasObj.GetComponent<RectTransform>();
            if (canvasRect != null)
            {
                canvasRect.sizeDelta = new Vector2(430f, 310f); // Set size to match physical screen dimensions
            }

            tabletCanvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            // UI Background Panel for Camera POV Controls
            GameObject panelObj = new GameObject("CameraPOVPanel");
            panelObj.transform.SetParent(tabletCanvasObj.transform, false);
            
            UnityEngine.UI.Image panelImg = panelObj.AddComponent<UnityEngine.UI.Image>();
            panelImg.color = new Color(0.04f, 0.12f, 0.22f, 0.85f); // Semi-transparent glassmorphic blue
            
            UnityEngine.UI.Outline panelOutline = panelObj.AddComponent<UnityEngine.UI.Outline>();
            panelOutline.effectColor = new Color(0.15f, 0.6f, 0.95f, 0.5f);
            panelOutline.effectDistance = new Vector2(1.5f, 1.5f);
            
            RectTransform panelRect = panelObj.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0f, 0.5f); // Anchor middle-left
            panelRect.anchorMax = new Vector2(0f, 0.5f);
            panelRect.pivot = new Vector2(0f, 0.5f);
            panelRect.anchoredPosition = new Vector2(10f, 0f);
            panelRect.sizeDelta = new Vector2(180f, 280f);
            
            string[] views = { "< BACK", "Ammeter View", "Test Tubes View", "Textbook View" };
            string[] buttonNames = { "Btn_DefaultView", "Btn_AmmeterView", "Btn_TestTubesView", "Btn_TextbookView" };
            
            for (int i = 0; i < views.Length; i++)
            {
                string viewName = views[i];
                string btnName = buttonNames[i];
                
                GameObject btn = new GameObject(btnName);
                btn.transform.SetParent(panelObj.transform, false);
                
                UnityEngine.UI.Image btnImg = btn.AddComponent<UnityEngine.UI.Image>();
                btnImg.color = (i == 0) ? new Color(0.85f, 0.2f, 0.2f, 0.85f) : new Color(0.1f, 0.45f, 0.85f, 0.85f); // Red for BACK, Blue for others
                
                UnityEngine.UI.Button button = btn.AddComponent<UnityEngine.UI.Button>();
                
                UnityEngine.UI.ColorBlock cb = button.colors;
                if (i == 0)
                {
                    cb.normalColor = new Color(0.85f, 0.2f, 0.2f, 0.85f);
                    cb.highlightedColor = new Color(0.95f, 0.35f, 0.35f, 1f);
                    cb.pressedColor = new Color(0.65f, 0.1f, 0.1f, 1f);
                }
                else
                {
                    cb.normalColor = new Color(0.1f, 0.45f, 0.85f, 0.85f);
                    cb.highlightedColor = new Color(0.15f, 0.55f, 0.95f, 1f);
                    cb.pressedColor = new Color(0.05f, 0.35f, 0.75f, 1f);
                }
                button.colors = cb;
                
                RectTransform btnRect = btn.GetComponent<RectTransform>();
                btnRect.anchorMin = new Vector2(0.5f, 1f); // Anchor top-center
                btnRect.anchorMax = new Vector2(0.5f, 1f);
                btnRect.pivot = new Vector2(0.5f, 1f);
                btnRect.anchoredPosition = new Vector2(0f, -8 - i * 45);
                btnRect.sizeDelta = new Vector2(150f, 38f);
                
                GameObject btnTextObj = new GameObject("Text");
                btnTextObj.transform.SetParent(btn.transform, false);
                UnityEngine.UI.Text btnTxt = btnTextObj.AddComponent<UnityEngine.UI.Text>();
                btnTxt.text = viewName;
                btnTxt.alignment = TextAnchor.MiddleCenter;
                btnTxt.color = Color.white;
                btnTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                btnTxt.fontSize = 15;
                btnTxt.fontStyle = FontStyle.Bold;
                btnTxt.raycastTarget = false;
                
                RectTransform textRect2 = btnTextObj.GetComponent<RectTransform>();
                textRect2.anchorMin = Vector2.zero;
                textRect2.anchorMax = Vector2.one;
                textRect2.offsetMin = Vector2.zero;
                textRect2.offsetMax = Vector2.zero;
            }

            // Right Side Header (Title)
            GameObject titleObj = new GameObject("TabletTitle");
            titleObj.transform.SetParent(tabletCanvasObj.transform, false);
            UnityEngine.UI.Text titleTxt = titleObj.AddComponent<UnityEngine.UI.Text>();
            titleTxt.text = "E-LAB DATA PAD";
            titleTxt.alignment = TextAnchor.MiddleCenter;
            titleTxt.color = new Color(0.15f, 0.75f, 1f, 1f); // Electric cyan
            titleTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            titleTxt.fontSize = 18;
            titleTxt.fontStyle = FontStyle.Bold;
            
            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(1f, 1f); // Top Right
            titleRect.anchorMax = new Vector2(1f, 1f);
            titleRect.pivot = new Vector2(1f, 1f);
            titleRect.anchoredPosition = new Vector2(-15f, -15f);
            titleRect.sizeDelta = new Vector2(200f, 30f);

            // Right Side Readout Panel
            GameObject statusObj = new GameObject("StatusPanel");
            statusObj.transform.SetParent(tabletCanvasObj.transform, false);
            
            UnityEngine.UI.Image statusImg = statusObj.AddComponent<UnityEngine.UI.Image>();
            statusImg.color = new Color(0.03f, 0.08f, 0.16f, 0.9f);
            
            UnityEngine.UI.Outline statusOutline = statusObj.AddComponent<UnityEngine.UI.Outline>();
            statusOutline.effectColor = new Color(0.15f, 0.6f, 0.95f, 0.3f);
            statusOutline.effectDistance = new Vector2(1f, 1f);

            RectTransform statusRect = statusObj.GetComponent<RectTransform>();
            statusRect.anchorMin = new Vector2(1f, 0.5f); // Middle Right
            statusRect.anchorMax = new Vector2(1f, 0.5f);
            statusRect.pivot = new Vector2(1f, 0.5f);
            statusRect.anchoredPosition = new Vector2(-15f, 20f);
            statusRect.sizeDelta = new Vector2(200f, 110f);

            GameObject statusTextObj = new GameObject("Text");
            statusTextObj.transform.SetParent(statusObj.transform, false);
            UnityEngine.UI.Text statusTxt = statusTextObj.AddComponent<UnityEngine.UI.Text>();
            statusTxt.text = "SISTEM: AKTIF\nKUASA: BATERI 9V\nSAMBUNGAN: OK\nARUS: BERPULSAR\nVOLTAN: 9.0 V";
            statusTxt.alignment = TextAnchor.MiddleLeft;
            statusTxt.color = new Color(0.7f, 0.85f, 0.95f, 0.9f);
            statusTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            statusTxt.fontSize = 13;
            statusTxt.fontStyle = FontStyle.Normal;
            statusTxt.lineSpacing = 1.2f;

            RectTransform statusTextRect = statusTextObj.GetComponent<RectTransform>();
            statusTextRect.anchorMin = Vector2.zero;
            statusTextRect.anchorMax = Vector2.one;
            statusTextRect.offsetMin = new Vector2(12f, 10f);
            statusTextRect.offsetMax = new Vector2(-12f, -10f);

            // Right Side Reset Button
            GameObject resetBtnObj = new GameObject("Btn_ResetExperiment");
            resetBtnObj.transform.SetParent(tabletCanvasObj.transform, false);
            
            UnityEngine.UI.Image resetImg = resetBtnObj.AddComponent<UnityEngine.UI.Image>();
            resetImg.color = new Color(0.7f, 0.15f, 0.15f, 0.9f); // Modern flat red button

            UnityEngine.UI.Button resetBtn = resetBtnObj.AddComponent<UnityEngine.UI.Button>();
            UnityEngine.UI.ColorBlock rcb = resetBtn.colors;
            rcb.normalColor = new Color(0.7f, 0.15f, 0.15f, 0.9f);
            rcb.highlightedColor = new Color(0.85f, 0.2f, 0.2f, 1f);
            rcb.pressedColor = new Color(0.5f, 0.1f, 0.1f, 1f);
            resetBtn.colors = rcb;

            UnityEngine.UI.Outline resetOutline = resetBtnObj.AddComponent<UnityEngine.UI.Outline>();
            resetOutline.effectColor = new Color(0.9f, 0.3f, 0.3f, 0.5f);
            resetOutline.effectDistance = new Vector2(1f, 1f);

            RectTransform resetRect = resetBtnObj.GetComponent<RectTransform>();
            resetRect.anchorMin = new Vector2(1f, 0f); // Bottom Right
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
            resetTextRect.offsetMin = Vector2.zero;
            resetTextRect.offsetMax = Vector2.zero;
        }

        // Auto-link UI Drag and Drop for 3D
        GasLabelDragUI[] labels = Object.FindObjectsByType<GasLabelDragUI>(FindObjectsInactive.Include);
        foreach (var label in labels)
        {
            var txt = label.GetComponentInChildren<UnityEngine.UI.Text>();
            var tmp = label.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            string labelText = tmp != null ? tmp.text : (txt != null ? txt.text : "");

            if (labelText.ToLower().Contains("oksigen") || labelText.ToLower().Contains("oxygen") || label.name.ToLower().Contains("oxygen") || label.name.ToLower().Contains("oksigen"))
            {
                label.target3DCollider = anodeTube.GetComponent<Collider>();
                label.electrolysisManager3D = managerScript;
            }
            else if (labelText.ToLower().Contains("hidrogen") || labelText.ToLower().Contains("hydrogen") || label.name.ToLower().Contains("hydrogen") || label.name.ToLower().Contains("hidrogen"))
            {
                label.target3DCollider = cathodeTube.GetComponent<Collider>();
                label.electrolysisManager3D = managerScript;
            }
        }

        // Note: The user will need to assign the UI Success Panel manually via Inspector if desired.
        
        // Setup the real-time 3D graph board on the table
        SetupRealTimeGraph(experimentOffset);

        // 4. Modernize UI Elements
        ModernizeUI();

        // Final Polish: Post-Processing Cinematic Effects
        SetupPostProcessing();

        // Setup Tooltips for Lab Equipment
        SetupTooltips();

        // Setup Universal Tutorial System
        SetupTutorialOverlay();

        // Mark active scene dirty and save it so changes persist in Play Mode
        var activeScene = EditorSceneManager.GetActiveScene();
        if (KidFriendlySceneTheme.Includes(activeScene)) KidFriendlySceneColourSetup.Apply(activeScene, false);
        Lab2VoiceSetup.Apply(activeScene);
        EditorSceneManager.MarkSceneDirty(activeScene);
        EditorSceneManager.SaveScene(activeScene);

        Debug.Log("3D Lab 2 Generated Successfully! Environment and camera positioned.");
    }

    private static void SetupTutorialOverlay()
    {
        GameObject existingTut = GameObject.Find("Lab2TutorialManager");
        if (existingTut != null) DestroyImmediate(existingTut);

        GameObject tutorialManagerObj = new GameObject("Lab2TutorialManager");
        tutorialManagerObj.AddComponent<Lab2TutorialManager>();
    }

    private static void SetupTooltips()
    {
        AddTooltip("Beaker_3D", "Bikar");
        AddTooltip("TestTube_Anode (Oxygen)", "Tabung Uji Anod");
        AddTooltip("TestTube_Cathode (Hydrogen)", "Tabung Uji Katod");
        AddTooltip("Battery_9V_3D", "Bateri 9V");
        AddTooltip("Ammeter_3D", "Ammeter");
        AddTooltip("PhysicalSwitch_3D", "Suis");
        AddTooltip("RetortStand_3D", "Kaki Retort");
        AddTooltip("Lab2_ChemicalBottle_H2SO4", "Botol Asid Sulfurik");
        AddTooltip("Lab2_ChemicalBottle_H2O", "Botol Air Suling");
        AddTooltip("Lab2_ChemicalBottle_Indicator", "Botol Penunjuk Semesta");
        AddTooltip("Lab2_Notebook", "Buku Laporan");
        AddTooltip("Lab2_RealTimeGraph", "Graf Arus");
        AddTooltip("HologramDataBoard", "Skrin Data Hologram");
    }

    private static void AddTooltip(string objectName, string tooltipText)
    {
        GameObject obj = GameObject.Find(objectName);
        if (obj != null)
        {
            // Ensure object has a collider
            if (obj.GetComponent<Collider>() == null)
            {
                if (obj.GetComponent<MeshFilter>() != null || objectName == "RetortStand_3D")
                {
                    obj.AddComponent<BoxCollider>();
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
    }

    private static void ModernizeUI()
    {
        UnityEngine.UI.Image[] images = Object.FindObjectsByType<UnityEngine.UI.Image>(FindObjectsInactive.Include);
        foreach(var img in images) 
        {
            if (img.gameObject.name.Contains("Button") || img.gameObject.name.Contains("Btn") || img.gameObject.name.Contains("View") || img.gameObject.name.Contains("Rotate")) 
            {
                var color = img.color;
                if (color.b > 0.5f && color.r < 0.5f) { // If it's blue
                    img.color = new Color(0.1f, 0.4f, 0.9f, 0.6f); // Semi-transparent blue
                    var outline = img.gameObject.GetComponent<UnityEngine.UI.Outline>();
                    if (outline == null) outline = img.gameObject.AddComponent<UnityEngine.UI.Outline>();
                    outline.effectColor = new Color(0.3f, 0.8f, 1f, 0.8f);
                    outline.effectDistance = new Vector2(2, -2);
                }
            } 
            else if (img.gameObject.name.Contains("Panel") || img.gameObject.name.Contains("Background")) 
            {
                if (img.color.a > 0.9f) {
                    var c = img.color;
                    c.a = 0.7f;
                    img.color = c;
                    var outline = img.gameObject.GetComponent<UnityEngine.UI.Outline>();
                    if (outline == null) outline = img.gameObject.AddComponent<UnityEngine.UI.Outline>();
                    outline.effectColor = new Color(0.2f, 0.5f, 0.8f, 0.5f);
                    outline.effectDistance = new Vector2(3, -3);
                }
            }
        }
    }

    private static void SetupPostProcessing()
    {
        // Ensure there is a Global Volume in the scene
        GameObject volumeObj = GameObject.Find("Global_PostProcessing");
        if (volumeObj == null)
        {
            volumeObj = new GameObject("Global_PostProcessing");
        }
        
        Volume volume = volumeObj.GetComponent<Volume>();
        if (volume == null)
        {
            volume = volumeObj.AddComponent<Volume>();
            volume.isGlobal = true;
        }

        // Create or Load the Volume Profile as a persistent asset file in the project
        string assetPath = "Assets/Settings/Cinematic_Lab_Profile.asset";
        if (!System.IO.Directory.Exists("Assets/Settings"))
        {
            System.IO.Directory.CreateDirectory("Assets/Settings");
        }

        VolumeProfile profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(assetPath);
        if (profile != null)
        {
            AssetDatabase.DeleteAsset(assetPath);
        }

        profile = ScriptableObject.CreateInstance<VolumeProfile>();
        profile.name = "Cinematic_Lab_Profile";
        AssetDatabase.CreateAsset(profile, assetPath);

        // 1. Bloom (Cyber glow effects matching Lab 3 and 4)
        Bloom bloom = profile.Add<Bloom>(true);
        if (bloom != null)
        {
            bloom.active = true;
            bloom.intensity.Override(0.8f);
            bloom.threshold.Override(1.1f);
            bloom.scatter.Override(0.5f);
            bloom.tint.Override(new Color(0.6f, 0.85f, 1f));
            AssetDatabase.AddObjectToAsset(bloom, profile);
        }

        // 2. Color Adjustments (Vibrant Sci-Fi style matching Lab 3 and 4)
        ColorAdjustments colorAdj = profile.Add<ColorAdjustments>(true);
        if (colorAdj != null)
        {
            colorAdj.active = true;
            colorAdj.postExposure.Override(0.15f); // Reduced from 1.2f to prevent overexposure
            colorAdj.contrast.Override(15f);
            colorAdj.saturation.Override(25f);
            AssetDatabase.AddObjectToAsset(colorAdj, profile);
        }

        // 3. Tone mapping (ACES filmic tone mapping matching Lab 3 and 4)
        Tonemapping tonemapping = profile.Add<Tonemapping>(true);
        if (tonemapping != null)
        {
            tonemapping.active = true;
            tonemapping.mode.Override(TonemappingMode.ACES);
            AssetDatabase.AddObjectToAsset(tonemapping, profile);
        }

        AssetDatabase.SaveAssets();
        volume.sharedProfile = profile;

        // Ensure the Main Camera has Post-Processing enabled
        Camera mainCam = Camera.main;
        if (mainCam == null)
        {
            GameObject camObj = GameObject.Find("Main Camera");
            if (camObj != null) mainCam = camObj.GetComponent<Camera>();
        }

        if (mainCam != null)
        {
            UniversalAdditionalCameraData camData = mainCam.GetComponent<UniversalAdditionalCameraData>();
            if (camData == null)
            {
                camData = mainCam.gameObject.AddComponent<UniversalAdditionalCameraData>();
            }
            camData.renderPostProcessing = true;
            camData.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
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
                    clone.name = rootObj.name; // Buang "(Clone)" suffix
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

    private static void DestroyIfExists(string objName)
    {
        GameObject[] allObjects = Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include);
        foreach (GameObject obj in allObjects)
        {
            if (obj && obj.name == objName)
            {
                try
                {
                    DestroyImmediate(obj);
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning("Failed to destroy " + objName + ": " + e.Message);
                }
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

    private static void SetupRealTimeGraph(Vector3 experimentOffset)
    {
        DestroyIfExists("Lab2_RealTimeGraph");

        GameObject graphObj = new GameObject("Lab2_RealTimeGraph");
        graphObj.transform.position = experimentOffset + new Vector3(-1.587f, -0.012f, -0.681f);
        graphObj.transform.rotation = Quaternion.Euler(0f, 340.731f, 0f);
        graphObj.transform.localScale = new Vector3(2.264f, 2.264f, 2.264f);

        RealTimeGraph3D graphScript = graphObj.AddComponent<RealTimeGraph3D>();

        // Limits setup matching the elevated screen Y position
        graphScript.yMin = 0.26f;
        graphScript.yMax = 0.44f;

        // Materials
        Material metalMat = GetOrCreateMaterial("RetortMetalMat_Lab2", new Color(0.15f, 0.15f, 0.15f, 1f), false);
        Material rodMat = GetOrCreateMaterial("RetortRodMat_Lab2", new Color(0.7f, 0.7f, 0.7f, 1f), false);
        Material screenMat = GetOrCreateMaterial("GraphScreenMat_Lab2", new Color(0.05f, 0.15f, 0.3f, 0.9f), true);
        screenMat.EnableKeyword("_EMISSION");
        screenMat.SetColor("_EmissionColor", new Color(0.03f, 0.1f, 0.22f));
        screenMat.renderQueue = 3050; // Semi-transparent overlay

        // 1. Base Stand (flat on the table surface)
        GameObject baseStand = GameObject.CreatePrimitive(PrimitiveType.Cube);
        baseStand.name = "BaseStand";
        baseStand.transform.SetParent(graphObj.transform, false);
        baseStand.transform.localPosition = new Vector3(0f, 0.006f, 0f);
        baseStand.transform.localScale = new Vector3(0.18f, 0.012f, 0.12f);
        baseStand.GetComponent<MeshRenderer>().sharedMaterial = metalMat;
        Object.DestroyImmediate(baseStand.GetComponent<Collider>());

        // 1.5 Vertical Stand Column (connecting base to elevated monitor)
        GameObject neck = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        neck.name = "Neck";
        neck.transform.SetParent(graphObj.transform, false);
        neck.transform.localPosition = new Vector3(0f, 0.13f, 0.02f);
        neck.transform.localScale = new Vector3(0.025f, 0.12f, 0.025f); // 0.24m tall (cylinder has default height 2.0)
        neck.GetComponent<MeshRenderer>().sharedMaterial = rodMat;
        Object.DestroyImmediate(neck.GetComponent<Collider>());

        // 2. Casing Backboard
        GameObject backboard = GameObject.CreatePrimitive(PrimitiveType.Cube);
        backboard.name = "Backboard";
        backboard.transform.SetParent(graphObj.transform, false);
        backboard.transform.localPosition = new Vector3(0f, 0.35f, 0f);
        backboard.transform.localScale = new Vector3(0.36f, 0.26f, 0.015f);
        backboard.GetComponent<MeshRenderer>().sharedMaterial = metalMat;
        Object.DestroyImmediate(backboard.GetComponent<Collider>());

        // 3. Screen
        GameObject screen = GameObject.CreatePrimitive(PrimitiveType.Cube);
        screen.name = "Screen";
        screen.transform.SetParent(graphObj.transform, false);
        screen.transform.localPosition = new Vector3(0f, 0.35f, -0.006f);
        screen.transform.localScale = new Vector3(0.33f, 0.23f, 0.004f);
        screen.GetComponent<MeshRenderer>().sharedMaterial = screenMat;
        Object.DestroyImmediate(screen.GetComponent<Collider>());

        // 4. Axis lines (represented as thin cubes)
        GameObject xAxis = GameObject.CreatePrimitive(PrimitiveType.Cube);
        xAxis.name = "X_Axis";
        xAxis.transform.SetParent(graphObj.transform, false);
        xAxis.transform.localPosition = new Vector3(0.01f, 0.26f, -0.009f);
        xAxis.transform.localScale = new Vector3(0.28f, 0.002f, 0.002f);
        xAxis.GetComponent<MeshRenderer>().sharedMaterial = rodMat;
        Object.DestroyImmediate(xAxis.GetComponent<Collider>());

        GameObject yAxis = GameObject.CreatePrimitive(PrimitiveType.Cube);
        yAxis.name = "Y_Axis";
        yAxis.transform.SetParent(graphObj.transform, false);
        yAxis.transform.localPosition = new Vector3(-0.13f, 0.35f, -0.009f);
        yAxis.transform.localScale = new Vector3(0.002f, 0.20f, 0.002f);
        yAxis.GetComponent<MeshRenderer>().sharedMaterial = rodMat;
        Object.DestroyImmediate(yAxis.GetComponent<Collider>());

        // 5. Title & Label texts
        CreateGraphText("ANALISIS DATA GAS", graphObj, new Vector3(0f, 0.44f, -0.01f), 0.0018f, 42, new Color(0.15f, 0.75f, 1f, 1f));
        CreateGraphText("Isipadu Gas (cm3)", graphObj, new Vector3(-0.18f, 0.35f, -0.01f), 0.0014f, 32, Color.white, TextAnchor.MiddleCenter, 90f);
        CreateGraphText("Masa Eksperimen (s)", graphObj, new Vector3(0.01f, 0.20f, -0.01f), 0.0014f, 32, Color.white);

        // Ticks
        CreateGraphText("10.0", graphObj, new Vector3(-0.145f, 0.44f, -0.01f), 0.0012f, 26, Color.gray, TextAnchor.MiddleRight);
        CreateGraphText("5.0", graphObj, new Vector3(-0.145f, 0.35f, -0.01f), 0.0012f, 26, Color.gray, TextAnchor.MiddleRight);
        CreateGraphText("0.0", graphObj, new Vector3(-0.145f, 0.26f, -0.01f), 0.0012f, 26, Color.gray, TextAnchor.MiddleRight);

        CreateGraphText("0.0", graphObj, new Vector3(-0.13f, 0.235f, -0.01f), 0.0012f, 26, Color.gray);
        CreateGraphText("4.0", graphObj, new Vector3(0f, 0.235f, -0.01f), 0.0012f, 26, Color.gray);
        CreateGraphText("8.0", graphObj, new Vector3(0.13f, 0.235f, -0.01f), 0.0012f, 26, Color.gray);

        // Legend
        CreateGraphText("H2 (Cyan)", graphObj, new Vector3(0.06f, 0.44f, -0.01f), 0.0012f, 26, new Color(0.2f, 0.8f, 1f, 1f), TextAnchor.MiddleLeft);
        CreateGraphText("O2 (Red)", graphObj, new Vector3(0.06f, 0.41f, -0.01f), 0.0012f, 26, new Color(1f, 0.2f, 0.2f, 1f), TextAnchor.MiddleLeft);

        // 6. Glowing Line Renderers (Oxygen and Hydrogen)
        // IMPORTANT: LineRenderers require particle-type shaders. URP/Unlit does NOT work.
        Shader lineShader = Shader.Find("Sprites/Default");
        if (lineShader == null) lineShader = Shader.Find("Particles/Standard Unlit");

        Material lineO2Mat = new Material(lineShader);
        lineO2Mat.name = "GraphLineO2Mat";
        lineO2Mat.color = new Color(1f, 0.2f, 0.2f, 1f);

        Material lineH2Mat = new Material(lineShader);
        lineH2Mat.name = "GraphLineH2Mat";
        lineH2Mat.color = new Color(0.2f, 0.8f, 1f, 1f);

        GameObject o2LineObj = new GameObject("Line_Oxygen");
        o2LineObj.transform.SetParent(graphObj.transform, false);
        LineRenderer lrO2 = o2LineObj.AddComponent<LineRenderer>();
        lrO2.sharedMaterial = lineO2Mat;
        lrO2.useWorldSpace = false;
        lrO2.positionCount = 0;
        lrO2.startWidth = 0.006f;
        lrO2.endWidth = 0.006f;
        lrO2.startColor = Color.white;
        lrO2.endColor = Color.white;
        lrO2.numCornerVertices = 4;
        lrO2.numCapVertices = 4;
        graphScript.lineOxygen = lrO2;

        GameObject h2LineObj = new GameObject("Line_Hydrogen");
        h2LineObj.transform.SetParent(graphObj.transform, false);
        LineRenderer lrH2 = h2LineObj.AddComponent<LineRenderer>();
        lrH2.sharedMaterial = lineH2Mat;
        lrH2.useWorldSpace = false;
        lrH2.positionCount = 0;
        lrH2.startWidth = 0.006f;
        lrH2.endWidth = 0.006f;
        lrH2.startColor = Color.white;
        lrH2.endColor = Color.white;
        lrH2.numCornerVertices = 4;
        lrH2.numCapVertices = 4;
        graphScript.lineHydrogen = lrH2;

        EditorUtility.SetDirty(graphScript);
    }

    private static void CreateGraphText(string text, GameObject parent, Vector3 localPos, float scale, int fontSize, Color color, TextAnchor anchor = TextAnchor.MiddleCenter, float rotZ = 0f)
    {
        GameObject txtObj = new GameObject("Label_" + text.Replace(" ", "_").Replace("(", "").Replace(")", ""));
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
            var mr = txtObj.GetComponent<MeshRenderer>();
            if (mr != null) mr.sharedMaterial = font.material;
        }
        tm.anchor = anchor;
        tm.alignment = TextAlignment.Center;
    }

    private static GameObject CreateTestTube(string name, Vector3 pos, Material mat)
    {
        GameObject tube = new GameObject(name);
        MeshFilter mf = tube.AddComponent<MeshFilter>();
        mf.sharedMesh = GenerateTestTubeMesh();
        MeshRenderer mr = tube.AddComponent<MeshRenderer>();
        mr.sharedMaterial = mat;
        tube.transform.position = pos;
        
        CapsuleCollider col = tube.AddComponent<CapsuleCollider>();
        col.radius = 0.5f;
        col.height = 2.0f;
        col.direction = 1; // 1 = Y-axis

        return tube;
    }

    private static void CreateBeakerOutline(GameObject beaker)
    {
        // Cleanup old outlines if regenerating
        Transform oldTop = beaker.transform.Find("TopOutline");
        if (oldTop != null) Object.DestroyImmediate(oldTop.gameObject);
        Transform oldBot = beaker.transform.Find("BotOutline");
        if (oldBot != null) Object.DestroyImmediate(oldBot.gameObject);
        for(int i=0; i<4; i++) {
            Transform oldV = beaker.transform.Find("VertOutline_" + i);
            if (oldV != null) Object.DestroyImmediate(oldV.gameObject);
        }

        // Top Ring
        GameObject topRing = new GameObject("TopOutline");
        topRing.transform.SetParent(beaker.transform, false);
        topRing.transform.localPosition = new Vector3(0, 1f, 0); 
        
        LineRenderer lrTop = topRing.AddComponent<LineRenderer>();
        lrTop.useWorldSpace = false;
        lrTop.loop = true;
        lrTop.startWidth = 0.015f;
        lrTop.endWidth = 0.015f;
        lrTop.material = new Material(Shader.Find("Sprites/Default"));
        lrTop.startColor = new Color(0.1f, 0.1f, 0.1f, 0.8f);
        lrTop.endColor = new Color(0.1f, 0.1f, 0.1f, 0.8f);
        
        int segments = 48;
        lrTop.positionCount = segments;
        for (int i = 0; i < segments; i++)
        {
            float angle = (i * Mathf.PI * 2f) / segments;
            lrTop.SetPosition(i, new Vector3(Mathf.Cos(angle) * 0.5f, 0, Mathf.Sin(angle) * 0.5f));
        }

        // Bottom Ring
        GameObject botRing = Object.Instantiate(topRing, beaker.transform);
        botRing.name = "BotOutline";
        botRing.transform.localPosition = new Vector3(0, -1f, 0);
        
        // 4 Vertical pillars
        for (int i=0; i<4; i++) {
            GameObject vLine = new GameObject("VertOutline_" + i);
            vLine.transform.SetParent(beaker.transform, false);
            vLine.transform.localPosition = Vector3.zero;
            LineRenderer lrV = vLine.AddComponent<LineRenderer>();
            lrV.useWorldSpace = false;
            lrV.startWidth = 0.015f;
            lrV.endWidth = 0.015f;
            lrV.material = lrTop.material;
            lrV.startColor = lrTop.startColor;
            lrV.endColor = lrTop.endColor;
            
            float angle = (i * Mathf.PI * 2f) / 4f + (Mathf.PI / 4f); 
            lrV.positionCount = 2;
            lrV.SetPosition(0, new Vector3(Mathf.Cos(angle)*0.5f, 1f, Mathf.Sin(angle)*0.5f));
            lrV.SetPosition(1, new Vector3(Mathf.Cos(angle)*0.5f, -1f, Mathf.Sin(angle)*0.5f));
        }
    }

    private static Mesh GenerateTestTubeMesh()
    {
        Mesh mesh = new Mesh();
        mesh.name = "TestTubeMesh";

        int segments = 32;
        int profileSteps = 12; // Smooth dome
        
        int totalProfilePoints = 3 + profileSteps;
        Vector2[] profile = new Vector2[totalProfilePoints];
        
        float height = 2f;
        float radius = 0.5f;
        float yBottom = -height / 2f;
        float yDomeStart = height / 2f - radius;
        
        // 0: Flared bottom rim
        profile[0] = new Vector2(radius * 1.3f, yBottom);
        // 1: Neck tapering in
        profile[1] = new Vector2(radius, yBottom + 0.15f);
        // 2: Straight body up to dome
        profile[2] = new Vector2(radius, yDomeStart);
        
        // 3..N: Dome arc
        for (int i = 0; i < profileSteps; i++)
        {
            float t = (float)(i + 1) / profileSteps;
            float angle = t * Mathf.PI / 2f; // 0 to 90 degrees
            float x = Mathf.Cos(angle) * radius;
            float y = yDomeStart + Mathf.Sin(angle) * radius;
            profile[3 + i] = new Vector2(x, y);
        }
        
        // Ensure exact center at top
        profile[totalProfilePoints - 1].x = 0f;

        Vector3[] vertices = new Vector3[totalProfilePoints * segments];
        
        for (int p = 0; p < totalProfilePoints; p++)
        {
            float pr = profile[p].x;
            float py = profile[p].y;
            
            for (int s = 0; s < segments; s++)
            {
                float angle = (float)s / segments * Mathf.PI * 2f;
                float px = Mathf.Sin(angle) * pr;
                float pz = Mathf.Cos(angle) * pr;
                vertices[p * segments + s] = new Vector3(px, py, pz);
            }
        }
        
        int numQuads = (totalProfilePoints - 1) * segments;
        int[] triangles = new int[numQuads * 6];
        int tIndex = 0;
        
        for (int p = 0; p < totalProfilePoints - 1; p++)
        {
            for (int s = 0; s < segments; s++)
            {
                int nextS = (s + 1) % segments;
                
                int v0 = p * segments + s;
                int v1 = p * segments + nextS;
                int v2 = (p + 1) * segments + s;
                int v3 = (p + 1) * segments + nextS;
                
                // Reversed winding order to ensure normals face outwards properly 
                // Since Unity uses clockwise winding order
                triangles[tIndex++] = v0;
                triangles[tIndex++] = v1;
                triangles[tIndex++] = v2;
                
                triangles[tIndex++] = v2;
                triangles[tIndex++] = v1;
                triangles[tIndex++] = v3;
            }
        }
        
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        return mesh;
    }

    private static void CreateCubicBezierWire(string name, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, Color color)
    {
        GameObject wire = GameObject.Find(name);
        if (wire == null) wire = new GameObject(name);
        
        LineRenderer lr = wire.GetComponent<LineRenderer>();
        if (lr == null) lr = wire.AddComponent<LineRenderer>();
        lr.startWidth = 0.02f;
        lr.endWidth = 0.02f;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = color;
        lr.endColor = color;

        // Attach and configure the WirePulseAnimator
        WirePulseAnimator animator = wire.GetComponent<WirePulseAnimator>();
        if (animator == null) animator = wire.AddComponent<WirePulseAnimator>();
        animator.baseColor = color;
        // Cyan glow pulse for negative (black) wire representing electron flow
        // Yellow/gold glow pulse for positive (red) wires representing conventional current
        animator.pulseColor = (color == Color.black) ? new Color(0f, 0.8f, 1f, 1f) : new Color(1f, 0.9f, 0.4f, 1f); 
        animator.speed = 1.8f;
        animator.pulseWidth = 0.22f;
        animator.SetActive(false);
        int segments = 25; // Smoother curve
        
        // Explicitly set the gradient to fix the white wire issue in Editor mode
        Gradient staticGrad = new Gradient();
        staticGrad.SetKeys(
            new GradientColorKey[] { new GradientColorKey(color, 0f), new GradientColorKey(color, 1f) },
            new GradientAlphaKey[] { new GradientAlphaKey(color.a, 0f), new GradientAlphaKey(color.a, 1f) }
        );
        lr.colorGradient = staticGrad;
        
        lr.positionCount = segments;
        for (int i = 0; i < segments; i++)
        {
            float t = i / (float)(segments - 1);
            float u = 1f - t;
            float tt = t * t;
            float uu = u * u;
            float uuu = uu * u;
            float ttt = tt * t;

            Vector3 pos = uuu * p0; // (1-t)^3 * P0
            pos += 3f * uu * t * p1; // 3(1-t)^2 * t * P1
            pos += 3f * u * tt * p2; // 3(1-t) * t^2 * P2
            pos += ttt * p3; // t^3 * P3

            lr.SetPosition(i, pos);
        }
    }

    private static Mesh GenerateOpenBeakerMesh()
    {
        Mesh mesh = new Mesh();
        mesh.name = "OpenBeakerMesh";

        int segments = 32;
        float radius = 0.5f; // Standard primitive cylinder radius
        float height = 2f;   // Standard primitive cylinder height
        float yBottom = -height / 2f;
        float yTop = height / 2f;

        Vector3[] vertices = new Vector3[segments * 2 + 1];
        int[] triangles = new int[segments * 6 + segments * 3];

        // Bottom center vertex
        vertices[0] = new Vector3(0, yBottom, 0);

        // Generate vertices
        for (int i = 0; i < segments; i++)
        {
            float angle = (float)i / segments * Mathf.PI * 2f;
            float x = Mathf.Sin(angle) * radius;
            float z = Mathf.Cos(angle) * radius;

            vertices[i + 1] = new Vector3(x, yBottom, z);           // Bottom edge
            vertices[i + 1 + segments] = new Vector3(x, yTop, z);   // Top edge
        }

        int tIndex = 0;

        // Bottom Cap Triangles
        for (int i = 0; i < segments; i++)
        {
            int next = (i + 1) % segments;
            triangles[tIndex++] = 0;
            triangles[tIndex++] = i + 1;
            triangles[tIndex++] = next + 1;
        }

        // Side Wall Triangles
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

    private static void CreateBubbleParticles(GameObject electrode, string name, float emissionRate, Color color)
    {
        Transform existing = electrode.transform.Find(name);
        if (existing != null) return; // Prevent duplicate particles

        GameObject bubbles = new GameObject(name);
        bubbles.transform.SetParent(electrode.transform, false);
        // Position at the center of the electrode local space (which is Y = 0)
        bubbles.transform.localPosition = Vector3.zero; 
        
        ParticleSystem ps = bubbles.AddComponent<ParticleSystem>();
        
        float startLifetime = name.Contains("Hydrogen") ? 4.5f : 5.0f;
        float startSize = name.Contains("Hydrogen") ? 0.024f : 0.036f; // Doubled size for visibility

        var main = ps.main;
        main.duration = 5f;
        main.loop = true;
        main.startLifetime = startLifetime;
        main.startSpeed = 0f; // Handled by Velocity over Lifetime for stable constant speed
        main.startSize = startSize;
        main.startColor = color;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.gravityModifier = 0f; // No gravity, constant rising speed
        
        var emission = ps.emission;
        emission.rateOverTime = emissionRate; 
        
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Cone; 
        shape.angle = 0f; // A cone with angle 0 acts exactly like a cylinder
        shape.radius = 0.04f; // Fits perfectly inside the 0.08f radius test tube
        shape.length = 0.6f; // Matches electrode length
        shape.rotation = new Vector3(90f, 0f, 0f); // Rotate cone to align along Y-axis
        
        // Velocity over Lifetime for constant rising speed and organic jiggle
        var velocityModule = ps.velocityOverLifetime;
        velocityModule.enabled = true;
        velocityModule.space = ParticleSystemSimulationSpace.World;
        velocityModule.x = new ParticleSystem.MinMaxCurve(-0.02f, 0.02f);
        velocityModule.y = new ParticleSystem.MinMaxCurve(0.08f, 0.15f); // Gentle rise
        velocityModule.z = new ParticleSystem.MinMaxCurve(-0.02f, 0.02f);

        // Color over Lifetime to fade out near the water level
        var colorModule = ps.colorOverLifetime;
        colorModule.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(color, 0.0f), new GradientColorKey(color, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(color.a, 0.0f), new GradientAlphaKey(color.a, 0.7f), new GradientAlphaKey(0.0f, 1.0f) }
        );
        colorModule.color = new ParticleSystem.MinMaxGradient(gradient);

        // Size over Lifetime to grow slightly as bubbles rise
        var sizeModule = ps.sizeOverLifetime;
        sizeModule.enabled = true;
        AnimationCurve curve = new AnimationCurve();
        curve.AddKey(0.0f, 0.6f);
        curve.AddKey(1.0f, 1.2f);
        sizeModule.size = new ParticleSystem.MinMaxCurve(1.0f, curve);
        
        ParticleSystemRenderer psr = bubbles.GetComponent<ParticleSystemRenderer>();
        Material particleMat = AssetDatabase.GetBuiltinExtraResource<Material>("Default-ParticleSystem.mat");
        if (particleMat == null) particleMat = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Particle.mat");
        if (particleMat == null) particleMat = new Material(Shader.Find("Sprites/Default"));
        psr.material = particleMat;
    }

    private static Material GetOrCreateMaterial(string name, Color color, bool isTransparent)
    {
        string path = "Assets/Materials/" + name + ".mat";
        
        // Ensure folder exists
        if (!AssetDatabase.IsValidFolder("Assets/Materials"))
        {
            AssetDatabase.CreateFolder("Assets", "Materials");
        }

        // Try URP Unlit for glass and water materials to ensure clear, glitch-free transparency, else Simple Lit
        Shader shader = null;
        if (name.Contains("Glass") || name.Contains("Water") || name.Contains("Line"))
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
            if (shader != null)
            {
                mat.shader = shader;
            }
            if (shader != null && shader.name.Contains("Universal Render Pipeline"))
            {
                mat.SetColor("_BaseColor", color);
                if (isTransparent)
                {
                    mat.SetFloat("_Surface", 1);
                    mat.SetInt("_Blend", 0);
                    mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    
                    int zWriteVal = 1; // Enable Z-Write to prevent background culling and sorting glitches
                    mat.SetInt("_ZWrite", zWriteVal);
                    
                    int cullVal = 2; // Cull Back (render front faces only) to avoid back-face sorting issues
                    mat.SetInt("_Cull", cullVal);

                    mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                    mat.DisableKeyword("_SURFACE_TYPE_OPAQUE");
                }
            }
            else
            {
                mat.color = color;
                if (isTransparent)
                {
                    int zWriteVal = 1;
                    mat.SetInt("_ZWrite", zWriteVal);
                    int cullVal = 2;
                    mat.SetInt("_Cull", cullVal);
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
                mat.SetFloat("_Surface", 1); // 1 = Transparent
                mat.SetInt("_Blend", 0); // 0 = Alpha
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                
                int zWriteVal = 1;
                mat.SetInt("_ZWrite", zWriteVal);
                
                int cullVal = 2;
                mat.SetInt("_Cull", cullVal); 
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
                
                int zWriteVal = 1;
                mat.SetInt("_ZWrite", zWriteVal);
                
                int cullVal = 2;
                mat.SetInt("_Cull", cullVal);
                
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.DisableKeyword("_ALPHABLEND_ON");
                mat.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.renderQueue = 3000;
            }
        }

        // Save the material to the project so it doesn't turn Magenta
        AssetDatabase.CreateAsset(mat, path);
        AssetDatabase.SaveAssets();
        return mat;
    }

    private static Mesh GenerateRingMesh(float innerRadius, float outerRadius, float height, int segments)
    {
        Mesh mesh = new Mesh();
        mesh.name = "RingMesh";

        int numVertices = segments * 4;
        Vector3[] vertices = new Vector3[numVertices];
        int[] triangles = new int[segments * 24]; // 8 quads = 24 triangles per segment

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

            // Vertices for segment i
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
            triangles[tIndex++] = curr + 2; // bOuter1
            triangles[tIndex++] = curr + 3; // tOuter1
            triangles[tIndex++] = next + 2; // bOuter2

            triangles[tIndex++] = next + 2; // bOuter2
            triangles[tIndex++] = curr + 3; // tOuter1
            triangles[tIndex++] = next + 3; // tOuter2

            // Inner wall
            triangles[tIndex++] = next + 0; // bInner2
            triangles[tIndex++] = curr + 1; // tInner1
            triangles[tIndex++] = curr + 0; // bInner1

            triangles[tIndex++] = next + 1; // tInner2
            triangles[tIndex++] = curr + 1; // tInner1
            triangles[tIndex++] = next + 0; // bInner2

            // Top face
            triangles[tIndex++] = curr + 1; // tInner1
            triangles[tIndex++] = next + 1; // tInner2
            triangles[tIndex++] = curr + 3; // tOuter1

            triangles[tIndex++] = curr + 3; // tOuter1
            triangles[tIndex++] = next + 1; // tInner2
            triangles[tIndex++] = next + 3; // tOuter2

            // Bottom face
            triangles[tIndex++] = curr + 0; // bInner1
            triangles[tIndex++] = curr + 2; // bOuter1
            triangles[tIndex++] = next + 0; // bInner2

            triangles[tIndex++] = next + 0; // bInner2
            triangles[tIndex++] = curr + 2; // bOuter1
            triangles[tIndex++] = next + 2; // bOuter2
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        return mesh;
    }

    private static Texture2D CreateGridMatTexture()
    {
        int width = 512;
        int height = 512;
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);

        // Base color: brighter teal-green
        Color baseColor = new Color(0.12f, 0.45f, 0.38f, 1f);
        Color fineGridColor = new Color(0.18f, 0.52f, 0.44f, 1f);
        Color thickGridColor = new Color(0.24f, 0.60f, 0.50f, 1f);
        Color borderColor = new Color(0.85f, 0.85f, 0.8f, 1f);

        // Fill base
        Color[] pixels = new Color[width * height];
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = baseColor;

        // Fine grid lines every 16 pixels
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (x % 16 == 0 || y % 16 == 0)
                    pixels[y * width + x] = fineGridColor;
            }
        }

        // Thick grid lines every 64 pixels
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (x % 64 < 2 || y % 64 < 2)
                    pixels[y * width + x] = thickGridColor;
            }
        }

        // Border (4px on each edge)
        int borderW = 4;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (x < borderW || x >= width - borderW || y < borderW || y >= height - borderW)
                    pixels[y * width + x] = borderColor;
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        return tex;
    }

    private static void CreateChemicalBottle(string name, Vector3 pos, Color liquidColor, string labelName, string formula, bool isAmberGlass)
    {
        // Materials
        Color glassColor = isAmberGlass
            ? new Color(0.55f, 0.35f, 0.1f, 0.35f)   // Amber brown glass
            : new Color(0.85f, 0.9f, 0.95f, 0.25f);   // Clear glass
        Material bottleGlassMat = GetOrCreateMaterial("BottleGlass_" + name, glassColor, true);
        Material bottleLiquidMat = GetOrCreateMaterial("BottleLiquid_" + name, liquidColor, true);
        Material bottleCapMat = GetOrCreateMaterial("BottleCap_" + name, new Color(0.15f, 0.15f, 0.15f, 1f), false);
        Material bottleLabelMat = GetOrCreateMaterial("BottleLabel_" + name, new Color(0.95f, 0.95f, 0.92f, 1f), false);

        // Container
        GameObject bottle = new GameObject(name);
        bottle.transform.position = pos;

        // Glass Body (cylinder)
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        body.name = "GlassBody";
        body.transform.SetParent(bottle.transform);
        body.transform.localPosition = new Vector3(0f, 0.12f, 0f);
        body.transform.localScale = new Vector3(0.1f, 0.12f, 0.1f);
        body.GetComponent<MeshRenderer>().sharedMaterial = bottleGlassMat;
        DestroyImmediate(body.GetComponent<Collider>());

        // Liquid inside (slightly smaller cylinder)
        GameObject liquid = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        liquid.name = "Liquid";
        liquid.transform.SetParent(bottle.transform);
        liquid.transform.localPosition = new Vector3(0f, 0.1f, 0f);
        liquid.transform.localScale = new Vector3(0.085f, 0.09f, 0.085f);
        liquid.GetComponent<MeshRenderer>().sharedMaterial = bottleLiquidMat;
        DestroyImmediate(liquid.GetComponent<Collider>());

        // Neck (thinner cylinder on top)
        GameObject neck = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        neck.name = "Neck";
        neck.transform.SetParent(bottle.transform);
        neck.transform.localPosition = new Vector3(0f, 0.26f, 0f);
        neck.transform.localScale = new Vector3(0.045f, 0.025f, 0.045f);
        neck.GetComponent<MeshRenderer>().sharedMaterial = bottleGlassMat;
        DestroyImmediate(neck.GetComponent<Collider>());

        // Cap
        GameObject cap = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cap.name = "Cap";
        cap.transform.SetParent(bottle.transform);
        cap.transform.localPosition = new Vector3(0f, 0.30f, 0f);
        cap.transform.localScale = new Vector3(0.05f, 0.015f, 0.05f);
        cap.GetComponent<MeshRenderer>().sharedMaterial = bottleCapMat;
        DestroyImmediate(cap.GetComponent<Collider>());

        // Label (thin cube on front of bottle)
        GameObject label = GameObject.CreatePrimitive(PrimitiveType.Cube);
        label.name = "Label";
        label.transform.SetParent(bottle.transform);
        label.transform.localPosition = new Vector3(0f, 0.12f, -0.053f);
        label.transform.localScale = new Vector3(0.07f, 0.08f, 0.002f);
        label.GetComponent<MeshRenderer>().sharedMaterial = bottleLabelMat;
        DestroyImmediate(label.GetComponent<Collider>());

        // Label Text (chemical name)
        GameObject labelTextObj = new GameObject("LabelText");
        labelTextObj.transform.SetParent(bottle.transform);
        labelTextObj.transform.localPosition = new Vector3(0f, 0.13f, -0.056f);
        labelTextObj.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
        labelTextObj.transform.localScale = new Vector3(0.012f, 0.012f, 0.012f);
        TextMesh labelTM = labelTextObj.AddComponent<TextMesh>();
        labelTM.text = labelName + "\n" + formula;
        labelTM.characterSize = 0.1f;
        labelTM.fontSize = 48;
        labelTM.fontStyle = FontStyle.Bold;
        labelTM.anchor = TextAnchor.MiddleCenter;
        labelTM.alignment = TextAlignment.Center;
        labelTM.color = new Color(0.1f, 0.1f, 0.1f, 1f);
    }
}
#endif

