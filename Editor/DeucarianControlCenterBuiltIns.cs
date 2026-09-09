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
                    "Registered tools remain owned by their packages and open as standalone workflows.",
                    PackageName,
                    DeucarianControlCenterStatus.Info,
                    tools.Count + " tool(s)",
                    -1000,
                    details,
                    actions: new[]
                    {
                        new DeucarianControlCenterAction("deucarian.editor.workspace-preview", "Open Editor UI Preview",
                            DeucarianEditorWorkspacePreviewWindow.Open,
                            "Inspect the shared editor layout using isolated sample content.")
                    },
                    searchTerms: new[]
                    {
                        "developer", "legacy", "shortcuts", "tools", "registry"
                    })
            };
        }
    }
}
