using System.Collections.Generic;

namespace valleyfold.v1.Models;

public class PaperModel
{
    private List<PaperCrease> _edges;
    public List<PaperFace> Faces;

    public void AddFace(PaperFace face)
    {
        Faces ??= new List<PaperFace>();
        Faces.Add(face);
    }
}