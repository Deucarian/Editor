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
            var focus = DeucarianEditorWorkspaceControls.Region("control-center-focus", "dw-overview-focus");
            focus.AddToClassList("dw-focus--" + status.ToString().ToLowerInvariant());
            string icon = status == DeucarianControlCenterStatus.Error ? DeucarianEditorIconIds.Error
                : status == DeucarianControlCenterStatus.Warning ? DeucarianEditorIconIds.Warning : DeucarianEditorIconIds.Success;
            focus.Add(DeucarianEditorWorkspaceControls.Icon(icon));
            var text = DeucarianEditorWorkspaceControls.Region(null, "dw-focus-text");
            text.Add(DeucarianEditorWorkspaceControls.Label("PROJECT STATUS", "dw-eyebrow"));
            text.Add(DeucarianEditorWorkspaceControls.Label(attention == null
                ? "No issues reported" : "Needs your attention", "dw-focus-title"));
            text.Add(DeucarianEditorWorkspaceControls.Label(attention == null
                ? "Continue with a tool below."
                : DescribeAttention(attention), "dw-focus-description"));
            focus.tooltip = "Status reported by checks from installed packages.";
            var area = attention?.Area ?? DeucarianControlCenterArea.Project;
            string target = attention?.Id;
            var action = DeucarianEditorWorkspaceControls.Button(attention == null
                ? "View project checks" : "Review " + DeucarianControlCenterAreaIds.GetDisplayName(area), () => navigate(area, target));
            action.name = "control-center-focus-action";
            action.EnableInClassList("dw-primary", attention != null);
            text.Add(action);
            focus.Add(text);
            return focus;
        }

        internal static DeucarianControlCenterCard FindAttention(DeucarianControlCenterSnapshot snapshot)
        {
            DeucarianControlCenterCard result = null;
            foreach (var card in AllCards(snapshot))
            {
                if (card.Status < DeucarianControlCenterStatus.Warning || IsSummary(card)) continue;
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

        private static IEnumerable<DeucarianControlCenterCard> AllCards(DeucarianControlCenterSnapshot snapshot)
        {
            foreach (var card in snapshot.Cards) yield return card;
            foreach (var section in snapshot.Sections)
                foreach (var card in section.Cards) yield return card;
        }

        private static bool IsSummary(DeucarianControlCenterCard card) =>
            card.Id == "deucarian.readiness.overview" || TryGetSummaryArea(card.Id, out _);

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
