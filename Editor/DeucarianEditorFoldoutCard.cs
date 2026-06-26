using System;

namespace Deucarian.Editor
{
    public static class DeucarianEditorFoldoutCard
    {
        public static bool Draw(
            string stateKey,
            string title,
            string summary,
            Action drawContent,
            bool defaultOpen = true,
            bool enabled = true,
            Action drawHeaderActions = null)
        {
            return DeucarianEditorAccordion.DrawFoldoutCard(stateKey, title, summary, drawContent, defaultOpen, enabled, drawHeaderActions);
        }
    }
}
