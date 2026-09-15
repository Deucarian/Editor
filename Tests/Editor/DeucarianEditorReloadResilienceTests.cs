using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Deucarian.Editor.Tests
{
    public sealed class DeucarianEditorReloadResilienceTests
    {
        private const string Tool = "test.reload.resilience";
        private readonly List<IDisposable> registrations = new List<IDisposable>();
        private readonly List<EditorWindow> windows = new List<EditorWindow>();
        private DeucarianEditorPageSession session;

        [SetUp]
        public void SetUp()
        {
            ReloadController.ThrowOnRestore = false;
            ReloadController.Created.Clear();
            ReloadController.ReleaseCount = 0;
        }

        [TearDown]
        public void TearDown()
        {
            session?.Dispose();
            session = null;
            foreach (var owner in windows)
            {
                if (owner == null) continue;
                string homeKey = DeucarianEditorReloadSnapshot.Key(owner, "home");
                string toolKey = DeucarianEditorReloadSnapshot.Key(owner, Tool);
                UnityEngine.Object.DestroyImmediate(owner);
                SessionState.EraseString(homeKey);
                SessionState.EraseString(toolKey);
            }
            windows.Clear();
            foreach (var registration in registrations) registration.Dispose();
            registrations.Clear();
            // Keep a failing lifecycle regression from leaving its test controller alive.
            foreach (var controller in ReloadController.Created)
                if (controller != null) UnityEngine.Object.DestroyImmediate(controller);
            ReloadController.Created.Clear();
            ReloadController.ThrowOnRestore = false;
        }

        [Test]
        public void CaptureFailureStillSavesOtherPagesAndReleasesEveryPageExactlyOnce()
        {
            var window = CreateWindow<TestWindow>();
            int homeReleased = 0, failedReleased = 0, healthyReleased = 0;
            Register(Tool, () => new DeucarianEditorPage(new VisualElement(),
                dispose: () => failedReleased++,
                captureReloadState: () => throw new InvalidOperationException("Capture failed")));
            Register(Tool + ".healthy", () => new DeucarianEditorPage(new VisualElement(),
                dispose: () => healthyReleased++, captureReloadState: () => "healthy-draft"));
            session = new DeucarianEditorPageSession(window, "home",
                new DeucarianEditorPage(new VisualElement(), dispose: () => homeReleased++,
                    captureReloadState: () => "home-draft"));
            session.Navigate(Tool);
            session.Navigate(Tool + ".healthy");

            Assert.DoesNotThrow(() => session.Dispose());
            Assert.That(homeReleased, Is.EqualTo(1));
            Assert.That(failedReleased, Is.EqualTo(1));
            Assert.That(healthyReleased, Is.EqualTo(1));
            Assert.That(session.PageCount, Is.Zero);
            Assert.That(session.Navigate(Tool), Is.False);
            Assert.That(window.rootVisualElement.Q<DeucarianEditorPageHost>(), Is.Null);
            var saved = DeucarianEditorReloadSnapshot.Load(DeucarianEditorReloadSnapshot.Key(window, "home"));
            Assert.That(saved.Find("home").ownerState, Is.EqualTo("home-draft"));
            Assert.That(saved.Find(Tool + ".healthy").ownerState, Is.EqualTo("healthy-draft"));
            session.Dispose();
            Assert.That(homeReleased + failedReleased + healthyReleased, Is.EqualTo(3));
        }

        [Test]
        public void RestoreFailureReleasesCreatedControllerAndLeavesPreviousPageUsable()
        {
            var window = CreateWindow<TestWindow>();
            Register(Tool, ControllerPage);
            session = NewSession(window);
            session.Navigate(Tool);
            session.Dispose();
            Assert.That(ReloadController.ReleaseCount, Is.EqualTo(1));
            session = NewSession(window);
            ReloadController.ThrowOnRestore = true;

            Assert.Throws<InvalidOperationException>(() => session.Navigate(Tool));
            Assert.That(ReloadController.ReleaseCount, Is.EqualTo(2));
            Assert.That(ReloadController.Created[1] == null, Is.True,
                "A controller whose restore failed must be destroyed, not retained without a page owner.");
            Assert.That(session.ActiveToolId, Is.EqualTo("home"));
            Assert.That(session.PageCount, Is.EqualTo(1));
            Assert.That(window.rootVisualElement.Q<Label>("home-content"), Is.Not.Null);
            ReloadController.ThrowOnRestore = false;
            Assert.That(session.Navigate(Tool), Is.True, "A fresh retry can restore the retained saved draft.");
            Assert.That(ReloadController.Created[2].Draft, Is.EqualTo("controller-draft"));
        }

        [Test]
        public void RestoredSelectionWinsOverOldRouteWhileNewExplicitRouteStillApplies()
        {
            var window = CreateWindow<TestWindow>();
            Register(Tool, SelectionPage);
            session = NewSession(window);
            session.Navigate(Tool, "select:Palette A");
            var draft = window.rootVisualElement.Q<TextField>("selected-draft");
            Assert.That(draft.value, Is.EqualTo("Palette A"));
            draft.value = "Palette B with pending changes";
            session.Dispose();
            session = NewSession(window);
            session.RestoreSelection();

            Assert.That(session.ActiveToolId, Is.EqualTo(Tool));
            Assert.That(window.rootVisualElement.Q<TextField>("selected-draft").value,
                Is.EqualTo("Palette B with pending changes"));
            Assert.That(session.Navigate(Tool, "select:Palette C"), Is.True);
            Assert.That(window.rootVisualElement.Q<TextField>("selected-draft").value, Is.EqualTo("Palette C"));
        }

        [Test]
        public void NativeToolWindowRebuildDoesNotReplayOriginalSelectionRouteOverRestoredDraft()
        {
            Register(Tool, SelectionPage);
            var window = CreateToolWindow("select:Palette A");
            window.CreateGUI();
            var draft = window.rootVisualElement.Q<TextField>("selected-draft");
            Assert.That(draft.value, Is.EqualTo("Palette A"), "The route is required on an explicit first open.");
            draft.value = "Palette B with pending changes";

            window.CreateGUI();

            Assert.That(window.rootVisualElement.Q<TextField>("selected-draft").value,
                Is.EqualTo("Palette B with pending changes"));
        }

        [Test]
        public void NativeToolWindowHomeRestoreFailureReleasesUnownedController()
        {
            Register(Tool, ControllerPage);
            var window = CreateToolWindow(null);
            window.CreateGUI();
            ReloadController.Created[0].Draft = "Pending draft before failed reload";
            ReloadController.ThrowOnRestore = true;

            Assert.Throws<InvalidOperationException>(() => window.CreateGUI());

            Assert.That(ReloadController.ReleaseCount, Is.EqualTo(2),
                "Both the replaced page and the failed replacement require release.");
            Assert.That(ReloadController.Created[1] == null, Is.True);
            ReloadController.ThrowOnRestore = false;
            window.CreateGUI();
            Assert.That(ReloadController.Created[2].Draft, Is.EqualTo("Pending draft before failed reload"),
                "A failed home restore must retain the saved draft for the next attempt.");
        }

        private T CreateWindow<T>() where T : EditorWindow
        {
            var window = ScriptableObject.CreateInstance<T>();
            windows.Add(window);
            return window;
        }

        [Test]
        public void TemporaryPageRestoreFailureRetainsSelectionAcrossAnotherReloadThenRecovers()
        {
            bool unavailable = false;
            Register(Tool, () => unavailable ? throw new InvalidOperationException("Assets still importing") : SelectionPage());
            var window = CreateWindow<TestWindow>();
            session = NewSession(window);
            session.Navigate(Tool, "select:Definition A");
            window.rootVisualElement.Q<TextField>("selected-draft").value = "Newly created definition B";
            session.Dispose();
            unavailable = true;
            session = NewSession(window);
            session.RestoreSelection();
            Assert.That(session.ActiveToolId, Is.EqualTo("home"));
            session.Dispose();
            Assert.That(DeucarianEditorReloadSnapshot.Load(DeucarianEditorReloadSnapshot.Key(window, "home")).activeToolId, Is.EqualTo(Tool));
            session = NewSession(window);
            session.RestoreSelection();
            unavailable = false;
            session.RestoreSelection();
            Assert.That(session.ActiveToolId, Is.EqualTo(Tool));
            Assert.That(window.rootVisualElement.Q<TextField>("selected-draft").value, Is.EqualTo("Newly created definition B"));
        }

        private DeucarianEditorToolWindow CreateToolWindow(string route)
        {
            var window = CreateWindow<DeucarianEditorToolWindow>();
            var serialized = new SerializedObject(window);
            serialized.FindProperty("toolId").stringValue = Tool;
            serialized.FindProperty("route").stringValue = route ?? string.Empty;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return window;
        }

        private void Register(string id, Func<IDeucarianEditorPage> factory)
        {
            registrations.Add(DeucarianToolRegistry.Register(new DeucarianToolDescriptor(
                id, "Reload resilience", "Test", DeucarianControlCenterArea.Developer,
                () => { }, "com.deucarian.editor", createPage: factory)));
        }

        private static DeucarianEditorPageSession NewSession(EditorWindow window) =>
            new DeucarianEditorPageSession(window, "home", root =>
                root.Add(new Label("Home") { name = "home-content" }));

        private static IDeucarianEditorPage ControllerPage() =>
            DeucarianEditorWindowPages.Create<ReloadController>((controller, root) =>
            {
                root.Clear();
                root.Add(new Label(controller.Draft));
            });

        private static IDeucarianEditorPage SelectionPage()
        {
            var field = new TextField { name = "selected-draft" };
            return new DeucarianEditorPage(field,
                activate: route =>
                {
                    if (route != null && route.StartsWith("select:", StringComparison.Ordinal))
                        field.value = route.Substring(7);
                },
                captureReloadState: () => JsonUtility.ToJson(new SelectionState { draft = field.value }),
                restoreReloadState: state => field.SetValueWithoutNotify(JsonUtility.FromJson<SelectionState>(state).draft));
        }

        [Serializable] private sealed class SelectionState { public string draft; }
        private sealed class TestWindow : EditorWindow { }

        private sealed class ReloadController : EditorWindow, IDeucarianEditorReloadState
        {
            internal static readonly List<ReloadController> Created = new List<ReloadController>();
            internal static bool ThrowOnRestore;
            internal static int ReleaseCount;
            internal string Draft = "controller-draft";
            private void OnEnable() => Created.Add(this);
            private void OnDisable() => ReleaseCount++;
            public string CaptureReloadState() => Draft;
            public void RestoreReloadState(string state)
            {
                if (ThrowOnRestore) throw new InvalidOperationException("Restore failed");
                Draft = state;
            }
        }
    }
}
