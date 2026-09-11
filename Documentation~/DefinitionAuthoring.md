# Reusable definitions, typed callers

Create reusable content once and select it from C# or an Inspector dropdown.
The owning package supplies its asset type, defaults, validation and runtime
projection. Editor supplies the authoring interface, synchronization and code
generation. Runtime services still own active state and lifetimes.

## Create through the editor

1. Open **Control Center → Developer → Definitions**, then select the domain.
   Domain Create menus and integrated Labs use the same workflow. Notifications
   also exposes this page in the Notification Lab; audio roles in Audio Palette Lab.
2. Choose **Create**, enter a useful name and complete the fields. A notification
   includes its severity, title, message, lifetime and sound policy. References
   such as a reward's currency select existing definition assets.
3. Save and synchronize. The editor maintains the asset, its editable declaration,
   the project catalog and generated typed keys. Let Unity finish compiling.
4. Select the definition in a package component's typed dropdown, or use its
   generated property from C#. Assign scene hosts once; callers do not create
   stores, presenters, pools or sessions for each operation.

Existing project assets appear in Definitions. **Enable code editing** adopts an
existing asset without replacing its GUID. Duplicate in Definitions to create a
new identity. Rename the display name to change its generated property name.

## Create and edit through code

The editable source is an **Editor-only `.definition.cs` declaration**. It is a
small, deliberately constrained C# data initializer. It supports strings,
booleans, finite numbers, enums/flags, arrays, inline sections and exact Unity
asset references. Arbitrary methods, constructors with side effects and runtime
expressions are not evaluated by synchronization.

For a first definition, the domain Create menu produces a valid template with
the necessary assembly references. Open that declaration and edit its values.
To add another definition in code, copy the template into the same Editor
folder, give the file, declaration class and `Name` distinct names, and assign a
new stable `Id` (for example a new GUID). The synchronizer creates its asset and
refreshes the catalog and caller keys after import. Preserve the frame and
`definition-value` markers. Keep application methods in ordinary C# files.

The synchronizer formats owned declarations. `.g.cs` caller accessors are outputs;
do not edit them. A typical caller is:

```csharp
using Deucarian.Generated;
using Deucarian.Notifications;

NotificationManager.Show(ProjectNotifications.ConnectionLost);
NotificationManager.Resolve(ProjectNotifications.ConnectionLost);
```

The property exists after a notification named `ConnectionLost` has been created.
The same `NotificationKey` can be a `[SerializeField]` selected in the Inspector.
A key identifies a definition; it is not a reference to an active popup.

## Which files belong to whom?

| File | Purpose | Edit it? |
| --- | --- | --- |
| Definition `.asset` and subassets | Serialized defaults and asset references | Yes, through the Inspector or definition editor |
| `Editor/*.definition.cs` | Editable declaration of those same defaults | Yes, within the declaration grammar |
| `Assets/DeucarianGeneratedKeys/<KeyType>/*.g.cs` | Named, typed C# accessors | Generated |
| Generated key/authoring `.asmdef` | Runtime/editor boundaries and direct references | Generated; keep custom assemblies outside their owned folders |
| Generated project catalog in Resources | Explicit runtime lookup without assembly scanning | Generated |
| `ProjectSettings/DeucarianDefinitions.json` | Last synchronized identities, GUIDs and hashes | Managed by the editor; include in project version control |

Commit definition assets, declarations and their `.meta` files together. Include
generated outputs so a checkout has its compilation inputs. Do not commit Library.
If your caller uses a custom assembly definition, reference the owning runtime
assembly and `Deucarian.GeneratedKeys.<KeyType>` for generated static accessors.
For example: `Deucarian.Notifications` and
`Deucarian.GeneratedKeys.NotificationKey`. Inspector-only callers need the owning
runtime assembly, without a reference to a generated accessor assembly.

## Conflicts, deletion and errors

Edits to only one side update the other. If both the asset and code changed since
their last agreement, synchronization stops. Review both versions, then choose
**Keep code** or **Keep asset**. It does not decide which edit you intended.

An established ID cannot be changed in place. A removed asset is not silently
recreated for an existing synchronized record. Restore it or remove the definition
through Definitions. Use reference inspection before deletion; callers of removed
generated properties will need updating. Name collisions after C# sanitization
are rejected with a request to choose distinct names.

Build validation rejects stale declarations, conflicts, missing required assets
and stale catalogs. Compile-time types catch the wrong key domain or payload type;
they cannot prove that a scene host is configured or that an external service will
succeed. Those failures remain explicit results or actionable exceptions.

## Content and contracts

Definition assets fit reusable content: notifications, audio, screens, gameplay
definitions and placement policies. API request DTOs, command handlers, save
migrations, authentication providers and runtime instances retain explicit C#
contracts. Their sample startup components show that composition once. Generic
package components can close over those types and reuse the same typed keys and
services; assets do not invent business logic or authentication credentials.

The typed convenience layer does not remove every legacy ID from advanced
composition. Some recipe formats retain raw identifiers, including opaque
references to a higher-level package that the core cannot depend on without a
cycle. Use the typed host/trigger entry points for ordinary calls. A generated
key proves its domain at compile time; catalog readiness and scene validation
still check that the referenced content is available in the configured scope.

## Samples

In Package Manager, import a runtime package's **Definition Workflow** sample.
Open its `DefinitionWorkflow.unity` scene and enter Play mode. Each sample contains
its caller, configured hosts, necessary local assets and any explicit startup
composition. Sample definitions generate project keys after import. Mock service
adapters are identified in their scene instructions and use no real backend.
