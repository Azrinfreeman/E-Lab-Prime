# Unity MCP Token and Work Efficiency — Implementation Plan

Status: Implemented after approval. See UnityMcpEfficiencyVerification.md for delivered changes, testing, and deferred validation.
Date: 10 September 2026
Project: E-Lab Prime

## Objective

Reduce repeated discovery, excessive tool output, unnecessary compilation, and repeated visual checks while preserving implementation quality, user changes, and required validation. Start with workflow improvements; add small editor utilities only where repeated work justifies them.

Token efficiency and elapsed-time efficiency are separate measurements. Fewer calls do not necessarily mean fewer tokens, and fewer screenshots must not mean skipping visual verification. Exact per-task token data is not available from the inspected project files; no percentage reduction is claimed.

## Observed sources of waste

| Finding | Cost | Proposed correction |
|---|---|---|
| Full tool descriptions repeatedly printed, often with duplicated MCP content and structuredContent | Large context with little new information | Discover only needed tool schemas once per session; return one normalized result representation |
| Broad hierarchy, scene YAML, and repository status output | Truncation and repeated follow-up reads | Summary first, then bounded queries for affected objects and files |
| Numerous captures while Unity was paused or still opening | Wrong-state pictures and repeated transitions | Verify the requested view is ready before capture; capture asynchronously without forcing editor frames |
| Capture utility calls EditorApplication.Step; previous stack trace points to ScreenshotUtility.cs:191 | Recursive PlayerLoop errors and time spent diagnosing tooling | Validate an alternative capture path; stop retrying the same failing path |
| Several edit/refresh/play cycles for a single colour pass | Recompilation and domain reload overhead | Inventory colour writers first, then one coherent edit batch and focused verification |
| .agents/AGENTS.md still prescribes dark glass styling and contains web-only guidance | Conflicting instructions and irrelevant context | Consolidate current project guidance and link optional reference documents |
| Windows reader failures and Git ownership checks repeated | Recovery calls that do not advance the task | Record the known failure signature and use the permitted recovery path without changing global security settings |
| Repeated screenshot names silently became suffixed filenames | Confusion about which result is final | Per-run capture manifest with exact returned paths and a designated final image |

These findings come from this conversation, the local project guidance, the last verification report, and the installed screenshot utility. They are not a complete performance profile of Unity MCP.

## Phase 1 — Compact, dependable project instructions

Propose a root AGENTS.md as the short entry point, approximately 500–800 words. Verify actual instruction discovery in a fresh session: .agents/AGENTS.md should not be assumed to apply repository-wide solely because of its location. Codex's documented discovery walks from the project root to the current directory. [Official instruction discovery](https://learn.chatgpt.com/docs/agent-configuration/agents-md)

Consolidate useful existing rules into that entry point. Convert .agents/AGENTS.md to a short pointer after preserving its relevant content, so manual reads cannot reintroduce contradictory guidance. Move detailed UI and web references into Docs/AgentWorkflow/ and read them only for relevant tasks. Record that the bright palette is approved for MainMenu; remaining scenes require their own rollout review.

The entry point should specify:

- Planning-only requests produce Markdown and read-only inspection; no implementation or editor mutation.
- Define affected files/scenes and acceptance checks before editing. Do not expand scope to unrelated fixes.
- Read the Unity skill once when applicable; reuse session knowledge. Recheck live readiness after compilation, reload, instance changes, and before state-dependent actions.
- Use source files for code searches and Unity MCP for live scene/component state. Do not parse entire scene YAML when a targeted editor query answers the question.
- Inspect runtime writers, serialized values, animation tracks, and shared assets before a visual edit.
- Preserve dirty scenes, existing changes, event wiring, scientific colours, user accounts, and saved progress.
- One agent by default. Do not delegate unless requested or required by applicable instructions.
- Keep progress messages concise and useful; final responses state result, verification, and actual limitations.

## Phase 2 — Bounded discovery and result handling

Add Docs/AgentWorkflow/UnityMcpWorkflow.md with a reusable operating sequence:

1. Read current scene, instance, compile/play/pause status, and dirty flag. Read custom-tool availability once per connection; refresh it if capabilities change.
2. Query only the affected panel or components, including inactive objects where relevant. Initial hierarchy page: 25–50 items; explicit cursor and truncation fields.
3. Search named source directories and relevant file patterns. Exclude Library, Temp, Logs, Publish, generated project files, and output by default; deliberately include them for package/build diagnostics.
4. Save verbose inventories locally when useful; return counts, relevant findings, and a file path. Prefer up to 20–30 matching rows initially. Expand if the result is incomplete or a failure needs detail.
5. In functions orchestration, emit structuredContent when present; otherwise parse the text result. Do not print the same payload twice. Keep success/error and truncation information visible.
6. Batch independent reads and inspect every result. Unity mutations, refreshes, play transitions, approvals, and dependent calls remain sequential. Unity batching must not assume thread-safe parallel mutation.
7. Retry transient failures only after checking the relevant state. After two identical failures, change strategy or report the blocker; do not continue an identical retry loop.

Suggested output budgets are workflow defaults, not hard limits: 1,000–2,000 tokens for routine discovery, five relevant console entries initially, and one full stack trace per distinct failure. Do not truncate away evidence needed to finish correctly.

Cache stable schemas and file locations within the session. Cache scene mappings with scene identity and a generation marker, not durable instance IDs; invalidate after scene/hierarchy changes, play transitions, or reloads.

## Phase 3 — Small editor-only utilities

Proposed files, added only after approval:

| File | Purpose |
|---|---|
| Assets/Script/Editor/AgentWorkflow/UnityTaskSnapshot.cs | Return concise editor state, scoped object/component counts, missing references, and relevant serialized values |
| Assets/Script/Editor/AgentWorkflow/UnityTaskVerification.cs | Run explicitly selected checks and return passed/failed/skipped entries with evidence |
| Assets/Script/Editor/AgentWorkflow/UnityTaskCapture.cs | Capture a ready Game view and record exact output path, dimensions, scene, panel, and result |
| Docs/AgentWorkflow/ProjectMap.md | Short map of key scenes, existing setup tools, runtime UI colour writers, palette, and known tooling issues |

Keep helpers under Editor so they do not add player runtime behaviour. Reuse existing APIs after inspecting the installed package and Unity API signatures. Start as editor methods callable through execute_code; add custom MCP tool registration only if usage demonstrates a benefit.

Snapshot queries must be read-only, bounded, and report ambiguity instead of picking the first same-named object. Include inactive objects when requested. Resolve transient instance IDs fresh.

Verification must distinguish pass, failure, and skipped. Use explicit scene identity and required references. Runtime probes restore transient changes in a finally block or equivalent completion handler. Do not write PlayerPrefs, invoke reset/debug unlock buttons, change quality/audio settings, or launch labs merely to test a colour pass.

Capture must wait for the requested panel, completed opening overlay, and stable layout; use bounded asynchronous frame completion. Do not call EditorApplication.Step or force a recursive player loop from an MCP callback. Include Screen Space Overlay UI. Preserve pause/play/background settings and restore them afterward. Do not edit Library/PackageCache. Prototype this helper before relying on it; if capture remains unreliable, report the limitation and use a supported fallback.

Use one baseline and one final capture per materially changed view. Additional captures are justified by a new visual change or unresolved defect. Default review images can use 960–1280 px longest edge; increase resolution or inspect detail crops for small text. Export final images at useful resolution and write a manifest so suffixes cannot misidentify the final result.

## Phase 4 — Compile and verification discipline

For script changes: inspect dependencies, edit the complete coherent group, refresh once if the editing mechanism did not already import it, wait for compilation, then inspect console errors. Do not refresh repeatedly while compiling.

| Change | Required check |
|---|---|
| Documentation only | Read back document, check paths and consistency; no Play mode |
| Colours or layout | Scoped references/state checks, affected interaction states, rendered review at relevant resolutions |
| Runtime logic | Compilation, focused existing tests or behavioural probes, relevant regressions |
| Scene/prefab wiring | Missing references, event targets, dirty-state handling, runtime navigation |
| Packages/build settings | Broader compatibility and relevant build checks |

Run one focused verification pass after the coherent edit batch. Repeat only failed or affected checks after fixes. Broaden validation when evidence warrants it. Existing project verification requirements remain intact.

Record baseline errors without clearing the user's console. Report new script errors separately from pre-existing or tool-origin errors. Do not describe compilation success as proof of gameplay correctness.

## Phase 5 — Optional MCP configuration tuning

Inspect the effective server registration and tool usage before editing .codex/config.toml. The local file currently contains approval/sandbox declarations but does not establish the active Unity server transport or the effective managed permissions.

Codex documents enabled_tools and disabled_tools filters, with a different configuration location for plugin-provided servers. Use the correct registration type; do not invent a second Unity server connection. [Official MCP configuration](https://learn.chatgpt.com/docs/extend/mcp?surface=cli)

Consider filtering rarely used generation, modeling, profiler, and build tools only after measuring their cost and verifying how to restore them. Preserve scene, component, asset, script, console, readiness, and test capabilities needed by actual tasks. Filtering is optional: deferred discovery may already limit tool-schema overhead.

Do not change models, reasoning settings, approval policy, sandbox permissions, global Git trust, or installed packages as an efficiency shortcut. No automatic account-credit consumption or usage-reset actions.

## Measurement and acceptance

Use a representative scoped UI task and a small runtime-logic task. Establish a baseline before tuning; compare equivalent tasks or controlled fixtures without redoing destructive changes in the working scene.

Record tool calls, tool-output characters, captures, compilation cycles, repeated failures, elapsed time, validation outcomes, and actual token counts only when exposed. Label character counts as a proxy, never as measured tokens. Account-wide usage percentages cannot establish exact per-task savings.

Acceptance:

- A fresh session loads the intended concise guidance without contradictory UI rules.
- Inspection results are bounded, not duplicated, and identify truncation clearly.
- One coherent script edit batch triggers only the necessary compile cycle; no redundant refresh.
- Stable views capture without recursive PlayerLoop errors or persistent editor-state changes.
- Required verification still completes; skipped checks are explicitly reported.
- User scene changes, preferences, progress, and unrelated assets are preserved.
- Measured output volume and avoidable repeated calls improve without increased defects or rework. Set percentage targets only after collecting a baseline.

## Rollout and rollback

Implement phases 1–2 first, measure them, then add the helpers incrementally. Leave optional tool filtering until last. Do not build a general automation framework for a few saved calls.

Keep each phase reviewable. Back up the existing instruction/configuration content before replacing it; restore only this task's changes if needed. Editor helper removal must not affect builds or gameplay. No Unity package upgrade is included.

Delivered after approval: compact guidance and editor-only helpers. Optional configuration filtering and end-to-end capture validation remain deferred as recorded in UnityMcpEfficiencyVerification.md.