using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpeckleCore;
using SpeckleCoreGeometryClasses;

namespace SpeckleElements
{

  public interface ISpeckleElement
  {
    Dictionary<string, object> parameters { get; set; }
    string GUID { get; set; }
  }

  [Serializable]
  public class GridLine : SpeckleLine, ISpeckleElement
  {
    public override string Type { get => "GridLine"; }

    public Dictionary<string, object> parameters { get => Properties; set => Properties = value; }

    public string GUID { get; set; } = Guid.NewGuid().ToString();

    public GridLine( ) { }
  }

  [Serializable]
  public class Level : SpeckleMesh, ISpeckleElement
  {
    public override string Type { get => "Level"; }

    public Dictionary<string, object> parameters { get => Properties; set => Properties = value; }

    public string GUID { get; set; } = Guid.NewGuid().ToString();

    public double Elevation { get; set; }

    public Level( ) { }
  }
}
