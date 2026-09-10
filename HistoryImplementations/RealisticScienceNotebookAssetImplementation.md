# Lab2–Lab5 realistic notebook asset implementation

Status: approved and implemented. See RealisticScienceNotebookAssetReport.md for delivered assets, verification and limitations.

## Goal

Replace the flat-sheet appearance of the books in Lab2_3D, Lab3_3D, Lab4_3D and Lab5_3D with a convincing open science notebook. Keep the friendly colour palette and all existing experiment-specific text, results, graphs and buttons.

The reference screenshot shows thin flat pages, a wide purple centre strip, and a pen and graph competing with the observation text. Existing scene builders create NotebookCover, LeftPage and RightPage from primitive shapes, with separate text and interaction objects. Improving the mesh alone will not resolve the page layout collisions; both geometry and content placement are included.

## Proposed appearance

Use an open, sewn hardcover lab notebook as the shared design:

- A substantial hard cover visible below the paper, with rounded corners and a narrow overhang.
- A rounded spine and recessed centre binding, replacing the broad flat purple gap.
- Two visible stacks of pages with fine edge lines and slightly uneven paper edges.
- Gently curved inner page edges near the binding, while keeping the main writing areas flat for readable Unity text.
- Warm ivory paper with restrained grain; avoid the current grey/olive appearance.
- A matte, lightly textured cover, subtle seam detail and a small ribbon bookmark.
- A pen resting along the outer edge or bottom margin, clear of text, charts and controls.
- Small lab-specific title tabs and page numbers. Cover accents can vary by lab while sharing the same mesh.

Keep detail strongest in Textbook View, but make the thickness and binding visible from the normal experiment camera too. Realistic construction should remain consistent with the stylised classroom.

## Asset deliverables after approval

Create a reusable Unity prefab with separate cover, spine, page-stack and open-page meshes, plus labelled content anchors. Use a project-native procedural mesh generator or an available local modelling/export tool after checking the toolchain. Save the generated meshes as real reusable project assets; do not rely on runtime mesh generation.

Proposed structure:

- Assets/Art/ScienceNotebook/Meshes/
- Assets/Art/ScienceNotebook/Materials/
- Assets/Art/ScienceNotebook/Textures/ (only if needed)
- Assets/Prefabs/ScienceNotebook/ScienceNotebookOpen.prefab
- Assets/Script/Editor/ScienceNotebookAssetSetup.cs

Prefer shared geometry and materials with scene-specific accent overrides. Target a lightweight mesh, approximately 3,000–8,000 triangles per book, with at most four material slots. Model page stacks as blocks with edge detail instead of hundreds of individual pages. Use the existing render pipeline and shaders; no package or external service dependency is required.

AI-generated raster textures are optional for paper grain or cover fabric. If used, generate only surface textures, never bake experiment text, diagrams, results or buttons into them. The actual deliverable must be a 3D book asset, not a flat picture of a book. Choose the generation method during implementation based on available tools and quality.

## Integration and page layout

Preserve each existing notebook root, camera target, interaction component, event binding and text reference. Attach the new visual prefab beneath that root and replace only the old decorative renderers. Keep compatibility objects such as NotebookCover where scripts use exact names. Verify hover highlighting still targets the visible cover.

Add LeftContentAnchor, RightContentAnchor and ControlsAnchor. Reposition existing text, graph and button objects beneath or relative to these anchors without losing serialized references. Decorative meshes must not intercept clicks on Log Data, Kembali, notebook interactions or the tablet.

Use a consistent two-page layout:

| Scene | Left page | Right page |
|---|---|---|
| Lab2 | Aim, electrode/gas mapping and equation | Expected/completed result summary, observations and theoretical ratio |
| Lab3 | Heating aim, method and comparison guidance | Existing temperature record and conclusion; retain validated runtime values |
| Lab4 | Evaporation aim and controlled factors | Observations and a dedicated graph area with legible labels |
| Lab5 | Dissolving-rate aim and factors | Observations and a dedicated result/graph area where present |

Keep text away from the spine and outer edges. Separate chart and prose regions so the graph axes do not cross the observations, as seen in the screenshot. Inspect all existing graph objects before deciding placement; retain graph data and meaning. If the current spread cannot fit the text and chart legibly, enlarge the usable page area or rebalance the two pages before considering extra page navigation.

Place Log Data and Kembali in a consistent lower margin, styled to match the existing game. Keep buttons visually distinct from printed content. Preserve replayable voice controls and ensure their screen overlay does not cover essential notebook content in Textbook View.

## Implementation sequence

1. Inspect all four live notebook hierarchies, camera framing, page dimensions, graphs, colliders, runtime text writers and setup tools. Record dirty state and back up affected scenes/assets.
2. Build the shared mesh and material assets. Establish comfortable page margins and content-anchor sizes.
3. Integrate and visually verify Lab2 as the first complete example, including normal view, Textbook View and expected/completed result states.
4. Apply the same asset to Lab3, Lab4 and Lab5 with their own page content and graph layouts. Preserve all experiment logic.
5. Update the relevant notebook and scene setup utilities so future regeneration retains the asset and correct page layout. Make the installer repeat-safe without duplicate books or colliders.
6. Run focused visual and interaction checks, correct any collisions or clipped text, restore the original editor state and provide screenshots plus an implementation report.

The existing text-content helpers currently assign positions and scales directly. Coordinate them with the new anchors so running a content setup does not restore the old flat-page layout.

## Acceptance checks

- All four books visibly have cover thickness, a spine, page stacks and a centre binding from both camera views.
- Text and chart regions remain inside the pages with no overlap, flickering surfaces, inverted normals or pen obstruction.
- Warm paper and dark text remain readable under actual scene lighting, not only in an isolated render.
- Review 1920 × 1080 and 1280 × 720, plus another supported aspect ratio. Check the voice overlay against textbook controls.
- Log Data, Kembali, hover feedback and textbook camera navigation work; preserve original event targets.
- Startup, reset and updated-result text still fit. Verify presentation states using isolated probes rather than writing real account progress.
- No missing references, duplicate colliders, pink materials or new compilation/runtime errors caused by the change.
- Only the four requested scenes and their necessary assets/setup integration change. Other labs and MainMenu remain untouched.

## Scope boundaries

This pass does not add page-turn animation, a new notebook inventory, new quiz logic or changed scientific results. It creates and integrates the book asset and fixes the associated page composition. Existing scientific data and user progress must be preserved.

## Final handoff

Deliver the reusable prefab, meshes/materials, repeatable setup tooling, updated scenes, and before/after images for each notebook. Report exactly which interaction, resolution and runtime-state checks passed, and identify any checks not performed.

