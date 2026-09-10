# Unity MCP workflow

## Start small

Read root AGENTS.md and the Unity skill once. Confirm instance, active scene, dirty flag, compilation, play and pause state before dependent work. Read custom tools once per connection. Reuse stable schemas; invalidate scene mappings after reload, play transition, or hierarchy change.

Source searches answer code questions. MCP answers live scene questions. First select paths, then request components/properties; include inactive objects deliberately. Follow cursors only when required to cover the task. Initial limit: 20–30 matches or 25–50 hierarchy items. Larger output is justified by missing evidence, not convenience.

Avoid printing full ALL_TOOLS descriptions. Discover the exact needed tool and print its schema once. Return structuredContent if available; otherwise parse the tool's text JSON. Print one representation, retain errors and truncation, and do not discard failed results from independent batches.

```javascript
const results = await Promise.allSettled(independentReads);
for (const item of results) {
  if (item.status === 'rejected') { text({error: String(item.reason)}); continue; }
  const r = item.value;
  if (r.structuredContent !== undefined) text(r.structuredContent);
  else for (const c of r.content ?? []) if (c.type === 'text') {
    try { text(JSON.parse(c.text)); } catch { text(c.text); }
  }
}
```

Use roughly 1,000–2,000 output tokens for routine discovery. Store genuinely useful full inventories under output/<task>; print counts and relevant findings. Do not save private account data in diagnostics. Limit initial console reads to five relevant entries, one full stack trace per distinct error. Keep user logs intact.

## Editor-only helpers

All classes below are in ELab.EditorWorkflow. Invoke through execute_code and return the JObject once. They do not attach components or save scenes.

```csharp
return ELab.EditorWorkflow.UnityTaskSnapshot.Read(
    "Assets/Scenes/MainMenu.unity", "Canvas/background/PanelAkaun", 0, 20);
return ELab.EditorWorkflow.UnityTaskVerification.Check(
    "Assets/Scenes/MainMenu.unity", "Canvas/background/PanelAkaun");
return ELab.EditorWorkflow.UnityTaskVerification.Reference(
    "Assets/Scenes/MainMenu.unity", "MenuManager", "MainMenuManager", "panelUtama");
```

Snapshot includes active scene, dirty/play/pause/compile state, generation marker, counts, missing scripts, cursor, and bounded rows. Resolve exact hierarchy paths afresh. Missing or duplicate paths return errors. Verification is structural; it reports runtime/visual checks skipped, rather than implying gameplay passed. Add task-specific probes for actual behaviour.

## Capture without forced player stepping

The installed MCP synchronous composited screenshot path called EditorApplication.Step and raised recursive PlayerLoop errors. Do not patch its package cache or retry it repeatedly.

UnityTaskCapture uses the native asynchronous file capture API. It requires an already running, unpaused Play session, an exact panel path, and an optional blocking-overlay path. It waits for active hierarchy and three stable layout frames, requests the next native capture, then verifies PNG completion. It never starts, stops, unpauses, or changes background execution settings. If Game view is not advancing, it times out with a clear reason. Its visual capture path must be validated in the current environment before relying on it for acceptance.

```csharp
return ELab.EditorWorkflow.UnityTaskCapture.Start(
    "Assets/Scenes/MainMenu.unity", "Canvas/background/PanelLogin",
    "Canvas/OpeningSequenceOverlay", 15);
// Later, without a blocking loop:
return ELab.EditorWorkflow.UnityTaskCapture.Status();
// If the task changes:
return ELab.EditorWorkflow.UnityTaskCapture.Cancel();
```

Start returns immediately. Do other independent work before one status check; no polling loop. Capture uses a unique output/agent-captures/<id>/capture.png and manifest.json. Use the exact returned path. A cancelled/timed-out native request may still write its unique file; only completed manifests are candidate review artifacts. The helper checks layout geometry, not all animations or text readiness: the caller must select a meaningful stable view and visually inspect the output.

Use one baseline and final capture per changed view. View a reduced 960–1280 px preview when suitable; inspect larger detail for small text. The helper saves native resolution and does not reduce image detail silently. Record which file is final. Do not recapture unchanged states. Never skip visual checks because this helper is unavailable: use a supported alternative or report the limitation.

## Edit and verify

Inventory runtime writers, serialized overrides, animation tracks, shared assets, and relevant controls before a coherent edit batch. Refresh once if external editing did not import automatically. Wait for compilation and inspect errors before invoking changed types. Do not repeatedly refresh while compiling.

- Documents: read back; check links and consistency. No Play mode.
- UI: scoped reference/state checks, interaction states, rendered review at relevant dimensions.
- Runtime logic: compilation and meaningful focused behavioural tests or existing tests.
- Wiring: required references, event target/signature correctness, navigation and dirty-state checks.
- Package/build changes: relevant compatibility and build checks.

Repeat only failed or affected checks after a fix; broaden when new evidence warrants it. Restore transient probe changes in finally/completion cleanup. Never use PlayerPrefs writes, reset progress, debug unlock, or real account transactions to test colours. Preserve baseline console errors and distinguish tool-origin errors.

## Recovery and measurement

After two identical failures, inspect state and change strategy. Windows sandbox-helper failure is not proof that project config can override managed permissions. Use the permitted escalation path only for authorized actions. Do not change global trust or security settings.

Record calls, output characters, captures, compiles, repeated failures, duration, and validation outcomes. Label characters as a proxy; actual tokens only when exposed. Maintain a small task report, not a growing always-loaded history. Compare equivalent fixtures; do not attribute savings from smaller task scope to better tooling.

Optional MCP filtering remains deferred until actual schema overhead is measured and the effective server/plugin registration is known. Do not create duplicate registrations or change model settings automatically.
