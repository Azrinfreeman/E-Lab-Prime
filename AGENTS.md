# E-Lab Prime — working agreements

## Scope and communication

Follow the user's latest scope. Planning-only requests produce Markdown and read-only inspection; do not implement until approved. Approval persists across turns. Define affected scenes/files and acceptance checks before editing. Finish authorized work without repeated permission questions; ask only for missing decisions that block safe progress. Keep updates concise and useful, and report outcome, verification, and actual limitations. One agent by default; delegate only when requested or required by applicable instructions.

## Efficient discovery

Use the Unity MCP skill when applicable; read it once per conversation and reuse known schemas. Read editor readiness before state-dependent operations and after compilation, domain reload, or instance changes. Read custom tools once per connection and again if capabilities change. Confirm active project and scene rather than assuming the last task's scene is still open.

Use source files for code searches and MCP for live scene/component state. Start with a scoped summary: 25–50 hierarchy entries, 20–30 search results, five relevant console entries. Expand when required; always expose truncation and cursors. Do not dump entire scene YAML, tool catalogs, repository status, or duplicated MCP content/structuredContent. Print one result representation and retain failures. Default routine discovery output to roughly 1,000–2,000 tokens, not a hard correctness limit.

Search named directories with rg and include/exclude globs. Exclude Library, Temp, Logs, Publish, output, and generated project files unless the question specifically requires them. Read the optional [workflow](Docs/AgentWorkflow/UnityMcpWorkflow.md) or [project map](Docs/AgentWorkflow/ProjectMap.md) only when useful. Cache stable file locations and schemas, never assume instance IDs survive scene changes or reloads.

## Implementation

Batch independent reads and inspect every result. Keep dependent calls, scene mutations, compilation, and play transitions sequential. Unity batching does not imply thread-safe parallel editing.

Inspect runtime writers, serialized overrides, animations, and shared assets before UI edits. Make one coherent edit batch, then compile. Explicitly report missing expected objects, ambiguous names, invalid assets, and broken references; never silently skip critical targets. Search recursively within the intended scope, including inactive objects when needed. Prefer durable editor utilities for repeated work over large repeated execute_code snippets.

Preserve existing dirty scenes and unrelated modifications. Do not globally replace shared materials, fonts, prefabs, scientific colours, or user data for a local UI change. Never write PlayerPrefs or invoke reset/debug-unlock/account actions merely to verify appearance. Do not change sandbox permissions, approval policy, global Git trust, models, packages, or account credits as an efficiency shortcut.

## Visual direction

The bright kid-friendly palette is implemented for MainMenu; do not restore its old dark-glass defaults. Other scenes keep their current appearance until the user approves their rollout. Use [UI guidance](Docs/AgentWorkflow/UIDesign.md) for relevant tasks. Preserve rounded controls, readable typography, and existing responsive button behaviour. Web-specific guidance is optional in [WebDesign.md](Docs/AgentWorkflow/WebDesign.md).

## Verification and stopping rules

After script edits, wait for compilation and check errors before using changed types. Refresh once only if the editing mechanism did not already import the changes. Verify implementation logic and actual editor/runtime state appropriate to the change. Documentation-only edits need no Play mode. UI changes need affected state/interaction checks and rendered review; runtime logic needs focused behavioural tests. Check event targets, not only event method names.

Run one focused verification pass, then repeat only checks affected by fixes or new evidence. Do not skip meaningful validation to meet an output budget. Distinguish passed, failed, and skipped checks. Preserve baseline console entries and identify new failures. One stack trace per distinct error is enough initially.

Capture only after the intended view is ready; start with one baseline/final image per materially changed view. Use exact returned paths. Do not use a screenshot path that recursively forces EditorApplication.Step; see the workflow's known issue. After two identical failures, inspect the cause and switch strategy. Restore transient preview state and document unresolved limitations. Never claim exact token savings from character counts or account-wide usage percentages.
