using System;
using UnityEditor;

namespace Deucarian.Editor
{
    /// <summary>Source-compatible migration facade for the former setup window.</summary>
    [Obsolete("Use DeucarianControlCenterWindow instead.")]
    public sealed class DeucarianProjectSetupWindow : EditorWindow
    {
        public static void Open()
        {
            DeucarianControlCenterWindow.Open();
        }

        public static void Open(string issueCode)
        {
            DeucarianControlCenterWindow.OpenProjectIssue(issueCode);
        }
    }
}
