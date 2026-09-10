using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ELab.EditorWorkflow
{
    // Native asynchronous Game-view capture. Never steps, unpauses, starts, or stops the player.
    [InitializeOnLoad]
    public static class UnityTaskCapture
    {
        private static JObject result = new JObject { ["status"] = "idle" };
        private static string scene, panel, blocker, file, manifest, layout;
        private static double deadline;
        private static int stableFrames, lastFrame;
        private static bool pending, requested;

        static UnityTaskCapture()
        {
            AssemblyReloadEvents.beforeAssemblyReload += () => Finish("failed", "Assembly reload interrupted capture");
            EditorApplication.quitting += () => Finish("failed", "Editor closed");
        }

        public static JObject Start(string expectedScene, string panelPath, string blockingOverlayPath = null, int timeoutSeconds = 15)
        {
            if (pending) return new JObject { ["success"] = false, ["error"] = "A capture is already pending; poll Status or Cancel it." };
            try
            {
                if (!EditorApplication.isPlaying || EditorApplication.isPaused)
                    throw new InvalidOperationException("Capture requires an already running, unpaused Play session. No editor state was changed.");
                if (timeoutSeconds < 2 || timeoutSeconds > 60) throw new ArgumentException("Timeout must be 2..60 seconds");
                UnityTaskSnapshot.Resolve(expectedScene, panelPath);
                if (blockingOverlayPath != null) UnityTaskSnapshot.Resolve(expectedScene, blockingOverlayPath);
                scene = expectedScene; panel = panelPath; blocker = blockingOverlayPath;
                var folder = System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath, "../output/agent-captures", Guid.NewGuid().ToString("N")));
                Directory.CreateDirectory(folder);
                file = System.IO.Path.Combine(folder, "capture.png"); manifest = System.IO.Path.Combine(folder, "manifest.json");
                result = new JObject { ["status"] = "waiting", ["scene"] = scene, ["panel"] = panel, ["path"] = file, ["manifest"] = manifest };
                deadline = EditorApplication.timeSinceStartup + timeoutSeconds;
                lastFrame = Time.frameCount; stableFrames = 0; layout = null; requested = false; pending = true;
                EditorApplication.update += Tick;
                return Status();
            }
            catch (Exception e) { return new JObject { ["success"] = false, ["error"] = e.Message }; }
        }

        public static JObject Status() => (JObject)result.DeepClone();
        public static JObject Cancel() { Finish("cancelled", "Cancelled by caller; a queued native capture may still finish writing its unique file."); return Status(); }

        private static void Tick()
        {
            try
            {
                if (EditorApplication.timeSinceStartup > deadline) { Finish("failed", "Timed out waiting for ready view or PNG completion. Focus Game view and inspect state before retrying."); return; }
                if (!EditorApplication.isPlaying || EditorApplication.isPaused || EditorApplication.isCompiling || SceneManager.GetActiveScene().path != scene)
                { Finish("failed", "Editor state changed during capture"); return; }
                if (requested)
                {
                    if (!File.Exists(file)) return;
                    var png = File.ReadAllBytes(file);
                    // Require a complete PNG ending in IEND, not a partially written file.
                    if (png.Length < 32 || png[0] != 137 || png[1] != 80 || png[png.Length - 8] != 73 || png[png.Length - 7] != 69 || png[png.Length - 6] != 78 || png[png.Length - 5] != 68) return;
                    result["width"] = BigEndian(png, 16); result["height"] = BigEndian(png, 20);
                    Finish("completed", "Captured native Game view. Visually inspect the file before accepting it."); return;
                }
                if (lastFrame == Time.frameCount) return;
                lastFrame = Time.frameCount;
                var target = UnityTaskSnapshot.Resolve(scene, panel);
                if (!target.gameObject.activeInHierarchy || (blocker != null && UnityTaskSnapshot.Resolve(scene, blocker).gameObject.activeInHierarchy))
                { stableFrames = 0; return; }
                string signature = string.Join(";", target.GetComponentsInChildren<RectTransform>().Select(t => t.rect.ToString() + t.position.ToString() + t.lossyScale.ToString()));
                stableFrames = signature == layout ? stableFrames + 1 : 0; layout = signature;
                if (stableFrames < 3) return;
                Canvas.ForceUpdateCanvases();
                ScreenCapture.CaptureScreenshot(file, 1);
                requested = true; result["status"] = "capturing";
            }
            catch (IOException) { /* File may still be locked by the native writer; bounded by deadline. */ }
            catch (Exception e) { Finish("failed", e.Message); }
        }
        private static int BigEndian(byte[] data, int i) => (data[i] << 24) | (data[i + 1] << 16) | (data[i + 2] << 8) | data[i + 3];
        private static void Finish(string status, string message)
        {
            if (!pending) return;
            EditorApplication.update -= Tick; pending = false;
            result["status"] = status; result["message"] = message;
            result["finishedUtc"] = DateTime.UtcNow.ToString("o");
            try { File.WriteAllText(manifest, result.ToString()); }
            catch (Exception e) { result["manifestError"] = e.Message; Debug.LogWarning("Capture manifest: " + e.Message); }
        }
    }
}
