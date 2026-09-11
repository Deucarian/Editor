using System;
using System.Collections;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Deucarian.Editor.Tests
{
    public sealed class DeucarianEditorNavigationStateTests
    {
        private static readonly int[] Scales = { 75, 100, 150 };
        [UnityTest]
        public IEnumerator ExpansionAndScrollBelongToTheWindowAcrossPagesAndSearch([ValueSource(nameof(Scales))] int scale)
        {
            int previousScale = DeucarianEditorAppearance.WorkspaceScalePercent;
            DeucarianEditorAppearance.WorkspaceScalePercent = scale;
            var window = ScriptableObject.CreateInstance<WorkspaceLayoutTestWindow>();
            var other = ScriptableObject.CreateInstance<WorkspaceLayoutTestWindow>();
            DeucarianEditorWorkspace home = null, otherHome = null;
            using (DeucarianToolRegistry.Register(new DeucarianToolDescriptor("test.shared.nav", "Shared navigation test", "Test",
                DeucarianControlCenterArea.Developer, () => { }, "com.deucarian.editor", createPage: () => Page("test.shared.nav"),
                navigationPath: "Test group/Nested")))
            {
                window.Show();
                window.position = new Rect(40, 40, 1800, 940);
                other.Show();
                using (var session = new DeucarianEditorPageSession(window, "home", root => home = BuildHome(root)))
                using (var otherSession = new DeucarianEditorPageSession(other, "home", root => otherHome = BuildHome(root)))
                {
                    try
                    {
                        yield return new WaitForSecondsRealtime(.2f);
                        var root = window.rootVisualElement;
                        root.Q<Foldout>("workspace-group-Test group").value = true;
                        root.Q<Foldout>("workspace-group-Test group/Nested").value = true;
                        var scroll = root.Q<ScrollView>("workspace-navigation-scroll");
                        scroll.style.height = 140;
                        scroll.style.flexGrow = 0;
                        for (int i = 0; i < 120 && scroll.verticalScroller.highValue < 35; i++)
                        { window.Repaint(); yield return null; }
                        Assert.That(scroll.verticalScroller.highValue, Is.GreaterThanOrEqualTo(35));
                        yield return new WaitForSecondsRealtime(.2f);
                        scroll.scrollOffset = new Vector2(0, 35);
                        yield return null;
                        Assert.That(scroll.scrollOffset.y, Is.GreaterThan(0));
                        Assert.That(root.Q<DeucarianEditorPageHost>().NavigationState.ScrollOffset, Is.EqualTo(35));
                        session.Navigate("test.shared.nav");
                        root.Q<ScrollView>("workspace-navigation-scroll").style.height = 140;
                        root.Q<ScrollView>("workspace-navigation-scroll").style.flexGrow = 0;
                        yield return new WaitForSecondsRealtime(.2f);
                        Assert.That(root.Q<Foldout>("workspace-group-Test group").value, Is.True);
                        Assert.That(root.Q<Foldout>("workspace-group-Test group/Nested").value, Is.True);
                        var selected = root.Q<Button>("workspace-nav-test.shared.nav");
                        var destinationScroll = root.Q<ScrollView>("workspace-navigation-scroll");
                        double deadline = EditorApplication.timeSinceStartup + 2;
                        while (EditorApplication.timeSinceStartup < deadline &&
                            (selected.worldBound.yMin < destinationScroll.contentViewport.worldBound.yMin - 1 ||
                             selected.worldBound.yMax > destinationScroll.contentViewport.worldBound.yMax + 1))
                        { window.Repaint(); yield return null; }
                        Assert.That(selected.worldBound.yMin, Is.GreaterThanOrEqualTo(destinationScroll.contentViewport.worldBound.yMin - 1));
                        Assert.That(selected.worldBound.yMax, Is.LessThanOrEqualTo(destinationScroll.contentViewport.worldBound.yMax + 1),
                            "Navigating reveals the whole selected destination, including items below the previous scroll position.");
                        root.Q<Foldout>("workspace-group-Test group/Nested").value = false;
                        yield return new WaitForSecondsRealtime(.2f);
                        float destinationOffset = destinationScroll.scrollOffset.y;
                        var search = root.Q<TextField>("workspace-search");
                        search.value = "Shared navigation test";
                        Assert.That(root.Q<Foldout>("workspace-group-Test group/Nested").value, Is.True);
                        for (int i = 0; i < 4; i++) yield return null;
                        search.value = "";
                        root.Q<ScrollView>("workspace-navigation-scroll").style.height = 140;
                        root.Q<ScrollView>("workspace-navigation-scroll").style.flexGrow = 0;
                        yield return new WaitForSecondsRealtime(.2f);
                        Assert.That(root.Q<ScrollView>("workspace-navigation-scroll").scrollOffset.y, Is.EqualTo(destinationOffset).Within(1),
                            "Clearing a filter restores the full tree's scroll position.");
                        session.Navigate("home");
                        for (int i = 0; i < 5; i++) yield return null;
                        Assert.That(root.Q<Foldout>("workspace-group-Test group/Nested").value, Is.False);
                        Assert.That(root.Q<Foldout>("workspace-group-Test group").value, Is.True);
                        Assert.That(other.rootVisualElement.Q<Foldout>("workspace-group-Test group").value, Is.False,
                            "A separate window must not share navigation preferences.");
                    }
                    finally { home?.Dispose(); otherHome?.Dispose(); window.Close(); other.Close(); DeucarianEditorAppearance.WorkspaceScalePercent = previousScale; }
                }
            }
        }

        private static IDeucarianEditorPage Page(string id)
        {
            var root = new VisualElement();
            var workspace = new DeucarianEditorWorkspace(root, "Test");
            DeucarianEditorWorkspaceNavigation.Populate(workspace, id);
            return new DeucarianEditorPage(root, dispose: workspace.Dispose);
        }

        private static DeucarianEditorWorkspace BuildHome(VisualElement root)
        {
            var workspace = new DeucarianEditorWorkspace(root, "Test");
            DeucarianEditorWorkspaceNavigation.Populate(workspace, "home");
            return workspace;
        }
    }
}
