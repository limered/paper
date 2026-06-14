# 01 — Fix AddVertexOnBorderEdge.AddsTheVertexToFrame Id mismatch

Status: done

## Parent

None — standalone test fix.

## What to build

`Testing.Basics.FrameTests+AddVertexOnBorderEdge.AddsTheVertexToFrame` is failing because the assertion ignores the `Id` field on `Vertex`.

```csharp
Assert.Equivalent(new Vertex { Coord = point }, _frame.Vertices[id]);
//                ^^^^^^^^^^^^^^^^^^^^^^^^^^^^
//                Id defaults to 0; the actual vertex has Id = 4
```

The square is initialised with four vertices (Ids 0–3), so the fifth vertex added by `AddVertexToEdge` correctly receives `Id = 4`. `Assert.Equivalent` performs structural equality, including the `Id` field, and the test fails on the `Id.Value` mismatch:

```
Assert.Equivalent() Failure: Mismatched value on member 'Id.Value'
Expected: 0
Actual:   4
```

The test was almost certainly written before `Vertex.Id` was introduced (or before it became part of the equivalence check). The production code is correct.

Fix: tighten the assertion so it only checks the field the test actually cares about — that the new vertex sits at the supplied coordinate. Either compare `Coord` directly, or construct the expected vertex with the predicted `Id`.

## Acceptance criteria

- [ ] `dotnet test valleyfold/valleyfold.sln` passes with zero failures.
- [ ] The `AddsTheVertexToFrame` test still verifies the intent: a vertex with the expected coordinate has been added to the frame.
- [ ] No production code changes — `EdgeCommands.AddVertexToEdge` already behaves correctly.

## Blocked by

None — can start immediately.
