using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Deucarian.Editor
{
    public static class DeucarianEditorUIResources
    {
        public const string AssetRoot = "Packages/com.deucarian.editor/Editor/Assets";
        public const string IconsPath = AssetRoot + "/Icons";
        public const string LogosPath = AssetRoot + "/Logos";
        public const string FontsPath = AssetRoot + "/Fonts";
        public const string StylesPath = AssetRoot + "/Styles";
        public const string ImagesPath = AssetRoot + "/Images";

        public const string SharedStyleSheetPath = StylesPath + "/DeucarianEditor.uss";
        public const string PlaceholderLogoPath = LogosPath + "/DeucarianPlaceholderLogo.png";
        public const string InstallerBackgroundPath = ImagesPath + "/DeucarianInstallerBackground.png";
        public const string BrandMarkDarkPath = LogosPath + "/DeucarianMarkDark.png";
        public const string BrandMarkLightPath = LogosPath + "/DeucarianMarkLight.png";
        public const string BrandLogoDarkPath = LogosPath + "/DeucarianLogoDark.png";
        public const string BrandLogoLightPath = LogosPath + "/DeucarianLogoLight.png";
        public const string BackgroundDarkPath = ImagesPath + "/DeucarianBackgroundDark.png";
        public const string BackgroundLightPath = ImagesPath + "/DeucarianBackgroundLight.png";
        public const string DisplayFontPath = FontsPath + "/DINish-Light.otf";
        public const string BodyFontPath = FontsPath + "/DINish-Regular.otf";
        public const string StrongFontPath = FontsPath + "/DINish-SemiBold.otf";
        public const string PackageInstallerPlaceholderHeroPath = ImagesPath + "/DeucarianPackageInstallerPlaceholderHero.png";
        public const string PackagePlaceholderIconPath = IconsPath + "/DeucarianPackagePlaceholderIcon.png";

        public static StyleSheet LoadSharedStyleSheet()
        {
            return LoadStyleSheet(SharedStyleSheetPath);
        }

        public static Texture2D LoadPlaceholderLogo()
        {
            return LoadTexture(PlaceholderLogoPath);
        }

        public static Texture2D LoadInstallerBackground()
        {
            return LoadTexture(InstallerBackgroundPath);
        }

        public static Texture2D LoadBrandMark()
        {
            return LoadTexture(EditorGUIUtility.isProSkin ? BrandMarkDarkPath : BrandMarkLightPath);
        }

        public static Texture2D LoadBrandLogo()
        {
            return LoadTexture(EditorGUIUtility.isProSkin ? BrandLogoDarkPath : BrandLogoLightPath);
        }

        public static Texture2D LoadBrandBackground()
        {
            return LoadTexture(EditorGUIUtility.isProSkin ? BackgroundDarkPath : BackgroundLightPath);
        }

        public static Font LoadDisplayFont()
        {
            return LoadAsset<Font>(DisplayFontPath);
        }

        public static Font LoadBodyFont()
        {
            return LoadAsset<Font>(BodyFontPath);
        }

        public static Font LoadStrongFont()
        {
            return LoadAsset<Font>(StrongFontPath);
        }

        public static Texture2D LoadPackageInstallerPlaceholderHero()
        {
            return LoadTexture(PackageInstallerPlaceholderHeroPath);
        }

        public static Texture2D LoadPackagePlaceholderIcon()
        {
            return LoadTexture(PackagePlaceholderIconPath);
        }

        public static StyleSheet LoadStyleSheet(string assetPath)
        {
            return LoadAsset<StyleSheet>(assetPath);
        }

        public static Texture2D LoadTexture(string assetPath)
        {
            return LoadAsset<Texture2D>(assetPath);
        }

        public static T LoadAsset<T>(string assetPath)
            where T : UnityEngine.Object
        {
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                return null;
            }

            return AssetDatabase.LoadAssetAtPath<T>(assetPath);
        }

        public static bool TryAddSharedStyleSheet(VisualElement root)
        {
            return TryAddStyleSheet(root, SharedStyleSheetPath);
        }

        public static bool TryAddStyleSheet(VisualElement root, string assetPath)
        {
            if (root == null)
            {
                return false;
            }

            StyleSheet styleSheet = LoadStyleSheet(assetPath);
            if (styleSheet == null)
            {
                return false;
            }

            root.styleSheets.Add(styleSheet);
            return true;
        }
    }
}
