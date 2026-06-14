using System.Collections.Generic;
using Godot;
using valleyfold.ThreeDeeModels;
using valleyfold.TwoDeeModels;

namespace Testing.ThreeDeeModels;

public class LayerUpdaterTests
{
    // ADR-0003 (supersedes ADR-0002 §"Layer-update rule on fold"):
    //   newLayer(f) = maxStationary + 1 + (maxMoved - layers[f])
    //
    // Inverts the relative ordering within the moving stack — what a 180°
    // valley fold physically does. No overlap → no-op. Empty moved set →
    // no-op.

    private static Vertex3D V(float x, float z) =>
        new Vertex3D { Coord = new Vector3(x, 0f, z) };

    /// <summary>Constructs a frame with the given face polygons and a
    /// Frame3D whose vertex list mirrors them. Face Ids are assigned via
    /// <see cref="Frame.AddFace"/>; layers are reset to 0 for every face.</summary>
    private static (Frame frame, Frame3D frame3d, List<Face> faces) BuildScene(
        List<Vertex3D> vertices,
        List<List<Id>> facePolygons)
    {
        var frame = new Frame();
        for (var i = 0; i < vertices.Count; i++)
            frame.AddVertex(new Vector2(vertices[i].Coord.X, vertices[i].Coord.Z));

        var faces = new List<Face>();
        for (var f = 0; f < facePolygons.Count; f++)
        {
            var face = new Face { Vertices = facePolygons[f] };
            frame.AddFace(face);
            faces.Add(face);
        }

        var frame3d = new Frame3D();
        for (var i = 0; i < vertices.Count; i++)
            frame3d.Vertices.Add(vertices[i]);
        frame3d.ResetLayers(frame);

        return (frame, frame3d, faces);
    }

    [Fact]
    public void EmptyMovingSet_IsNoOp()
    {
        var (frame, frame3d, faces) = BuildScene(
            new List<Vertex3D>
            {
                V(0, 0), V(1, 0), V(1, 1), V(0, 1)
            },
            new List<List<Id>>
            {
                new List<Id> { 0, 1, 2, 3 }
            });

        LayerUpdater.ApplyLayerUpdate(frame, frame3d, new List<Face>());

        Assert.Equal(0, frame3d.LayerOf(faces[0]));
    }

    [Fact]
    public void NoStationaryOverlap_LeavesLayersUnchanged()
    {
        // Two squares side by side, no positive-area overlap. Moving the
        // right square must NOT bump it above the left square.
        var (frame, frame3d, faces) = BuildScene(
            new List<Vertex3D>
            {
                V(0, 0), V(1, 0), V(1, 1), V(0, 1),
                V(2, 0), V(3, 0), V(3, 1), V(2, 1)
            },
            new List<List<Id>>
            {
                new List<Id> { 0, 1, 2, 3 },
                new List<Id> { 4, 5, 6, 7 }
            });

        LayerUpdater.ApplyLayerUpdate(frame, frame3d, new[] { faces[1] });

        Assert.Equal(0, frame3d.LayerOf(faces[0]));
        Assert.Equal(0, frame3d.LayerOf(faces[1]));
    }

    [Fact]
    public void SingleOverlap_BumpsMovingByOne()
    {
        // Stationary square (0,0)..(1,1) at layer 0; moving square fully
        // contained inside it, also starting at layer 0. After the update
        // the moving face must sit one layer above the stationary face.
        var (frame, frame3d, faces) = BuildScene(
            new List<Vertex3D>
            {
                V(0, 0), V(1, 0), V(1, 1), V(0, 1),
                V(0.25f, 0.25f), V(0.75f, 0.25f), V(0.75f, 0.75f), V(0.25f, 0.75f)
            },
            new List<List<Id>>
            {
                new List<Id> { 0, 1, 2, 3 },
                new List<Id> { 4, 5, 6, 7 }
            });

        LayerUpdater.ApplyLayerUpdate(frame, frame3d, new[] { faces[1] });

        Assert.Equal(0, frame3d.LayerOf(faces[0]));
        Assert.Equal(1, frame3d.LayerOf(faces[1]));
    }

    [Fact]
    public void StackedStationary_MovesAboveHighestOverlapping()
    {
        // Three faces — A and B fully overlap and B is already at layer 1
        // (from a prior simulated fold). Moving face C overlaps both. Bump
        // should clear the highest stationary layer (1) → C lands at layer 2.
        var (frame, frame3d, faces) = BuildScene(
            new List<Vertex3D>
            {
                V(0, 0), V(1, 0), V(1, 1), V(0, 1),
                V(0, 0), V(1, 0), V(1, 1), V(0, 1),
                V(0.25f, 0.25f), V(0.75f, 0.25f), V(0.75f, 0.75f), V(0.25f, 0.75f)
            },
            new List<List<Id>>
            {
                new List<Id> { 0, 1, 2, 3 },
                new List<Id> { 4, 5, 6, 7 },
                new List<Id> { 8, 9, 10, 11 }
            });
        frame3d.BumpLayers(new[] { faces[1] }, 1); // seed B at layer 1

        LayerUpdater.ApplyLayerUpdate(frame, frame3d, new[] { faces[2] });

        Assert.Equal(0, frame3d.LayerOf(faces[0]));
        Assert.Equal(1, frame3d.LayerOf(faces[1]));
        Assert.Equal(2, frame3d.LayerOf(faces[2]));
    }

    [Fact]
    public void MovingSetInvertsRelativeOrdering()
    {
        // Per ADR-0003 (supersedes ADR-0002's preserve-order rule): a
        // valley fold's 180° rotation flips up/down within the moving
        // stack. Two moving faces M1 (layer 0) and M2 (layer 1, on top
        // of M1) overlap one stationary S at layer 0. Per the rule
        // newLayer(f) = maxStationary + 1 + (maxMoved - layers[f]),
        // with maxStationary=0 and maxMoved=1:
        //   M1: 0 + 1 + (1 - 0) = 2  (was on bottom of M, now on top)
        //   M2: 0 + 1 + (1 - 1) = 1  (was on top of M, now just above S)
        // This is the fix for the poke_through_bug screenshots.
        var (frame, frame3d, faces) = BuildScene(
            new List<Vertex3D>
            {
                V(0, 0), V(1, 0), V(1, 1), V(0, 1),       // stationary
                V(0, 0), V(1, 0), V(1, 1), V(0, 1),       // M1
                V(0.25f, 0.25f), V(0.75f, 0.25f),         // M2 (small, inside)
                V(0.75f, 0.75f), V(0.25f, 0.75f)
            },
            new List<List<Id>>
            {
                new List<Id> { 0, 1, 2, 3 },
                new List<Id> { 4, 5, 6, 7 },
                new List<Id> { 8, 9, 10, 11 }
            });
        frame3d.BumpLayers(new[] { faces[2] }, 1); // M2 starts at layer 1

        LayerUpdater.ApplyLayerUpdate(frame, frame3d, new[] { faces[1], faces[2] });

        Assert.Equal(0, frame3d.LayerOf(faces[0]));
        Assert.Equal(2, frame3d.LayerOf(faces[1])); // M1 inverted to top
        Assert.Equal(1, frame3d.LayerOf(faces[2])); // M2 inverted to just above S
    }

    [Fact]
    public void OverlapBetweenMovingFaces_DoesNotCount()
    {
        // Two moving faces that overlap each other but no stationary face
        // is involved. Must be a no-op — only stationary-overlap drives
        // the layer bump.
        var (frame, frame3d, faces) = BuildScene(
            new List<Vertex3D>
            {
                V(5, 5), V(6, 5), V(6, 6), V(5, 6),   // stationary (far away)
                V(0, 0), V(1, 0), V(1, 1), V(0, 1),   // M1
                V(0.5f, 0.5f), V(1.5f, 0.5f),         // M2 (overlaps M1)
                V(1.5f, 1.5f), V(0.5f, 1.5f)
            },
            new List<List<Id>>
            {
                new List<Id> { 0, 1, 2, 3 },
                new List<Id> { 4, 5, 6, 7 },
                new List<Id> { 8, 9, 10, 11 }
            });

        LayerUpdater.ApplyLayerUpdate(frame, frame3d, new[] { faces[1], faces[2] });

        Assert.Equal(0, frame3d.LayerOf(faces[0]));
        Assert.Equal(0, frame3d.LayerOf(faces[1]));
        Assert.Equal(0, frame3d.LayerOf(faces[2]));
    }
}
