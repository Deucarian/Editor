# Deucarian Editor

## Overview

`com.deucarian.editor` is an editor-only Unity package for shared Deucarian editor tooling, branded editor chrome, fixed icons, layout helpers, and UX standards.

This package is not runtime theming. It is not user-customizable. Deucarian editor windows use fixed package-owned brand colors and resources so every Deucarian package presents the same clean, technical, readable editor experience.

Runtime theme assets from `com.deucarian.theming` must never control Deucarian editor windows.

## Installation

Install from the Unity Package Manager using:

```text
https://github.com/Deucarian/Editor.git#main
```

## Usage

### Core APIs

- `DeucarianEditorChrome` draws fixed package headers, section headers, section boxes, inline help, and footer version text.
- `DeucarianEditorFields.DrawAssetFieldWithSelectButton` draws an asset object field with the project selection action on the same row.
- `DeucarianEditorIcons` resolves known Deucarian package icons and provides safe fallback content.
- `DeucarianEditorUIResources` resolves shared UI Toolkit USS, logo, hero, and package placeholder assets.
- `DeucarianEditorStatusBadge` draws fixed-color GUILayout and fixed-rect status badges for info, success, warning, error, and disabled states.
- `DeucarianEditorStyles` exposes shared cached `GUIStyle` instances.
- `DeucarianEditorColors` contains fixed Deucarian editor colors with minimal light/dark skin readability adaptation.

### UI Toolkit Assets

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

### Deucarian UX Standards

#### Asset Fields

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

#### Menus

Packages with meaningful tooling should expose editor menu entries under:

```text
Tools/Deucarian/<PackageName>/...
```

Examples:

- `Tools/Deucarian/Theming/Open Theme Manager`
- `Tools/Deucarian/Logging/Open Logging Settings`
- `Tools/Deucarian/Object Loading/Open Manager`

Do not create menus for packages without meaningful tooling.

The Package Installer is the bootstrap exception and may keep `Deucarian/Package Installer` for discoverability and backwards compatibility.

#### Actions Sections

Actions sections should contain real actions such as create, apply, scan, repair, import, export, or refresh.

Do not use actions sections for selecting assets already visible in object fields.

## Example Manager Window

```csharp
using Deucarian.Editor;
using UnityEditor;
using UnityEngine;

public sealed class ExampleManagerWindow : EditorWindow
{
    private UnityEngine.Object settingsAsset;

    private void OnGUI()
    {
        DeucarianEditorChrome.DrawPackageHeader(
            "Example",
            "Example Deucarian manager window.",
            DeucarianEditorIcons.GetPackageIcon("editor"));

        DeucarianEditorChrome.DrawSectionHeader("Assets");

        settingsAsset = DeucarianEditorFields.DrawAssetFieldWithSelectButton(
            "Settings",
            settingsAsset,
            "Select");
    }
}
```

## Samples

This package only includes editor helpers. See `Samples~/README.md` for notes on adding lightweight example content when a package-specific sample is needed.

## Tests

Run the package's EditMode tests in Unity. Tests cover constants, style accessors, icon fallbacks, status badge helpers, and asset field API availability.

## License

See [LICENSE.md](LICENSE.md).
