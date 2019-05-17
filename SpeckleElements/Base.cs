extern alias SpeckleNewtonsoft;
using SNJ = SpeckleNewtonsoft.Newtonsoft.Json;
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

  // TODO: We need a consensus on how to define/set family types or whatever they're called
  // for the various objects that support them, ie walls, floors, etc.

  [Serializable]
  public partial class GridLine : SpeckleLine, ISpeckleElement
  {
    public override string Type { get => base.Type + "/" + "GridLine"; }

    [SNJ.JsonIgnore]
    public SpeckleLine baseLine
    {
      get => this as SpeckleLine;
      set => this.Value = value.Value;
    }

    [SNJ.JsonIgnore]
    public Level level
    {
      get => ( Properties != null && Properties.ContainsKey( "level" ) ) ? ( Properties[ "level" ] as Level ) : null;
      set => Properties[ "level" ] = value;
    }

    [SNJ.JsonIgnore]
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

    [SNJ.JsonIgnore]
    public SpecklePolyline baseCurve
    {
      get => this as SpecklePolyline;
      set => this.Value = value.Value;
    }

    [SNJ.JsonIgnore]
    public double elevation
    {
      get => ( Properties != null && Properties.ContainsKey( "elevation" ) ) ? ( ( double ) Properties[ "elevation" ] ) : 0;
      set => Properties[ "elevation" ] = value;
    }

    [SNJ.JsonIgnore]
    public string levelName
    {
      get => ( Properties != null && Properties.ContainsKey( "levelName" ) ) ? ( ( string ) Properties[ "levelName" ] ) : null;
      set => Properties[ "levelName" ] = value;
    }

    [SNJ.JsonIgnore]
    public bool createView
    {
      get => ( Properties != null && Properties.ContainsKey( "createView" ) ) ? ( ( bool ) Properties[ "createView" ] ) : false;
      set => Properties[ "createView" ] = value;
    }

    [SNJ.JsonIgnore]
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

    [SNJ.JsonIgnore]
    public SpeckleObject baseCurve
    {
      get => ( Properties != null && Properties.ContainsKey( "baseCurve" ) ) ? ( ( SpeckleObject ) Properties[ "baseCurve" ] ) : null;
      set => Properties[ "baseCurve" ] = value;
    }

    [SNJ.JsonIgnore]
    public SpeckleMesh displayMesh
    {
      get => this as SpeckleMesh;
      set { this.Vertices = value.Vertices; this.Faces = value.Faces; }
    }

    [SNJ.JsonIgnore]
    public string wallType
    {
      get => ( Properties != null && Properties.ContainsKey( "wallType" ) ) ? ( ( string ) Properties[ "wallType" ] ) : null;
      set => Properties[ "wallType" ] = value;
    }

    [SNJ.JsonIgnore]
    public double height
    {
      get => ( Properties != null && Properties.ContainsKey( "height" ) ) ? ( ( double ) Properties[ "height" ] ) : 1;
      set => Properties[ "height" ] = value;
    }

    [SNJ.JsonIgnore]
    public double offset
    {
      get => ( Properties != null && Properties.ContainsKey( "offset" ) ) ? ( ( double ) Properties[ "offset" ] ) : 0;
      set => Properties[ "offset" ] = value;
    }

    [SNJ.JsonIgnore]
    public Level level
    {
      get => ( Properties != null && Properties.ContainsKey( "level" ) ) ? ( Properties[ "level" ] as Level ) : null;
      set => Properties[ "level" ] = value;
    }

    [SNJ.JsonIgnore]
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

    [SNJ.JsonIgnore]
    public SpeckleObject baseCurve
    {
      get => ( Properties != null && Properties.ContainsKey( "baseCurve" ) ) ? ( ( SpeckleObject ) Properties[ "baseCurve" ] ) : null;
      set => Properties[ "baseCurve" ] = value;
    }

    [SNJ.JsonIgnore]
    public SpeckleMesh displayMesh
    {
      get => this as SpeckleMesh;
      set { this.Vertices = value.Vertices; this.Faces = value.Faces; }
    }

    [SNJ.JsonIgnore]
    public string floorType
    {
      get => ( Properties != null && Properties.ContainsKey( "floorType" ) ) ? ( ( string ) Properties[ "floorType" ] ) : null;
      set => Properties[ "floorType" ] = value;
    }

    [SNJ.JsonIgnore]
    public Level level
    {
      get => ( Properties != null && Properties.ContainsKey( "level" ) ) ? ( Properties[ "level" ] as Level ) : null;
      set => Properties[ "level" ] = value;
    }

    [SNJ.JsonIgnore]
    public SpeckleLine slopedArrow
    {
      get => ( Properties != null && Properties.ContainsKey( "slopedArrow" ) ) ? ( Properties[ "slopedArrow" ] as SpeckleLine ) : null;
      set => Properties[ "slopedArrow" ] = value;
    }

    [SNJ.JsonIgnore]
    public double slope
    {
      get => ( Properties != null && Properties.ContainsKey( "slope" ) ) ? ( ( double ) Properties[ "slope" ] ) : 0;
      set => Properties[ "slope" ] = value;
    }

    [SNJ.JsonIgnore]
    public Dictionary<string, object> parameters
    {
      get => ( Properties != null && Properties.ContainsKey( "parameters" ) ) ? ( Properties[ "parameters" ] as Dictionary<string, object> ) : null;
      set => Properties[ "parameters" ] = value;
    }

    public Floor( ) { }
  }

  [Serializable]
  public partial class Column : SpeckleMesh, ISpeckleElement
  {
    public override string Type { get => base.Type + "/" + "Column"; }

    [SNJ.JsonIgnore]
    public SpeckleLine baseLine
    {
      get => ( Properties != null && Properties.ContainsKey( "baseLine" ) ) ? ( ( SpeckleLine ) Properties[ "baseLine" ] ) : null;
      set => Properties[ "baseLine" ] = value;
    }

    [SNJ.JsonIgnore]
    public SpeckleMesh displayMesh
    {
      get => this as SpeckleMesh;
      set { this.Vertices = value.Vertices; this.Faces = value.Faces; }
    }

    [SNJ.JsonIgnore]
    public string columnFamily
    {
      get => ( Properties != null && Properties.ContainsKey( "columnFamily" ) ) ? ( ( string ) Properties[ "columnFamily" ] ) : null;
      set => Properties[ "columnFamily" ] = value;
    }

    [SNJ.JsonIgnore]
    public string columnType
    {
      get => ( Properties != null && Properties.ContainsKey( "columnType" ) ) ? ( ( string ) Properties[ "columnType" ] ) : null;
      set => Properties[ "columnType" ] = value;
    }

    [SNJ.JsonIgnore]
    public Level level
    {
      get => ( Properties != null && Properties.ContainsKey( "level" ) ) ? ( Properties[ "level" ] as Level ) : null;
      set => Properties[ "level" ] = value;
    }

    [SNJ.JsonIgnore]
    public Dictionary<string, object> parameters
    {
      get => ( Properties != null && Properties.ContainsKey( "parameters" ) ) ? ( Properties[ "parameters" ] as Dictionary<string, object> ) : null;
      set => Properties[ "parameters" ] = value;
    }
    public Column( ) { }
  }

  [Serializable]
  public partial class Beam : SpeckleMesh, ISpeckleElement
  {
    public override string Type { get => base.Type + "/" + "Beam"; }

    [SNJ.JsonIgnore]
    public SpeckleLine baseLine
    {
      get => ( Properties != null && Properties.ContainsKey( "baseLine" ) ) ? ( ( SpeckleLine ) Properties[ "baseLine" ] ) : null;
      set => Properties[ "baseLine" ] = value;
    }

    [SNJ.JsonIgnore]
    public string beamFamily
    {
      get => ( Properties != null && Properties.ContainsKey( "beamFamily" ) ) ? ( ( string ) Properties[ "beamFamily" ] ) : null;
      set => Properties[ "beamFamily" ] = value;
    }

    [SNJ.JsonIgnore]
    public string beamType
    {
      get => ( Properties != null && Properties.ContainsKey( "beamType" ) ) ? ( ( string ) Properties[ "beamType" ] ) : null;
      set => Properties[ "beamType" ] = value;
    }

    [SNJ.JsonIgnore]
    public Level level
    {
      get => ( Properties != null && Properties.ContainsKey( "level" ) ) ? ( Properties[ "level" ] as Level ) : null;
      set => Properties[ "level" ] = value;
    }

    [SNJ.JsonIgnore]
    public SpeckleMesh displayMesh
    {
      get => this as SpeckleMesh;
      set { this.Vertices = value.Vertices; this.Faces = value.Faces; }
    }

    [SNJ.JsonIgnore]
    public Dictionary<string, object> parameters
    {
      get => ( Properties != null && Properties.ContainsKey( "parameters" ) ) ? ( Properties[ "parameters" ] as Dictionary<string, object> ) : null;
      set => Properties[ "parameters" ] = value;
    }

    public Beam( ) { }
  }

  [Serializable]
  public partial class Shaft : SpeckleMesh, ISpeckleElement
  {
    public override string Type { get => base.Type + "/" + "Shaft"; }

    [SNJ.JsonIgnore]
    public Level bottomLevel
    {
      get => ( Properties != null && Properties.ContainsKey( "bottomLevel" ) ) ? ( Properties[ "bottomLevel" ] as Level ) : null;
      set => Properties[ "bottomLevel" ] = value;
    }

    [SNJ.JsonIgnore]
    public Level topLevel
    {
      get => ( Properties != null && Properties.ContainsKey( "topLevel" ) ) ? ( Properties[ "topLevel" ] as Level ) : null;
      set => Properties[ "topLevel" ] = value;
    }

    [SNJ.JsonIgnore]
    public SpeckleObject baseCurve
    {
      get => ( Properties != null && Properties.ContainsKey( "baseCurve" ) ) ? ( ( SpeckleObject ) Properties[ "baseCurve" ] ) : null;
      set => Properties[ "baseCurve" ] = value;
    }

    [SNJ.JsonIgnore]
    public SpeckleMesh displayMesh
    {
      get => this as SpeckleMesh;
      set { this.Vertices = value.Vertices; this.Faces = value.Faces; }
    }

    [SNJ.JsonIgnore]
    public Dictionary<string, object> parameters
    {
      get => ( Properties != null && Properties.ContainsKey( "parameters" ) ) ? ( Properties[ "parameters" ] as Dictionary<string, object> ) : null;
      set => Properties[ "parameters" ] = value;
    }

    public Shaft( ) { }
  }

  [Serializable]
  public partial class Topography : SpeckleMesh, ISpeckleElement
  {
    public override string Type { get => base.Type + "/" + "Topography"; }

    [SNJ.JsonIgnore]
    public SpeckleMesh topographyMesh
    {
      get => this as SpeckleMesh;
      set { this.Vertices = value.Vertices; this.Faces = value.Faces; }
    }

    [SNJ.JsonIgnore]
    public Dictionary<string, object> parameters
    {
      get => ( Properties != null && Properties.ContainsKey( "parameters" ) ) ? ( Properties[ "parameters" ] as Dictionary<string, object> ) : null;
      set => Properties[ "parameters" ] = value;
    }

    public Topography( ) { }
  }

  [Serializable]
  public partial class FamilyInstance : SpeckleMesh, ISpeckleElement
  {
    public override string Type { get => base.Type + "/" + "FamilyInstance"; }

    [SNJ.JsonIgnore]
    public List<SpecklePoint> points
    {
      get => ( Properties != null && Properties.ContainsKey( "points" ) ) ? ( Properties[ "points" ] as List<SpecklePoint> ) : null;
      set => Properties[ "points" ] = value;
    }

    [SNJ.JsonIgnore]
    public string view
    {
      get => ( Properties != null && Properties.ContainsKey( "view" ) ) ? ( ( string ) Properties[ "view" ] ) : null;
      set => Properties[ "view" ] = value;
    }

    [SNJ.JsonIgnore]
    public Level level
    {
      get => ( Properties != null && Properties.ContainsKey( "level" ) ) ? ( Properties[ "level" ] as Level ) : null;
      set => Properties[ "level" ] = value;
    }

    [SNJ.JsonIgnore]
    public string familyName
    {
      get => ( Properties != null && Properties.ContainsKey( "familyName" ) ) ? ( ( string ) Properties[ "familyName" ] ) : null;
      set => Properties[ "familyName" ] = value;
    }

    [SNJ.JsonIgnore]
    public string familyType
    {
      get => ( Properties != null && Properties.ContainsKey( "familyType" ) ) ? ( ( string ) Properties[ "familyType" ] ) : null;
      set => Properties[ "familyType" ] = value;
    }

    [SNJ.JsonIgnore]
    public Dictionary<string, object> parameters
    {
      get => ( Properties != null && Properties.ContainsKey( "parameters" ) ) ? ( Properties[ "parameters" ] as Dictionary<string, object> ) : null;
      set => Properties[ "parameters" ] = value;
    }

    public FamilyInstance( ) { }
  }

  [Serializable]
  public partial class GenericElement : SpeckleMesh, ISpeckleElement
  {
    public override string Type { get => base.Type + "/" + "GenericElement"; }

    [SNJ.JsonIgnore]
    public Dictionary<string, object> parameters
    {
      get => ( Properties != null && Properties.ContainsKey( "parameters" ) ) ? ( Properties[ "parameters" ] as Dictionary<string, object> ) : null;
      set => Properties[ "parameters" ] = value;
    }

    public GenericElement( ) { }
  }

  [Serializable]
  public partial class Room: SpeckleMesh, ISpeckleElement
  {
    public override string Type { get => base.Type + "/" + "Room"; }

    [SNJ.JsonIgnore]
    public SpeckleObject baseCurve
    {
      get => (Properties != null && Properties.ContainsKey( "baseCurve" )) ? ((SpeckleObject) Properties[ "baseCurve" ]) : null;
      set => Properties[ "baseCurve" ] = value;
    }

    [SNJ.JsonIgnore]
    public SpecklePoint roomLocation
    {
      get => (Properties != null && Properties.ContainsKey( "roomLocation" )) ? ((SpecklePoint) Properties[ "roomLocation" ]) : null;
      set => Properties[ "roomLocation" ] = value;
    }

    [SNJ.JsonIgnore]
    public string roomName
    {
      get => (Properties != null && Properties.ContainsKey( "roomName" )) ? ((string) Properties[ "roomName" ]) : null;
      set => Properties[ "roomName" ] = value;
    }
    
    // WTF Moment: room number is a string property in revit. LOLOLOL
    [SNJ.JsonIgnore]
    public string roomNumber
    {
      get => (Properties != null && Properties.ContainsKey( "roomNumber" )) ? ((string) Properties[ "roomNumber" ]) : null;
      set => Properties[ "roomNumber" ] = value;
    }

    [SNJ.JsonIgnore]
    public double roomArea
    {
      get => (Properties != null && Properties.ContainsKey( "roomArea" )) ? ((double) Properties[ "roomArea" ]) : 0;
      set => Properties[ "roomArea" ] = value;
    }

    [SNJ.JsonIgnore]
    public double roomVolume
    {
      get => (Properties != null && Properties.ContainsKey( "roomVolume" )) ? ((double) Properties[ "roomVolume" ]) : 0;
      set => Properties[ "roomVolume" ] = value;
    }


    // Number
    // Name
    // Center Point
    // Area
    // 

    [SNJ.JsonIgnore]
    public Dictionary<string, object> parameters
    {
      get => (Properties != null && Properties.ContainsKey( "parameters" )) ? (Properties[ "parameters" ] as Dictionary<string, object>) : null;
      set => Properties[ "parameters" ] = value;
    }

    public Room() { }
  }
}
