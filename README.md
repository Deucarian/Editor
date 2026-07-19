# Deucarian Editor

Current package version: `1.0.5`.

## What this is

`com.deucarian.editor` is an editor-only Unity package for shared Deucarian editor tooling, branded editor chrome, fixed icons, layout helpers, and UX standards.

This package is not runtime theming. It is not user-customizable. Deucarian editor windows use package-owned Tideline light/dark colors, DINish interface typography, and brand resources so every Deucarian package presents the same quiet, readable editor experience. The editor treatment follows the active Unity skin.

Runtime theme assets from `com.deucarian.theming` must never control Deucarian editor windows.

## When to use it

- You are building a Deucarian package editor window or inspector and need the shared editor shell.
- You need fixed Deucarian editor colors, spacing, status badges, cards, buttons, sidebars, or window chrome.
- You need shared editor-only logo, icon, USS, hero, or placeholder resources.
- You need a consistent menu and UX standard for Deucarian tools.

## When not to use it

- Do not use this package for runtime theming, runtime UI, gameplay UI, or user-customizable theme assets.
- Do not put package installation, registry metadata, dependency resolution, diagnostics, logging, or domain package behavior here.
- Do not add runtime/domain dependencies unless governance approves a narrow editor-only need.

## Install

Stable:

```json
"com.deucarian.editor": "https://github.com/Deucarian/Editor.git#main"
```

Development:

```json
"com.deucarian.editor": "https://github.com/Deucarian/Editor.git#develop"
```

## Unity compatibility

Requires Unity 2021.3 or newer.

## 60-second quick start

Create an editor window under `Tools/Deucarian/<PackageName>/...` and draw the shared header plus package fields:

```csharp
using Deucarian.Editor;
using UnityEditor;
using UnityEngine;

public sealed class ExampleManagerWindow : EditorWindow
{
    private UnityEngine.Object settingsAsset;

    private void OnGUI()
    {
    DeucarianEditorChrome.DrawBrandHeader(
        "Example",
        "Example Deucarian manager window.");

        DeucarianEditorChrome.DrawSectionHeader("Assets");

        settingsAsset = DeucarianEditorFields.DrawAssetFieldWithSelectButton(
            "Settings",
            settingsAsset,
            "Select");
    }
}
```

For a hybrid UI Toolkit/IMGUI tool, compose the shared workbench and add only
package-owned controls and state to its regions:

```csharp
private DeucarianEditorWorkbench workbench;

public void CreateGUI()
{
    workbench = DeucarianEditorWorkbench.Create(
        rootVisualElement,
        new DeucarianEditorWorkbenchOptions
        {
            IncludeToolbar = true,
            IncludeDrawer = true,
            IncludeFooter = true
        });

    workbench.Toolbar.Add(
        DeucarianEditorWorkbenchToolbar.CreateActionButton("Refresh", Refresh));
    workbench.AddImGuiContent(DrawPackageContent);
}

private void OnDisable()
{
    workbench?.Dispose();
}
```

Open a responsive shared dialog without coupling the Editor package to a consumer workflow:

```csharp
DeucarianEditorDialog.Show(
    new DeucarianEditorDialogOptions(
        "Confirm operation",
        "Review the details before continuing.",
        DeucarianEditorIconIds.Warning,
        new[]
        {
            new DeucarianEditorDialogAction(
                "continue",
                "Continue",
                DeucarianEditorIconIds.Play,
                DeucarianEditorDialogActionStyle.Primary),
            new DeucarianEditorDialogAction(
                "cancel",
                "Cancel",
                DeucarianEditorIconIds.Clear)
        })
    {
        Details = details,
        DefaultActionId = "continue",
        CancelActionId = "cancel"
    },
    result =>
    {
        if (result.ActionId == "continue")
        {
            ContinueOperation();
        }
    });
```

The workbench uses the established Installer layout modes: Narrow below 900 px,
Compact from 900 through 1179 px, and Wide at 1180 px or above. The shared
drawer/footer factories provide presentation only; operation models and package
behavior remain in the consuming package.

## Samples

This package only includes editor helpers. See `Samples~/README.md` for notes on adding lightweight example content when a package-specific sample is needed.

## Public API map

- `DeucarianEditorChrome`: fixed package headers, section headers, section boxes, inline help, and footer version text.
- `DeucarianEditorFields.DrawAssetFieldWithSelectButton`: asset object field with the project selection action on the same row.
- `DeucarianEditorIcons` and `DeucarianEditorIconIds`: safe dynamic access to the package-owned Lucide catalog, stable shared IDs, legacy package aliases, and the canonical Lucide package fallback.
- `DeucarianEditorUIResources`: shared UI Toolkit USS, logo, hero, and package placeholder assets.
- `DeucarianEditorTheme`, `DeucarianEditorSpacing`, and `DeucarianEditorTextures`: fixed Deucarian visual tokens.
- `DeucarianEditorPalette` and `DeucarianEditorGraphTheme`: the five brand territories and semantic light/dark graph roles for canvases, surfaces, borders, edges, interaction states, and statuses.
- `DeucarianEditorWindowChrome` and `DeucarianEditorAmbientGlass`: shared wallpaper, readability overlay, ambient glow, grain, vignette, and fixed-window chrome.
- `DeucarianEditorCards`, `DeucarianEditorSidebar`, `DeucarianEditorButtons`, and `DeucarianEditorStatusPanel`: shared IMGUI frosted-glass cards, sidebars, buttons, validation/status cards, and bottom status bars.
- `DeucarianEditorStatusBadge`: fixed-color GUILayout and fixed-rect status badges for info, success, warning, error, and disabled states.
- `DeucarianEditorStyles`: shared cached `GUIStyle` instances.
- `DeucarianEditorColors`: fixed Deucarian editor colors with minimal light/dark skin readability adaptation.
- `DeucarianEditorWorkbench` and `DeucarianEditorWorkbenchOptions`: composable hybrid window shell exposing toolbar, content, optional drawer, and optional footer regions.
- `DeucarianEditorResponsiveLayout`: legacy preview calculations plus the exact Wide/Compact/Narrow workbench resolver and idempotent UI Toolkit class application.
- `DeucarianEditorWorkbenchToolbar` and `DeucarianEditorWorkbenchSurfaces`: domain-neutral toolbar, drawer, row, and footer factories backed by shared USS contracts.
- `DeucarianEditorIconToolbar`: IMGUI icon-only actions backed exclusively by vendored Lucide IDs.
- `DeucarianEditorWorkbenchGUI`: shared 24 px IMGUI actions, surface colors, panel scopes, separators, key/value rows, legacy marker rows, and Lucide status rows.
- `DeucarianEditorDialog`: branded responsive callback-based utility dialogs with a Lucide icon, wrapped message/details content, icon-bearing actions, and deterministic Enter/Escape/window-close completion.
- `DeucarianEditorCardScope`, `DeucarianEditorFoldoutScope`, and `DeucarianEditorWorkbenchPanelScope`: exception-safe layout scopes for composable drawing.

## Integrations

Works with:

- Deucarian editor windows and inspectors in other packages,
- package-owned UI Toolkit files that need shared resources,
- tools under `Tools/Deucarian/<PackageName>/...`.

Does not own:

- runtime theming,
- runtime UI frameworks,
- package installation logic,
- registry governance,
- diagnostics export,
- domain package behavior.

## UI Toolkit assets

Shared UI Toolkit assets live under these package-owned locations:

- `Editor/Assets/Icons/`
- `Editor/Assets/Logos/`
- `Editor/Assets/Styles/`
- `Editor/Assets/Images/`

Current placeholder assets are intentionally named for easy replacement:

- Drop the real Deucarian logo at `Editor/Assets/Logos/DeucarianPlaceholderLogo.png`.
- Drop the real Package Installer hero image at `Editor/Assets/Images/DeucarianPackageInstallerPlaceholderHero.png`.
- Drop the real default package icon at `Editor/Assets/Icons/DeucarianPackagePlaceholderIcon.png`.

Package-specific UI Toolkit files stay in the owning package. Long-term shared logos, icons, and editor brand imagery belong here in `com.deucarian.editor`.

## Deucarian UX standards

### Asset fields

Asset rows must keep the select action inline with the visible object field.

Good:

```text
Theme:
[ ObjectField ................................ ] [ Select ]
```

Bad:

```text
Theme:
[ ObjectField ]

Actions:
[ Select Theme ]
```

If an asset is already visible in an `ObjectField`, do not create a separate action row just to select or ping that asset.

### Menus

Packages with meaningful tooling should expose editor menu entries under:

```text
Tools/Deucarian/<PackageName>/...
```

Examples:

- `Tools/Deucarian/Theming/Open Theme Manager`
- `Tools/Deucarian/Logging/Open Logging Settings`
- `Tools/Deucarian/Object Loading/Open Manager`

Do not create menus for packages without meaningful tooling.

The Package Installer uses the same shared tooling root: `Tools/Deucarian/Package Installer`.

### Actions sections

Actions sections should contain real actions such as create, apply, scan, repair, import, export, or refresh.

Do not use actions sections for selecting assets already visible in object fields.

## Troubleshooting

- If runtime code needs this package, stop and check the ownership boundary; Editor is editor-only.
- If a package wants custom runtime colors, use the runtime theming owner instead of editor shell tokens.
- If a package-specific tool needs a menu, keep it under `Tools/Deucarian/<PackageName>/...`.
- If a UI Toolkit asset is package-specific, keep it in the owning package rather than moving it here.

## Validation

Run the shared package validator from the repository root:

```powershell
python C:/Repositories/Package-Registry/Tools/deucarian_package_validator.py --registry-root C:/Repositories/Package-Registry --repository-root . --config deucarian-package.json
```

Run the package's EditMode tests in Unity after code or assembly definition changes. Tests cover constants, style accessors, icon fallbacks, status badge helpers, and asset field API availability.

Documentation-only updates should still pass:

```powershell
git diff --check
```

## Architecture / Contributor Notes

- [AGENTS.md](AGENTS.md) contains repository-specific ownership and Codex guidance.
- Deucarian architecture rules live in [Package Registry](https://github.com/Deucarian/Package-Registry/blob/develop/ARCHITECTURE.md).
- Capability ownership is tracked in [CAPABILITY_OWNERSHIP.md](https://github.com/Deucarian/Package-Registry/blob/develop/CAPABILITY_OWNERSHIP.md).

## License

MIT. See [LICENSE.md](LICENSE.md).
