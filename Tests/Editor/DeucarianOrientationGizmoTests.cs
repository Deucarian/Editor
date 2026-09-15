using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;

namespace Deucarian.Editor.Tests
{
    public sealed class DeucarianOrientationGizmoTests
    {
        [Test]
        public void RenderedPreviewKeepsAxesAboveItsImageAndTracksCameraRotationOnly()
        {
            var owner = new GameObject("Orientation fixture");
            try
            {
                var camera = owner.AddComponent<Camera>(); camera.enabled = false;
                var preview = new DeucarianEditorSpatialPreview();
                preview.SetCamera(camera); preview.SetRenderedTexture(null);
                var gizmo = preview.Q("preview-orientation");
                Assert.NotNull(gizmo);
                Assert.Greater(preview.IndexOf(gizmo), preview.IndexOf(preview.Q("scene-render")));
                Assert.That(gizmo.pickingMode, Is.EqualTo(PickingMode.Ignore));
                foreach (string axis in new[] { "x", "y", "z" })
                {
                    var label = gizmo.Q<Label>("preview-axis-" + axis);
                    Assert.That(label.text, Is.EqualTo(axis.ToUpperInvariant()));
                    Assert.That(label.pickingMode, Is.EqualTo(PickingMode.Ignore));
                }
                var x = gizmo.Q<Label>("preview-axis-x");
                float before = x.style.left.value.value;
                camera.transform.position = new Vector3(100, -100, 200);
                preview.SetCamera(camera);
                Assert.That(x.style.left.value.value, Is.EqualTo(before));
                camera.transform.rotation = Quaternion.Euler(0, 90, 0);
                preview.SetCamera(camera);
                Assert.That(x.style.left.value.value, Is.Not.EqualTo(before));
                Assert.That(camera.transform.position, Is.EqualTo(new Vector3(100, -100, 200)));
                preview.SetRenderedTexture(null);
                Assert.That(preview.Query<DeucarianEditorOrientationGizmo>().ToList().Count, Is.EqualTo(1));
            }
            finally { Object.DestroyImmediate(owner); }
        }
    }
}
