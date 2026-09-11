using UnityEngine;
using UnityEngine.UIElements;

namespace Deucarian.Editor
{
    /// <summary>Resolution-independent, static illumination behind workspace content.</summary>
    internal sealed class DeucarianEditorWorkspaceBackdrop : VisualElement
    {
        private readonly CustomStyleProperty<Color> surface = new CustomStyleProperty<Color>("--dw-background");
        private Color background = new Color32(27, 40, 44, 255);

        internal DeucarianEditorWorkspaceBackdrop()
        {
            name = "workspace-backdrop";
            pickingMode = PickingMode.Ignore;
            style.position = Position.Absolute;
            style.left = style.right = style.top = style.bottom = 0;
            RegisterCallback<CustomStyleResolvedEvent>(evt =>
            {
                if (evt.customStyle.TryGetValue(surface, out var value)) background = value;
                MarkDirtyRepaint();
            });
            generateVisualContent += Draw;
        }

        private void Draw(MeshGenerationContext context)
        {
            const int divisions = 16;
            const int side = divisions + 1;
            if (contentRect.width <= 0 || contentRect.height <= 0) return;
            var mesh = context.Allocate(side * side, divisions * divisions * 6);
            bool dark = background.grayscale < 0.5f;
            for (int y = 0; y <= divisions; y++)
            for (int x = 0; x <= divisions; x++)
            {
                float u = x / (float)divisions;
                float v = y / (float)divisions;
                float light = Mathf.Exp(-((u - 0.76f) * (u - 0.76f) * 4f + (v - 0.38f) * (v - 0.38f) * 3f));
                Color tint = Color.Lerp(background * (dark ? 0.7f : 0.97f), background, light);
                tint.a = 1;
                mesh.SetNextVertex(new Vertex {
                    position = new Vector3(u * contentRect.width, v * contentRect.height, Vertex.nearZ), tint = tint
                });
            }
            for (int y = 0; y < divisions; y++)
            for (int x = 0; x < divisions; x++)
            {
                ushort a = (ushort)(y * side + x);
                mesh.SetNextIndex(a);
                mesh.SetNextIndex((ushort)(a + 1));
                mesh.SetNextIndex((ushort)(a + side));
                mesh.SetNextIndex((ushort)(a + 1));
                mesh.SetNextIndex((ushort)(a + side + 1));
                mesh.SetNextIndex((ushort)(a + side));
            }
        }
    }
}
