
# RealmWeaver — AI Agent Instructions

## Project Overview
RealmWeaver is a 3D low-poly isometric, hexagonal
turn-based tactical strategy game developed in Unity.

## Environment
- Unity version: 6000.3.4f1
- Render pipeline: Universal Render Pipeline (URP)
- Language: C#
- Target platform: Windows PC
- Version control: Git and Git LFS
- Team size: 3 developers

## Project Structure
All custom project assets are located in Assets/_Project/.

Scripts:
- Scripts/Core: Core game systems
- Scripts/Grid: Hexagonal grid and coordinates
- Scripts/Cards: Card and ability systems
- Scripts/Combat: Combat resolution
- Scripts/TurnSystem: Turn management
- Scripts/UI: User interface controllers

Scenes:
- MainBattle: Integration scene
- Sandbox_Gameplay: Gameplay experimentation
- Sandbox_UI: UI experimentation
- Sandbox_GridTech: Grid and technical experiments

## Development Rules
1. Read relevant existing scripts before modifying them.
2. Prefer modular, maintainable C# code.
3. Use Unity 6 compatible APIs.
4. Follow the existing folder structure.
5. Do not modify Library, Temp, Logs or Obj.
6. Preserve Unity .meta files and their GUIDs.
7. Do not manually edit scene or prefab YAML unless necessary.
8. Never delete or replace existing scenes without approval.
9. Do not modify Packages or ProjectSettings without approval.
10. Do not install new Unity packages without approval.
11. Avoid introducing unnecessary dependencies.
12. Do not modify unrelated systems.
13. Review Git changes before committing.
14. Do not push to GitHub automatically.
15. Ask for approval before destructive actions.

## Workflow
1. Analyze the requested task and relevant files.
2. Explain the implementation plan.
3. Implement changes only in the relevant modules.
4. Check for compilation problems.
5. Explain how to test changes in Unity Editor.
6. Summarize modified files and remaining issues.

## Communication
- Explain all work in Vietnamese.
- Use English for C# identifiers and technical names.
- Clearly distinguish completed changes from untested changes.
