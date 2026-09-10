using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ELab.EditorWorkflow
{
    [InitializeOnLoad]
    public static class UnityTaskSnapshot
    {
        private static readonly string session = Guid.NewGuid().ToString("N");
        private static int generation;
        static UnityTaskSnapshot()
        {
            EditorApplication.hierarchyChanged += () => generation++;
            EditorApplication.playModeStateChanged += _ => generation++;
        }

        public static string Path(Transform t) => t.parent == null ? t.name : Path(t.parent) + "/" + t.name;

        // Exact paths only. Duplicate paths are errors, including inactive objects.
        public static Transform Resolve(string expectedScene, string path)
        {
            var scene = SceneManager.GetActiveScene();
            if (scene.path != expectedScene) throw new InvalidOperationException("Scene mismatch: " + scene.path);
            var found = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Transform>(true))
                .Where(t => Path(t) == path).ToArray();
            if (found.Length != 1) throw new InvalidOperationException("Expected one target at " + path + "; found " + found.Length);
            return found[0];
        }

        public static JObject Read(string expectedScene, string path = null, int cursor = 0, int limit = 20)
        {
            try
            {
                var scene = SceneManager.GetActiveScene();
                if (scene.path != expectedScene) throw new InvalidOperationException("Scene mismatch: " + scene.path);
                if (cursor < 0 || limit < 1 || limit > 50) throw new ArgumentException("cursor >= 0, limit 1..50 required");
                var roots = path == null ? scene.GetRootGameObjects().Select(g => g.transform) : new[] { Resolve(expectedScene, path) };
                var nodes = roots.SelectMany(t => t.GetComponentsInChildren<Transform>(true)).ToArray();
                var rows = new JArray(nodes.Skip(cursor).Take(limit).Select(t => new JObject {
                    ["path"] = Path(t), ["active"] = t.gameObject.activeInHierarchy,
                    ["components"] = new JArray(t.GetComponents<Component>().Select(c => c == null ? "MISSING" : c.GetType().Name))
                }));
                return new JObject {
                    ["success"] = true, ["scene"] = scene.path, ["dirty"] = scene.isDirty,
                    ["playing"] = EditorApplication.isPlaying, ["paused"] = EditorApplication.isPaused,
                    ["compiling"] = EditorApplication.isCompiling, ["updating"] = EditorApplication.isUpdating,
                    ["generation"] = session + ":" + generation, ["total"] = nodes.Length,
                    ["missingScripts"] = nodes.Sum(t => GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject)),
                    ["truncated"] = cursor + rows.Count < nodes.Length,
                    ["nextCursor"] = cursor + rows.Count < nodes.Length ? new JValue(cursor + rows.Count) : JValue.CreateNull(),
                    ["items"] = rows
                };
            }
            catch (Exception e) { return new JObject { ["success"] = false, ["error"] = e.Message }; }
        }
    }
}
