using System;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Deucarian.Editor.Tests
{
    public sealed class DeucarianEditorReloadTests
    {
        private const string Tool = "test.reload.page";
        private TestWindow window;
        private DeucarianEditorPageSession session;
        private IDisposable registration;

        [SetUp] public void SetUp() => window = ScriptableObject.CreateInstance<TestWindow>();
        [TearDown] public void TearDown()
        {
            session?.Dispose();
            registration?.Dispose();
            SessionState.EraseString(DeucarianEditorReloadSnapshot.Key(window, "home"));
            UnityEngine.Object.DestroyImmediate(window);
        }

        [Test]
        public void RecreatedWindowSessionRestoresPageRouteDraftAndNavigationWithoutOpeningWindows()
        {
            int opened = 0;
            string route = null;
            registration = Register(() => DraftPage(value => route = value), () => opened++);
            session = NewSession(window);
            session.Navigate(Tool, "details");
            window.rootVisualElement.Q<TextField>("draft").value = "Keep pending edits";
            var navigation = window.rootVisualElement.Q<DeucarianEditorPageHost>().NavigationState;
            navigation.SetExpanded("Theming", false);
            navigation.ScrollOffset = 42;
            var bounds = window.position;
            session.Dispose();
            session = NewSession(window);
            session.RestoreSelection();
            Assert.That(session.ActiveToolId, Is.EqualTo(Tool));
            Assert.That(route, Is.Null, "Old selection commands must not overwrite a restored owner draft.");
            Assert.That(window.rootVisualElement.Q<TextField>("draft").value, Is.EqualTo("Keep pending edits"));
            navigation = window.rootVisualElement.Q<DeucarianEditorPageHost>().NavigationState;
            Assert.That(navigation.IsExpanded("Theming", true), Is.False);
            Assert.That(navigation.ScrollOffset, Is.EqualTo(42));
            Assert.That(window.position, Is.EqualTo(bounds));
            Assert.That(opened, Is.Zero);
        }

        [Test]
        public void DelayedRegistrationRestoresButExplicitNavigationCancelsPendingDestination()
        {
            registration = Register(() => DraftPage());
            session = NewSession(window);
            session.Navigate(Tool);
            session.Dispose();
            registration.Dispose(); registration = null;
            session = NewSession(window);
            session.RestoreSelection();
            Assert.That(session.ActiveToolId, Is.EqualTo("home"));
            session.Navigate("home");
            registration = Register(() => DraftPage());
            session.RestoreSelection();
            Assert.That(session.ActiveToolId, Is.EqualTo("home"), "Late registrations must not override the user's navigation.");
        }

        [Test]
        public void CachedInactiveDraftRestoresLazilyAndOtherWindowsRemainIndependent()
        {
            registration = Register(() => DraftPage());
            session = NewSession(window);
            session.Navigate(Tool);
            window.rootVisualElement.Q<TextField>("draft").value = "First window";
            session.Navigate("home");
            session.Dispose();
            var otherWindow = ScriptableObject.CreateInstance<TestWindow>();
            try
            {
                using (var other = NewSession(otherWindow))
                {
                    other.Navigate(Tool);
                    Assert.That(otherWindow.rootVisualElement.Q<TextField>("draft").value, Is.Empty);
                }
                session = NewSession(window);
                session.RestoreSelection();
                Assert.That(session.PageCount, Is.EqualTo(1));
                session.Navigate(Tool);
                Assert.That(window.rootVisualElement.Q<TextField>("draft").value, Is.EqualTo("First window"));
            }
            finally
            {
                SessionState.EraseString(DeucarianEditorReloadSnapshot.Key(otherWindow, "home"));
                UnityEngine.Object.DestroyImmediate(otherWindow);
            }
        }

        [Test]
        public void UnregisteredTextIsNeverAutomaticallySerialized()
        {
            session = new DeucarianEditorPageSession(window, "home", root => root.Add(new TextField { value = "unsanitized-input" }));
            session.Dispose();
            string json = SessionState.GetString(DeucarianEditorReloadSnapshot.Key(window, "home"), "");
            Assert.That(json, Does.Not.Contain("unsanitized-input"));
        }

        private static DeucarianEditorPageSession NewSession(EditorWindow owner) =>
            new DeucarianEditorPageSession(owner, "home", root => root.Add(new Label("Home")));
        private static IDisposable Register(Func<IDeucarianEditorPage> factory, Action open = null) =>
            DeucarianToolRegistry.Register(new DeucarianToolDescriptor(Tool, "Reload page", "Test",
                DeucarianControlCenterArea.Developer, open ?? (() => { }), "com.deucarian.editor", createPage: factory));
        private static IDeucarianEditorPage DraftPage(Action<string> activate = null)
        {
            var field = new TextField { name = "draft" };
            return new DeucarianEditorPage(field, activate,
                captureReloadState: () => JsonUtility.ToJson(new Draft { text = field.value }),
                restoreReloadState: state => field.SetValueWithoutNotify(JsonUtility.FromJson<Draft>(state).text));
        }
        [Serializable] private sealed class Draft { public string text; }
        private sealed class TestWindow : EditorWindow { }
    }
}
