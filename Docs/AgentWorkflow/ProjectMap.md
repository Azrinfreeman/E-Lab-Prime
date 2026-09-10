# Project map

Verify these locations against the current checkout; do not treat this document as live editor state.

- Unity scenes: Assets/Scenes. MainMenu is the colour pilot; Login, Boot, and labs are separate scope.
- Palette: Assets/Script/KidFriendlyPalette.cs.
- Recolour tool: Assets/Script/Editor/MainMenuKidFriendlyColourSetup.cs (Apply; no implicit save).
- Navigation: Assets/Script/MenuManager.cs defines MainMenuManager.
- Runtime colour writers: MainMenuAccountPanelController, MainMenuSessionStatusUI, AchievementManager, AmaliCardProgressUI, AmaliPanelStatsUI.
- Guides/opening: MainMenuProfessorGuide, OpeningSequenceController.
- Existing setup utilities: Assets/Script/Editor/MainMenu*Setup.cs.
- Workflow helpers: Assets/Script/Editor/AgentWorkflow; editor-only, no scene attachment required.
- Plans and verification records: HistoryImplementations. Read only records relevant to the current task.
- Review artifacts: output/<task>. Use a manifest or the tool's exact returned path; captures may add suffixes.

Known environment issues:

- Windows execution/file-edit startup has failed with `apply deny-read ACLs`. Use supported permission escalation only when needed and authorized; do not change global permissions. Do not retry the same broken reader repeatedly.
- Git may reject repository ownership. A command-local exact-path safe.directory override can support authorized reads; do not add a global wildcard trust setting.
- Installed Unity MCP screenshot code has forced EditorApplication.Step during composited capture and raised recursive PlayerLoop errors. Prefer the asynchronous UnityTaskCapture helper after its readiness checks. Do not patch Library/PackageCache.
- The active scene can change between tasks; the efficiency implementation began with Lab6_0 open, not MainMenu.
