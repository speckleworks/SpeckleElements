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

  // TODO: We need a consensus on how to define/set family types or whatever they're called
  // for the various objects that support them, ie walls, floors, etc.

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

  [Serializable]
  public partial class Wall : SpeckleMesh, ISpeckleElement
  {
    public override string Type { get => base.Type + "/" + "Wall"; }

    [JsonIgnore]
    public SpeckleObject baseCurve
    {
      get => ( Properties != null && Properties.ContainsKey( "baseCurve" ) ) ? ( ( SpeckleObject ) Properties[ "baseCurve" ] ) : null;
      set => Properties[ "baseCurve" ] = value;
    }

    [JsonIgnore]
    public string wallType
    {
      get => ( Properties != null && Properties.ContainsKey( "wallType" ) ) ? ( ( string ) Properties[ "wallType" ] ) : null;
      set => Properties[ "wallType" ] = value;
    }

    [JsonIgnore]
    public double height
    {
      get => ( Properties != null && Properties.ContainsKey( "height" ) ) ? ( ( double ) Properties[ "height" ] ) : 1;
      set => Properties[ "height" ] = value;
    }

    [JsonIgnore]
    public double offset
    {
      get => ( Properties != null && Properties.ContainsKey( "offset" ) ) ? ( ( double ) Properties[ "offset" ] ) : 0;
      set => Properties[ "offset" ] = value;
    }

    [JsonIgnore]
    public Level level
    {
      get => ( Properties != null && Properties.ContainsKey( "level" ) ) ? ( Properties[ "level" ] as Level ) : null;
      set => Properties[ "level" ] = value;
    }

    [JsonIgnore]
    public Dictionary<string, object> parameters
    {
      get => ( Properties != null && Properties.ContainsKey( "parameters" ) ) ? ( Properties[ "parameters" ] as Dictionary<string, object> ) : null;
      set => Properties[ "parameters" ] = value;
    }

    public Wall( ) { }
  }

  [Serializable]
  public partial class Floor : SpeckleMesh, ISpeckleElement
  {
    public override string Type { get => base.Type + "/" + "Floor"; }

    [JsonIgnore]
    public SpeckleObject baseCurve
    {
      get => ( Properties != null && Properties.ContainsKey( "baseCurve" ) ) ? ( ( SpeckleObject ) Properties[ "baseCurve" ] ) : null;
      set => Properties[ "baseCurve" ] = value;
    }

    [JsonIgnore]
    public string floorType
    {
      get => ( Properties != null && Properties.ContainsKey( "floorType" ) ) ? ( ( string ) Properties[ "floorType" ] ) : null;
      set => Properties[ "floorType" ] = value;
    }

    [JsonIgnore]
    public Level level
    {
      get => ( Properties != null && Properties.ContainsKey( "level" ) ) ? ( Properties[ "level" ] as Level ) : null;
      set => Properties[ "level" ] = value;
    }

    [JsonIgnore]
    public SpeckleLine slopedArrow
    {
      get => ( Properties != null && Properties.ContainsKey( "slopedArrow" ) ) ? ( Properties[ "slopedArrow" ] as SpeckleLine ) : null;
      set => Properties[ "slopedArrow" ] = value;
    }

    [JsonIgnore]
    public double slope
    {
      get => ( Properties != null && Properties.ContainsKey( "slope" ) ) ? ( ( double ) Properties[ "slope" ] ) : 0;
      set => Properties[ "slope" ] = value;
    }

    [JsonIgnore]
    public Dictionary<string, object> parameters
    {
      get => ( Properties != null && Properties.ContainsKey( "parameters" ) ) ? ( Properties[ "parameters" ] as Dictionary<string, object> ) : null;
      set => Properties[ "parameters" ] = value;
    }

    public Floor( ) { }
  }



}
