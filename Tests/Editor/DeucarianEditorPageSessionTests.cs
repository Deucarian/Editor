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
    public sealed class DeucarianEditorPageSessionTests
    {
        private const string ToolId = "test.workspace.page";
        private TestWindow window;
        private DeucarianEditorPageSession session;
        private IDisposable registration;

        [SetUp]
        public void SetUp()
        {
            window = ScriptableObject.CreateInstance<TestWindow>();
            session = new DeucarianEditorPageSession(window, "home",
                root => root.Add(new TextField { name = "home-draft" }));
        }

        [TearDown]
        public void TearDown()
        {
            session?.Dispose();
            registration?.Dispose();
            if (window != null) UnityEngine.Object.DestroyImmediate(window);
        }

        [Test]
        public void NavigationKeepsWindowBoundsAndCachesDraftsWithoutLaunching()
        {
            int launches = 0, creates = 0;
            var draft = new TextField { name = "guest-draft", value = "Keep this draft" };
            registration = Register(() => { creates++; return new DeucarianEditorPage(draft); }, () => launches++);
            var bounds = window.position;
            var title = window.titleContent.text;
            var homeDraft = window.rootVisualElement.Q<TextField>("home-draft");
            homeDraft.value = "Keep home too";
            Assert.That(session.Navigate(ToolId), Is.True);
            Assert.That(window.rootVisualElement.Q<TextField>("guest-draft"), Is.SameAs(draft));
            Assert.That(window.titleContent.text, Is.EqualTo("Test page"));
            Assert.That(session.Navigate("home"), Is.True);
            Assert.That(window.rootVisualElement.Q<TextField>("home-draft"), Is.SameAs(homeDraft));
            Assert.That(homeDraft.value, Is.EqualTo("Keep home too"));
            Assert.That(window.titleContent.text, Is.EqualTo(title));
            Assert.That(session.Navigate(ToolId), Is.True);
            Assert.That(session.Navigate(ToolId), Is.True);
            Assert.That(draft.value, Is.EqualTo("Keep this draft"));
            Assert.That(creates, Is.EqualTo(1));
            Assert.That(launches, Is.Zero);
            Assert.That(window.position, Is.EqualTo(bounds));
            Assert.That(session.PageCount, Is.EqualTo(2));
        }

        [Test]
        public void MissingPageNeverFallsBackToOpeningAnotherWindow()
        {
            int launches = 0;
            registration = Register(null, () => launches++);
            Assert.That(session.Navigate(ToolId), Is.False);
            Assert.That(session.Navigate("missing"), Is.False);
            Assert.That(session.ActiveToolId, Is.EqualTo("home"));
            Assert.That(launches, Is.Zero);
        }

        [Test]
        public void ClickingTheActivePageDoesNotReactivateItButExplicitRoutesStillWork()
        {
            int activations = 0;
            string route = null;
            registration = Register(() => new DeucarianEditorPage(new VisualElement(), value => { activations++; route = value; }));
            session.Navigate(ToolId);
            session.Navigate(ToolId);
            session.Navigate(ToolId, "");
            Assert.That(activations, Is.EqualTo(1));
            session.Navigate(ToolId, "details");
            Assert.That(activations, Is.EqualTo(2));
            Assert.That(route, Is.EqualTo("details"));
        }

        [Test]
        public void PreviousPreviewStopsBeforeTheNextPageActivatesAndCachedFailureIsCleanedUp()
        {
            session.Dispose();
            var calls = new System.Collections.Generic.List<string>();
            bool fail = false;
            session = new DeucarianEditorPageSession(window, "home", _ => { },
                _ => calls.Add("home.activate"), () => calls.Add("home.stop"));
            registration = Register(() => new DeucarianEditorPage(new VisualElement(),
                _ => { calls.Add("next.activate"); if (fail) throw new InvalidOperationException("Failed"); },
                () => calls.Add("next.stop")));
            session.Navigate(ToolId);
            Assert.That(calls, Is.EqualTo(new[] { "home.stop", "next.activate" }));
            session.Navigate("home");
            calls.Clear();
            fail = true;
            Assert.Throws<InvalidOperationException>(() => session.Navigate(ToolId));
            Assert.That(calls, Is.EqualTo(new[] { "home.stop", "next.activate", "next.stop", "home.activate" }));
            Assert.That(session.ActiveToolId, Is.EqualTo("home"));
            Assert.That(session.PageCount, Is.EqualTo(2));
            fail = false;
            Assert.That(session.Navigate(ToolId), Is.True);
        }

        [Test]
        public void FailedActivationKeepsThePreviousPageAndReleasesTheNewPage()
        {
            int releases = 0;
            registration = Register(() => new DeucarianEditorPage(new VisualElement(),
                _ => throw new InvalidOperationException("Preparation failed"), dispose: () => releases++));
            Assert.Throws<InvalidOperationException>(() => session.Navigate(ToolId));
            Assert.That(session.ActiveToolId, Is.EqualTo("home"));
            Assert.That(session.PageCount, Is.EqualTo(1));
            Assert.That(window.rootVisualElement.Q("home-draft"), Is.Not.Null);
            Assert.That(releases, Is.EqualTo(1));
        }

        [Test]
        public void SeparateWindowsHaveIndependentPageInstances()
        {
            int creates = 0, releases = 0;
            registration = Register(() =>
            {
                creates++;
                return new DeucarianEditorPage(new TextField { value = "Original" }, dispose: () => releases++);
            });
            var second = ScriptableObject.CreateInstance<TestWindow>();
            try
            {
                using (var other = new DeucarianEditorPageSession(second, "home", _ => { }))
                {
                    session.Navigate(ToolId);
                    other.Navigate(ToolId);
                    window.rootVisualElement.Q<TextField>().value = "First window";
                    Assert.That(second.rootVisualElement.Q<TextField>().value, Is.EqualTo("Original"));
                    Assert.That(creates, Is.EqualTo(2));
                }
                Assert.That(releases, Is.EqualTo(1));
                Assert.That(session.Navigate(ToolId), Is.True);
            }
            finally { UnityEngine.Object.DestroyImmediate(second); }
        }

        [Test]
        public void DisposeIsIdempotentAndStopsFurtherNavigation()
        {
            int released = 0, deactivated = 0;
            registration = Register(() => new DeucarianEditorPage(new VisualElement(),
                deactivate: () => deactivated++, dispose: () => released++));
            session.Navigate(ToolId);
            session.Dispose();
            session.Dispose();
            Assert.That(released, Is.EqualTo(1));
            Assert.That(deactivated, Is.EqualTo(1));
            Assert.That(session.Navigate("home"), Is.False);
        }

        [Test]
        public void CompatibilityControllerIsNeverStandaloneAndIsReleasedSynchronously()
        {
            TestWindow controller = null;
            var page = DeucarianEditorWindowPages.Create<TestWindow>((owner, root) =>
            { controller = owner; root.Add(new Label("Embedded")); });
            Assert.That(DeucarianEditorWindowPages.IsPageController(controller), Is.True);
            Assert.That(DeucarianEditorWindowPages.GetStandalone<TestWindow>(), Is.SameAs(window));
            var bounds = new Rect(23, 45, 930, 720);
            page.Update(bounds);
            Assert.That(controller.position, Is.EqualTo(bounds));
            page.Dispose();
            page.Dispose();
            Assert.That(controller == null, Is.True);
        }

        [Test]
        public void FailedCompatibilityBuildDoesNotLeakItsController()
        {
            TestWindow controller = null;
            Assert.Throws<InvalidOperationException>(() => DeucarianEditorWindowPages.Create<TestWindow>(
                (owner, _) => { controller = owner; throw new InvalidOperationException("Build failed"); }));
            Assert.That(controller == null, Is.True);
        }

        [UnityTest]
        public IEnumerator SidebarRoutesInPlaceAndAdvancedStaysInTheSameWindow()
        {
            session.Dispose();
            DeucarianEditorWorkspace workspace = null;
            session = new DeucarianEditorPageSession(window, "home", root =>
            {
                workspace = new DeucarianEditorWorkspace(root, "Test");
                DeucarianEditorWorkspaceNavigation.Populate(workspace, "home");
            });
            window.Show();
            yield return null;
            int visible = VisibleWindows();
            var bounds = window.position;
            try
            {
                yield return Click(window.rootVisualElement.Q<Button>("workspace-nav-" + DeucarianToolIds.ControlCenter));
                Assert.That(session.ActiveToolId, Is.EqualTo(DeucarianToolIds.ControlCenter));
                yield return Click(window.rootVisualElement.Q<Button>("workspace-nav-advanced"));
                Assert.That(session.ActiveToolId, Is.EqualTo(DeucarianToolIds.ControlCenter));
                Assert.That(window.rootVisualElement.Q("workspace-nav-advanced").ClassListContains("dw-selected"), Is.True);
                Assert.That(VisibleWindows(), Is.EqualTo(visible));
                Assert.That(window.position, Is.EqualTo(bounds));
                Assert.That(session.PageCount, Is.EqualTo(2));
            }
            finally { workspace?.Dispose(); }
        }

        [UnityTest]
        public IEnumerator ExplicitOpenCreatesIndependentNativeWindows()
        {
            int released = 0;
            registration = Register(() => new DeucarianEditorPage(new TextField { name = "draft" },
                dispose: () => released++));
            var first = DeucarianEditorToolWindow.Open(ToolId);
            var second = DeucarianEditorToolWindow.Open(ToolId);
            try
            {
                yield return null;
                Assert.That(first, Is.Not.SameAs(second));
                first.rootVisualElement.Q<TextField>("draft").value = "First";
                Assert.That(second.rootVisualElement.Q<TextField>("draft").value, Is.Empty);
            }
            finally { first.Close(); second.Close(); }
            Assert.That(released, Is.EqualTo(2));
        }

        [UnityTest]
        public IEnumerator SelectingOverviewAgainDoesNotRebuildItsContents()
        {
            window.Show();
            session.Navigate(DeucarianToolIds.ControlCenter, "overview");
            for (int i = 0; i < 4; i++) yield return null;
            var content = window.rootVisualElement.Q<ScrollView>("control-center-content");
            Assert.That(content.childCount, Is.GreaterThan(0));
            var first = content.ElementAt(0);
            session.Navigate(DeucarianToolIds.ControlCenter, "overview");
            Assert.That(content.Contains(first), Is.True);
        }

        private static IDisposable Register(Func<IDeucarianEditorPage> factory, Action open = null) =>
            DeucarianToolRegistry.Register(new DeucarianToolDescriptor(ToolId, "Test page", "Test",
                DeucarianControlCenterArea.Developer, open ?? (() => { }), "com.deucarian.editor", createPage: factory));

        private static int VisibleWindows() => Resources.FindObjectsOfTypeAll<EditorWindow>()
            .Count(candidate => !DeucarianEditorWindowPages.IsPageController(candidate));

        private static IEnumerator Click(Button button)
        {
            Assert.That(button, Is.Not.Null);
            button.Focus();
            yield return null;
            using (var evt = NavigationSubmitEvent.GetPooled())
            { evt.target = button; button.SendEvent(evt); }
        }

        public sealed class TestWindow : EditorWindow { }
    }
}
