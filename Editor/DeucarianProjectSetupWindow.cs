using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Deucarian.Editor
{
    /// <summary>Guided, package-contributed Deucarian project readiness UI.</summary>
    public sealed class DeucarianProjectSetupWindow : EditorWindow
    {
        private string focusedIssueCode;
        private Vector2 scroll;

        [MenuItem("Tools/Deucarian/Project Setup", priority = -1000)]
        public static void Open()
        {
            Open(null);
        }

        public static void Open(string issueCode)
        {
            var window = GetWindow<DeucarianProjectSetupWindow>(
                "Project Setup");
            window.focusedIssueCode = issueCode;
            window.minSize = new Vector2(500f, 360f);
            window.Show();
            window.Focus();
        }

        private void OnEnable()
        {
            DeucarianProjectValidationRegistry.Changed += Repaint;
        }

        private void OnDisable()
        {
            DeucarianProjectValidationRegistry.Changed -= Repaint;
        }

        private void OnGUI()
        {
            scroll = EditorGUILayout.BeginScrollView(scroll);
            DeucarianEditorChrome.DrawPackageHeader(
                "Project Setup",
                "Explicit configuration and readiness checks contributed by installed capabilities.",
                DeucarianEditorIcons.GetPackageIcon("editor"));

            IReadOnlyList<DeucarianProjectIssue> issues =
                DeucarianProjectValidationRegistry.Evaluate();
            if (issues.Count == 0)
            {
                EditorGUILayout.HelpBox(
                    "All contributed project checks pass.",
                    MessageType.Info);
            }
            else
            {
                foreach (DeucarianProjectIssue issue in issues)
                {
                    DrawIssue(issue);
                }
            }

            GUILayout.Space(8f);
            if (GUILayout.Button("Refresh checks"))
            {
                focusedIssueCode = null;
                Repaint();
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawIssue(DeucarianProjectIssue issue)
        {
            bool focused = string.Equals(
                issue.Code,
                focusedIssueCode,
                StringComparison.Ordinal);
            MessageType messageType = issue.Severity ==
                DeucarianProjectIssueSeverity.Error
                ? MessageType.Error
                : issue.Severity == DeucarianProjectIssueSeverity.Warning
                    ? MessageType.Warning
                    : MessageType.Info;
            using (new EditorGUILayout.VerticalScope(
                       focused ? EditorStyles.helpBox : GUIStyle.none))
            {
                EditorGUILayout.HelpBox(
                    issue.Code + " · " + issue.OwningPackage + "\n" +
                    issue.Explanation +
                    (string.IsNullOrWhiteSpace(issue.AffectedPath)
                        ? string.Empty
                        : "\n" + issue.AffectedPath),
                    messageType);
                using (new EditorGUILayout.HorizontalScope())
                {
                    DrawAction("Fix", issue.Fix);
                    DrawAction("Select", issue.Select);
                    DrawAction("Open Setup", issue.OpenSetup);
                }
            }
        }

        private static void DrawAction(string label, Action action)
        {
            if (action != null && GUILayout.Button(label))
            {
                try
                {
                    action();
                }
                catch (Exception exception)
                {
                    EditorUtility.DisplayDialog(
                        "Project Setup action failed",
                        "The setup action failed (" +
                        exception.GetType().Name + "). No project data was " +
                        "discarded.",
                        "OK");
                }
            }
        }

        [SettingsProvider]
        private static SettingsProvider CreateSettingsProvider()
        {
            return new SettingsProvider(
                "Project/Deucarian/Project Setup",
                SettingsScope.Project)
            {
                label = "Project Setup",
                guiHandler = _ =>
                {
                    IReadOnlyList<DeucarianProjectIssue> issues =
                        DeucarianProjectValidationRegistry.Evaluate();
                    int blockers = 0;
                    foreach (DeucarianProjectIssue issue in issues)
                    {
                        if (issue.IsBlocking)
                        {
                            blockers++;
                        }
                    }

                    EditorGUILayout.HelpBox(
                        blockers == 0
                            ? "All contributed project checks pass."
                            : blockers + " blocking project issue(s) remain.",
                        blockers == 0 ? MessageType.Info : MessageType.Error);
                    if (GUILayout.Button("Open Project Setup"))
                    {
                        Open();
                    }
                }
            };
        }
    }
}
