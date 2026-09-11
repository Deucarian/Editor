using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Deucarian.Editor
{
    /// <summary>Shared specimen geometry, optionally projected through a caller-owned isolated camera.</summary>
    public sealed class DeucarianEditorSpatialPreview : VisualElement
    {
        private Quaternion rotation = Quaternion.AngleAxis(22, Vector3.right) * Quaternion.AngleAxis(-32, Vector3.up);
        private float zoom = 1;
        private readonly bool showCube;
        private readonly bool solidCube;
        private readonly Label[] axes;
        private Camera projectionCamera;
        private Image renderedImage;

        /// <summary>Displays a caller-owned render texture in place of the illustrative geometry.</summary>
        public void SetRenderedTexture(Texture texture)
        {
            if (renderedImage == null)
            {
                renderedImage = new Image { name = "scene-render", pickingMode = PickingMode.Ignore, scaleMode = ScaleMode.StretchToFill };
                renderedImage.StretchToParentSize();
                Add(renderedImage);
                if (axes != null) foreach (var axis in axes) axis.style.display = DisplayStyle.None;
            }
            renderedImage.image = texture;
            MarkDirtyRepaint();
        }

        /// <summary>The caller retains camera ownership. Passing null restores the illustrative view.</summary>
        public void SetCamera(Camera camera)
        {
            projectionCamera = camera;
            PositionAxes();
            MarkDirtyRepaint();
        }

        public DeucarianEditorSpatialPreview(bool showCube = true, bool solidCube = false)
        {
            this.showCube = showCube;
            this.solidCube = showCube && solidCube;
            name = "spatial-preview";
            AddToClassList("dw-spatial-preview");
            if (this.solidCube) AddToClassList("dw-spatial-solid");
            generateVisualContent += Draw;
            tooltip = "Illustrative navigation preview. Your scene camera is unchanged.";
            if (showCube)
            {
                axes = new Label[3];
                for (int i = 0; i < 3; i++)
                {
                    axes[i] = DeucarianEditorWorkspaceControls.Label(i == 0 ? "X" : i == 1 ? "Y" : "Z", "dw-spatial-axis");
                    axes[i].pickingMode = PickingMode.Ignore;
                    Add(axes[i]);
                }
                RegisterCallback<GeometryChangedEvent>(_ => PositionAxes());
            }
        }

        public void SetView(Quaternion orientation, float magnification = 1)
        {
            rotation = orientation;
            zoom = Mathf.Clamp(magnification, 0.2f, 3);
            PositionAxes();
            MarkDirtyRepaint();
        }

        private void PositionAxes()
        {
            if (renderedImage != null || axes == null || contentRect.width < 1) return;
            for (int i = 0; i < 3; i++)
            {
                var end = Vector3.zero;
                end[i] = solidCube ? (rotation * Axis(i)).z >= 0 ? 1 : -1 : 2.2f;
                bool visible = InFront(end);
                axes[i].style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
                if (!visible) continue;
                var point = Project(end, contentRect);
                axes[i].style.left = Mathf.Clamp(point.x - 14, contentRect.xMin, Mathf.Max(contentRect.xMin, contentRect.xMax - 28));
                axes[i].style.top = Mathf.Clamp(point.y - 20, contentRect.yMin, Mathf.Max(contentRect.yMin, contentRect.yMax - 30));
            }
        }

        private void Draw(MeshGenerationContext context)
        {
            if (renderedImage != null || contentRect.width < 1 || contentRect.height < 1) return;
            BuildGeometry(contentRect, out var vertices, out var indices);
            var mesh = context.Allocate(vertices.Length, indices.Length);
            mesh.SetAllVertices(vertices);
            mesh.SetAllIndices(indices);
        }

        internal void BuildGeometry(Rect bounds, out Vertex[] vertices, out ushort[] indices)
        {
            if (solidCube && projectionCamera == null) { BuildSolidGeometry(bounds, out vertices, out indices); return; }
            var lines = new List<Vector3>();
            for (int i = -4; i <= 4; i++)
            {
                lines.Add(new Vector3(i, -1, -4)); lines.Add(new Vector3(i, -1, 4));
                lines.Add(new Vector3(-4, -1, i)); lines.Add(new Vector3(4, -1, i));
            }
            int gridVertices = lines.Count;
            for (int axis = 0; showCube && axis < 3; axis++)
            {
                var end = Vector3.zero; end[axis] = 2;
                lines.Add(Vector3.zero); lines.Add(end);
            }
            for (int axis = 0; showCube && axis < 3; axis++)
            for (int a = -1; a <= 1; a += 2)
            for (int b = -1; b <= 1; b += 2)
            {
                var start = axis == 0 ? new Vector3(-1, a, b) : axis == 1 ? new Vector3(a, -1, b) : new Vector3(a, b, -1);
                var end = start; end[axis] = 1;
                lines.Add(start); lines.Add(end);
            }
            vertices = new Vertex[lines.Count * 2];
            indices = new ushort[lines.Count * 3];
            var clip = new Rect(bounds.x + 1, bounds.y + 1, Mathf.Max(0, bounds.width - 2), Mathf.Max(0, bounds.height - 2));
            for (int i = 0; i < lines.Count; i += 2)
            {
                Vector3 worldStart = lines[i], worldEnd = lines[i + 1];
                bool visible = ClipNearPlane(ref worldStart, ref worldEnd);
                Vector2 start = visible ? Project(worldStart, bounds) : clip.center;
                Vector2 end = visible ? Project(worldEnd, bounds) : clip.center;
                if (!Clip(clip, ref start, ref end)) start = end = clip.center;
                var direction = end - start;
                var offset = new Vector2(-direction.y, direction.x).normalized * (i < gridVertices ? 0.4f : 0.8f);
                Color32 color = i < gridVertices ? new Color32(56, 78, 84, 130) : new Color32(213, 230, 235, 255);
                ushort first = (ushort)(i * 2);
                vertices[first] = VertexAt(start - offset, color); vertices[first + 1] = VertexAt(start + offset, color);
                vertices[first + 2] = VertexAt(end - offset, color); vertices[first + 3] = VertexAt(end + offset, color);
                int index = i * 3;
                indices[index] = first; indices[index + 1] = (ushort)(first + 2); indices[index + 2] = (ushort)(first + 1);
                indices[index + 3] = (ushort)(first + 2); indices[index + 4] = (ushort)(first + 3); indices[index + 5] = (ushort)(first + 1);
            }
        }

        private void BuildSolidGeometry(Rect bounds, out Vertex[] vertices, out ushort[] indices)
        {
            vertices = new Vertex[12]; indices = new ushort[18];
            for (int axis = 0; axis < 3; axis++)
            {
                Vector3 normal = Axis(axis) * ((rotation * Axis(axis)).z >= 0 ? 1 : -1);
                Vector3 across = Axis((axis + 1) % 3), up = Axis((axis + 2) % 3);
                var color = axis == 1 ? new Color32(202, 217, 226, 255) : axis == 0 ?
                    new Color32(161, 181, 194, 255) : new Color32(132, 154, 169, 255);
                int offset = axis * 4;
                for (int corner = 0; corner < 4; corner++)
                {
                    Vector3 point = normal + across * (corner < 2 ? -1 : 1) + up * (corner % 2 == 0 ? -1 : 1);
                    Vector2 projected = Project(point, bounds);
                    projected.x = Mathf.Clamp(projected.x, bounds.xMin + 1, bounds.xMax - 1);
                    projected.y = Mathf.Clamp(projected.y, bounds.yMin + 1, bounds.yMax - 1);
                    vertices[offset + corner] = VertexAt(projected, color);
                }
                bool clockwise = Vector3.Cross(vertices[offset + 2].position - vertices[offset].position,
                    vertices[offset + 1].position - vertices[offset].position).z >= 0;
                int start = axis * 6;
                indices[start] = (ushort)offset;
                indices[start + 1] = (ushort)(offset + (clockwise ? 2 : 1));
                indices[start + 2] = (ushort)(offset + (clockwise ? 1 : 2));
                indices[start + 3] = (ushort)(offset + 2);
                indices[start + 4] = (ushort)(offset + (clockwise ? 3 : 1));
                indices[start + 5] = (ushort)(offset + (clockwise ? 1 : 3));
            }
        }

        private static Vector3 Axis(int index) { var value = Vector3.zero; value[index] = 1; return value; }

        private Vector2 Project(Vector3 point, Rect bounds)
        {
            if (projectionCamera != null)
            {
                Vector3 viewport = projectionCamera.WorldToViewportPoint(point);
                return new Vector2(bounds.xMin + viewport.x * bounds.width,
                    bounds.yMax - viewport.y * bounds.height);
            }
            Vector3 transformed = rotation * point;
            float size = Mathf.Min(bounds.width, bounds.height) * 0.21f * zoom;
            return bounds.center + new Vector2(transformed.x, -transformed.y) * size;
        }

        private bool InFront(Vector3 point) => projectionCamera == null ||
            Vector3.Dot(point - projectionCamera.transform.position, projectionCamera.transform.forward)
                >= projectionCamera.nearClipPlane;

        private bool ClipNearPlane(ref Vector3 start, ref Vector3 end)
        {
            if (projectionCamera == null) return true;
            Vector3 position = projectionCamera.transform.position;
            Vector3 forward = projectionCamera.transform.forward;
            float near = Mathf.Max(0.0001f, projectionCamera.nearClipPlane);
            float startDepth = Vector3.Dot(start - position, forward);
            float endDepth = Vector3.Dot(end - position, forward);
            if (startDepth < near && endDepth < near) return false;
            if (startDepth >= near && endDepth >= near) return true;
            Vector3 intersection = Vector3.LerpUnclamped(start, end, (near - startDepth) / (endDepth - startDepth));
            if (startDepth < near) start = intersection; else end = intersection;
            return true;
        }

        private static bool Clip(Rect bounds, ref Vector2 start, ref Vector2 end)
        {
            var direction = end - start;
            float enter = 0, leave = 1;
            if (!Limit(-direction.x, start.x - bounds.xMin, ref enter, ref leave) ||
                !Limit(direction.x, bounds.xMax - start.x, ref enter, ref leave) ||
                !Limit(-direction.y, start.y - bounds.yMin, ref enter, ref leave) ||
                !Limit(direction.y, bounds.yMax - start.y, ref enter, ref leave)) return false;
            end = start + direction * leave;
            start += direction * enter;
            return true;
        }

        private static bool Limit(float direction, float distance, ref float enter, ref float leave)
        {
            if (Mathf.Abs(direction) < .0001f) return distance >= 0;
            float at = distance / direction;
            if (direction < 0) { if (at > leave) return false; enter = Mathf.Max(enter, at); }
            else { if (at < enter) return false; leave = Mathf.Min(leave, at); }
            return true;
        }

        private static Vertex VertexAt(Vector2 point, Color32 color) =>
            new Vertex { position = new Vector3(point.x, point.y, Vertex.nearZ), tint = color };
    }
}
