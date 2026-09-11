using System;
using System.Collections.Generic;
using UnityEngine.UIElements;
using Ui = Deucarian.Editor.DeucarianEditorWorkspaceControls;

namespace Deucarian.Editor
{
    internal static class DeucarianControlCenterAdvancedPresentation
    {
        internal static void Build(VisualElement root, DeucarianControlCenterSnapshot snapshot,
            DeucarianControlCenterArea area, string focusedId, Action refresh,
            Action<DeucarianControlCenterAction> execute, IDictionary<string, bool> expandedChecks)
        {
            bool checks = area == DeucarianControlCenterArea.Project;
            var heading = Ui.Region(null, "dw-section-heading-row");
            heading.Add(Ui.Label(checks ? "Project checks" : "Developer tools", "dw-section-title"));
            if (checks)
            {
                var run = Ui.IconButton("Run checks", DeucarianEditorIconIds.Play, refresh, DeucarianEditorButtonRole.Primary);
                run.name = "control-center-run-checks"; heading.Add(run);
            }
            root.Add(heading);
            var cards = new List<DeucarianControlCenterCard>(snapshot.GetCards(area));
            if (checks)
            {
                var status = DeucarianControlCenterStatus.Success;
                foreach (var card in cards) if (card.Status > status) status = card.Status;
                var line = Ui.Region("advanced-check-status", "dw-inline-status");
                line.Add(Ui.Icon(status == DeucarianControlCenterStatus.Error ? DeucarianEditorIconIds.Error
                    : status == DeucarianControlCenterStatus.Warning ? DeucarianEditorIconIds.Warning : DeucarianEditorIconIds.Success));
                line.Add(Ui.Label(status >= DeucarianControlCenterStatus.Warning ? "Checks need your attention." : "No issues reported."));
                line.AddToClassList("dw-card-status--" + status.ToString().ToLowerInvariant()); root.Add(line);
            }
            if (cards.Count > 0)
            {
                var list = Ui.Region("advanced-check-list", "dw-navigation-list"); root.Add(list);
                foreach (var card in cards)
                {
                    string id = card.Id;
                    bool expanded = expandedChecks.TryGetValue(id, out var value) ? value : id == focusedId;
                    AddCheck(list, card, expanded, execute, next => expandedChecks[id] = next);
                }
            }
            if (checks) root.Add(Ui.Label("Other tools", "dw-section-title"));
            var tools = Ui.Region("advanced-tool-list", "dw-navigation-list"); root.Add(tools);
            foreach (var tool in snapshot.Tools)
            {
                if (tool.Id == DeucarianToolIds.ControlCenter || tool.Area != DeucarianControlCenterArea.Developer) continue;
                var captured = tool;
                var row = Ui.Button(string.Empty, () => DeucarianEditorNavigation.Open(root, captured.Id));
                row.name = "control-center-open-" + tool.Id; row.tooltip = tool.Description;
                row.AddToClassList("dw-navigation-row"); row.Add(Ui.Icon(tool.IconKey));
                row.Add(Ui.Label(tool.DisplayName, "dw-navigation-title"));
                row.Add(Ui.Icon(DeucarianEditorIconIds.ChevronRight)); tools.Add(row);
            }
            if (tools.childCount == 0) root.Add(Ui.Label("No developer tools are installed.", "dw-note"));
        }

        private static void AddCheck(VisualElement list, DeucarianControlCenterCard card, bool expanded,
            Action<DeucarianControlCenterAction> execute, Action<bool> changed)
        {
            var item = Ui.Region("control-center-card-" + card.Id, "dw-check-item"); list.Add(item);
            item.style.flexBasis = StyleKeyword.Auto; item.style.flexGrow = 0;
            var details = Ui.Region(null, "dw-check-details");
            var row = Ui.Button(string.Empty, () =>
            {
                expanded = !expanded; changed(expanded); Ui.Show(details, expanded);
                item.EnableInClassList("dw-check-expanded", expanded);
            });
            row.name = "control-center-check-" + card.Id; row.AddToClassList("dw-navigation-row");
            row.Add(Ui.Label(card.Title, "dw-navigation-title"));
            var status = Ui.Label(card.StatusText, "dw-navigation-status");
            status.AddToClassList("dw-card-status--" + card.Status.ToString().ToLowerInvariant()); row.Add(status);
            row.Add(Ui.Icon(DeucarianEditorIconIds.ChevronRight)); item.Add(row);
            if (!string.IsNullOrEmpty(card.Description)) details.Add(Ui.Label(card.Description, "dw-note"));
            foreach (string detail in card.Details) details.Add(Ui.Label(detail, "dw-note"));
            var actions = Ui.Actions();
            foreach (var action in card.Actions)
            {
                var captured = action; var button = Ui.Button(action.Label, () => execute(captured));
                button.name = "control-center-action-" + action.Id; button.tooltip = action.Description; actions.Add(button);
            }
            details.Add(actions); item.Add(details); Ui.Show(details, expanded);
            item.EnableInClassList("dw-check-expanded", expanded);
        }
    }
}
