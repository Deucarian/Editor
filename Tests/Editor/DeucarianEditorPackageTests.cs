using System;
using NUnit.Framework;
using UnityEngine;

namespace Deucarian.Editor.Tests
{
    public sealed class DeucarianEditorPackageTests
    {
        private static readonly string[] KnownPackageKeys =
        {
            "package-installer",
            "theming",
            "logging",
            "object-loading",
            "api-helper",
            "session",
            "selection",
            "generic-ui-items",
            "editor"
        };

        [Test]
        public void PackageConstants_AreCorrect()
        {
            Assert.AreEqual("com.deucarian.editor", DeucarianEditorPackageConstants.PackageName);
            Assert.AreEqual("Deucarian Editor", DeucarianEditorPackageConstants.DisplayName);
            Assert.AreEqual("0.1.0", DeucarianEditorPackageConstants.Version);
            Assert.AreEqual("Deucarian", DeucarianEditorPackageConstants.MenuRoot);
        }

        [Test]
        public void PackageIconLookup_DoesNotThrowForKnownKeys()
        {
            foreach (string key in KnownPackageKeys)
            {
                Texture2D icon = null;
                Assert.DoesNotThrow(() => icon = DeucarianEditorIcons.GetPackageIcon(key));
                Assert.NotNull(icon, key);
                Assert.IsTrue(DeucarianEditorIcons.IsKnownPackageKey(key), key);
            }
        }

        [Test]
        public void FallbackIconAndContent_ReturnNonNull()
        {
            Assert.NotNull(DeucarianEditorIcons.GetFallbackIcon("Missing"));
            Assert.NotNull(DeucarianEditorIcons.GetPackageContent("missing-package", "Missing", "Tooltip"));
        }

        [Test]
        public void StyleAccessors_DoNotThrow()
        {
            Assert.DoesNotThrow(() =>
            {
                Assert.NotNull(DeucarianEditorStyles.PackageHeaderTitle);
                Assert.NotNull(DeucarianEditorStyles.PackageHeaderSubtitle);
                Assert.NotNull(DeucarianEditorStyles.SectionTitle);
                Assert.NotNull(DeucarianEditorStyles.SectionBox);
                Assert.NotNull(DeucarianEditorStyles.MutedLabel);
                Assert.NotNull(DeucarianEditorStyles.StatusBadge);
                Assert.NotNull(DeucarianEditorStyles.ToolbarButton);
                Assert.NotNull(DeucarianEditorStyles.FooterVersionText);
            });
        }

        [Test]
        public void StatusBadgeHelpers_DoNotThrow()
        {
            foreach (DeucarianEditorStatus status in Enum.GetValues(typeof(DeucarianEditorStatus)))
            {
                Assert.DoesNotThrow(() => DeucarianEditorStatusBadge.GetColor(status));
                Assert.DoesNotThrow(() => DeucarianEditorStatusBadge.GetContent(status.ToString(), status));
                Assert.IsTrue(DeucarianEditorStatusBadge.IsValid(status));
            }
        }

        [Test]
        public void AssetFieldHelperApi_IsAvailable()
        {
            Assert.NotNull(typeof(DeucarianEditorFields).GetMethod("DrawAssetFieldWithSelectButton"));
        }

        [Test]
        public void UxStandards_BuildExpectedMenuPath()
        {
            Assert.AreEqual(
                "Deucarian/Theming/Open Theme Manager",
                DeucarianEditorUxStandards.GetPackageMenuPath("Theming", "Open Theme Manager"));
        }
    }
}
