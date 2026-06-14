using Godot;
using valleyfold.ThreeDeeModels;
using valleyfold.TwoDeeModels;

namespace Testing.ThreeDeeModels;

public class Frame3DTests
{
    public class ImportMetadataFromFrame : Frame3DTests
    {
        [Fact]
        public void DoesNotThrow_WhenFrameHasMoreVerticesThanFrame3D()
        {
            // Reproduces the OOB seen after a fold completes:
            //
            //   System.ArgumentOutOfRangeException at
            //   Frame3D.ImportMetadataFromFrame (Frame3D.cs:113)
            //   from StartPointSelectionState.OnProcess
            //
            // After a fold, EdgeCommands.AddVertexToEdge grows
            // frame.Vertices in the input handler. Frame3D.Vertices is
            // only rebuilt by PaperRenderer.RebuildFrame3D each frame.
            // If the selector's _Process runs before the renderer's in
            // the same frame, frame.Vertices.Count > frame3d.Vertices.Count
            // and the metadata copy walks off the end of frame3d.Vertices.
            //
            // ImportMetadataFromFrame must tolerate this transient
            // mismatch — the next renderer tick rebuilds frame3d and any
            // new vertices arrive with IsSelected already copied from
            // their 2D originals via Vertex.To3D, so no metadata is
            // lost.
            var frame = new Frame();
            frame.Vertices.Add(new Vertex { Coord = new Vector2(0, 0), IsSelected = true });
            frame.Vertices.Add(new Vertex { Coord = new Vector2(1, 0), IsSelected = false });
            frame.Vertices.Add(new Vertex { Coord = new Vector2(1, 1), IsSelected = false });

            var frame3d = new Frame3D();
            frame3d.Vertices.Add(new Vertex3D { Coord = new Vector3(0, 0, 0), IsSelected = false });
            frame3d.Vertices.Add(new Vertex3D { Coord = new Vector3(1, 0, 0), IsSelected = false });

            var ex = Record.Exception(() => frame3d.ImportMetadataFromFrame(frame));

            Assert.Null(ex);
            // Existing entries still synced.
            Assert.True(frame3d.Vertices[0].IsSelected);
            Assert.False(frame3d.Vertices[1].IsSelected);
        }
    }
}
