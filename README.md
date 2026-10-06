# E-Lab Prime

A Unity virtual science laboratory prototype aimed at Form 1 learners, with Bahasa Melayu instructions, interactive experiments, guided activities, and local progress rewards.

The project combines five 3D lab scenes with three further experiment scenes, alongside a main menu, professor guide, account prototype, achievements, and settings. It demonstrates scene-based simulation, interactive apparatus, experiment records, graph displays, educational feedback, and reusable UI tooling.

## Lab activities

| Activity | Topic | Enabled scene |
| --- | --- | --- |
| 1 | Pendulum timing and experiment records | `Lab1_3D` |
| 2 | Water electrolysis | `Lab2_3D` |
| 3 | Heating pure water and salt water | `Lab3_3D` |
| 4 | Factors affecting evaporation | `Lab4_3D` |
| 5 | Factors affecting sugar dissolution | `Lab5_3D` |
| 6 | Air composition | `Lab6_0` |
| 7 | Light reflection | `Lab7` |
| 8 | Light refraction | `Lab8` |

The [build configuration](ProjectSettings/EditorBuildSettings.asset) enables **11 scenes**: Boot, MainMenu, Login, and these eight labs. Older and backup lab scenes are also retained in the repository; they are outside the current enabled build list.

## Learning experience

- Manipulate apparatus and adjust experiment variables through scene-specific controls.
- Review tutorial prompts, with replayable voice-instruction components for Labs 1–5.
- Use notebook content, data tables, graphs, and quizzes where implemented for each activity.
- Track completion, XP, credits, and achievements through local `PlayerPrefs` storage.
- Navigate a bright main menu with professor guidance and student/guest session indicators.

Simulation values and expected textbook results are instructional approximations. The project has not been verified here as curriculum-endorsed software or as a substitute for supervised practical work. Each lab has its own interaction and completion logic; a full end-to-end demonstration is still needed.

## Technology

| Component | Repository configuration |
| --- | --- |
| Unity Editor | **6000.5.9f1** |
| Language | C# |
| Universal Render Pipeline | **17.5.0** |
| Input System | **1.20.0** |
| Cinemachine | **3.1.7** |
| Unity UI | **2.5.0** |
| Development integration | Unity MCP Git dependency referencing `main` |

See [ProjectVersion.txt](ProjectSettings/ProjectVersion.txt) and [manifest.json](Packages/manifest.json) for the authoritative setup. The Git dependency is not pinned to an immutable revision in the manifest.

## Open the project

1. Clone the repository using an account with access:

   ```sh
   git clone https://github.com/Azrinfreeman/E-Lab-Prime.git
   ```

2. Install Unity **6000.5.9f1** through Unity Hub and open the repository root.
3. Allow package restoration and asset import to finish. Network access is needed for package retrieval, including the Unity MCP Git dependency.
4. Resolve import/compile errors, then open `Assets/Scenes/Boot.unity` to review startup and the menu flow.
5. Use a test profile or guest mode when inspecting local account and progress features. Avoid reset or debug-unlock controls unless changing that test data is intended.

The MainMenu account panel stores a local student profile and password in `PlayerPrefs`; it is a prototype, without verified remote authentication or cloud synchronization. The separate Login scene controller uses mock authentication. Use dummy credentials, and do not treat either flow as production account security.

Editor setup commands under `Assets/Script/Editor` can rebuild UI or modify scenes. They are development utilities, not a required step for opening the project.

## Source guide

| Area | Starting point |
| --- | --- |
| Lab 1–5 apparatus, tutorials, voice, and notebooks | [Assets/Script/3d](Assets/Script/3d) |
| Air composition | [AirCompositionManager.cs](Assets/Script/AirCompositionManager.cs) |
| Reflection and refraction | [ReflectionManager.cs](Assets/Script/ReflectionManager.cs), [RefractionLabManager.cs](Assets/Script/RefractionLabManager.cs) |
| Main menu navigation | [MenuManager.cs](Assets/Script/MenuManager.cs) |
| Local accounts | [MainMenuAccountPanelController.cs](Assets/Script/MainMenuAccountPanelController.cs) |
| XP, credits, and achievements | [EconomyManager.cs](Assets/Script/EconomyManager.cs), [AchievementManager.cs](Assets/Script/AchievementManager.cs) |
| Startup presentation | [StudioSplashController.cs](Assets/Script/Startup/StudioSplashController.cs) |
| Lab 3–5 textbook content | [ExperimentTextbookContent.cs](Assets/Script/ExperimentTextbookContent.cs) |
| Existing development records | [HistoryImplementations](HistoryImplementations) |

## Validation and demonstration status

This documentation was checked against source, package metadata, enabled scenes, and existing implementation records. **No Unity import, compilation, Play Mode, experiment completion, platform build, or new screenshot capture was performed for this change.**

Existing reports record scoped compilation, scene checks, and previews, with stated limitations. Some files are proposals; a filename containing “Implementation” does not establish that every proposed change was delivered. The tracked `Publish` and `output` folders contain earlier artifacts that were not executed or certified during this review. No GitHub release was available at review time.

See [portfolio validation notes](Docs/PortfolioValidation.md) for the exact review scope and follow-up checks. Before sharing a playable demonstration, verify all eight experiment flows, educational content, progress persistence, and the intended device/build, then add representative screenshots or video.

## Asset permissions

No repository-wide license was added. Review the applicable permissions for fonts, audio, models, logos, packages, and other included assets before redistribution, and preserve third-party notices.
