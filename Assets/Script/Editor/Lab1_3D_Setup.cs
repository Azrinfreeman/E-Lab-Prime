#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class Lab1_3D_Setup : EditorWindow
{
    [MenuItem("E-Lab Tools/Generate 3D Lab 1 (Bandul Ringkas)")]
    public static void SetupLab1_3D()
    {
        Scene activeScene = EditorSceneManager.GetActiveScene();
        if (activeScene.name != "Lab1_3D")
        {
            activeScene = EditorSceneManager.OpenScene("Assets/Scenes/Lab1_3D.unity", OpenSceneMode.Single);
        }

        // Remove old manager if exists
        GameObject existingTut = GameObject.Find("Lab1_TutorialManager");
        if (existingTut != null)
        {
            DestroyImmediate(existingTut);
        }

        // Create new Tutorial Manager GameObject and attach Lab1TutorialManager
        GameObject tutObj = new GameObject("Lab1_TutorialManager");
        tutObj.AddComponent<Lab1TutorialManager>();

        // Hide legacy 2D UI instructions if present to prevent clutter
        GameObject[] rootObjs = activeScene.GetRootGameObjects();
        foreach (GameObject root in rootObjs)
        {
            Transform[] children = root.GetComponentsInChildren<Transform>(true);
            foreach (Transform child in children)
            {
                if (child.name.StartsWith("Tut_") || child.name.Contains("Instruction") || child.name == "ArahanPanel")
                {
                    if (child.GetComponent<UnityEngine.UI.Text>() != null || child.GetComponent<TMPro.TextMeshProUGUI>() != null)
                    {
                        child.gameObject.SetActive(false);
                    }
                }
            }
        }

        if (KidFriendlySceneTheme.Includes(activeScene)) KidFriendlySceneColourSetup.Apply(activeScene, false);

        Lab1VoiceSetup.Apply(activeScene);
        EditorSceneManager.MarkSceneDirty(activeScene);
        EditorSceneManager.SaveScene(activeScene);

        Debug.Log("3D Lab 1 (Bandul Ringkas) Setup & Universal Instruction System Attached Successfully!");
    }
}
#endif

