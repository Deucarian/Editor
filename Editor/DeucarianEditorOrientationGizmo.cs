using UnityEngine;
using UnityEngine.UIElements;

namespace Deucarian.Editor
{
    /// <summary>Non-interactive world-axis reference for a caller-owned rendered preview.</summary>
    internal sealed class DeucarianEditorOrientationGizmo : VisualElement
    {
        private readonly Label[] labels = new Label[3];
        private readonly Vector2[] directions = new Vector2[3];
        private Quaternion cameraRotation = Quaternion.identity;

        internal DeucarianEditorOrientationGizmo()
        {
            name = "preview-orientation";
            pickingMode = PickingMode.Ignore;
            AddToClassList("dw-orientation-gizmo");
            for (int axis = 0; axis < 3; axis++)
            {
                string text = axis == 0 ? "X" : axis == 1 ? "Y" : "Z";
                labels[axis] = DeucarianEditorWorkspaceControls.Label(text, "dw-spatial-axis");
                labels[axis].name = "preview-axis-" + text.ToLowerInvariant();
                labels[axis].pickingMode = PickingMode.Ignore;
                Add(labels[axis]);
            }
            generateVisualContent += Draw;
            RegisterCallback<GeometryChangedEvent>(_ => PositionAxes());
            PositionAxes();
        }

        internal void SetCamera(Camera camera)
        {
            cameraRotation = camera != null ? camera.transform.rotation : Quaternion.identity;
            PositionAxes();
            MarkDirtyRepaint();
        }

        private void PositionAxes()
        {
            Quaternion inverse = Quaternion.Inverse(cameraRotation);
            for (int axis = 0; axis < 3; axis++)
            {
                Vector3 world = Vector3.zero; world[axis] = 1;
                Vector3 local = inverse * world;
                directions[axis] = new Vector2(local.x, -local.y);
                Vector2 labelPosition = new Vector2(62, 62) + directions[axis] * 46;
                labels[axis].style.left = labelPosition.x - 14;
                labels[axis].style.top = labelPosition.y - 15;
                labels[axis].style.color = AxisColor(axis);
            }
        }

        private void Draw(MeshGenerationContext context)
        {
            var mesh = context.Allocate(12, 18);
            var center = new Vector2(62, 62);
            for (int axis = 0; axis < 3; axis++)
            {
                Vector2 end = center + directions[axis] * 34;
                Vector2 offset = new Vector2(-directions[axis].y, directions[axis].x).normalized * 1.5f;
                Color32 color = AxisColor(axis);
                mesh.SetNextVertex(At(center - offset, color));
                mesh.SetNextVertex(At(center + offset, color));
                mesh.SetNextVertex(At(end - offset, color));
                mesh.SetNextVertex(At(end + offset, color));
                ushort first = (ushort)(axis * 4);
                mesh.SetNextIndex(first); mesh.SetNextIndex((ushort)(first + 2)); mesh.SetNextIndex((ushort)(first + 1));
                mesh.SetNextIndex((ushort)(first + 2)); mesh.SetNextIndex((ushort)(first + 3)); mesh.SetNextIndex((ushort)(first + 1));
            }
        }

        private static Color AxisColor(int axis) => axis == 0 ? DeucarianEditorSurfacePalette.Error :
            axis == 1 ? DeucarianEditorSurfacePalette.Success : DeucarianEditorSurfacePalette.Info;

        private static Vertex At(Vector2 position, Color32 color) =>
            new Vertex { position = new Vector3(position.x, position.y, Vertex.nearZ), tint = color };
    }
}
