# Typed definition keys

## From a definition to a caller

`.g.cs` means generated C#. Unity compiles these files normally. The editor reads your project definitions and generates their named keys; the running player uses that compiled code without scanning assemblies to discover keys.

The path is: **source asset → generated key → code member or Inspector dropdown → configured runtime owner**.

For example, a custom audio role with display name `Confirm` and stable ID `ui.confirm` produces `Deucarian.Generated.ProjectAudioRoles.Confirm`, whose type is `AudioRoleKey`. A caller can pass it to `ThemeAudio.Play` directly, or initialize a serialized `AudioRoleKey` field with it and let a designer choose another declared role from the dropdown. The key carries the identity; the audio host and palette resolve the actual cue. A key does not contain or load an audio clip itself.

Create the source definition before using its generated member and let Unity finish compilation. A nonexistent member or a key of the wrong domain fails compilation. The scope must still contain the runtime definition and have its required services configured.

| Domain | Source asset | Generated set | Generated assembly |
| --- | --- | --- | --- |
| Custom audio roles | `DeucarianAudioRole` | `ProjectAudioRoles` | `Deucarian.GeneratedKeys.AudioRoleKey` |
| Screen routes | `UIFlowRoute` | `ProjectScreens` | `Deucarian.GeneratedKeys.ScreenKey` |
| Weapons | `WeaponDefinitionAsset` | `ProjectWeapons` | `Deucarian.GeneratedKeys.WeaponKey` |
| Attacks | `AttackDefinitionAsset` | `ProjectAttacks` | `Deucarian.GeneratedKeys.AttackKey` |
| Run upgrades | `RunUpgradeDefinitionAsset` | `ProjectUpgrades` | `Deucarian.GeneratedKeys.UpgradeKey` |

## Definitions and serialization

Each domain owns its serializable key type and the definitions that populate it. Ordinary callers accept that domain key, never a raw ID. A public, marked definition set exposes named typed values for code; the same values populate its Inspector dropdown. Persisted fields store only the stable ID, with no asset reference. Generic keys retain the payload type in code and in the picker.

Code-first definitions are declared once in a public runtime key set. Keep each stable ID unique. Do not generate IDs per caller or expose a public string constructor/conversion. Define project identifiers in the project, not in the shared Editor package. Existing value enums remain enums; runtime-created objects use handles issued by their owning scope.

Asset-authored audio roles, screen routes, weapons, attacks and upgrades have domain-owned `DeucarianAssetKeySource` providers. Importing or editing a source asset under Assets generates its `Project*` code set beneath `Assets/DeucarianGeneratedKeys`. The generated domain assembly references the domain's key assembly. Callers with their own asmdef add a reference to `Deucarian.GeneratedKeys.<KeyTypeName>`, for example `Deucarian.GeneratedKeys.WeaponKey`, alongside the package assemblies their code uses; Assembly-CSharp sees it automatically. Commit source definitions, generated `.g.cs` and `.asmdef` files, and their `.meta` files together so builds are reproducible. Edit the source definitions instead of generated files, which are replaced during regeneration.

The stable ID is independent of the display name. Changing a definition's display name changes its generated member name after regeneration, causing old code references to fail compilation, while existing serialized selections retain their ID. Renaming an asset file preserves an explicitly authored display name and ID. A UIFlowRoute with an empty display name uses the asset name as its fallback label, so set an explicit name when code members must survive file renames. Deleting a definition removes its code member and marks serialized selections as missing. Duplicate IDs or code names report the source and repair. Let Unity finish compilation after imports before building.

## Recovery and build validation

If generated code is missing or stale, reimport a source definition. Check that it is under `Assets`, its ID is unique, and its display name produces a unique code member. Generation is paused during Play Mode and builds. A dedicated test harness can also disable automatic refresh through `DeucarianKeyGeneration.AutomaticRefreshDisabledSessionKey`; restore normal refresh before interactive definition authoring. Domain Inspectors surface generation errors with repair guidance. Editor integrations can explicitly call `DeucarianKeyGeneration.Refresh(source)` and `Validate(source)` when they own an import workflow.

The picker is shared Editor UI; domain drawers own the type and repair text. Discovery/reflection happens only in the editor. Build callbacks validate generated source and selected keys in included scenes, their dependencies, preloaded assets and Resources. Custom content pipelines can call `DeucarianKeyValidation.Validate` for additional content such as separately built bundles. Runtime code never scans assemblies to find keys.

A key guarantees an existing declared identity and the correct domain/payload type. The scope must still be configured with the corresponding runtime definition. Missing hosts and bindings report what to configure. Expired runtime targets, unavailable resources and cancellation remain explicit outcomes. Type safety does not prove scene wiring, external connectivity or content delivery.
