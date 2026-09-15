using System;
using System.IO;

namespace Deucarian.Editor
{
    /// <summary>Every asset category uses at most origin / package / asset, regardless of folder depth.</summary>
    public static class DeucarianEditorAssetMenu
    {
        public static string Path(string assetPath, string assetName, long subAssetId = 0)
        {
            string normalized = (assetPath ?? string.Empty).Replace('\\', '/');
            string[] parts = normalized.Split('/');
            bool package = parts.Length > 2 && parts[0] == "Packages";
            string group = package ? "Packages/" + Segment(parts[1]) : "Project";
            int start = package ? 2 : parts.Length > 1 && parts[0] == "Assets" ? 1 : 0;
            string folders = parts.Length > start + 1
                ? string.Join(" › ", parts, start, parts.Length - start - 1) : string.Empty;
            string file = System.IO.Path.GetFileName(normalized);
            string label = string.IsNullOrEmpty(assetName) ? file : assetName;
            if (subAssetId == 0 && !string.Equals(label, file, StringComparison.Ordinal)) label += " · " + file;
            if (subAssetId != 0) label += " · " + file + " [" + subAssetId + "]";
            if (!string.IsNullOrEmpty(folders)) label += " (" + folders + ")";
            return group + "/" + Segment(label);
        }

        private static string Segment(string value) => value.Replace('/', '∕').Replace('\\', '∕');
    }
}
