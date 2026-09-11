using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Deucarian.Editor
{
    /// <summary>Editor-only geometry preview; no camera, scene objects or render textures.</summary>
    public sealed class DeucarianEditorSpatialPreview : VisualElement
    {
        private Quaternion rotation = Quaternion.Euler(22, -32, 0);
        private float zoom = 1;
        private readonly bool showCube;

        public DeucarianEditorSpatialPreview(bool showCube = true)
        {
            this.showCube = showCube;
            name = "spatial-preview";
            AddToClassList("dw-spatial-preview");
            generateVisualContent += Draw;
            tooltip = "Illustrative navigation preview. Your scene camera is unchanged.";
        }

        public void SetView(Quaternion orientation, float magnification = 1)
        {
            rotation = orientation;
            zoom = Mathf.Clamp(magnification, 0.2f, 3);
            MarkDirtyRepaint();
        }

        private void Draw(MeshGenerationContext context)
        {
            if (contentRect.width < 1 || contentRect.height < 1) return;
            var lines = new List<Vector3>();
            for (int i = -4; i <= 4; i++)
            {
                lines.Add(new Vector3(i, -1, -4)); lines.Add(new Vector3(i, -1, 4));
                lines.Add(new Vector3(-4, -1, i)); lines.Add(new Vector3(4, -1, i));
            }
            int gridVertices = lines.Count;
            for (int axis = 0; showCube && axis < 3; axis++)
            for (int a = -1; a <= 1; a += 2)
            for (int b = -1; b <= 1; b += 2)
            {
                var start = axis == 0 ? new Vector3(-1, a, b) : axis == 1 ? new Vector3(a, -1, b) : new Vector3(a, b, -1);
                var end = start; end[axis] = 1;
                lines.Add(start); lines.Add(end);
            }
            var mesh = context.Allocate(lines.Count * 2, lines.Count * 3);
            for (int i = 0; i < lines.Count; i += 2)
            {
                Vector2 start = Project(lines[i]); Vector2 end = Project(lines[i + 1]);
                var direction = end - start;
                var offset = new Vector2(-direction.y, direction.x).normalized * (i < gridVertices ? 0.4f : 0.8f);
                Color32 color = i < gridVertices ? new Color32(56, 78, 84, 130) : new Color32(213, 230, 235, 255);
                AddVertex(mesh, start - offset, color); AddVertex(mesh, start + offset, color);
                AddVertex(mesh, end - offset, color); AddVertex(mesh, end + offset, color);
                ushort first = (ushort)(i * 2);
                mesh.SetNextIndex(first); mesh.SetNextIndex((ushort)(first + 1)); mesh.SetNextIndex((ushort)(first + 2));
                mesh.SetNextIndex((ushort)(first + 2)); mesh.SetNextIndex((ushort)(first + 1)); mesh.SetNextIndex((ushort)(first + 3));
            }
        }

        private Vector2 Project(Vector3 point)
        {
            Vector3 transformed = rotation * point;
            float size = Mathf.Min(contentRect.width, contentRect.height) * 0.21f * zoom;
            return contentRect.center + new Vector2(transformed.x, -transformed.y) * size;
        }

        private static void AddVertex(MeshWriteData mesh, Vector2 point, Color32 color) =>
            mesh.SetNextVertex(new Vertex { position = new Vector3(point.x, point.y, Vertex.nearZ), tint = color });
    }
}
