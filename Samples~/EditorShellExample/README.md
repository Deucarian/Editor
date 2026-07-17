# Editor Shell Example

This sample composes a small editor window from the shared Deucarian visual shell, package header, and panel primitives.

After importing the sample, open **Tools > Deucarian > Samples > Editor Shell Example**. Use `EditorShellExampleView.Create()` as a compact reference for keeping package-specific controls inside the shared editor chrome.

The view builder is separate from the `EditorWindow`, which keeps the composition straightforward to exercise in EditMode tests.
