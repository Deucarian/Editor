# Deucarian Editor

## In-window navigation

The left sidebar changes pages in the current window, keeping each page's draft and session alive. Right-click a sidebar item and choose **Open in new window** for an independent workspace. Closing a workspace releases its pages; ordinary page changes do not reset lab messages or stop package operations.


## Shared workspace

Shared responsive workspace, searchable list/detail surfaces, form bindings, stable audio navigation and Control Center migration. Visual tokens remain owned here; domain operations stay in consumers.

Requires Editor 1.5.2 or newer. Development is delivered through Git `#develop`; this change does not promote the stable `#main` channel.

Current package version: `1.9.0`.

Feature setup pages compose `DeucarianEditorFeatureSection`: the shared section owns switch, status, details, actions and responsive styling; the domain supplies saved state and commands. `SetState` never invokes a command. Unknown connections use an empty ring, not a success checkmark. Register concise `navigationLabel` values and an optional `navigationGroupIcon` while preserving stable tool IDs and full display names.

All editor surfaces share the workspace's charcoal/teal palette and DINish typography. New pages use workspace controls; existing IMGUI forms use `DeucarianEditorInputGUI`, `DeucarianEditorTextGUI` and `DeucarianEditorActionGUI`. These helpers own visual treatment without changing package actions. Do not copy styles into consuming packages or mutate Unity's shared `EditorStyles`.

Custom inspectors can return `DeucarianEditorInspector.Create(OnInspectorGUI)` from `CreateInspectorGUI`. This adds shared presentation without a second sidebar and preserves native serialized property editing. Embedded legacy windows pass their owner to the header, footer and settings-page helpers so the workspace remains the only page header.

## Reading and scaling the workspace

The overview starts with one project-status focus, then short area statuses and recent or pinned tools. Full source details stay in their owning sections and search; a status action navigates to the relevant check, never runs its fix automatically.

Use **UI scale** in the footer to resize the complete workspace from 75% to 150%. The new 100% is the compact size previously labelled 75%. Click the percentage to reset to that default. The slider stays in a fixed, unscaled footer while the workspace resizes. Existing scale preferences start at the new default once, then retain subsequent choices. The preference is local to this project, persists across editor restarts, and applies to other shared workspace pages and windows. It does not change Unity's own menus, unrelated inspectors, runtime UI, or theme assets.

## What this is

`com.deucarian.editor` is an editor-only Unity package for shared Deucarian editor tooling, branded editor chrome, fixed icons, layout helpers, and UX standards.

This package is not runtime theming. Its visual theme is package-owned. Deucarian editor windows use package-owned Tideline light/dark colors, DINish interface typography, and brand resources so every Deucarian package presents the same quiet, readable editor experience. The editor treatment follows the active Unity skin.

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

Create a package-owned standalone editor window and draw the shared header plus package fields:

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

## Deucarian Control Center

Open `Tools/Deucarian/Control Center...` to review project readiness, search installed capabilities, and deep-link to the standalone workflows owned by each package. Packages contribute sanitized cards, sections, and tools through `DeucarianControlCenterRegistry` and `DeucarianToolRegistry`; the shell does not copy domain state or workflow logic.

Use `DeucarianControlCenterWindow.Open(area)` or a stable tool ID for cross-package navigation. `DeucarianProjectSetupWindow` remains as an obsolete source-compatible redirect for the 1.2 migration release.

## Samples

This package only includes editor helpers. See `Samples~/README.md` for notes on adding lightweight example content when a package-specific sample is needed.

## Public API map

- `DeucarianEditorChangeReview`, `DeucarianEditorChangeItem`, and `DeucarianEditorHistoryItem`: shared review layout, caller-selected staged/unstaged rows, read-only diff, contextual forms/actions, and compact optional history. Added in 1.8.1; contains no Git commands or package state.

- `DeucarianControlCenterWindow`, `DeucarianControlCenterRegistry`, and `DeucarianToolRegistry`: the responsive ecosystem shell, explicit status contributions, stable navigation IDs, search, and deep links.

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
- `DeucarianEditorWorkbenchGUI`: shared IMGUI styles, surface colors, panel scopes, separators, key/value rows, legacy marker rows, and Lucide status rows. Standard actions are 36 px; primary actions are 42 px and form rows are 32 px before workspace scaling.
- `DeucarianEditorDialog`: branded responsive callback-based utility dialogs with a Lucide icon, wrapped message/details content, icon-bearing actions, and deterministic Enter/Escape/window-close completion.
- `DeucarianEditorCardScope`, `DeucarianEditorFoldoutScope`, and `DeucarianEditorWorkbenchPanelScope`: exception-safe layout scopes for composable drawing.

## Integrations

Works with:

- Deucarian editor windows and inspectors in other packages,
- package-owned UI Toolkit files that need shared resources,
- tools registered under stable IDs for Control Center discovery.

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

Everyday navigation is deliberately small and task-oriented:

- `Tools/Deucarian/Control Center...` opens the ecosystem overview.
- Mature package workflows remain standalone and register a stable tool ID.
- Only approved high-frequency entries belong directly under `Tools/Deucarian`.
- Developer and migration discovery belongs under `Tools/Deucarian/Advanced`.

Do not integrate packages through literal menu strings or mirror Package Registry taxonomy in menus. Use `DeucarianToolRegistry.TryOpen`, a stable tool ID, or an explicit public open API. Menu aliases are compatibility conveniences, not cross-package contracts.

### Actions sections

Actions sections should contain real actions such as create, apply, scan, repair, import, export, or refresh.

Do not use actions sections for selecting assets already visible in object fields.

## Troubleshooting

### Editor Component Gallery

Open **Control Center → Developer → Editor Component Gallery** to try shared
fields, buttons, disclosures, status styles and empty states. Its local examples
save nothing and expose no application settings. Notification testing has one
home: **Notifications → Notification Lab**, contributed by Notifications.

Each Control Center window owns its sidebar expansion and scroll state. Cached
pages retain their controls and drafts; clicking the current page does nothing
unless the action supplies an explicit route. Page activation must refresh bound
data without rebuilding controls or replacing user selections. The old page is
deactivated before the next page activates; failed activation is cleaned up and
the previous page reactivated. Page adapters must make these lifecycle operations
safe to repeat and release their owned resources when disposed.

The workspace composes the existing workbench with shared navigation, heading,
tabs, destination, form/preview and footer regions. Its stylesheet owns the
colours, typography, spacing, responsive layout and interaction states. Consumers
supply their content and actions; they must not duplicate these visual rules.

Use `DeucarianEditorWorkspace` in an editor window, add content through its named
regions, and dispose it before rebuilding or closing the window. Reuse
`DeucarianEditorWorkspaceControls`, `DeucarianEditorChoiceBar` and
`DeucarianEditorMessageRow` for shared presentation. Choice selection can be
synchronized silently; message progress is supplied by the caller, not a timer
owned by Editor. Do not use the internal specimen as a production adapter.

For a live test tool, `DeucarianEditorLabWorkspace` owns the Test/Appearance/Audio
pages, destination controls and keyed preview rows. Bind field values and commands
through `DeucarianEditorWorkspaceForm`. Pass caller-selected visible and overflow
rows as `DeucarianEditorMessageData`; Editor does not decide lifetimes, priorities,
queue limits or runtime targeting. Refreshing row state preserves controls and
focus instead of recreating them for every countdown tick.

Migrating a live tool requires an adapter in its owning package. The adapter
retains domain state, commands, subscriptions and cleanup; **all layout and
styling stay in Editor**. Editor does not acquire dependencies on Notifications,
Theming, Installer or other domain packages.

### Package boundaries

- If runtime code needs this package, stop and check the ownership boundary; Editor is editor-only.
- If a package wants custom runtime colors, use the runtime theming owner instead of editor shell tokens.
- Register package-specific tools with a stable `DeucarianToolRegistry` ID and contribute them to Control Center; add a global menu entry only when `menu-policy.json` explicitly approves it.
- If a UI Toolkit asset is package-specific, keep it in the owning package rather than moving it here.

## Validation

### Change review in a Control Center page

Create `DeucarianEditorChangeReview(workspace.Content)` once in a plain
`IDeucarianEditorPage`. Bind repository identity and explicit actions through
`Context` and `Actions`, and draft/commit fields through `Commit`; each is a
`DeucarianEditorWorkspaceForm`. The existing workspace owns navigation and the
fixed UI scale footer. The review owns no window or additional scale control.

`SetChanges(items, inspectedId)` updates stable keyed rows in caller order.
Each `DeucarianEditorChangeItem` supplies `id`, `path`, `status`, `staged`,
`selected`, an `inspect` callback, a `select(bool)` callback, optional
`relatedPath` text (such as a rename or Unity meta partner), and
`selectionEnabled`. Selecting a checkbox only asks the caller to change the
review selection; staging must remain an explicit domain action. Use distinct
IDs when a file has both staged and unstaged changes. Refreshes do not call
commands or recreate surviving controls. Preserve selection in the caller's
session and call `RefreshForms()` to synchronize form values.

`SetSummary(text)` shows the owner's staged/selected summary.
`SetDiff(title, content, binary, truncated)` displays plain, copyable, read-only
text, with an explicit notice for binary changes or incomplete output.
`SetHistory(items)` updates a collapsed, display-only recent-history list.
All text supplied by the caller must already be sanitized; Editor performs no
repository discovery, source parsing, credential removal, or validation.

The review renders at most 500 change rows, 65,536 diff characters, and 20
history entries, and reports display truncation visibly. Callers should bound
their own I/O and support filtering or an external-client handoff. Row captions
are also bounded and never interpreted as rich text. Layout changes preserve
drafts, checkbox state and unchanged diff selection; narrow columns stack the
list and diff inside the page's scroll area. Dispose the review with its page.

EditMode tests cover duplicate IDs, refresh/no-command behavior, caller-owned
selection, changed callback binding, display limits, binary notices, disposal,
and live normal/narrow/scaled layout with focus and draft preservation.

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

## In-window navigation and submenus

Every registered tool contributes a fresh `IDeucarianEditorPage` through `createPage`. Its factory must not call `Show`, `GetWindow`, or run a domain command. Page sessions cache instances per native window and dispose them with that window.

Use `navigationPath` for readable submenu groups, for example `"Experience/Audio"`. Omitting it uses the tool's registered area. The shared sidebar discovers installed registrations, preserves parent groups during filtering, and offers an explicit **Open in new window** context action. Runtime-only packages do not need artificial menus.

A page button calls `DeucarianEditorNavigation.Open(sourceElement, toolId, route)`. Card actions declare `navigationToolId` and optional `navigationRoute`; project checks declare `setupToolId` and optional `setupRoute`. Search carries the same destination. Non-navigation actions retain explicit execution and confirmation behavior. Do not use a global current-window lookup.

Older IMGUI tools can compose `DeucarianEditorImGuiPage` while retaining their domain renderer. New tools should use plain composed pages and shared workspace controls. Native file pickers, confirmations, and deliberately external documentation links remain explicit external interactions.
