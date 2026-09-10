using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace ELab.EditorWorkflow
{
    public static class UnityTaskVerification
    {
        // Read-only structural checks. Behaviour and visual checks are explicitly skipped.
        public static JObject Check(string scene, string path, bool checkButtons = true)
        {
            var checks = new JArray();
            try
            {
                var root = UnityTaskSnapshot.Resolve(scene, path);
                var nodes = root.GetComponentsInChildren<Transform>(true);
                int missing = nodes.Sum(t => GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject));
                checks.Add(Row("missingScripts", missing == 0 ? "passed" : "failed", missing + " missing"));
                if (checkButtons)
                {
                    var buttons = root.GetComponentsInChildren<Button>(true);
                    int invalid = 0, events = 0;
                    foreach (var b in buttons)
                    {
                        if (b.targetGraphic == null) invalid++;
                        for (int i = 0; i < b.onClick.GetPersistentEventCount(); i++)
                        {
                            events++;
                            if (b.onClick.GetPersistentTarget(i) == null || string.IsNullOrEmpty(b.onClick.GetPersistentMethodName(i))) invalid++;
                        }
                    }
                    checks.Add(Row("buttonReferences", buttons.Length == 0 ? "skipped" : invalid == 0 ? "passed" : "failed",
                        buttons.Length + " buttons; " + events + " persistent listeners; " + invalid + " missing targets/graphics/method names. Runtime listeners and method signatures not validated."));
                }
                checks.Add(Row("runtimeAndVisual", "skipped", "Read-only structural probe; run task-specific behaviour and rendered checks separately."));
                return new JObject { ["success"] = !checks.Any(c => (string)c["status"] == "failed"), ["checks"] = checks };
            }
            catch (Exception e) { return new JObject { ["success"] = false, ["error"] = e.Message, ["checks"] = checks }; }
        }

        public static JObject Reference(string scene, string path, string componentType, string property)
        {
            try
            {
                var root = UnityTaskSnapshot.Resolve(scene, path);
                var matches = root.GetComponents<Component>().Where(c => c != null && (c.GetType().FullName == componentType || c.GetType().Name == componentType)).ToArray();
                if (matches.Length != 1) throw new InvalidOperationException("Expected one matching component; found " + matches.Length);
                using (var serialized = new SerializedObject(matches[0]))
                {
                    var field = serialized.FindProperty(property);
                    if (field == null || field.propertyType != SerializedPropertyType.ObjectReference)
                        throw new ArgumentException("Expected an object-reference property: " + property);
                    return Row(property, field.objectReferenceValue == null ? "failed" : "passed",
                        field.objectReferenceValue == null ? "Reference is null" : field.objectReferenceValue.GetType().Name);
                }
            }
            catch (Exception e) { return Row(property, "failed", e.Message); }
        }
        private static JObject Row(string name, string status, string evidence) => new JObject { ["check"] = name, ["status"] = status, ["evidence"] = evidence };
    }
}
