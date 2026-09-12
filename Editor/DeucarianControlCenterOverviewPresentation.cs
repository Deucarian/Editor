using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Deucarian.Editor
{
    internal static class DeucarianControlCenterOverviewPresentation
    {
        internal static VisualElement CreateFocus(DeucarianControlCenterSnapshot snapshot,
            Action<DeucarianControlCenterArea, string> navigate)
        {
            DeucarianControlCenterCard attention = FindAttention(snapshot);
            var status = attention?.Status ?? DeucarianControlCenterStatus.Success;
            var summary = new DeucarianEditorStatusSummary("control-center-focus");
            summary.Set(attention == null ? "No issues reported" : "Needs your attention",
                attention == null ? "Your installed package checks have no reported issues." : DescribeAttention(attention),
                status == DeucarianControlCenterStatus.Error ? DeucarianEditorStatus.Error
                : status == DeucarianControlCenterStatus.Warning ? DeucarianEditorStatus.Warning : DeucarianEditorStatus.Success);
            summary.Root.tooltip = "Status reported by checks from installed packages.";
            string target = attention?.Id;
            var action = DeucarianEditorWorkspaceControls.Button("Review project", () => navigate(DeucarianControlCenterArea.Project, target), true);
            action.name = "control-center-focus-action";
            summary.Actions.Add(action);
            return summary.Root;
        }

        internal static DeucarianControlCenterCard FindAttention(DeucarianControlCenterSnapshot snapshot)
        {
            DeucarianControlCenterCard result = null;
            foreach (var card in DeucarianProjectCheckReview.AllCards(snapshot))
            {
                if (card.Status < DeucarianControlCenterStatus.Warning || DeucarianProjectCheckReview.IsSummary(card)) continue;
                if (result == null || card.Status > result.Status) result = card;
            }
            return result;
        }

        internal static string DescribeAttention(DeucarianControlCenterCard card)
        {
            if (card.StatusText.Length > 0 && !string.Equals(card.StatusText, card.Status.ToString(), StringComparison.OrdinalIgnoreCase))
                return card.Title + " · " + card.StatusText;
            return card.Description.Length > 0 ? card.Description : card.Title;
        }

        internal static bool TryGetSummaryArea(string id, out DeucarianControlCenterArea area)
        {
            switch (id)
            {
                case "deucarian.overview.connections": area = DeucarianControlCenterArea.Connections; return true;
                case "deucarian.overview.build-packages": area = DeucarianControlCenterArea.BuildAndPackages; return true;
                case "deucarian.overview.diagnostics": area = DeucarianControlCenterArea.Diagnostics; return true;
                default: area = DeucarianControlCenterArea.Overview; return false;
            }
        }

        internal static VisualElement CreateSummary(DeucarianControlCenterCard card,
            Action<DeucarianControlCenterArea, string> navigate)
        {
            TryGetSummaryArea(card.Id, out var area);
            var button = DeucarianEditorWorkspaceControls.Button(string.Empty, () => navigate(area, null));
            button.name = "control-center-card-" + card.Id;
            button.AddToClassList("dw-overview-summary");
            button.tooltip = "Open " + card.Title;
            var text = DeucarianEditorWorkspaceControls.Region(null, "dw-tool-text");
            text.Add(DeucarianEditorWorkspaceControls.Label(card.Title, "dw-summary-title"));
            var status = DeucarianEditorWorkspaceControls.Label(card.StatusText, "dw-summary-status");
            status.AddToClassList("dw-card-status--" + card.Status.ToString().ToLowerInvariant());
            text.Add(status);
            button.Add(text);
            button.Add(DeucarianEditorWorkspaceControls.Icon(DeucarianEditorIconIds.ChevronRight));
            return button;
        }
    }
}
