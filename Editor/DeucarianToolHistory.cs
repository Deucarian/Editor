using System;
using System.Collections.Generic;
using UnityEngine;

namespace Deucarian.Editor
{
    public static class DeucarianToolHistory
    {
        [Serializable]
        private sealed class State
        {
            public List<string> favorites = new List<string>();
            public List<string> recent = new List<string>();
        }
        private static State state;
        private const string Preference = "tools.history";

        public static bool IsFavorite(string id) => Current.favorites.Contains(id);
        public static int RecentIndex(string id) => Current.recent.IndexOf(id);

        public static void SetFavorite(string id, bool favorite)
        {
            if (string.IsNullOrWhiteSpace(id)) return;
            Current.favorites.Remove(id);
            if (favorite) Current.favorites.Add(id);
            Save();
        }

        public static void RecordOpened(string id)
        {
            if (string.IsNullOrWhiteSpace(id) || id == DeucarianToolIds.ControlCenter) return;
            Current.recent.Remove(id);
            Current.recent.Insert(0, id);
            if (Current.recent.Count > 12) Current.recent.RemoveRange(12, Current.recent.Count - 12);
            Save();
        }

        private static State Current
        {
            get
            {
                if (state != null) return state;
                try { state = JsonUtility.FromJson<State>(DeucarianEditorProjectPreferences.GetString(Preference, "{}")); }
                catch (ArgumentException) { }
                if (state == null) state = new State();
                if (state.favorites == null) state.favorites = new List<string>();
                if (state.recent == null) state.recent = new List<string>();
                return state;
            }
        }

        private static void Save() => DeucarianEditorProjectPreferences.SetString(Preference, JsonUtility.ToJson(Current));
    }
}
