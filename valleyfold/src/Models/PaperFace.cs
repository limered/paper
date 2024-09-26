using System.Collections.Generic;
using Godot;

namespace valleyfold.Models;

public class PaperFace
{
	private List<Vector3> _corners;
	private bool _isTopFaced;

	public PaperFace(
		List<Vector3> corners, 
		bool isTopFaced)
	{
		_corners = corners;
		_isTopFaced = isTopFaced;
	}
}