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
            bool focusChanged = focusedTargetId != renderedFocusId;
            if (!samePage || (focusChanged && !string.IsNullOrEmpty(focusedTargetId)))
                checkFilter = DeucarianProjectCheckFilter.All;
            if (focusChanged && !string.IsNullOrEmpty(focusedTargetId)) expandedChecks.Remove(focusedTargetId);
            renderedFocusId = focusedTargetId;
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
            DeucarianEditorWorkspaceControls.Show(sidebar, selectedArea != DeucarianControlCenterArea.Overview);
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
                if ((!samePage || focusChanged) && !string.IsNullOrEmpty(focusedTargetId))
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
            if (selectedArea == DeucarianControlCenterArea.Project)
            {
                var cards = DeucarianProjectCheckReview.Collect(snapshot);
                var labels = new string[4];
                var names = new[] { "All", "Errors", "Warnings", "Information" };
                for (int i = 0; i < labels.Length; i++)
                {
                    var filter = (DeucarianProjectCheckFilter)i;
                    labels[i] = names[i] + " (" + cards.FindAll(card => DeucarianProjectCheckReview.Matches(card, filter)).Count + ")";
                }
                var tabs = new DeucarianEditorChoiceBar(labels, (int)checkFilter, tabs: true);
                tabs.name = "control-center-check-filters";
                tabs.Changed += index =>
                {
                    checkFilter = (DeucarianProjectCheckFilter)index;
                    Render(snapshot, selectedArea, null, renderedQuery);
                };
                result.Add(tabs);
                return result;
            }
            if (selectedArea == DeucarianControlCenterArea.Developer) return result;
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
            if (area == DeucarianControlCenterArea.Overview)
            {
                content.Add(DeucarianControlCenterOverviewPresentation.CreateFocus(snapshot, navigate));
                content.Add(DeucarianControlCenterContinueWorking.Create(snapshot, Root));
                return;
            }
            if (area == DeucarianControlCenterArea.Project || area == DeucarianControlCenterArea.Developer)
            {
                DeucarianControlCenterAdvancedPresentation.Build(content, snapshot, area, focusedTargetId, refresh, ExecuteCardAction, expandedChecks, navigate, checkFilter);
                return;
            }
            DeucarianControlCenterVisuals.AddPageHeading(content,
                DeucarianControlCenterAreaIds.GetDisplayName(area), GetAreaDescription(area));
            VisualElement cards = CreateCardHost();
            content.Add(cards);
            foreach (DeucarianControlCenterCard card in snapshot.Cards)
            {
                if (card.Area == area)
                {
                    cards.Add(DeucarianControlCenterCardRenderer.Create(card, focusedTargetId, ExecuteCardAction));
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

            if (!string.IsNullOrEmpty(result.NavigationToolId))
            {
                DeucarianEditorNavigation.Open(Root, result.NavigationToolId, result.NavigationRoute);
                return;
            }

            if (Confirm(result.Title, result.RequiresConfirmation))
            {
                InvokeSafely(result.Title, result.Invoke);
            }
        }

        private void ExecuteCardAction(DeucarianControlCenterAction action)
        {
            if (!string.IsNullOrEmpty(action.NavigationToolId))
                DeucarianEditorNavigation.Open(Root, action.NavigationToolId, action.NavigationRoute);
            else if (Confirm(action.Label, action.RequiresConfirmation)) InvokeSafely(action.Label, action.Invoke);
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
