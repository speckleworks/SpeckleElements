using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
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
  public partial class GridLine : SpeckleLine, ISpeckleElement
  {
    public override string Type { get => base.Type + "/" + "GridLine"; }

    [JsonIgnore]
    public Dictionary<string, object> parameters
    {
      get => ( Properties != null && Properties.ContainsKey( "parameters" ) ) ? ( Properties[ "parameters" ] as Dictionary<string, object> ) : null;
      set => Properties[ "parameters" ] = value;
    }

    public GridLine( ) { }
  }

  [Serializable]
  public partial class Level : SpecklePolyline, ISpeckleElement
  {
    public override string Type { get => base.Type + "/" + "Level"; }

    [JsonIgnore]
    public double elevation
    {
      get => ( Properties != null && Properties.ContainsKey( "elevation" ) ) ? ( ( double ) Properties[ "elevation" ] ) : 0;
      set => Properties[ "elevation" ] = value;
    }

    [JsonIgnore]
    public Dictionary<string, object> parameters
    {
      get => ( Properties != null && Properties.ContainsKey( "parameters" ) ) ? ( Properties[ "parameters" ] as Dictionary<string, object> ) : null;
      set => Properties[ "parameters" ] = value;
    }

    public Level( ) { }
  }

  public partial class Wall : SpecklePolycurve, ISpeckleElement
  {
    public override string Type { get => base.Type + "/" + "Wall"; }

    [JsonIgnore]
    public double family
    {
      get => ( Properties != null && Properties.ContainsKey( "family" ) ) ? ( ( double ) Properties[ "family" ] ) : 0;
      set => Properties[ "family" ] = value;
    }

    [JsonIgnore]
    public Dictionary<string, object> parameters
    {
      get => ( Properties != null && Properties.ContainsKey( "parameters" ) ) ? ( Properties[ "parameters" ] as Dictionary<string, object> ) : null;
      set => Properties[ "parameters" ] = value;
    }

    public Wall() { }
  }

}
