using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Deucarian.Editor
{
    /// <summary>Session-only state scoped to an existing native window; no global current window.</summary>
    [Serializable]
    internal sealed class DeucarianEditorReloadSnapshot
    {
        public string activeToolId;
        public float navigationScroll;
        public Group[] groups = Array.Empty<Group>();
        public List<Page> pages = new List<Page>();

        [Serializable] internal sealed class Group { public string path; public bool expanded; }
        [Serializable] internal sealed class Scroll { public string name; public int index; public Vector2 offset; }
        [Serializable] internal sealed class Page
        {
            public string toolId;
            public string route;
            public string ownerState;
            public List<Scroll> scrolls = new List<Scroll>();
        }

        internal static string Key(EditorWindow window, string homeId) =>
            "Deucarian.Workspace.Reload." + window.GetInstanceID() + "." + homeId;

        internal static DeucarianEditorReloadSnapshot Load(string key)
        {
            string json = SessionState.GetString(key, string.Empty);
            if (string.IsNullOrEmpty(json)) return new DeucarianEditorReloadSnapshot();
            // Keep the last good snapshot until the replacement session saves. A
            // failed page factory/restore must remain retryable without losing drafts.
            try { return JsonUtility.FromJson<DeucarianEditorReloadSnapshot>(json) ?? new DeucarianEditorReloadSnapshot(); }
            catch (ArgumentException) { return new DeucarianEditorReloadSnapshot(); }
        }

        internal void Save(string key) => SessionState.SetString(key, JsonUtility.ToJson(this));

        internal Page Find(string toolId) => pages.Find(page => page.toolId == toolId);

        internal void Capture(string toolId, string route, IDeucarianEditorPage page,
            IDeucarianEditorReloadState homeOwner = null)
        {
            var entry = Find(toolId);
            if (entry == null) { entry = new Page { toolId = toolId }; pages.Add(entry); }
            entry.route = route;
            // Only explicitly participating owners serialize their bounded, sanitized draft DTO.
            entry.ownerState = homeOwner != null ? homeOwner.CaptureReloadState()
                : (page as IDeucarianEditorReloadState)?.CaptureReloadState();
            entry.scrolls.Clear();
            var occurrences = new Dictionary<string, int>(StringComparer.Ordinal);
            foreach (var scroll in page.Root.Query<ScrollView>().ToList())
            {
                if (string.IsNullOrEmpty(scroll.name) || scroll.name.StartsWith("workspace-navigation-", StringComparison.Ordinal)) continue;
                occurrences.TryGetValue(scroll.name, out int index);
                occurrences[scroll.name] = index + 1;
                entry.scrolls.Add(new Scroll { name = scroll.name, index = index, offset = scroll.scrollOffset });
            }
        }

        internal static void RestoreScroll(Page entry, VisualElement root)
        {
            if (entry == null || entry.scrolls.Count == 0) return;
            int attempts = 0;
            bool restored = false;
            root.schedule.Execute(() => {
                if (restored || root.panel == null) return;
                attempts++;
                bool laidOut = true;
                foreach (var saved in entry.scrolls)
                {
                    var matches = root.Query<ScrollView>(saved.name).ToList();
                    if (saved.index >= matches.Count) continue;
                    var scroll = matches[saved.index];
                    if (scroll.contentViewport.layout.height <= 0 || float.IsNaN(scroll.contentViewport.layout.height))
                    { laidOut = false; continue; }
                    scroll.scrollOffset = saved.offset;
                    if ((scroll.scrollOffset - saved.offset).sqrMagnitude > 1f) laidOut = false;
                }
                // Allow delayed lists to grow before accepting a clamped position, but never
                // retain a repeating callback indefinitely for collapsed or removed content.
                restored = laidOut && attempts >= 10 || attempts >= 180;
            }).Every(16).Until(() => restored);
        }
    }
}
