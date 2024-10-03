using System.Collections.Generic;
using System.Linq;
using Godot;

namespace valleyfold.Models;

public class PaperFace
{
	public readonly List<Corner> Corners;
	
	private bool _isTopFaced;

	public PaperFace(
		List<Corner> corners, 
		bool isTopFaced)
	{
		Corners = corners;
		_isTopFaced = isTopFaced;
	}

	public Vector3[] Normals() {
		return Corners.Select(_ => new Vector3(0, 1, 0)).ToArray();
	}

	public int[] Triangulate()
	{
		return Geometry2D.TriangulatePolygon(Corners.Select(c => new Vector2(c.Position.X, c.Position.Z)).ToArray());
	}
}