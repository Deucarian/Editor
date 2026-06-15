using System.Collections.Generic;
using UnityEngine;

namespace Deucarian.Editor
{
    internal static class DeucarianEditorTextureCache
    {
        private static readonly Dictionary<string, Texture2D> Textures = new Dictionary<string, Texture2D>();

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
