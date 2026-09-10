using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class MainMenuAccountPanelSetup
{
    [MenuItem("E-Lab Prime/Validate Main Menu Account Panel")]
    public static void ValidateAccountPanel()
    {
        UnityEngine.SceneManagement.Scene scene = EditorSceneManager.GetActiveScene();
        if (scene.name != "MainMenu")
        {
            Debug.LogWarning("Open MainMenu before validating the account panel.");
            return;
        }

        MainMenuAccountPanelController accountController = Object.FindFirstObjectByType<MainMenuAccountPanelController>(FindObjectsInactive.Include);
        if (accountController == null)
        {
            Debug.LogWarning("MainMenuAccountPanelController was not found in MainMenu.");
            return;
        }

        if (accountController.sessionStatusUI == null)
        {
            accountController.sessionStatusUI = Object.FindFirstObjectByType<MainMenuSessionStatusUI>(FindObjectsInactive.Include);
        }

        EditorUtility.SetDirty(accountController);
        MainMenuKidFriendlyColourSetup.Apply();
        EditorSceneManager.MarkSceneDirty(scene);
        Debug.Log("Main Menu account panel references are valid.");
    }
}
