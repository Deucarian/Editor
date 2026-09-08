using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Deucarian.Editor
{
    public sealed class DeucarianEditorWorkbenchPanelScope : IDisposable
    {
        private readonly float trailingSpace;
        private bool disposed;

        internal DeucarianEditorWorkbenchPanelScope(float trailingSpace)
        {
            this.trailingSpace = trailingSpace;
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            disposed = true;
            DeucarianEditorWorkbenchGUI.EndPanel(trailingSpace);
        }
    }
}
