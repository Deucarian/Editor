using NUnit.Framework;
using UnityEngine;

namespace Deucarian.Editor.Tests
{
    public sealed class DeucarianSpatialPreviewGeometryTests
    {
        [TestCase(-6f)]
        [TestCase(0f)]
        [TestCase(6f)]
        public void CameraProjectedGeometryClipsNearPlaneAndNeverProducesInvalidVertices(float cameraZ)
        {
            var owner = new GameObject("Projection test");
            try
            {
                var camera = owner.AddComponent<Camera>(); camera.enabled = false;
                camera.transform.position = new Vector3(0, 0, cameraZ);
                camera.nearClipPlane = .01f;
                var preview = new DeucarianEditorSpatialPreview(); preview.SetCamera(camera);
                var bounds = new Rect(0, 0, 360, 280);
                preview.BuildGeometry(bounds, out var vertices, out var indices);
                foreach (var vertex in vertices)
                {
                    Assert.IsFalse(float.IsNaN(vertex.position.x) || float.IsInfinity(vertex.position.x));
                    Assert.IsFalse(float.IsNaN(vertex.position.y) || float.IsInfinity(vertex.position.y));
                    Assert.IsTrue(bounds.Contains(vertex.position));
                }
                preview.SetCamera(null);
                preview.BuildGeometry(bounds, out vertices, out indices);
                Assert.Greater(vertices.Length, 0);
            }
            finally { Object.DestroyImmediate(owner); }
        }

        [TestCase(0.2f, false)]
        [TestCase(1f, false)]
        [TestCase(3f, false)]
        [TestCase(0.2f, true)]
        [TestCase(1f, true)]
        [TestCase(3f, true)]
        public void StrokesFaceThePanelAndStayInsideTheirViewport(float zoom, bool solid)
        {
            var preview = new DeucarianEditorSpatialPreview(solidCube: solid);
            var bounds = new Rect(3, 7, 360, 280);
            preview.SetView(Quaternion.Euler(22, -32, 0), zoom);
            preview.BuildGeometry(bounds, out var vertices, out var indices);
            Assert.Greater(vertices.Length, 0);
            foreach (var vertex in vertices)
                Assert.That(bounds.Contains(vertex.position), Is.True, vertex.position.ToString());
            int visible = 0;
            for (int i = 0; i < indices.Length; i += 3)
            {
                Vector3 a = vertices[indices[i]].position, b = vertices[indices[i + 1]].position,
                    c = vertices[indices[i + 2]].position;
                float area = Vector3.Cross(b - a, c - a).z;
                Assert.GreaterOrEqual(area, -0.001f, "Clockwise UI winding is required in downward Y coordinates.");
                if (area > .001f) visible++;
            }
            Assert.Greater(visible, 0);
        }
    }
}
