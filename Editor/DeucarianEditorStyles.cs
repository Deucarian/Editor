using UnityEditor;
using UnityEngine;

namespace Deucarian.Editor
{
    public static class DeucarianEditorStyles
    {
        private static bool? cachedProSkin;
        private static GUIStyle packageHeaderBox;
        private static GUIStyle packageHeaderTitle;
        private static GUIStyle packageHeaderSubtitle;
        private static GUIStyle sectionTitle;
        private static GUIStyle sectionBox;
        private static GUIStyle mutedLabel;
        private static GUIStyle statusBadge;
        private static GUIStyle toolbarButton;
        private static GUIStyle footerVersionText;

        public static GUIStyle PackageHeaderBox
        {
            get
            {
                EnsureStyles();
                return packageHeaderBox;
            }
        }

        public static GUIStyle PackageHeaderTitle
        {
            get
            {
                EnsureStyles();
                return packageHeaderTitle;
            }
        }

        public static GUIStyle PackageHeaderSubtitle
        {
            get
            {
                EnsureStyles();
                return packageHeaderSubtitle;
            }
        }

        public static GUIStyle SectionTitle
        {
            get
            {
                EnsureStyles();
                return sectionTitle;
            }
        }

        public static GUIStyle SectionBox
        {
            get
            {
                EnsureStyles();
                return sectionBox;
            }
        }

        public static GUIStyle MutedLabel
        {
            get
            {
                EnsureStyles();
                return mutedLabel;
            }
        }

        public static GUIStyle StatusBadge
        {
            get
            {
                EnsureStyles();
                return statusBadge;
            }
        }

        public static GUIStyle ToolbarButton
        {
            get
            {
                EnsureStyles();
                return toolbarButton;
            }
        }

        public static GUIStyle FooterVersionText
        {
            get
            {
                EnsureStyles();
                return footerVersionText;
            }
        }

        public static void ClearCache()
        {
            cachedProSkin = null;
            packageHeaderBox = null;
            packageHeaderTitle = null;
            packageHeaderSubtitle = null;
            sectionTitle = null;
            sectionBox = null;
            mutedLabel = null;
            statusBadge = null;
            toolbarButton = null;
            footerVersionText = null;
        }

        private static void EnsureStyles()
        {
            bool proSkin = EditorGUIUtility.isProSkin;
            if (cachedProSkin.HasValue && cachedProSkin.Value == proSkin && packageHeaderTitle != null)
            {
                return;
            }

            cachedProSkin = proSkin;

            packageHeaderBox = CopyStyle(() => EditorStyles.helpBox);
            packageHeaderBox.padding = new RectOffset(12, 12, 10, 10);
            packageHeaderBox.margin = new RectOffset(0, 0, 0, 10);
            packageHeaderBox.normal.background = TextureForColor("header", DeucarianEditorColors.HeaderBackground);

            packageHeaderTitle = CopyStyle(() => EditorStyles.boldLabel);
            packageHeaderTitle.fontSize = 18;
            packageHeaderTitle.fontStyle = FontStyle.Bold;
            packageHeaderTitle.wordWrap = true;
            packageHeaderTitle.alignment = TextAnchor.MiddleLeft;
            packageHeaderTitle.normal.textColor = DeucarianEditorColors.TitleText;

            packageHeaderSubtitle = CopyStyle(() => EditorStyles.label);
            packageHeaderSubtitle.fontSize = 11;
            packageHeaderSubtitle.wordWrap = true;
            packageHeaderSubtitle.alignment = TextAnchor.MiddleLeft;
            packageHeaderSubtitle.normal.textColor = DeucarianEditorColors.MutedText;

            sectionTitle = CopyStyle(() => EditorStyles.boldLabel);
            sectionTitle.fontSize = 12;
            sectionTitle.fontStyle = FontStyle.Bold;
            sectionTitle.wordWrap = true;
            sectionTitle.margin = new RectOffset(0, 0, 8, 4);
            sectionTitle.normal.textColor = DeucarianEditorColors.TitleText;

            sectionBox = CopyStyle(() => EditorStyles.helpBox);
            sectionBox.padding = new RectOffset(10, 10, 8, 8);
            sectionBox.margin = new RectOffset(0, 0, 0, 8);
            sectionBox.normal.background = TextureForColor("section", DeucarianEditorColors.SectionBackground);

            mutedLabel = CopyStyle(() => EditorStyles.label);
            mutedLabel.wordWrap = true;
            mutedLabel.normal.textColor = DeucarianEditorColors.MutedText;

            statusBadge = CopyStyle(() => EditorStyles.miniBoldLabel);
            statusBadge.alignment = TextAnchor.MiddleCenter;
            statusBadge.padding = new RectOffset(7, 7, 2, 3);
            statusBadge.fixedHeight = 18;
            statusBadge.clipping = TextClipping.Clip;
            statusBadge.normal.textColor = DeucarianEditorColors.BadgeText;

            toolbarButton = CopyStyle(() => EditorStyles.toolbarButton);
            toolbarButton.alignment = TextAnchor.MiddleCenter;
            toolbarButton.padding = new RectOffset(8, 8, 2, 2);

            footerVersionText = CopyStyle(() => EditorStyles.miniLabel);
            footerVersionText.alignment = TextAnchor.MiddleRight;
            footerVersionText.wordWrap = true;
            footerVersionText.margin = new RectOffset(0, 0, 6, 0);
            footerVersionText.normal.textColor = DeucarianEditorColors.MutedText;
        }

        private static GUIStyle CopyStyle(System.Func<GUIStyle> styleFactory)
        {
            if (styleFactory != null)
            {
                try
                {
                    GUIStyle style = styleFactory();
                    if (style != null)
                    {
                        return new GUIStyle(style);
                    }
                }
                catch
                {
                    // Some EditorStyles are unavailable during headless batch-mode startup.
                }
            }

            return new GUIStyle();
        }

        private static Texture2D TextureForColor(string name, Color color)
        {
            return DeucarianEditorTextureCache.Get(name + "-" + EditorGUIUtility.isProSkin, color);
        }
    }
}
