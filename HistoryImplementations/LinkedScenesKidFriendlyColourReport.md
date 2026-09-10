# Linked scene colour rollout — implementation report

Implemented on 10 September 2026 following approval. MainMenu remains the unchanged colour reference.

## Coverage

| Scene | Result |
|---|---|
| Boot | Light background, bright accents and progress; original logos preserved through a copied splash profile. |
| MainMenu | Existing approved theme preserved byte-for-byte. |
| Login | Light panel, dark labels, yellow primary controls and scoped feedback colours. |
| Lab1_3D | Bright interface and decorative classroom materials. |
| Lab2_3D | Bright interface, classroom, tablet and notebook decoration. |
| Lab3_3D | Bright interface, classroom, quiz and heating-control colours. |
| Lab4_3D | Bright interface, classroom, tablet, notebook and quiz feedback. |
| Lab5_3D | Bright interface, classroom, tablet, notebook and quiz feedback. |
| Lab6_0 | Bright panels, controls and readable text; experiment imagery retained. |
| Lab7 | Bright panels, controls and readable text; experiment imagery retained. |
| Lab8 | Bright panels, controls and readable text; experiment imagery retained. |

KidFriendlySceneTheme scopes runtime changes to the exact approved scene paths. KidFriendlySceneColourSetup applies shared palette roles and creates decorative material variants. Existing setup tools invoke the theme for approved scenes. Fonts, layout, experiment colours, original shared materials, graph semantics, branding and user data were intentionally preserved. Specialist hologram/scientific displays retain their existing functional colours.

## Verification

- Unity scripts compiled; final console query returned zero errors.
- All 11 scenes inspected: zero missing scripts.
- Only the 10 intended scene files differ from the pre-rollout hash inventory. MainMenu and excluded scene files are unchanged.
- Ordered serialized event targets, methods and argument fields match their original components across all changed scenes (event-verification.json).
- Scene object IDs are unchanged except three embedded Material objects in Lab2, Lab3 and Lab4, replaced by decorative material assets; no GameObjects or components were removed.
- Isolated saved-scene previews were generated for every changed scene, plus experiment-view previews for Lab1–Lab5. Login preview regenerated after its final yellow-button correction.
- Lab7 entered and exited Play mode successfully. The asynchronous native capture completed at 2560 × 1440. Its image could not be displayed by the current image tool, so capture completion is not claimed as visual acceptance of that runtime image.
- Restored Lab7, clean, in edit mode. Background-running state restored to the project setting.

## Validation limits

Full experiment completion, account submission, progress-writing navigation and a platform build were not performed. Every runtime quiz/tutorial state and alternative aspect ratio has not been visually reviewed. Saved-scene previews do not prove all runtime interactions. Existing scientific/hologram presentation has deliberately not been blanket-recoloured.

## Evidence

Scene backups, before/after PNGs, per-scene mapping manifests, hashes and verification JSON are in `output/linked-scene-colours/`. `overview-after.jpg` is an overview from before the final Login button correction; `Login-after.png` is current. Native runtime capture and manifest are in `output/agent-captures/f39983ac71944d0c8ba6f965392339da/`.
