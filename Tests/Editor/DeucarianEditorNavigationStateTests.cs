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
        [UnityTest]
        public IEnumerator ExpansionAndScrollBelongToTheWindowAcrossPagesAndSearch()
        {
            var window = ScriptableObject.CreateInstance<WorkspaceLayoutTestWindow>();
            var other = ScriptableObject.CreateInstance<WorkspaceLayoutTestWindow>();
            DeucarianEditorWorkspace home = null, otherHome = null;
            using (DeucarianToolRegistry.Register(new DeucarianToolDescriptor("test.shared.nav", "Shared navigation test", "Test",
                DeucarianControlCenterArea.Developer, () => { }, "com.deucarian.editor", createPage: () => Page("test.shared.nav"),
                navigationPath: "Test group/Nested")))
            {
                window.Show();
                other.Show();
                using (var session = new DeucarianEditorPageSession(window, "home", root => home = BuildHome(root)))
                using (var otherSession = new DeucarianEditorPageSession(other, "home", root => otherHome = BuildHome(root)))
                {
                    try
                    {
                        for (int i = 0; i < 5; i++) yield return null;
                        var root = window.rootVisualElement;
                        root.Q<Foldout>("workspace-group-Test group").value = true;
                        root.Q<Foldout>("workspace-group-Test group/Nested").value = true;
                        var scroll = root.Q<ScrollView>("workspace-navigation-scroll");
                        scroll.style.height = 60;
                        scroll.style.flexGrow = 0;
                        for (int i = 0; i < 4; i++) yield return null;
                        scroll.scrollOffset = new Vector2(0, 35);
                        yield return null;
                        Assert.That(scroll.scrollOffset.y, Is.GreaterThan(0));
                        Assert.That(root.Q<DeucarianEditorPageHost>().NavigationState.ScrollOffset, Is.EqualTo(35));
                        session.Navigate("test.shared.nav");
                        root.Q<ScrollView>("workspace-navigation-scroll").style.height = 60;
                        root.Q<ScrollView>("workspace-navigation-scroll").style.flexGrow = 0;
                        for (int i = 0; i < 5; i++) yield return null;
                        Assert.That(root.Q<Foldout>("workspace-group-Test group").value, Is.True);
                        Assert.That(root.Q<Foldout>("workspace-group-Test group/Nested").value, Is.True);
                        Assert.That(root.Q<ScrollView>("workspace-navigation-scroll").scrollOffset.y, Is.EqualTo(35).Within(1));
                        root.Q<Foldout>("workspace-group-Test group/Nested").value = false;
                        var search = root.Q<TextField>("workspace-search");
                        search.value = "Shared navigation test";
                        Assert.That(root.Q<Foldout>("workspace-group-Test group/Nested").value, Is.True);
                        search.value = "";
                        session.Navigate("home");
                        for (int i = 0; i < 5; i++) yield return null;
                        Assert.That(root.Q<Foldout>("workspace-group-Test group/Nested").value, Is.False);
                        Assert.That(root.Q<Foldout>("workspace-group-Test group").value, Is.True);
                        Assert.That(other.rootVisualElement.Q<Foldout>("workspace-group-Test group").value, Is.False,
                            "A separate window must not share navigation preferences.");
                    }
                    finally { home?.Dispose(); otherHome?.Dispose(); window.Close(); other.Close(); }
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
