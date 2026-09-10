using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class OpeningSequenceSetup
{
    [MenuItem("E-Lab Prime/Validate Opening Sequence Overlay")]
    public static void ValidateOpeningSequenceOverlay()
    {
        OpeningSequenceController controller = Object.FindFirstObjectByType<OpeningSequenceController>(FindObjectsInactive.Include);
        if (controller == null)
        {
            Debug.LogWarning("OpeningSequenceController was not found in the active scene.");
            return;
        }

        if (controller.overlayGroup == null)
        {
            controller.overlayGroup = controller.GetComponent<CanvasGroup>();
        }

        EditorUtility.SetDirty(controller);
        EditorSceneManager.MarkSceneDirty(controller.gameObject.scene);
        Debug.Log("Opening sequence overlay is valid and ready.");
    }
}
