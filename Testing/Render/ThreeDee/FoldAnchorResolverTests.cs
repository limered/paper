using System.Collections.Generic;
using valleyfold.ChangeTracking;
using valleyfold.Render.ThreeDee;
using valleyfold.TwoDeeModels;

namespace Testing.Render.ThreeDee;

public class FoldAnchorResolverTests
{
    // ADR-0002 contract: the renderer prefers the picked face (by Id) but
    // falls back to ADR-0001's "any face containing PickedVertex" rule when
    //   (a) PickedFace == -1 (legacy / vertex-anchored ChangeRecord), or
    //   (b) the picked face has been split out of existence by a later fold.

    private readonly Frame _frame;
    private readonly Face _faceA;
    private readonly Face _faceB;

    public FoldAnchorResolverTests()
    {
        // Two hand-constructed faces with explicit, distinct Ids. Vertex
        // membership is the only thing the resolver looks at (besides Id) so
        // edges and 2D coordinates are irrelevant here.
        _frame = new Frame();
        _faceA = new Face { Id = 0, Vertices = new List<Id> { 0, 1, 2 } };
        _faceB = new Face { Id = 1, Vertices = new List<Id> { 2, 3, 4 } };
        _frame.Faces.AddRange(new[] { _faceA, _faceB });
    }

    [Fact]
    public void PickedFaceSetAndPresent_ReturnsThatFace()
    {
        var change = new ChangeRecord { PickedFace = 1, PickedVertex = 0 };

        var resolved = FoldAnchorResolver.Resolve(_frame, change);

        Assert.Same(_faceB, resolved);
    }

    [Fact]
    public void PickedFaceMinusOne_FallsBackToPickedVertex()
    {
        // Legacy ChangeRecord shape from ApplyVertexValleyFold: PickedFace
        // defaults to -1, only PickedVertex is meaningful.
        var change = new ChangeRecord { PickedFace = -1, PickedVertex = 1 };

        var resolved = FoldAnchorResolver.Resolve(_frame, change);

        Assert.Same(_faceA, resolved);
    }

    [Fact]
    public void PickedFaceSetButMissing_FallsBackToPickedVertex()
    {
        // PickedFace.Id 99 doesn't exist in the frame (could happen if the
        // face was split by a later fold between record and replay). The
        // resolver should fall back to the vertex-anchored lookup rather
        // than returning null.
        var change = new ChangeRecord { PickedFace = 99, PickedVertex = 3 };

        var resolved = FoldAnchorResolver.Resolve(_frame, change);

        Assert.Same(_faceB, resolved);
    }

    [Fact]
    public void PickedFaceSetAndPresent_TakesPrecedenceOverPickedVertex()
    {
        // Vertex 2 is shared by both faces. With PickedFace set, the
        // resolver must NOT consult the vertex-anchored fallback.
        var change = new ChangeRecord { PickedFace = 1, PickedVertex = 2 };

        var resolved = FoldAnchorResolver.Resolve(_frame, change);

        Assert.Same(_faceB, resolved);
    }

    [Fact]
    public void NeitherResolvable_ReturnsNull()
    {
        // PickedFace missing AND no face contains PickedVertex 99.
        var change = new ChangeRecord { PickedFace = -1, PickedVertex = 99 };

        var resolved = FoldAnchorResolver.Resolve(_frame, change);

        Assert.Null(resolved);
    }
}
