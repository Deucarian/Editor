namespace Deucarian.Editor
{
    public static class DeucarianEditorUxStandards
    {
        public const string MenuRoot = DeucarianEditorPackageConstants.PackageToolMenuRoot;
        public const string AssetFieldSelectButtonLabel = "Select";

        public static string GetPackageMenuPath(string packageName, string command = null)
        {
            string safePackageName = string.IsNullOrWhiteSpace(packageName) ? DeucarianEditorPackageConstants.DisplayName : packageName.Trim();
            if (string.IsNullOrWhiteSpace(command))
            {
                return MenuRoot + "/" + safePackageName;
            }

            return MenuRoot + "/" + safePackageName + "/" + command.Trim();
        }
    }
}
