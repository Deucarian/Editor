using System;
using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Deucarian.Editor.Tests
{
    public sealed class DeucarianEditorNavigationTests
    {
        [Test]
        public void ImGuiAdapterBindsNavigationInsideItsOwnPageAndReleasesItsController()
        {
            VisualElement source = null;
            WorkspaceLayoutTestWindow controller = null;
            var page = DeucarianEditorImGuiPage.Create<WorkspaceLayoutTestWindow>(
                "test.adapter", _ => { }, bindNavigation: (window, content) => { controller = window; source = content; });
            Assert.That(page.Root.Contains(source), Is.True);
            Assert.That(DeucarianEditorWindowPages.IsPageController(controller), Is.True);
            page.Dispose();
            Assert.That(controller == null, Is.True);
        }

        [UnityTest]
        public IEnumerator ToolButtonsCardActionsAndSearchNavigateWithoutInvokingStandaloneCallbacks()
        {
            const string id = "test.navigation.audio";
            int standalone = 0, commands = 0;
            string route = null;
            var tool = new DeucarianToolDescriptor(id, "Test audio", "Audio palette",
                DeucarianControlCenterArea.Developer, () => standalone++, "com.deucarian.editor",
                createPage: () => new DeucarianEditorPage(new TextField { name = "draft", value = "Preserve me" },
                    value => route = value), navigationPath: "Experience/Audio");
            using (DeucarianToolRegistry.Register(tool))
            using (var view = new DeucarianControlCenterView((_, __) => { }, () => { }))
            {
                var card = new DeucarianControlCenterCard("test.audio.card", DeucarianControlCenterArea.Experience,
                    "Test audio", "Audio palette", "com.deucarian.editor", actions: new[]
                    {
                        new DeucarianControlCenterAction("open-audio", "Open audio", () => standalone++,
                            navigationToolId: id, navigationRoute: "palette:test"),
                        new DeucarianControlCenterAction("run-check", "Run check", () => commands++)
                    });
                var snapshot = new DeucarianControlCenterSnapshot(DateTime.UtcNow, new[] { card },
                    Array.Empty<DeucarianControlCenterSection>(), new[] { tool });
                var window = ScriptableObject.CreateInstance<WorkspaceLayoutTestWindow>();
                try
                {
                    using (var session = new DeucarianEditorPageSession(window, "home", root => root.Add(view.Root)))
                    {
                        window.Show();
                        foreach (string source in new[] { "tool", "card", "tool-search", "action-search" })
                        {
                            session.Navigate("home");
                            view.Render(snapshot, source == "tool" ? DeucarianControlCenterArea.Developer : DeucarianControlCenterArea.Experience,
                                null, source.EndsWith("search") ? "audio" : "");
                            string name = source == "tool" ? "control-center-open-" + id
                                : source == "card" ? "control-center-action-open-audio"
                                : "control-center-search-result-" + (source == "tool-search" ? id : "open-audio");
                            var button = view.Root.Q<Button>(name);
                            Assert.That(button, Is.Not.Null, source);
                            button.Focus();
                            yield return null;
                            using (var evt = NavigationSubmitEvent.GetPooled()) { evt.target = button; button.SendEvent(evt); }
                            Assert.That(session.ActiveToolId, Is.EqualTo(id), source);
                            Assert.That(window.rootVisualElement.Q<TextField>("draft").value, Is.EqualTo("Preserve me"));
                            Assert.That(standalone, Is.Zero, source);
                        }
                        Assert.That(route, Is.EqualTo("palette:test"));
                        Assert.That(commands, Is.Zero, "Navigation must not run neighboring commands.");
                    }
                }
                finally { window.Close(); }
            }
        }

        [UnityTest]
        public IEnumerator SubmenusDiscoverLateRegistrationsAndPreserveTheirParentDuringSearch()
        {
            var window = ScriptableObject.CreateInstance<WorkspaceLayoutTestWindow>();
            window.Show();
            try
            {
                using (var workspace = new DeucarianEditorWorkspace(window.rootVisualElement, "Navigation"))
                {
                    DeucarianEditorWorkspaceNavigation.Populate(workspace, "home");
                    yield return null;
                    using (DeucarianToolRegistry.Register(new DeucarianToolDescriptor("test.dynamic.audio",
                        "Custom sound tool", "Palette audition", DeucarianControlCenterArea.Experience, () => { },
                        "com.deucarian.editor", createPage: () => new DeucarianEditorPage(new VisualElement()),
                        navigationPath: "Experience/Audio")))
                    {
                        yield return null;
                        var child = workspace.Navigation.Q<Button>("workspace-nav-test.dynamic.audio");
                        Assert.That(child, Is.Not.Null);
                        Assert.That(child.GetFirstAncestorOfType<Foldout>().text, Is.EqualTo("Audio"));
                        workspace.SearchField.value = "Custom sound";
                        Assert.That(workspace.Navigation.Q<Foldout>("workspace-group-Experience").value, Is.True);
                        Assert.That(workspace.Navigation.Q<Foldout>("workspace-group-Experience/Audio").value, Is.True);
                        Assert.That(workspace.Navigation.Q<Button>("workspace-nav-test.dynamic.audio"), Is.Not.Null);
                        workspace.SearchField.value = "";
                    }
                    Assert.That(workspace.Navigation.Q<Button>("workspace-nav-test.dynamic.audio"), Is.Null);
                }
            }
            finally { window.Close(); }
        }
    }
}
