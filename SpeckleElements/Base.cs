using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpeckleCore;
using SpeckleCoreGeometryClasses;

namespace SpeckleElements
{
  /// <summary>
  /// Lol interface to force some revit specific props. Not really sure it's needed. It might go.
  /// </summary>
  public interface ISpeckleElement : ISpeckleInitializer
  {
    Dictionary<string, object> parameters { get; set; }
  }

  [Serializable]
  public class GridLine : SpeckleLine, ISpeckleElement
  {
    public override string Type { get => base.Type + "/" + "GridLine"; }

    public Dictionary<string, object> parameters
    {
      get => Properties.ContainsKey( "parameters" ) ? Properties[ "parameters" ] as Dictionary<string, object> : null;
      set => Properties[ "parameters" ] = value;
    }

    public GridLine( ) { }
  }
}
