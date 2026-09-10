using System.Collections.Generic;
using UnityEngine;

namespace Deucarian.Editor
{
    internal static class DeucarianEditorTextureCache
    {
        private static readonly Dictionary<string, Texture2D> Textures = new Dictionary<string, Texture2D>();

        public static Texture2D GetBordered(string key, Color fill, Color border)
        {
            if (Textures.TryGetValue(key, out var existing) && existing != null) return existing;
            var texture = new Texture2D(9, 9, TextureFormat.RGBA32, false)
            {
                hideFlags = HideFlags.HideAndDontSave, name = "Deucarian Editor " + key,
                filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp
            };
            for (int y = 0; y < 9; y++)
            for (int x = 0; x < 9; x++)
            {
                float dx = Mathf.Max(0, Mathf.Abs(x - 4) - 1);
                float dy = Mathf.Max(0, Mathf.Abs(y - 4) - 1);
                float distance = Mathf.Sqrt(dx * dx + dy * dy);
                texture.SetPixel(x, y, distance > 4 ? Color.clear : distance > 3 ? border : fill);
            }
            texture.Apply();
            Textures[key] = texture;
            return texture;
        }

        public static Texture2D Get(string key, Color color)
        {
            if (string.IsNullOrEmpty(key))
            {
                key = "default";
            }

            Texture2D texture;
            if (Textures.TryGetValue(key, out texture) && texture != null)
            {
                return texture;
            }

            texture = new Texture2D(1, 1, TextureFormat.RGBA32, false)
            {
                hideFlags = HideFlags.HideAndDontSave,
                name = "Deucarian Editor " + key
            };
            texture.SetPixel(0, 0, color);
            texture.Apply();
            Textures[key] = texture;
            return texture;
        }
    }
}
