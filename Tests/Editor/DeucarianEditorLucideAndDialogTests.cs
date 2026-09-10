using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace Deucarian.Editor.Tests
{
    public sealed class DeucarianEditorLucideAndDialogTests
    {
        private const string LucideAssetRoot =
            "Packages/com.deucarian.editor/Editor/Assets/Icons/Lucide";

        [Test]
        public void EveryPublicIconId_ResolvesItsOwnVendoredAsset()
        {
            FieldInfo[] fields = typeof(DeucarianEditorIconIds).GetFields(
                BindingFlags.Public | BindingFlags.Static);
            Assert.Greater(fields.Length, 40);

            foreach (FieldInfo field in fields)
            {
                string iconId = field.GetRawConstantValue() as string;
                Assert.IsNotEmpty(iconId, field.Name);
                Assert.IsTrue(DeucarianEditorIcons.IsKnownIconId(iconId), field.Name + ": " + iconId);
                Texture2D icon = DeucarianEditorIcons.GetIcon(iconId);
                Assert.NotNull(icon, field.Name);
                Assert.AreEqual(
                    LucideAssetRoot + "/" + iconId + ".png",
                    AssetDatabase.GetAssetPath(icon),
                    field.Name);
            }
        }

        [Test]
        public void EveryVendoredLucidePng_HasRealTextureSvgAndUniqueMeta()
        {
            string packageRoot = GetPackageRoot();
            string pngRoot = Path.Combine(packageRoot, "Editor", "Assets", "Icons", "Lucide");
            string svgRoot = Path.Combine(
                packageRoot,
                "Documentation~",
                "ThirdParty",
                "Lucide-1.22.0",
                "svg");
            string[] pngPaths = Directory.GetFiles(pngRoot, "*.png", SearchOption.TopDirectoryOnly);
            Assert.GreaterOrEqual(pngPaths.Length, 104);
            var guids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (string pngPath in pngPaths)
            {
                string iconId = Path.GetFileNameWithoutExtension(pngPath);
                Assert.IsTrue(DeucarianEditorIcons.IsKnownIconId(iconId), iconId);
                Texture2D texture = DeucarianEditorIcons.GetIcon(iconId);
                Assert.NotNull(texture, iconId);
                Assert.AreEqual(32, texture.width, iconId);
                Assert.AreEqual(32, texture.height, iconId);
                Assert.IsTrue(File.Exists(Path.Combine(svgRoot, iconId + ".svg")), iconId);

                string metaPath = pngPath + ".meta";
                Assert.IsTrue(File.Exists(metaPath), metaPath);
                string guidLine = File.ReadLines(metaPath)
                    .FirstOrDefault(line => line.StartsWith("guid: ", StringComparison.Ordinal));
                Assert.IsNotNull(guidLine, metaPath);
                string guid = guidLine.Substring("guid: ".Length).Trim();
                Assert.AreEqual(32, guid.Length, metaPath);
                Assert.IsTrue(guids.Add(guid), "Duplicate Unity GUID: " + guid);
            }
        }

        [TestCase(DeucarianEditorIconIds.Audio)]
        [TestCase(DeucarianEditorIconIds.Palette)]
        public void FeatureIcons_ImportAsWhiteGlyphsThatCanBeTinted(string iconId)
        {
            Texture2D icon = DeucarianEditorIcons.GetIcon(iconId);
            RenderTexture previous = RenderTexture.active;
            RenderTexture target = RenderTexture.GetTemporary(32, 32, 0, RenderTextureFormat.ARGB32);
            var readable = new Texture2D(32, 32, TextureFormat.RGBA32, false);
            try
            {
                Graphics.Blit(icon, target);
                RenderTexture.active = target;
                readable.ReadPixels(new Rect(0, 0, 32, 32), 0, 0);
                readable.Apply();
                Color[] glyph = readable.GetPixels().Where(pixel => pixel.a > 0.9f).ToArray();
                Assert.Greater(glyph.Length, 10, iconId + " has no visible glyph");
                Assert.IsTrue(glyph.All(pixel => pixel.r > 0.9f && pixel.g > 0.9f && pixel.b > 0.9f),
                    iconId + " must import white before the editor applies its tint");
            }
            finally
            {
                RenderTexture.active = previous;
                RenderTexture.ReleaseTemporary(target);
                UnityEngine.Object.DestroyImmediate(readable);
            }
        }

        [TestCase("../package")]
        [TestCase("folder/open")]
        [TestCase("folder\\open")]
        [TestCase("package.png")]
        [TestCase("-package")]
        [TestCase("package-")]
        [TestCase("package--check")]
        [TestCase("Package")]
        [TestCase("%2e%2e-package")]
        public void UnsafeIconIds_AreRejectedAndUseCanonicalPackageFallback(string iconId)
        {
            Texture2D fallback = DeucarianEditorIcons.GetIcon(DeucarianEditorIconIds.Package);
            Assert.NotNull(fallback);
            Assert.IsFalse(DeucarianEditorIcons.IsKnownIconId(iconId), iconId);
            Assert.AreSame(fallback, DeucarianEditorIcons.GetIcon(iconId), iconId);
            Assert.AreEqual(
                LucideAssetRoot + "/package.png",
                AssetDatabase.GetAssetPath(DeucarianEditorIcons.GetIcon(iconId)),
                iconId);
        }

        [Test]
        public void MissingSafeIconId_UsesCanonicalPackageFallback()
        {
            Texture2D fallback = DeucarianEditorIcons.GetIcon(DeucarianEditorIconIds.Package);
            Assert.IsFalse(DeucarianEditorIcons.IsKnownIconId("not-a-vendored-icon"));
            Assert.AreSame(fallback, DeucarianEditorIcons.GetIcon("not-a-vendored-icon"));
            Assert.AreSame(fallback, DeucarianEditorIcons.GetFallbackIcon("Anything"));
        }

        [Test]
        public void PackageIconLookup_PrefersDirectIdsAndPreservesLegacyAliases()
        {
            Assert.AreEqual(
                LucideAssetRoot + "/activity.png",
                AssetDatabase.GetAssetPath(DeucarianEditorIcons.GetPackageIcon("activity")));

            var aliases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "package-installer", "package-plus" },
                { "theming", "palette" },
                { "diagnostics", "info" },
                { "logging", "scroll-text" },
                { "object-loading", "folder-open" },
                { "api-helper", "braces" },
                { "session", "key-round" },
                { "selection", "mouse-pointer-click" },
                { "generic-ui-items", "component" },
                { "editor", "wrench" }
            };

            foreach (KeyValuePair<string, string> alias in aliases)
            {
                Texture2D icon = DeucarianEditorIcons.GetPackageIcon(alias.Key);
                Assert.NotNull(icon, alias.Key);
                Assert.AreEqual(
                    LucideAssetRoot + "/" + alias.Value + ".png",
                    AssetDatabase.GetAssetPath(icon),
                    alias.Key);
                Assert.IsTrue(DeucarianEditorIcons.IsKnownPackageKey(alias.Key), alias.Key);
            }
        }

        [Test]
        public void SharedWorkflowIconConstants_HaveStableSemanticSlugs()
        {
            Assert.AreEqual("activity", DeucarianEditorIconIds.Activity);
            Assert.AreEqual("scan", DeucarianEditorIconIds.ActualSize);
            Assert.AreEqual("circle-arrow-up", DeucarianEditorIconIds.Available);
            Assert.AreEqual("chevron-left", DeucarianEditorIconIds.Back);
            Assert.AreEqual("loader-circle", DeucarianEditorIconIds.Busy);
            Assert.AreEqual("locate-fixed", DeucarianEditorIconIds.Center);
            Assert.AreEqual("x", DeucarianEditorIconIds.Clear);
            Assert.AreEqual("git-compare-arrows", DeucarianEditorIconIds.Compare);
            Assert.AreEqual("layout-dashboard", DeucarianEditorIconIds.Dashboard);
            Assert.AreEqual("panel-right-open", DeucarianEditorIconIds.Details);
            Assert.AreEqual("circle-x", DeucarianEditorIconIds.Error);
            Assert.AreEqual("list-filter", DeucarianEditorIconIds.Filter);
            Assert.AreEqual("maximize-2", DeucarianEditorIconIds.Fit);
            Assert.AreEqual("package-x", DeucarianEditorIconIds.MissingPackage);
            Assert.AreEqual("circle", DeucarianEditorIconIds.Optional);
            Assert.AreEqual("package-check", DeucarianEditorIconIds.PackageCheck);
            Assert.AreEqual("search-x", DeucarianEditorIconIds.SearchClear);
            Assert.AreEqual("panel-bottom-close", DeucarianEditorIconIds.HideDetails);
            Assert.AreEqual("panel-bottom-open", DeucarianEditorIconIds.ShowDetails);
            Assert.AreEqual("circle-stop", DeucarianEditorIconIds.Stop);
            Assert.AreEqual("circle-check", DeucarianEditorIconIds.Success);
            Assert.AreEqual("boxes", DeucarianEditorIconIds.Suite);
            Assert.AreEqual("circle-arrow-up", DeucarianEditorIconIds.Update);
        }

        [Test]
        public void StatusIconRow_PublicApiCoexistsWithLegacyMarkerRow()
        {
            Assert.NotNull(typeof(DeucarianEditorWorkbenchGUI).GetMethod(
                "DrawStatusRow",
                new[] { typeof(string), typeof(string), typeof(DeucarianEditorStatus) }));
            Assert.NotNull(typeof(DeucarianEditorWorkbenchGUI).GetMethod(
                "DrawStatusIconRow",
                new[] { typeof(string), typeof(string), typeof(DeucarianEditorStatus) }));

            string source = File.ReadAllText(Path.Combine(
                GetPackageRoot(),
                "Editor",
                "DeucarianEditorWorkbenchGUI.cs"));
            int methodStart = source.IndexOf("public static void DrawStatusIconRow", StringComparison.Ordinal);
            int methodEnd = source.IndexOf("internal static void EndPanel", methodStart, StringComparison.Ordinal);
            Assert.GreaterOrEqual(methodStart, 0);
            Assert.Greater(methodEnd, methodStart);
            string method = source.Substring(methodStart, methodEnd - methodStart);
            StringAssert.Contains("GetStatusRowRects", method);
            StringAssert.Contains("DeucarianEditorIcons.DrawIcon", method);
            StringAssert.Contains("DeucarianEditorStatusBadge.GetColor(status)", method);
        }

        [Test]
        public void SharedIconToolbar_UsesVendoredLucideAssets()
        {
            GUIContent content = DeucarianEditorIconToolbar.BuildContent(
                DeucarianEditorIconIds.Activity,
                "Activity");
            Assert.NotNull(content.image);
            Assert.AreEqual("Activity", content.tooltip);
            Assert.AreEqual(
                LucideAssetRoot + "/activity.png",
                AssetDatabase.GetAssetPath(content.image));

            string source = File.ReadAllText(Path.Combine(
                GetPackageRoot(),
                "Editor",
                "DeucarianEditorWorkflowControls.cs"));
            StringAssert.DoesNotContain("EditorGUIUtility.IconContent", source);
            StringAssert.Contains("DeucarianEditorIcons.GetIconContent", source);
        }

        [Test]
        public void SharedIconToolbar_PreservesPublicNamedArgumentCompatibility()
        {
            MethodInfo button = typeof(DeucarianEditorIconToolbar).GetMethod("Button");
            MethodInfo toggle = typeof(DeucarianEditorIconToolbar).GetMethod("Toggle");
            MethodInfo buildContent = typeof(DeucarianEditorIconToolbar).GetMethod("BuildContent");

            Assert.NotNull(button);
            Assert.NotNull(toggle);
            Assert.NotNull(buildContent);
            Assert.AreEqual("iconName", button.GetParameters()[0].Name);
            Assert.AreEqual("iconName", toggle.GetParameters()[1].Name);
            Assert.AreEqual("iconName", buildContent.GetParameters()[0].Name);
        }

        [Test]
        public void DialogContracts_AreGenericAndResponsive()
        {
            var primary = new DeucarianEditorDialogAction(
                "continue",
                "Continue with this deliberately long action label that must remain readable",
                DeucarianEditorIconIds.Play,
                DeucarianEditorDialogActionStyle.Primary);
            var cancel = new DeucarianEditorDialogAction(
                "cancel",
                "Cancel",
                DeucarianEditorIconIds.Clear);
            var options = new DeucarianEditorDialogOptions(
                "Confirm operation",
                new string('M', 500),
                DeucarianEditorIconIds.Warning,
                new[] { primary, cancel })
            {
                Details = string.Join("\n", Enumerable.Repeat("Detailed line that must wrap.", 20)),
                DefaultActionId = primary.Id,
                CancelActionId = cancel.Id
            };

            Vector2 recommended = DeucarianEditorDialog.CalculateRecommendedSize(options);
            Assert.That(recommended.x, Is.InRange(
                DeucarianEditorDialog.MinimumWidth,
                DeucarianEditorDialog.MaximumWidth));
            Assert.That(recommended.y, Is.InRange(
                DeucarianEditorDialog.MinimumHeight,
                DeucarianEditorDialog.MaximumHeight));
            Assert.Greater(recommended.y, DeucarianEditorDialog.MinimumHeight);
            Assert.AreEqual("continue", primary.Id);
            Assert.AreEqual("play", primary.IconId);
            Assert.AreEqual(DeucarianEditorDialogActionStyle.Primary, primary.Style);

            var result = new DeucarianEditorDialogResult(
                cancel.Id,
                DeucarianEditorDialogCompletionReason.Escape,
                true);
            Assert.AreEqual("cancel", result.ActionId);
            Assert.AreEqual(DeucarianEditorDialogCompletionReason.Escape, result.Reason);
            Assert.IsTrue(result.WasCanceled);
        }

        [Test]
        public void DialogSource_HandlesActionsKeyboardCloseAndWrappedContent()
        {
            string packageRoot = GetPackageRoot();
            string source = File.ReadAllText(Path.Combine(
                packageRoot,
                "Editor",
                "DeucarianEditorDialog.cs"));
            string uss = File.ReadAllText(Path.Combine(
                packageRoot,
                "Editor",
                "Assets",
                "Styles",
                "DeucarianEditor.uss"));

            StringAssert.Contains("RegisterCallback<KeyDownEvent>", source);
            StringAssert.Contains("KeyCode.Return", source);
            StringAssert.Contains("KeyCode.KeypadEnter", source);
            StringAssert.Contains("KeyCode.Escape", source);
            StringAssert.Contains("DeucarianEditorDialogCompletionReason.WindowClosed", source);
            StringAssert.Contains("callback?.Invoke", source);
            StringAssert.Contains("if (_completed)", source);
            StringAssert.Contains("DeucarianEditorIconTextButton.Create", source);
            StringAssert.Contains("new ScrollView(ScrollViewMode.Vertical)", source);
            StringAssert.Contains("deucarian-dialog__copy-scroll", source);
            StringAssert.Contains(".deucarian-dialog__message", uss);
            StringAssert.Contains(".deucarian-dialog__copy-scroll", uss);
            StringAssert.Contains(".deucarian-dialog__details-text", uss);
            StringAssert.Contains("white-space: normal;", uss);
            StringAssert.Contains(".deucarian-dialog--narrow", uss);
            StringAssert.Contains("flex-direction: column;", uss);
            StringAssert.Contains(".deucarian-dialog__action--primary", uss);
        }

        [Test]
        public void DialogWindow_PrimaryActionIsEmphasizedWithoutChangingContentAlignment()
        {
            var primary = new DeucarianEditorDialogAction(
                "continue",
                "Continue with this deliberately long action label that must remain readable",
                DeucarianEditorIconIds.Play,
                DeucarianEditorDialogActionStyle.Primary);
            var secondary = new DeucarianEditorDialogAction(
                "cancel",
                "Cancel",
                DeucarianEditorIconIds.Clear);
            var options = new DeucarianEditorDialogOptions(
                "Confirm operation",
                "Review the operation.",
                DeucarianEditorIconIds.Warning,
                new[] { primary, secondary });
            Type windowType = typeof(DeucarianEditorDialog).Assembly.GetType(
                "Deucarian.Editor.DeucarianEditorDialogWindow",
                true);
            EditorWindow window = ScriptableObject.CreateInstance(windowType) as EditorWindow;
            Assert.NotNull(window);

            try
            {
                MethodInfo initialize = windowType.GetMethod(
                    "Initialize",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                MethodInfo createGui = windowType.GetMethod(
                    "CreateGUI",
                    BindingFlags.Instance | BindingFlags.Public);
                Assert.NotNull(initialize);
                Assert.NotNull(createGui);
                initialize.Invoke(window, new object[]
                {
                    options,
                    (Action<DeucarianEditorDialogResult>)(_ => { })
                });
                createGui.Invoke(window, null);

                Button primaryButton = window.rootVisualElement.Q<Button>(
                    "deucarian-dialog-action-continue");
                Button secondaryButton = window.rootVisualElement.Q<Button>(
                    "deucarian-dialog-action-cancel");
                Assert.NotNull(primaryButton);
                Assert.NotNull(secondaryButton);
                Assert.IsTrue(primaryButton.ClassListContains(
                    "deucarian-dialog__action--primary"));
                Assert.IsTrue(secondaryButton.ClassListContains(
                    "deucarian-dialog__action--secondary"));
                Assert.IsFalse(primaryButton.ClassListContains(
                    DeucarianEditorIconTextButton.LeadingClass));
                Assert.IsFalse(secondaryButton.ClassListContains(
                    DeucarianEditorIconTextButton.LeadingClass));
                Label primaryLabel = primaryButton.Q<Label>(
                    className: DeucarianEditorIconTextButton.LabelClass);
                VisualElement primaryContent = primaryButton.Q<VisualElement>(
                    className: DeucarianEditorIconTextButton.ContentClass);
                Assert.NotNull(primaryLabel);
                Assert.NotNull(primaryContent);
                Assert.AreEqual(StyleKeyword.Auto, primaryButton.style.height.keyword);
                Assert.AreEqual(StyleKeyword.None, primaryButton.style.maxHeight.keyword);
                Assert.AreEqual(StyleKeyword.Auto, primaryContent.style.height.keyword);
                Assert.AreEqual(StyleKeyword.None, primaryContent.style.maxHeight.keyword);
                Assert.AreEqual(StyleKeyword.Auto, primaryLabel.style.height.keyword);
                Assert.AreEqual(StyleKeyword.None, primaryLabel.style.maxHeight.keyword);
                Assert.AreEqual(WhiteSpace.Normal, primaryLabel.style.whiteSpace.value);
                Assert.AreEqual(Overflow.Hidden, primaryLabel.style.overflow.value);
                Assert.AreEqual(TextOverflow.Ellipsis, primaryLabel.style.textOverflow.value);
                Assert.AreEqual(primary.Label, primaryButton.tooltip);
            }
            finally
            {
                if (window != null)
                {
                    UnityEngine.Object.DestroyImmediate(window);
                }
            }
        }

        [Test]
        public void DialogAction_RejectsMissingStableId()
        {
            Assert.Throws<ArgumentException>(() =>
                new DeucarianEditorDialogAction(
                    " ",
                    "Continue",
                    DeucarianEditorIconIds.Play));
        }

        [Test]
        public void DialogWindowClose_CompletesCallbackExactlyOnceWithCancelAction()
        {
            var cancel = new DeucarianEditorDialogAction(
                "cancel",
                "Cancel",
                DeucarianEditorIconIds.Clear);
            var options = new DeucarianEditorDialogOptions(
                "Close behavior",
                "Closing the window is a cancellation.",
                DeucarianEditorIconIds.Info,
                new[] { cancel })
            {
                CancelActionId = cancel.Id
            };
            Type windowType = typeof(DeucarianEditorDialog).Assembly.GetType(
                "Deucarian.Editor.DeucarianEditorDialogWindow",
                true);
            EditorWindow window = ScriptableObject.CreateInstance(windowType) as EditorWindow;
            Assert.NotNull(window);
            int completionCount = 0;
            DeucarianEditorDialogResult result = default(DeucarianEditorDialogResult);

            try
            {
                MethodInfo initialize = windowType.GetMethod(
                    "Initialize",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                MethodInfo onDestroy = windowType.GetMethod(
                    "OnDestroy",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                Assert.NotNull(initialize);
                Assert.NotNull(onDestroy);
                initialize.Invoke(window, new object[]
                {
                    options,
                    (Action<DeucarianEditorDialogResult>)(value =>
                    {
                        completionCount++;
                        result = value;
                    })
                });

                onDestroy.Invoke(window, null);
                onDestroy.Invoke(window, null);

                Assert.AreEqual(1, completionCount);
                Assert.AreEqual("cancel", result.ActionId);
                Assert.AreEqual(DeucarianEditorDialogCompletionReason.WindowClosed, result.Reason);
                Assert.IsTrue(result.WasCanceled);
            }
            finally
            {
                if (window != null)
                {
                    UnityEngine.Object.DestroyImmediate(window);
                }
            }
        }

        private static string GetPackageRoot()
        {
            PackageInfo package = PackageInfo.FindForAssetPath(
                "Packages/com.deucarian.editor/package.json");
            string root = package == null ? Path.GetFullPath(".") : package.resolvedPath;
            Assert.IsTrue(File.Exists(Path.Combine(root, "package.json")), root);
            return root;
        }
    }
}
