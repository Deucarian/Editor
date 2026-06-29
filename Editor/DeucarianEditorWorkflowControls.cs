using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Deucarian.Editor
{
    public readonly struct DeucarianEditorStatusChip
    {
        public DeucarianEditorStatusChip(string label, DeucarianEditorStatus status, string tooltip = null)
        {
            Label = label ?? string.Empty;
            Status = status;
            Tooltip = tooltip ?? string.Empty;
        }

        public string Label { get; }
        public DeucarianEditorStatus Status { get; }
        public string Tooltip { get; }
    }

    public readonly struct DeucarianEditorTimelineEvent
    {
        public DeucarianEditorTimelineEvent(string label, string detail, bool visualAssigned, bool audioAssigned, bool enabled = true)
        {
            Label = label ?? string.Empty;
            Detail = detail ?? string.Empty;
            VisualAssigned = visualAssigned;
            AudioAssigned = audioAssigned;
            Enabled = enabled;
        }

        public string Label { get; }
        public string Detail { get; }
        public bool VisualAssigned { get; }
        public bool AudioAssigned { get; }
        public bool Enabled { get; }
    }

    public readonly struct DeucarianEditorSplitPaneWidths
    {
        public DeucarianEditorSplitPaneWidths(float left, float center, float right)
        {
            Left = left;
            Center = center;
            Right = right;
        }

        public float Left { get; }
        public float Center { get; }
        public float Right { get; }
    }

    public static class DeucarianEditorSearchField
    {
        public static string Draw(string value, string placeholder = "Search", params GUILayoutOption[] options)
        {
            value = value ?? string.Empty;
            using (new EditorGUILayout.HorizontalScope())
            {
                string next = EditorGUILayout.TextField(
                    new GUIContent(string.Empty, placeholder ?? string.Empty),
                    value,
                    SearchStyle,
                    options);
                if (GUILayout.Button(new GUIContent("x", "Clear search"), SearchCancelStyle, GUILayout.Width(22f), GUILayout.Height(20f)))
                    next = string.Empty;
                return next;
            }
        }

        private static GUIStyle searchStyle;
        private static GUIStyle searchCancelStyle;

        private static GUIStyle SearchStyle
        {
            get
            {
                if (searchStyle == null)
                {
                    GUIStyle source = GUI.skin.FindStyle("ToolbarSeachTextField") ?? GUI.skin.FindStyle("ToolbarSearchTextField") ?? EditorStyles.toolbarTextField;
                    searchStyle = new GUIStyle(source)
                    {
                        fixedHeight = 22f
                    };
                }

                return searchStyle;
            }
        }

        private static GUIStyle SearchCancelStyle
        {
            get
            {
                if (searchCancelStyle == null)
                {
                    GUIStyle source = GUI.skin.FindStyle("ToolbarSeachCancelButton") ?? GUI.skin.FindStyle("ToolbarSearchCancelButton") ?? EditorStyles.toolbarButton;
                    searchCancelStyle = new GUIStyle(source);
                }

                return searchCancelStyle;
            }
        }
    }

    public static class DeucarianEditorSegmentedControl
    {
        public static int Draw(int selectedIndex, IReadOnlyList<string> labels, params GUILayoutOption[] options)
        {
            if (labels == null || labels.Count == 0)
                return -1;

            selectedIndex = Mathf.Clamp(selectedIndex, 0, labels.Count - 1);
            using (new EditorGUILayout.HorizontalScope(options))
            {
                for (int i = 0; i < labels.Count; i++)
                {
                    bool selected = i == selectedIndex;
                    GUIStyle style = selected ? SelectedChipStyle : ChipStyle;
                    if (GUILayout.Button(new GUIContent(labels[i] ?? string.Empty), style, GUILayout.Height(24f)))
                        selectedIndex = i;
                }
            }

            return selectedIndex;
        }

        public static int DrawPageChips(int selectedIndex, params string[] labels)
        {
            return Draw(selectedIndex, labels);
        }

        private static GUIStyle chipStyle;
        private static GUIStyle selectedChipStyle;

        private static GUIStyle ChipStyle
        {
            get
            {
                if (chipStyle == null)
                {
                    chipStyle = CreateChipStyle("workflow-chip", new Color(0.08f, 0.20f, 0.25f, 0.78f), DeucarianEditorTheme.MutedText);
                }

                return chipStyle;
            }
        }

        private static GUIStyle SelectedChipStyle
        {
            get
            {
                if (selectedChipStyle == null)
                {
                    selectedChipStyle = CreateChipStyle("workflow-chip-selected", new Color(0.09f, 0.48f, 0.52f, 0.96f), DeucarianEditorTheme.Text);
                    selectedChipStyle.fontStyle = FontStyle.Bold;
                }

                return selectedChipStyle;
            }
        }

        private static GUIStyle CreateChipStyle(string key, Color background, Color text)
        {
            GUIStyle style = new GUIStyle(EditorStyles.miniButton)
            {
                alignment = TextAnchor.MiddleCenter,
                clipping = TextClipping.Clip,
                padding = new RectOffset(8, 8, 2, 3),
                margin = new RectOffset(0, 4, 0, 4),
                border = new RectOffset(4, 4, 4, 4)
            };
            style.normal.background = DeucarianEditorTextureCache.Get(key, background);
            style.hover.background = DeucarianEditorTextureCache.Get(key + "-hover", new Color(background.r + 0.03f, background.g + 0.05f, background.b + 0.05f, background.a));
            style.active.background = style.hover.background;
            style.focused.background = style.normal.background;
            style.normal.textColor = text;
            style.hover.textColor = Color.white;
            style.active.textColor = Color.white;
            return style;
        }
    }

    public static class DeucarianEditorStatusChipRow
    {
        public static void Draw(params DeucarianEditorStatusChip[] chips)
        {
            Draw((IReadOnlyList<DeucarianEditorStatusChip>)chips);
        }

        public static void Draw(IReadOnlyList<DeucarianEditorStatusChip> chips)
        {
            if (chips == null || chips.Count == 0)
                return;

            using (new EditorGUILayout.HorizontalScope())
            {
                for (int i = 0; i < chips.Count; i++)
                {
                    DeucarianEditorStatusChip chip = chips[i];
                    DeucarianEditorStatusBadge.Draw(new GUIContent(chip.Label, chip.Tooltip), chip.Status, GUILayout.MinWidth(40f), GUILayout.Height(18f));
                }

                GUILayout.FlexibleSpace();
            }
        }
    }

    public static class DeucarianEditorIconToolbar
    {
        public static bool Button(string iconName, string tooltip, bool enabled = true, params GUILayoutOption[] options)
        {
            GUIContent content = BuildContent(iconName, tooltip);
            using (new EditorGUI.DisabledScope(!enabled))
            {
                return GUILayout.Button(content, DeucarianEditorStyles.ToolbarButton, options);
            }
        }

        public static bool Toggle(bool value, string iconName, string tooltip, bool enabled = true, params GUILayoutOption[] options)
        {
            GUIContent content = BuildContent(iconName, tooltip);
            using (new EditorGUI.DisabledScope(!enabled))
            {
                bool next = GUILayout.Toggle(value, content, DeucarianEditorStyles.ToolbarButton, options);
                return next;
            }
        }

        public static GUIContent BuildContent(string iconName, string tooltip)
        {
            GUIContent content = null;
            if (!string.IsNullOrWhiteSpace(iconName))
                content = EditorGUIUtility.IconContent(iconName);
            if (content == null)
                content = new GUIContent(string.Empty);
            content.tooltip = tooltip ?? string.Empty;
            return content;
        }
    }

    public static class DeucarianEditorSplitPane
    {
        public static DeucarianEditorSplitPaneWidths Calculate(
            float totalWidth,
            float leftPreferred,
            float centerMinimum,
            float rightPreferred,
            float gutter = DeucarianEditorSpacing.Small)
        {
            float usable = Mathf.Max(1f, totalWidth - gutter * 2f);
            float left = Mathf.Clamp(leftPreferred, 220f, Mathf.Max(220f, usable * 0.34f));
            float right = Mathf.Clamp(rightPreferred, 280f, Mathf.Max(280f, usable * 0.34f));
            float center = usable - left - right;
            if (center < centerMinimum)
            {
                float deficit = centerMinimum - center;
                float reduceLeft = Mathf.Min(deficit * 0.5f, Mathf.Max(0f, left - 220f));
                left -= reduceLeft;
                deficit -= reduceLeft;
                float reduceRight = Mathf.Min(deficit, Mathf.Max(0f, right - 280f));
                right -= reduceRight;
                center = usable - left - right;
            }

            return new DeucarianEditorSplitPaneWidths(
                Mathf.Max(180f, left),
                Mathf.Max(260f, center),
                Mathf.Max(240f, right));
        }
    }

    public static class DeucarianEditorWizardHeader
    {
        public static int Draw(int currentStep, IReadOnlyList<string> steps)
        {
            if (steps == null || steps.Count == 0)
                return -1;

            currentStep = Mathf.Clamp(currentStep, 0, steps.Count - 1);
            using (new EditorGUILayout.HorizontalScope())
            {
                for (int i = 0; i < steps.Count; i++)
                {
                    DeucarianEditorStatus status = i < currentStep
                        ? DeucarianEditorStatus.Success
                        : i == currentStep
                            ? DeucarianEditorStatus.Info
                            : DeucarianEditorStatus.Disabled;
                    string label = (i + 1).ToString(System.Globalization.CultureInfo.InvariantCulture) + ". " + steps[i];
                    if (GUILayout.Button(label, i == currentStep ? StepSelectedStyle : StepStyle, GUILayout.Height(24f)))
                        currentStep = i;
                    GUILayout.Space(2f);
                    if (Event.current != null && Event.current.type == EventType.Repaint)
                    {
                        Rect last = GUILayoutUtility.GetLastRect();
                        Rect badge = new Rect(last.xMax - 10f, last.y + 5f, 8f, 8f);
                        EditorGUI.DrawRect(badge, DeucarianEditorStatusBadge.GetColor(status));
                    }
                }
            }

            return currentStep;
        }

        private static GUIStyle stepStyle;
        private static GUIStyle stepSelectedStyle;

        private static GUIStyle StepStyle
        {
            get
            {
                if (stepStyle == null)
                {
                    stepStyle = new GUIStyle(DeucarianEditorButtons.SecondaryStyle)
                    {
                        fontSize = 10,
                        clipping = TextClipping.Clip,
                        padding = new RectOffset(8, 14, 2, 3)
                    };
                }

                return stepStyle;
            }
        }

        private static GUIStyle StepSelectedStyle
        {
            get
            {
                if (stepSelectedStyle == null)
                {
                    stepSelectedStyle = new GUIStyle(DeucarianEditorButtons.PrimaryStyle)
                    {
                        fontSize = 10,
                        clipping = TextClipping.Clip,
                        padding = new RectOffset(8, 14, 2, 3)
                    };
                }

                return stepSelectedStyle;
            }
        }
    }

    public static class DeucarianEditorEventTimeline
    {
        public static void Draw(IReadOnlyList<DeucarianEditorTimelineEvent> events, Action<int> drawAction = null)
        {
            if (events == null || events.Count == 0)
            {
                EditorGUILayout.LabelField("No timeline events.", DeucarianEditorStyles.MutedLabel);
                return;
            }

            for (int i = 0; i < events.Count; i++)
            {
                DeucarianEditorTimelineEvent item = events[i];
                DeucarianEditorCards.DrawInlineCard(() =>
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField(item.Label, DeucarianEditorStyles.SectionTitle, GUILayout.MinWidth(82f));
                        GUILayout.FlexibleSpace();
                        DeucarianEditorStatusBadge.Draw(item.VisualAssigned ? "VFX" : "No VFX", item.VisualAssigned ? DeucarianEditorStatus.Success : DeucarianEditorStatus.Disabled, GUILayout.Width(58f));
                        DeucarianEditorStatusBadge.Draw(item.AudioAssigned ? "Audio" : "Muted", item.AudioAssigned ? DeucarianEditorStatus.Success : DeucarianEditorStatus.Disabled, GUILayout.Width(64f));
                        if (drawAction != null && DeucarianEditorMiniToolbar.Button("Play", item.Enabled, GUILayout.Width(46f), GUILayout.Height(22f)))
                            drawAction(i);
                    }

                    if (!string.IsNullOrWhiteSpace(item.Detail))
                        EditorGUILayout.LabelField(item.Detail, DeucarianEditorStyles.MutedLabel);
                });
            }
        }
    }

    public static class DeucarianEditorCompactObjectCard
    {
        public static bool Draw(
            string title,
            string subtitle,
            bool selected,
            IReadOnlyList<DeucarianEditorStatusChip> chips,
            Action drawActions = null,
            Action drawBody = null,
            params GUILayoutOption[] options)
        {
            GUIStyle style = selected ? SelectedCardStyle : CardStyle;
            Rect rect = EditorGUILayout.BeginVertical(style, options);
            DeucarianEditorVisualShell.DrawFrostedSurface(
                rect,
                selected ? DeucarianEditorTheme.GlassPanelStrong : DeucarianEditorTheme.GlassPanel,
                selected ? DeucarianEditorTheme.Accent : DeucarianEditorTheme.BorderSubtle);

            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUILayout.VerticalScope())
                {
                    EditorGUILayout.LabelField(title ?? string.Empty, TitleStyle);
                    if (!string.IsNullOrWhiteSpace(subtitle))
                        EditorGUILayout.LabelField(subtitle, DeucarianEditorStyles.MutedLabel);
                }

                GUILayout.FlexibleSpace();
                drawActions?.Invoke();
            }

            DeucarianEditorStatusChipRow.Draw(chips);
            drawBody?.Invoke();
            EditorGUILayout.EndVertical();
            GUILayout.Space(DeucarianEditorSpacing.Small);
            return Event.current != null && Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition);
        }

        private static GUIStyle cardStyle;
        private static GUIStyle selectedCardStyle;
        private static GUIStyle titleStyle;

        private static GUIStyle CardStyle
        {
            get
            {
                if (cardStyle == null)
                {
                    cardStyle = new GUIStyle(GUIStyle.none)
                    {
                        padding = new RectOffset(10, 10, 8, 8),
                        margin = new RectOffset(0, 0, 0, 0)
                    };
                }

                return cardStyle;
            }
        }

        private static GUIStyle SelectedCardStyle
        {
            get
            {
                if (selectedCardStyle == null)
                {
                    selectedCardStyle = new GUIStyle(CardStyle)
                    {
                        padding = new RectOffset(12, 12, 10, 10)
                    };
                }

                return selectedCardStyle;
            }
        }

        private static GUIStyle TitleStyle
        {
            get
            {
                if (titleStyle == null)
                {
                    titleStyle = new GUIStyle(EditorStyles.boldLabel)
                    {
                        wordWrap = true,
                        clipping = TextClipping.Clip
                    };
                    titleStyle.normal.textColor = DeucarianEditorTheme.Text;
                }

                return titleStyle;
            }
        }
    }

    public static class DeucarianEditorPreviewLabChrome
    {
        public static Rect Begin(string title, string subtitle = null, params GUILayoutOption[] options)
        {
            Rect rect = DeucarianEditorCards.BeginCard(title, false, subtitle);
            return rect;
        }

        public static void End()
        {
            DeucarianEditorCards.EndCard();
        }
    }

    public static class DeucarianEditorDiagnosticsDrawer
    {
        public static bool Draw(string stateKey, string title, Action content, bool defaultOpen = false)
        {
            return DeucarianEditorAccordion.DrawFoldoutCard(
                stateKey,
                title,
                "Advanced diagnostics and raw references.",
                content,
                defaultOpen);
        }
    }
}
