# Editor Shell Example

This sample composes a small editor window from the shared Deucarian visual shell, package header, and panel primitives.

After importing the sample, open **Tools > Deucarian > Control Center...** and choose **Developer > Tools > Editor Shell Example**. The sample's explicit `DeucarianToolRegistry` registration is a compact reference for discoverable standalone tools without extra global menus.

Use `EditorShellExampleView.Create()` as a reference for keeping package-specific controls inside shared editor chrome. The view builder remains separate from the `EditorWindow` so the composition is straightforward to exercise in EditMode tests.
