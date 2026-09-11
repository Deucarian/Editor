using System;
using System.Linq;
using UnityEditor;
using UnityEngine.UIElements;

namespace Deucarian.Editor
{
    internal sealed class DeucarianEditorChangeDiff
    {
        internal const int MaximumCharacters = 65536;
        private readonly Label title;
        private readonly Label note;
        private readonly TextField text;
        private readonly VisualElement lines;
        private readonly Foldout copy;
        private string rendered;
        private const int MaximumLines = 500;

        internal DeucarianEditorChangeDiff(VisualElement parent)
        {
            title = DeucarianEditorWorkspaceControls.Label("Select a file to inspect its diff.", "dw-section-title");
            title.name = "review-diff-title";
            title.enableRichText = false;
            note = DeucarianEditorWorkspaceControls.Label(string.Empty, "dw-muted");
            note.name = "review-diff-note";
            var scroll = new ScrollView(ScrollViewMode.VerticalAndHorizontal) { name = "review-diff-scroll" };
            scroll.AddToClassList("dw-review-diff-scroll");
            text = new TextField { name = "review-diff-text", multiline = true, isReadOnly = true };
            text.AddToClassList("dw-review-diff-text");
            text.tooltip = "Read-only diff. Copy text here; edit files in your editor.";
            lines = DeucarianEditorWorkspaceControls.Region("review-diff-lines", "dw-diff-lines");
            scroll.Add(lines);
            copy = new Foldout { text = "Select raw diff text", value = false }; copy.AddToClassList("dw-foldout");
            copy.Add(text); scroll.Add(copy);
            parent.Add(title);
            var copyButton = DeucarianEditorWorkspaceControls.IconButton("Copy diff", DeucarianEditorIconIds.Copy,
                () => EditorGUIUtility.systemCopyBuffer = text.value, DeucarianEditorButtonRole.Quiet);
            copyButton.name = "review-copy-diff"; parent.Add(copyButton);
            parent.Add(note);
            parent.Add(scroll);
            Set(title.text, string.Empty, false, false);
        }

        internal void Set(string heading, string content, bool binary, bool truncated)
        {
            content = content ?? string.Empty;
            bool clipped = content.Length > MaximumCharacters;
            string display = binary ? string.Empty : clipped ? content.Substring(0, MaximumCharacters) : content;
            // Avoid resetting text selection and caret during an unchanged status refresh.
            if (text.value != display) text.SetValueWithoutNotify(display);
            string[] sourceLines = display.Split('\n');
            bool lineLimit = sourceLines.Length > MaximumLines;
            if (rendered != display)
            {
                rendered = display; lines.Clear();
                foreach (string line in sourceLines.Take(MaximumLines))
                {
                    var row = DeucarianEditorWorkspaceControls.Label(line.TrimEnd('\r'), "dw-diff-line");
                    row.enableRichText = false;
                    if (line.StartsWith("+", StringComparison.Ordinal)) row.AddToClassList("dw-diff-added");
                    else if (line.StartsWith("-", StringComparison.Ordinal)) row.AddToClassList("dw-diff-removed");
                    else if (line.StartsWith("@@", StringComparison.Ordinal)) row.AddToClassList("dw-diff-range");
                    lines.Add(row);
                }
            }
            title.text = heading ?? string.Empty;
            note.text = binary ? "Binary change. Inspect it in the appropriate external editor."
                : truncated || clipped || lineLimit ? "Partial diff shown. Open the external client to inspect the complete change."
                : string.IsNullOrEmpty(content) ? "No text diff available for this selection." : string.Empty;
            DeucarianEditorWorkspaceControls.Show(note, !string.IsNullOrEmpty(note.text));
            DeucarianEditorWorkspaceControls.Show(text, !binary);
            DeucarianEditorWorkspaceControls.Show(lines, !binary);
            DeucarianEditorWorkspaceControls.Show(copy, !binary && !string.IsNullOrEmpty(display));
        }
    }
}
