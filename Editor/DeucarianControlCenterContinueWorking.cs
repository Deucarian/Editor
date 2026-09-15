using System;
using System.Collections.Generic;
using UnityEngine.UIElements;
using Controls = Deucarian.Editor.DeucarianEditorWorkspaceControls;

namespace Deucarian.Editor
{
    internal static class DeucarianControlCenterContinueWorking
    {
        internal static VisualElement Create(DeucarianControlCenterSnapshot snapshot, VisualElement navigationSource)
        {
            var root = Controls.Region("control-center-continue", "dw-continue");
            var tools = new List<DeucarianToolDescriptor>();
            Add(snapshot, tools, "deucarian.theming.project-setup");
            Add(snapshot, tools, "deucarian.notifications.lab");
            Add(snapshot, tools, DeucarianToolIds.PackageInstaller);
            foreach (var tool in Recent(snapshot))
                if (tools.Count < 3 && !tools.Contains(tool)) tools.Add(tool);
            if (tools.Count != 0) root.Add(Controls.Label("Continue working", "dw-section-title"));
            foreach (var tool in tools)
            {
                var row = Controls.Button(string.Empty, () => DeucarianEditorNavigation.Open(navigationSource, tool.Id));
                row.name = "control-center-continue-" + tool.Id;
                row.AddToClassList("dw-continue-row");
                row.tooltip = tool.Description;
                row.Add(Controls.Icon(Icon(tool)));
                row.Add(Controls.Label(Title(tool), "dw-continue-title"));
                row.Add(Controls.Label(Status(snapshot, tool), "dw-continue-status"));
                var arrow = Controls.Icon(DeucarianEditorIconIds.ChevronRight);
                arrow.AddToClassList("dw-continue-arrow");
                row.Add(arrow);
                root.Add(row);
                DeucarianEditorResponsiveLayout.AdaptToWidth(row, "dw-continue-stacked", 800);
            }
            var recent = Recent(snapshot);
            if (recent.Count == 0) return root;
            root.Add(Controls.Divider());
            root.Add(Controls.Label("Recent tools", "dw-section-title"));
            for (int i = 0; i < Math.Min(3, recent.Count); i++)
            {
                var tool = recent[i];
                var link = Controls.Button(string.Empty, () => DeucarianEditorNavigation.Open(navigationSource, tool.Id), DeucarianEditorButtonRole.Quiet);
                link.AddToClassList("dw-recent-tool");
                link.Add(Controls.Label(tool.DisplayName));
                link.Add(Controls.Icon(DeucarianEditorIconIds.ChevronRight));
                link.tooltip = tool.Description;
                root.Add(link);
            }
            return root;
        }

        private static void Add(DeucarianControlCenterSnapshot snapshot, List<DeucarianToolDescriptor> result, string id)
        {
            foreach (var tool in snapshot.Tools)
                if (tool.Id == id && tool.CreatePage != null) { result.Add(tool); return; }
        }

        private static List<DeucarianToolDescriptor> Recent(DeucarianControlCenterSnapshot snapshot)
        {
            var result = new List<DeucarianToolDescriptor>();
            foreach (var tool in snapshot.Tools)
                if (tool.Id != DeucarianToolIds.ControlCenter && tool.CreatePage != null &&
                    (DeucarianToolHistory.RecentIndex(tool.Id) >= 0 || DeucarianToolHistory.IsFavorite(tool.Id))) result.Add(tool);
            result.Sort((left, right) =>
            {
                int pin = DeucarianToolHistory.IsFavorite(right.Id).CompareTo(DeucarianToolHistory.IsFavorite(left.Id));
                if (pin != 0) return pin;
                int a = DeucarianToolHistory.RecentIndex(left.Id), b = DeucarianToolHistory.RecentIndex(right.Id);
                return (a < 0 ? int.MaxValue : a).CompareTo(b < 0 ? int.MaxValue : b);
            });
            return result;
        }

        private static string Title(DeucarianToolDescriptor tool) => tool.Id == "deucarian.theming.project-setup" ? "Theming"
            : tool.Id == "deucarian.notifications.lab" ? "Notifications"
            : tool.Id == DeucarianToolIds.PackageInstaller ? "Packages" : tool.DisplayName;
        private static string Icon(DeucarianToolDescriptor tool) => tool.Id == "deucarian.notifications.lab"
            ? DeucarianEditorIconIds.Notifications : tool.IconKey;
        private static string Status(DeucarianControlCenterSnapshot snapshot, DeucarianToolDescriptor tool)
        {
            if (tool.Id == "deucarian.notifications.lab") return "Test messages and presentation";
            foreach (var card in snapshot.Cards)
                if (card.OwningPackage == tool.OwningPackage && !string.IsNullOrEmpty(card.StatusText)) return card.StatusText;
            foreach (var section in snapshot.Sections)
                foreach (var card in section.Cards)
                    if (card.OwningPackage == tool.OwningPackage && !string.IsNullOrEmpty(card.StatusText)) return card.StatusText;
            return tool.Description;
        }
    }
}
