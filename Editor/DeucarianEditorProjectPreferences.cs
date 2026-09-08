using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Deucarian.Editor
{
    /// <summary>Project-local editor preferences. Never stores scene connections or live runtime objects.</summary>
    public static class DeucarianEditorProjectPreferences
    {
        private static readonly string ProjectPrefix = PrefixFor(Application.dataPath);

        public static string Key(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("A preference name is required.", nameof(name));
            return ProjectPrefix + name;
        }

        public static string GetString(string name, string fallback = "") => EditorPrefs.GetString(Key(name), fallback);
        public static void SetString(string name, string value) => EditorPrefs.SetString(Key(name), value ?? "");
        public static int GetInt(string name, int fallback = 0) => EditorPrefs.GetInt(Key(name), fallback);
        public static void SetInt(string name, int value) => EditorPrefs.SetInt(Key(name), value);
        public static bool GetBool(string name, bool fallback = false) => EditorPrefs.GetBool(Key(name), fallback);
        public static void SetBool(string name, bool value) => EditorPrefs.SetBool(Key(name), value);
        public static void Delete(string name) => EditorPrefs.DeleteKey(Key(name));
        public static void DeleteKey(string name) => Delete(name);

        internal static string PrefixFor(string projectPath)
        {
            string normalized = Path.GetFullPath(projectPath).Replace('\\', '/').TrimEnd('/');
            if (Application.platform == RuntimePlatform.WindowsEditor) normalized = normalized.ToLowerInvariant();
            using (var hash = SHA256.Create())
            {
                string id = BitConverter.ToString(hash.ComputeHash(Encoding.UTF8.GetBytes(normalized))).Replace("-", "");
                return "Deucarian.Project." + id.Substring(0, 24) + ".";
            }
        }
    }
}
