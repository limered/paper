using System.Collections.Generic;

namespace valleyfold.Models;

public class PaperModel
{
    private List<PaperFace> _faces;
    private List<PaperCrease> _edges;

    public void AddFace(PaperFace face)
    {
        _faces.Add(face);
    }
}