# Unity MCP efficiency implementation — results

Date: 10 September 2026
Status: Workflow and editor helpers implemented. Optional MCP filtering deferred; capture runtime validation and fresh-session instruction discovery remain unverified.

## Delivered

- Root AGENTS.md: concise project-wide scope, discovery, editing, and verification rules.
- .agents/AGENTS.md: compatibility pointer to root guidance.
- Docs/AgentWorkflow: optional UnityMcpWorkflow.md, ProjectMap.md, UIDesign.md, and WebDesign.md.
- Assets/Script/Editor/AgentWorkflow/UnityTaskSnapshot.cs: bounded exact-path scene inspection, pagination, generation marker, and explicit errors.
- UnityTaskVerification.cs: read-only missing-script, button-reference, and requested serialized-reference checks; runtime/visual checks explicitly skipped.
- UnityTaskCapture.cs: asynchronous native capture with readiness/timeout checks, unique filenames, and manifest. Does not force player frames or change play/pause/background settings.

No model, approval, sandbox, MCP registration, package, runtime script, or game scene changes were made. Existing scene changes from other work were preserved. The new helpers remain editor-only. Existing instruction/config backups are in output/efficiency-review.

## Tests performed

Unity compiled the helpers successfully. Console checks returned zero errors after the tests.

Read-only tests on the open Lab6_0 scene passed:

- Wrong scene rejected.
- Missing exact target rejected.
- Oversized page request rejected.
- Successive page results differ and include explicit nextCursor/truncated fields.
- Snapshot leaves dirty state unchanged.
- Capture refuses Edit mode without changing editor state.
- Serialized-reference inspection identifies a null reference. The camera targetTexture null was an intentional probe, not a game defect.

A disposable, unsaved additive fixture verified duplicate-path rejection, inactive-object inclusion, passing valid button references, and failing a missing button graphic. The fixture was closed in finally; the original scene was restored. Final state: Assets/Scenes/Lab6_0.unity, one loaded scene, not dirty, not playing.

MainMenu's SHA-256 before/after matched:
656BB5B3C5F0127546C0EC30D867E6D9E0757B633747828619069A5D3044E2CC

## Measurement

On the same 78-object Lab6_0 scene, an all-object path/component listing serialized to 10,114 characters. A three-row paged snapshot including readiness, totals, and cursors serialized to 567 characters.

This demonstrates bounded output. It is not an equivalent-information compression benchmark: the snapshot deliberately postpones unneeded rows. No exact token counts or wall-clock savings were measured. A representative future UI and runtime task should record calls, characters, compiles, captures, duration, repeated failures, and correctness before claiming percentage savings.

## Remaining checks and deliberate deferrals

- Fresh-session automatic instruction discovery is not testable by rereading files in this existing task. Confirm root guidance is loaded when the next task starts.
- Native asynchronous screenshot completion, final image content, timeout/cancellation during Play mode, and restoration across reload have not been exercised end-to-end. Capture precondition handling was tested. Treat the capture helper as provisional until tested in an already-authorized running preview; do not rely on it to claim visual acceptance yet.
- No real lab was started just to test capture. No PlayerPrefs/account/progress writes were performed.
- Optional tool filtering is not applied: no measured schema-overhead benefit or effective registration audit justified changing working tools.
- Structural verification does not validate persistent listener signatures, runtime listeners, full navigation, or gameplay. These limitations are included in its returned evidence.

## Rollback

Restore instruction content from output/efficiency-review/AGENTS.before.md and config.before.toml only if needed; configuration itself was not changed. Remove only the newly introduced root guidance/references/editor helper files and their Unity-generated metadata when rolling back this work. Do not revert unrelated project changes or the earlier MainMenu colour work.
