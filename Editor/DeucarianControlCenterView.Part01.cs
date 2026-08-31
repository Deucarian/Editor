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
            Root.style.flexGrow = 1f;
        }

        internal VisualElement Root { get; }

        internal void Render(
            DeucarianControlCenterSnapshot snapshot,
            DeucarianControlCenterArea selectedArea,
            string focusedTargetId,
            string query)
        {
            Root.Clear();
            cardHosts.Clear();
            layout = new VisualElement { name = "control-center-layout" };
            layout.style.flexGrow = 1f;
            layout.style.paddingTop = 8f;
            layout.style.paddingBottom = 8f;
            layout.style.paddingLeft = 8f;
            layout.style.paddingRight = 8f;
            Root.Add(layout);

            sidebar = CreateSidebar(snapshot, selectedArea);
            layout.Add(sidebar);
            var content = new ScrollView(ScrollViewMode.Vertical)
            {
                name = "control-center-content"
            };
            content.style.flexGrow = 1f;
            content.style.paddingLeft = 10f;
            content.style.paddingRight = 6f;
            layout.Add(content);

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
        }

        internal void SetLayoutMode(DeucarianEditorLayoutMode mode)
        {
            layoutMode = mode;
            if (layout == null || sidebar == null)
            {
                return;
            }

            bool narrow = mode == DeucarianEditorLayoutMode.Narrow;
            layout.style.flexDirection =
                narrow ? FlexDirection.Column : FlexDirection.Row;
            StyleLength sidebarWidth = narrow
                ? new StyleLength(StyleKeyword.Auto)
                : new StyleLength(190f);
            sidebar.style.width = sidebarWidth;
            sidebar.style.minWidth = sidebarWidth;
            sidebar.style.marginBottom = narrow ? 8f : 0f;
            sidebar.style.marginRight = narrow ? 0f : 8f;
            foreach (VisualElement host in cardHosts)
            {
                host.style.flexDirection =
                    narrow ? FlexDirection.Column : FlexDirection.Row;
            }
        }

        public void Dispose()
        {
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
            StylePanel(result);
            result.style.paddingTop = 6f;
            result.style.paddingBottom = 6f;
            result.style.paddingLeft = 6f;
            result.style.paddingRight = 6f;
            foreach (DeucarianControlCenterArea area in snapshot.Areas)
            {
                DeucarianControlCenterArea captured = area;
                var button = new Button(() => navigate(captured, null))
                {
                    text = DeucarianControlCenterAreaIds.GetDisplayName(area),
                    name = "control-center-area-" +
                        DeucarianControlCenterAreaIds.GetId(area)
                };
                button.style.unityTextAlign = TextAnchor.MiddleLeft;
                button.style.height = 30f;
                button.style.marginBottom = 3f;
                if (area == selectedArea)
                {
                    button.style.backgroundColor =
                        DeucarianEditorTheme.GlassPanelStrong;
                    button.style.color = DeucarianEditorTheme.Text;
                }

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
            AddPageHeading(
                content,
                DeucarianControlCenterAreaIds.GetDisplayName(area),
                GetAreaDescription(area));
            VisualElement cards = CreateCardHost();
            content.Add(cards);
            foreach (DeucarianControlCenterCard card in snapshot.Cards)
            {
                if (card.Area == area)
                {
                    cards.Add(CreateCard(card, focusedTargetId));
                }
            }

            foreach (DeucarianControlCenterSection section in snapshot.Sections)
            {
                if (section.Area != area)
                {
                    continue;
                }

                AddSectionHeading(content, section.Title, section.Description);
                VisualElement sectionCards = CreateCardHost();
                content.Add(sectionCards);
                foreach (DeucarianControlCenterCard card in section.Cards)
                {
                    sectionCards.Add(CreateCard(card, focusedTargetId));
                }
            }

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
                if (tool.Area == area)
                {
                    matching.Add(tool);
                }
            }

            if (matching.Count == 0)
            {
                return;
            }

            AddSectionHeading(
                content,
                "Tools",
                "Open the owning package's full workflow.");
            foreach (DeucarianToolDescriptor tool in matching)
            {
                DeucarianToolDescriptor captured = tool;
                var row = new VisualElement
                {
                    name = "control-center-tool-" + tool.Id
                };
                StylePanel(row);
                row.style.marginBottom = 6f;
                row.style.paddingLeft = 10f;
                row.style.paddingRight = 8f;
                row.style.paddingTop = 8f;
                row.style.paddingBottom = 8f;
                row.Add(CreateLabel(tool.DisplayName, true));
                if (tool.Description.Length > 0)
                {
                    row.Add(CreateMutedLabel(tool.Description));
                }

                var button = new Button(
                    () => InvokeSafely(captured.DisplayName, captured.Open))
                {
                    text = "Open"
                };
                button.style.marginTop = 6f;
                row.Add(button);
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
            AddPageHeading(
                content,
                "Search",
                results.Count + " result(s) for '" + query + "'.");
            foreach (DeucarianControlCenterSearchResult result in results)
            {
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
                row.Add(CreateLabel(result.Title, true));
                if (result.Description.Length > 0)
                {
                    row.Add(CreateMutedLabel(result.Description));
                }

                content.Add(row);
            }

            if (results.Count == 0)
            {
                content.Add(CreateMutedLabel(
                    "No cards, tools, actions, or areas match this search."));
            }
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

        private VisualElement CreateCard(
            DeucarianControlCenterCard card,
            string focusedTargetId)
        {
            var result = new VisualElement
            {
                name = "control-center-card-" + card.Id
            };
            StylePanel(result);
            result.style.flexGrow = 1f;
            result.style.flexBasis = 300f;
            result.style.minWidth = 250f;
            result.style.marginRight = 6f;
            result.style.marginBottom = 6f;
            result.style.paddingLeft = 12f;
            result.style.paddingRight = 12f;
            result.style.paddingTop = 10f;
            result.style.paddingBottom = 10f;
            if (string.Equals(card.Id, focusedTargetId, StringComparison.Ordinal))
            {
                SetBorderColor(result, DeucarianEditorTheme.Accent);
                result.style.borderLeftWidth = 3f;
            }

            VisualElement heading = new VisualElement();
            heading.style.flexDirection = FlexDirection.Row;
            heading.style.justifyContent = Justify.SpaceBetween;
            heading.Add(CreateLabel(card.Title, true));
            if (card.StatusText.Length > 0)
            {
                Label badge = CreateLabel(card.StatusText, false);
                badge.style.color = GetStatusColor(card.Status);
                heading.Add(badge);
            }

            result.Add(heading);
            if (card.Description.Length > 0)
            {
                result.Add(CreateMutedLabel(card.Description));
            }

            foreach (string detail in card.Details)
            {
                result.Add(CreateMutedLabel("• " + detail));
            }

            if (card.Actions.Count > 0)
            {
                var actions = new VisualElement();
                actions.style.flexDirection = FlexDirection.Row;
                actions.style.flexWrap = Wrap.Wrap;
                actions.style.marginTop = 8f;
                foreach (DeucarianControlCenterAction action in card.Actions)
                {
                    DeucarianControlCenterAction captured = action;
                    var button = new Button(() =>
                    {
                        if (Confirm(captured.Label, captured.RequiresConfirmation))
                        {
                            InvokeSafely(captured.Label, captured.Invoke);
                        }
                    })
                    {
                        text = action.Label,
                        name = "control-center-action-" + action.Id
                    };
                    button.style.marginRight = 5f;
                    actions.Add(button);
                }

                result.Add(actions);
            }

            return result;
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

        private static void AddPageHeading(
            VisualElement parent,
            string title,
            string description)
        {
            Label heading = CreateLabel(title, true);
            heading.style.fontSize = 20f;
            heading.style.marginBottom = 3f;
            parent.Add(heading);
            parent.Add(CreateMutedLabel(description));
        }

        private static void AddSectionHeading(
            VisualElement parent,
            string title,
            string description)
        {
            Label heading = CreateLabel(title, true);
            heading.style.fontSize = 15f;
            heading.style.marginTop = 10f;
            parent.Add(heading);
            if (!string.IsNullOrWhiteSpace(description))
            {
                parent.Add(CreateMutedLabel(description));
            }
        }

        private static Label CreateLabel(string text, bool strong)
        {
            var label = new Label(text ?? string.Empty);
            label.style.color = DeucarianEditorTheme.Text;
            label.style.whiteSpace = WhiteSpace.Normal;
            if (strong)
            {
                label.style.unityFontStyleAndWeight = FontStyle.Bold;
            }

            return label;
        }
    }
}
