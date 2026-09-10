using System;
using System.Collections.Generic;
using UnityEditor;

namespace Deucarian.Editor
{
    [InitializeOnLoad]
    internal sealed class DeucarianControlCenterBuiltIns :
        IDeucarianControlCenterCardProvider
    {
        private const string PackageName = "com.deucarian.editor";
        private static readonly DeucarianControlCenterBuiltIns Provider =
            new DeucarianControlCenterBuiltIns();
        private static readonly IDisposable ProviderRegistration;
        private static readonly IDisposable ToolRegistration;

        static DeucarianControlCenterBuiltIns()
        {
            DeucarianToolRegistry.Register(new DeucarianToolDescriptor("deucarian.editor.workspace-preview",
                "Editor Component Gallery", "Explore shared fields, buttons and status styles. No application settings.",
                DeucarianControlCenterArea.Developer, DeucarianEditorWorkspacePreviewWindow.Open, PackageName,
                createPage: DeucarianEditorWorkspacePreviewWindow.CreatePage));
            ProviderRegistration =
                DeucarianControlCenterRegistry.RegisterCardProvider(Provider);
            ToolRegistration = DeucarianToolRegistry.Register(
                new DeucarianToolDescriptor(
                    DeucarianToolIds.ControlCenter,
                    "Deucarian Control Center",
                    "Review project readiness and open installed Deucarian tools.",
                    DeucarianControlCenterArea.Overview,
                    () => DeucarianControlCenterWindow.Open(),
                    PackageName,
                    DeucarianEditorIconIds.Dashboard,
                    new[] { "project", "setup", "dashboard", "tools" },
                    -1000, createPage: DeucarianControlCenterWindow.CreatePage));
        }

        public string Id => "deucarian.editor.control-center";

        public IEnumerable<DeucarianControlCenterCard> Capture(
            DeucarianControlCenterContext context)
        {
            IReadOnlyList<DeucarianToolDescriptor> tools =
                DeucarianToolRegistry.GetTools();
            var details = new List<string>
            {
                tools.Count + " registered tool(s)",
                "Use stable tool IDs or explicit open APIs for cross-package navigation.",
                "Legacy menu shortcuts are discoverable here for one migration release."
            };
            return new[]
            {
                new DeucarianControlCenterCard(
                    "deucarian.tools.discovery",
                    DeucarianControlCenterArea.Developer,
                    "Developer tool discovery",
                    "Registered package tools open as pages in this workspace.",
                    PackageName,
                    DeucarianControlCenterStatus.Info,
                    tools.Count + " tool(s)",
                    -1000,
                    details,
                    actions: new[]
                    {
                        new DeucarianControlCenterAction("deucarian.editor.workspace-preview", "Open Component Gallery",
                            DeucarianEditorWorkspacePreviewWindow.Open,
                            "Inspect the shared editor layout using isolated sample content.", navigationToolId: "deucarian.editor.workspace-preview")
                    },
                    searchTerms: new[]
                    {
                        "developer", "legacy", "shortcuts", "tools", "registry"
                    })
            };
        }
    }
}
