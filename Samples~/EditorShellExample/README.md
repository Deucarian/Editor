# Editor Shell Example

This sample composes a native tool page from the shared workspace, form, feature section, and button controls.

After importing the sample, open **Tools > Deucarian > Control Center...** and choose **Developer > Shell example**. Navigation stays inside the current window. `EditorShellExampleWindow.Open()` is an explicit standalone entry point; that window also hosts in-place navigation.

Use `EditorShellExampleView.CreatePage()` as the reference: the page owns its draft and disposes its workspace when the host closes. A host can deactivate and reactivate it without rebuilding controls or discarding the draft. `Create()` remains a simple single-use view helper and disposes on detach.
