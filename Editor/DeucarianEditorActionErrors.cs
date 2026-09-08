using System;
using UnityEditor;

namespace Deucarian.Editor
{
    public static class DeucarianEditorActionErrors
    {
        public static void Show(string title, Exception exception, bool actionCompleted = false)
        {
            string explanation = actionCompleted
                ? "The action completed, but its status could not refresh. Refresh the status again after checking the issue below."
                : "The action could not complete. Check the issue below and the tool's current state before retrying.";
            string detail = exception == null ? "No further error details were supplied." : exception.Message;
            bool close = EditorUtility.DisplayDialog(title, explanation + "\n\n" + detail, "Close", "Copy technical details");
            if (!close) EditorGUIUtility.systemCopyBuffer = exception?.ToString() ?? detail;
        }
    }
}
