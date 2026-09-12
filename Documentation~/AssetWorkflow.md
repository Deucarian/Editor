# Shared asset workflow

Use one asset control per logical field, with an owner-supplied read/write pair. `AssetWithActions` is convenient for forms whose context stays attached; retain a `DeucarianEditorAssetField` when dependent content is rebuilt and reattach its `Root`. Do not recreate the native ObjectField on each value change: Unity's open selector still belongs to the original field.

## Responsibilities

| Editor owns | Domain package owns |
| --- | --- |
| Field/action styling, origin hints and cached Assets + Packages discovery | Compatible types, meaningful defaults and canonical runtime binding |
| Choose, None, Create, Customize and Use default affordances | Creation, validation and deep-copy policy |
| Reusable read-only package bindings | Explicit application to a scene, connection, runtime or project |

Defaults must match the owning package's actual behavior. Preserve explicit choices; do not take an arbitrary first search result. Optional scene references remain optional. Unconfigured deployment hosts and credentials stay empty.

Create and Customize are explicit actions with a project save location. Merely viewing a page must not create assets. The generic copy helper preserves Unity asset references; owners with editable child assets must supply a domain-specific deep copy. Theming copies editable palettes/profiles but shares clips, fonts and semantic roles.

Use `DeucarianEditorAssetCatalog` for a lazily queried type index. Invalidate on relevant project changes; never rescan on repaint. The reusable field manages its subscription while attached.

## Acceptance checks for consumers

- Select A, B, C and None while the native picker remains open; the displayed value and dependent preview must follow every selection.
- Rebuild dependent content without replacing the owning ObjectField.
- Find relevant bundled assets as well as project assets.
- Cancel Create/Customize without changing the selection or creating a file.
- Keep package assets read-only; a project copy must not mutate its source.
- Keep explicit Apply/Bind/Build operations separate from selecting a draft.
- Check narrow/wide windows and all supported workspace scales.

Overview search rows use the same workspace typography and scale system as other pages rather than legacy small Unity labels.
