using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Deucarian.Editor
{
    internal sealed partial class DeucarianControlCenterView
    {


        internal DeucarianControlCenterView(
            Action<DeucarianControlCenterArea, string> navigateAction,
            Action refreshAction)
        {
            navigate = navigateAction ??
                throw new ArgumentNullException(nameof(navigateAction));
            refresh = refreshAction ??
                throw new ArgumentNullException(nameof(refreshAction));
            Root = new VisualElement { name = "control-center-view" };
            Root.AddToClassList("dw-control-center");
        }

        internal VisualElement Root { get; }

        internal void Render(
            DeucarianControlCenterSnapshot snapshot,
            DeucarianControlCenterArea selectedArea,
            string focusedTargetId,
            string query)
        {
            bool samePage = content != null && renderedArea == selectedArea && renderedQuery == query;
            Vector2 scrollOffset = samePage ? content.scrollOffset : Vector2.zero;
            string focusedName = samePage ? (Root.focusController?.focusedElement as VisualElement)?.name : null;
            renderedArea = selectedArea;
            renderedQuery = query;
            int revision = ++renderRevision;
            cardHosts.Clear();
            searchResults.Clear();
            searchButtons.Clear();
            selectedSearchResult = -1;
            if (layout == null)
            {
            layout = new VisualElement { name = "control-center-layout" };
            layout.AddToClassList("dw-control-layout");
            Root.Add(layout);

            content = DeucarianEditorWorkspaceControls.Scroll("control-center-content");
            layout.Add(content);
            }
            sidebar?.RemoveFromHierarchy();
            sidebar = CreateSidebar(snapshot, selectedArea);
            layout.Insert(0, sidebar);
            content.Clear();

            if (string.IsNullOrWhiteSpace(query))
            {
                RenderArea(
                    content,
                    snapshot,
                    selectedArea,
                    focusedTargetId);
            }
            else
            {
                RenderSearch(content, snapshot, query.Trim());
            }

            SetLayoutMode(layoutMode);
            content.scrollOffset = scrollOffset;
            Root.schedule.Execute(() =>
            {
                if (revision != renderRevision) return;
                content.scrollOffset = scrollOffset;
                if (!string.IsNullOrEmpty(focusedName)) Root.Q<VisualElement>(focusedName)?.Focus();
                if (!samePage && !string.IsNullOrEmpty(focusedTargetId))
                {
                    var target = content.Q<VisualElement>("control-center-card-" + focusedTargetId);
                    if (target != null) content.ScrollTo(target);
                }
            });
        }

        internal void SetLayoutMode(DeucarianEditorLayoutMode mode)
        {
            layoutMode = mode;
            if (layout == null || sidebar == null)
            {
                return;
            }

            bool narrow = mode == DeucarianEditorLayoutMode.Narrow;
            layout.style.flexDirection = FlexDirection.Column;
            sidebar.style.flexDirection = FlexDirection.Row;
            sidebar.style.flexWrap = Wrap.Wrap;
            foreach (VisualElement host in cardHosts)
            {
                host.style.flexDirection = narrow ? FlexDirection.Column : FlexDirection.Row;
                foreach (VisualElement card in host.Children())
                {
                    card.style.flexBasis = narrow ? new StyleLength(StyleKeyword.Auto) : new StyleLength(280f);
                    card.style.flexGrow = narrow ? 0f : 1f;
                    card.style.minWidth = 0f;
                }
            }
        }

        public void Dispose()
        {
            renderRevision++;
            Root.Clear();
            cardHosts.Clear();
        }

        private VisualElement CreateSidebar(
            DeucarianControlCenterSnapshot snapshot,
            DeucarianControlCenterArea selectedArea)
        {
            var result = new VisualElement
            {
                name = "control-center-sidebar"
            };
            result.AddToClassList("dw-control-sections");
            foreach (DeucarianControlCenterArea area in snapshot.Areas)
            {
                DeucarianControlCenterArea captured = area;
                var button = DeucarianEditorWorkspaceControls.Button(
                    DeucarianControlCenterAreaIds.GetDisplayName(area), () => navigate(captured, null));
                button.name = "control-center-area-" + DeucarianControlCenterAreaIds.GetId(area);
                button.EnableInClassList("dw-selected", area == selectedArea);
                result.Add(button);
            }

            return result;
        }

        private void RenderArea(
            VisualElement content,
            DeucarianControlCenterSnapshot snapshot,
            DeucarianControlCenterArea area,
            string focusedTargetId)
        {
            if (area != DeucarianControlCenterArea.Overview)
                DeucarianControlCenterVisuals.AddPageHeading(content,
                    DeucarianControlCenterAreaIds.GetDisplayName(area), GetAreaDescription(area));
            else
                content.Add(DeucarianControlCenterOverviewPresentation.CreateFocus(snapshot, navigate));
            VisualElement cards = CreateCardHost();
            content.Add(cards);
            foreach (DeucarianControlCenterCard card in snapshot.Cards)
            {
                if (card.Area == area)
                {
                    if (area == DeucarianControlCenterArea.Overview && card.Id == "deucarian.readiness.overview") continue;
                    cards.Add(area == DeucarianControlCenterArea.Overview && DeucarianControlCenterOverviewPresentation.TryGetSummaryArea(card.Id, out _)
                        ? DeucarianControlCenterOverviewPresentation.CreateSummary(card, navigate)
                        : DeucarianControlCenterCardRenderer.Create(card, focusedTargetId, ExecuteCardAction));
                }
            }

            foreach (DeucarianControlCenterSection section in snapshot.Sections)
            {
                if (section.Area != area)
                {
                    continue;
                }

                DeucarianControlCenterVisuals.AddSectionHeading(content, section.Title, section.Description);
                VisualElement sectionCards = CreateCardHost();
                content.Add(sectionCards);
                foreach (DeucarianControlCenterCard card in section.Cards)
                {
                    sectionCards.Add(DeucarianControlCenterCardRenderer.Create(card, focusedTargetId, ExecuteCardAction));
                }
            }

            if (area == DeucarianControlCenterArea.Developer || area == DeucarianControlCenterArea.Overview)
                RenderTools(content, snapshot.Tools, area);
        }

        private void RenderTools(
            VisualElement content,
            IReadOnlyList<DeucarianToolDescriptor> tools,
            DeucarianControlCenterArea area)
        {
            var matching = new List<DeucarianToolDescriptor>();
            foreach (DeucarianToolDescriptor tool in tools)
            {
                if (tool.Id != DeucarianToolIds.ControlCenter &&
                    (area == DeucarianControlCenterArea.Developer || DeucarianToolHistory.IsFavorite(tool.Id) ||
                        DeucarianToolHistory.RecentIndex(tool.Id) >= 0 && DeucarianToolHistory.RecentIndex(tool.Id) < 3))
                {
                    matching.Add(tool);
                }
            }

            if (matching.Count == 0)
            {
                return;
            }

            matching.Sort((left, right) =>
            {
                int favorite = DeucarianToolHistory.IsFavorite(right.Id).CompareTo(DeucarianToolHistory.IsFavorite(left.Id));
                if (favorite != 0) return favorite;
                int leftIndex = DeucarianToolHistory.RecentIndex(left.Id);
                int rightIndex = DeucarianToolHistory.RecentIndex(right.Id);
                int recent = (leftIndex < 0 ? int.MaxValue : leftIndex).CompareTo(rightIndex < 0 ? int.MaxValue : rightIndex);
                return recent != 0 ? recent : string.Compare(left.DisplayName, right.DisplayName, StringComparison.OrdinalIgnoreCase);
            });

            DeucarianControlCenterVisuals.AddSectionHeading(
                content,
                area == DeucarianControlCenterArea.Overview ? "Your tools" : "Tools",
                string.Empty);
            foreach (DeucarianToolDescriptor tool in matching)
            {
                DeucarianToolDescriptor captured = tool;
                var row = new VisualElement
                {
                    name = "control-center-tool-" + tool.Id
                };
                row.AddToClassList("dw-tool-row");
                DeucarianEditorResponsiveLayout.AdaptToWidth(row, "dw-tool-stacked", 520);
                var text = DeucarianEditorWorkspaceControls.Region(null, "dw-tool-text");
                text.Add(DeucarianControlCenterVisuals.CreateLabel(tool.DisplayName, true));
                row.tooltip = tool.Description;
                if (area != DeucarianControlCenterArea.Overview && tool.Description.Length > 0)
                {
                    text.Add(DeucarianControlCenterVisuals.CreateMutedLabel(tool.Description));
                }

                row.Add(text);
                var button = DeucarianEditorWorkspaceControls.Button("Open",
                    () => InvokeSafely(captured.DisplayName, captured.Open));
                button.name = "control-center-open-" + tool.Id;
                bool favorite = DeucarianToolHistory.IsFavorite(tool.Id);
                var pin = DeucarianEditorWorkspaceControls.Button(favorite ? "Unpin" : "Pin", () =>
                {
                    DeucarianToolHistory.SetFavorite(captured.Id, !DeucarianToolHistory.IsFavorite(captured.Id));
                    refresh();
                });
                pin.name = "control-center-pin-" + tool.Id;
                pin.tooltip = "Keep this tool on the overview for this project.";
                row.Add(DeucarianEditorWorkspaceControls.Actions(button, pin));
                content.Add(row);
            }
        }

        private void RenderSearch(
            VisualElement content,
            DeucarianControlCenterSnapshot snapshot,
            string query)
        {
            IReadOnlyList<DeucarianControlCenterSearchResult> results =
                DeucarianControlCenterSearch.Search(snapshot, query);
            DeucarianControlCenterVisuals.AddPageHeading(
                content,
                "Search",
                results.Count + " result(s) for '" + query + "'.");
            foreach (DeucarianControlCenterSearchResult result in results)
            {
                searchResults.Add(result);
                DeucarianControlCenterSearchResult captured = result;
                var row = new Button(() => OpenSearchResult(captured))
                {
                    name = "control-center-search-result-" + result.TargetId
                };
                row.style.marginBottom = 6f;
                row.style.paddingLeft = 10f;
                row.style.paddingRight = 10f;
                row.style.paddingTop = 8f;
                row.style.paddingBottom = 8f;
                row.style.unityTextAlign = TextAnchor.MiddleLeft;
                row.Add(DeucarianControlCenterVisuals.CreateLabel(result.Title, true));
                if (result.Description.Length > 0)
                {
                    row.Add(DeucarianControlCenterVisuals.CreateMutedLabel(result.Description));
                }

                content.Add(row);
                searchButtons.Add(row);
            }

            if (results.Count == 0)
            {
                content.Add(DeucarianControlCenterVisuals.CreateMutedLabel(
                    "No cards, tools, actions, or areas match this search."));
            }
        }

        internal void MoveSearchSelection(int direction)
        {
            if (searchButtons.Count == 0) return;
            if (selectedSearchResult >= 0) searchButtons[selectedSearchResult].style.borderLeftWidth = 0;
            selectedSearchResult = selectedSearchResult < 0 ? (direction > 0 ? 0 : searchButtons.Count - 1)
                : (selectedSearchResult + direction + searchButtons.Count) % searchButtons.Count;
            var selected = searchButtons[selectedSearchResult];
            selected.style.borderLeftWidth = 3;
            selected.style.borderLeftColor = DeucarianEditorTheme.Accent;
            content.ScrollTo(selected);
        }

        internal void OpenSelectedSearchResult()
        {
            if (searchResults.Count > 0) OpenSearchResult(searchResults[Math.Max(0, selectedSearchResult)]);
        }

        private void OpenSearchResult(
            DeucarianControlCenterSearchResult result)
        {
            if (!result.CanInvoke)
            {
                navigate(result.Area, result.TargetId);
                return;
            }

            if (Confirm(result.Title, result.RequiresConfirmation))
            {
                InvokeSafely(result.Title, result.Invoke);
            }
        }

        private void ExecuteCardAction(DeucarianControlCenterAction action)
        {
            if (Confirm(action.Label, action.RequiresConfirmation)) InvokeSafely(action.Label, action.Invoke);
        }

        private VisualElement CreateCardHost()
        {
            var host = new VisualElement();
            host.style.flexDirection = FlexDirection.Row;
            host.style.flexWrap = Wrap.Wrap;
            host.style.marginBottom = 6f;
            cardHosts.Add(host);
            return host;
        }

    }
}
