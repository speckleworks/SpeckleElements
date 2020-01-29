using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SpeckleCore;
using SpeckleCoreGeometryClasses;

namespace SpeckleElementsClasses
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
    public SpeckleLine baseLine
    {
      get => this as SpeckleLine;
      set => this.Value = value.Value;
    }

    [JsonIgnore]
    public Level level
    {
      get => (Properties != null && Properties.ContainsKey("level")) ? (Properties["level"] as Level) : null;
      set => Properties["level"] = value;
    }

    [JsonIgnore]
    public Dictionary<string, object> parameters
    {
      get => (Properties != null && Properties.ContainsKey("parameters")) ? (Properties["parameters"] as Dictionary<string, object>) : null;
      set => Properties["parameters"] = value;
    }

    public GridLine() { }
  }

  [Serializable]
  public partial class Level : SpecklePolyline, ISpeckleElement
  {
    public override string Type { get => base.Type + "/" + "Level"; }

    [JsonIgnore]
    public SpecklePolyline baseCurve
    {
      get => this as SpecklePolyline;
      set => this.Value = value.Value;
    }

    [JsonIgnore]
    public double elevation
    {
      get => (Properties != null && Properties.ContainsKey("elevation")) ? ((double)Properties["elevation"]) : 0;
      set => Properties["elevation"] = value;
    }

    [JsonIgnore]
    public string levelName
    {
      get => (Properties != null && Properties.ContainsKey("levelName")) ? ((string)Properties["levelName"]) : null;
      set => Properties["levelName"] = value;
    }

    [JsonIgnore]
    public bool createView
    {
      get => (Properties != null && Properties.ContainsKey("createView")) ? ((bool)Properties["createView"]) : false;
      set => Properties["createView"] = value;
    }

    [JsonIgnore]
    public Dictionary<string, object> parameters
    {
      get => (Properties != null && Properties.ContainsKey("parameters")) ? (Properties["parameters"] as Dictionary<string, object>) : null;
      set => Properties["parameters"] = value;
    }

    public Level() { }
  }

  [Serializable]
  public partial class Wall : SpeckleMesh, ISpeckleElement
  {
    public override string Type { get => base.Type + "/" + "Wall"; }

    [JsonIgnore]
    public SpeckleObject baseCurve
    {
      get => (Properties != null && Properties.ContainsKey("baseCurve")) ? ((SpeckleObject)Properties["baseCurve"]) : null;
      set => Properties["baseCurve"] = value;
    }

    [JsonIgnore]
    public SpeckleMesh displayMesh
    {
      get => this as SpeckleMesh;
      set { this.Vertices = value.Vertices; this.Faces = value.Faces; }
    }

    [JsonIgnore]
    public string wallType
    {
      get => (Properties != null && Properties.ContainsKey("wallType")) ? ((string)Properties["wallType"]) : null;
      set => Properties["wallType"] = value;
    }

    [JsonIgnore]
    public double height
    {
      get => (Properties != null && Properties.ContainsKey("height")) ? ((double)Properties["height"]) : 1;
      set => Properties["height"] = value;
    }

    [JsonIgnore]
    public double offset
    {
      get => (Properties != null && Properties.ContainsKey("offset")) ? ((double)Properties["offset"]) : 0;
      set => Properties["offset"] = value;
    }

    [JsonIgnore]
    public Level baseLevel
    {
      get => (Properties != null && Properties.ContainsKey("baseLevel")) ? (Properties["baseLevel"] as Level) : null;
      set => Properties["baseLevel"] = value;
    }

    [JsonIgnore]
    public Level topLevel
    {
      get => (Properties != null && Properties.ContainsKey("topLevel")) ? (Properties["topLevel"] as Level) : null;
      set => Properties["topLevel"] = value;
    }

    [JsonIgnore]
    public Dictionary<string, object> parameters
    {
      get => (Properties != null && Properties.ContainsKey("parameters")) ? (Properties["parameters"] as Dictionary<string, object>) : null;
      set => Properties["parameters"] = value;
    }

    public Wall() { }
  }

  [Serializable]
  public partial class Floor : SpeckleMesh, ISpeckleElement
  {
    public override string Type { get => base.Type + "/" + "Floor"; }

    [JsonIgnore]
    public SpeckleObject baseCurve
    {
      get => (Properties != null && Properties.ContainsKey("baseCurve")) ? ((SpeckleObject)Properties["baseCurve"]) : null;
      set => Properties["baseCurve"] = value;
    }

    [JsonIgnore]
    public SpeckleMesh displayMesh
    {
      get => this as SpeckleMesh;
      set { this.Vertices = value.Vertices; this.Faces = value.Faces; }
    }

    [JsonIgnore]
    public string floorType
    {
      get => (Properties != null && Properties.ContainsKey("floorType")) ? ((string)Properties["floorType"]) : null;
      set => Properties["floorType"] = value;
    }

    [JsonIgnore]
    public Level level
    {
      get => (Properties != null && Properties.ContainsKey("level")) ? (Properties["level"] as Level) : null;
      set => Properties["level"] = value;
    }

    [JsonIgnore]
    public SpeckleLine slopedArrow
    {
      get => (Properties != null && Properties.ContainsKey("slopedArrow")) ? (Properties["slopedArrow"] as SpeckleLine) : null;
      set => Properties["slopedArrow"] = value;
    }

    [JsonIgnore]
    public double slope
    {
      get => (Properties != null && Properties.ContainsKey("slope")) ? ((double)Properties["slope"]) : 0;
      set => Properties["slope"] = value;
    }

    [JsonIgnore]
    public Dictionary<string, object> parameters
    {
      get => (Properties != null && Properties.ContainsKey("parameters")) ? (Properties["parameters"] as Dictionary<string, object>) : null;
      set => Properties["parameters"] = value;
    }

    public Floor() { }
  }

  [Serializable]
  public partial class Column : SpeckleMesh, ISpeckleElement
  {
    public override string Type { get => base.Type + "/" + "Column"; }

    [JsonIgnore]
    public SpeckleLine baseLine
    {
      get => (Properties != null && Properties.ContainsKey("baseLine")) ? ((SpeckleLine)Properties["baseLine"]) : null;
      set => Properties["baseLine"] = value;
    }

    [JsonIgnore]
    public SpeckleMesh displayMesh
    {
      get => this as SpeckleMesh;
      set { this.Vertices = value.Vertices; this.Faces = value.Faces; }
    }

    [JsonIgnore]
    public string columnFamily
    {
      get => (Properties != null && Properties.ContainsKey("columnFamily")) ? ((string)Properties["columnFamily"]) : null;
      set => Properties["columnFamily"] = value;
    }

    [JsonIgnore]
    public string columnType
    {
      get => (Properties != null && Properties.ContainsKey("columnType")) ? ((string)Properties["columnType"]) : null;
      set => Properties["columnType"] = value;
    }

    [JsonIgnore]
    public Level baseLevel
    {
      get => (Properties != null && Properties.ContainsKey("baseLevel")) ? (Properties["baseLevel"] as Level) : null;
      set => Properties["baseLevel"] = value;
    }

    [JsonIgnore]
    public Level topLevel
    {
      get => (Properties != null && Properties.ContainsKey("topLevel")) ? (Properties["topLevel"] as Level) : null;
      set => Properties["topLevel"] = value;
    }

    [JsonIgnore]
    public double? topOffset
    {
      get => (Properties != null && Properties.ContainsKey("topOffset")) ? ((double)Properties["topOffset"]) : 0;
      set => Properties["topOffset"] = value;
    }

    [JsonIgnore]
    public double? bottomOffset
    {
      get => (Properties != null && Properties.ContainsKey("bottomOffset")) ? ((double)Properties["bottomOffset"]) : 0;
      set => Properties["bottomOffset"] = value;
    }

    [JsonIgnore]
    public Dictionary<string, object> parameters
    {
      get => (Properties != null && Properties.ContainsKey("parameters")) ? (Properties["parameters"] as Dictionary<string, object>) : null;
      set => Properties["parameters"] = value;
    }
    public Column() { }
  }


  // !!! Brace and Beam use the same properties !!! 
  [Serializable]
  public partial class Brace : SpeckleMesh, ISpeckleElement
  {
    public override string Type { get => base.Type + "/" + "Brace"; }

    [JsonIgnore]
    public SpeckleLine baseLine
    {
      get => (Properties != null && Properties.ContainsKey("baseLine")) ? ((SpeckleLine)Properties["baseLine"]) : null;
      set => Properties["baseLine"] = value;
    }

    [JsonIgnore]
    public string braceFamily
    {
      get => (Properties != null && Properties.ContainsKey("braceFamily")) ? ((string)Properties["braceFamily"]) : null;
      set => Properties["braceFamily"] = value;
    }

    [JsonIgnore]
    public string braceType
    {
      get => (Properties != null && Properties.ContainsKey("braceType")) ? ((string)Properties["braceType"]) : null;
      set => Properties["braceType"] = value;
    }

    [JsonIgnore]
    public Level level
    {
      get => (Properties != null && Properties.ContainsKey("level")) ? (Properties["level"] as Level) : null;
      set => Properties["level"] = value;
    }

    [JsonIgnore]
    public SpeckleMesh displayMesh
    {
      get => this as SpeckleMesh;
      set { this.Vertices = value.Vertices; this.Faces = value.Faces; }
    }

    [JsonIgnore]
    public Dictionary<string, object> parameters
    {
      get => (Properties != null && Properties.ContainsKey("parameters")) ? (Properties["parameters"] as Dictionary<string, object>) : null;
      set => Properties["parameters"] = value;
    }

    public Brace() { }
  }

  // !!! Brace and Beam use the same properties !!!
  [Serializable]
  public partial class Beam : SpeckleMesh, ISpeckleElement
  {
    public override string Type { get => base.Type + "/" + "Beam"; }

    [JsonIgnore]
    public SpeckleLine baseLine
    {
      get => (Properties != null && Properties.ContainsKey("baseLine")) ? ((SpeckleLine)Properties["baseLine"]) : null;
      set => Properties["baseLine"] = value;
    }

    [JsonIgnore]
    public string beamFamily
    {
      get => (Properties != null && Properties.ContainsKey("beamFamily")) ? ((string)Properties["beamFamily"]) : null;
      set => Properties["beamFamily"] = value;
    }

    [JsonIgnore]
    public string beamType
    {
      get => (Properties != null && Properties.ContainsKey("beamType")) ? ((string)Properties["beamType"]) : null;
      set => Properties["beamType"] = value;
    }

    [JsonIgnore]
    public Level level
    {
      get => (Properties != null && Properties.ContainsKey("level")) ? (Properties["level"] as Level) : null;
      set => Properties["level"] = value;
    }

    [JsonIgnore]
    public SpeckleMesh displayMesh
    {
      get => this as SpeckleMesh;
      set { this.Vertices = value.Vertices; this.Faces = value.Faces; }
    }

    [JsonIgnore]
    public Dictionary<string, object> parameters
    {
      get => (Properties != null && Properties.ContainsKey("parameters")) ? (Properties["parameters"] as Dictionary<string, object>) : null;
      set => Properties["parameters"] = value;
    }

    public Beam() { }
  }


  [Serializable]
  public partial class Shaft : SpeckleMesh, ISpeckleElement
  {
    public override string Type { get => base.Type + "/" + "Shaft"; }

    [JsonIgnore]
    public Level bottomLevel
    {
      get => (Properties != null && Properties.ContainsKey("bottomLevel")) ? (Properties["bottomLevel"] as Level) : null;
      set => Properties["bottomLevel"] = value;
    }

    [JsonIgnore]
    public Level topLevel
    {
      get => (Properties != null && Properties.ContainsKey("topLevel")) ? (Properties["topLevel"] as Level) : null;
      set => Properties["topLevel"] = value;
    }

    [JsonIgnore]
    public SpeckleObject baseCurve
    {
      get => (Properties != null && Properties.ContainsKey("baseCurve")) ? ((SpeckleObject)Properties["baseCurve"]) : null;
      set => Properties["baseCurve"] = value;
    }

    [JsonIgnore]
    public SpeckleMesh displayMesh
    {
      get => this as SpeckleMesh;
      set { this.Vertices = value.Vertices; this.Faces = value.Faces; }
    }

    [JsonIgnore]
    public Dictionary<string, object> parameters
    {
      get => (Properties != null && Properties.ContainsKey("parameters")) ? (Properties["parameters"] as Dictionary<string, object>) : null;
      set => Properties["parameters"] = value;
    }

    public Shaft() { }
  }

  [Serializable]
  public partial class Topography : SpeckleMesh, ISpeckleElement
  {
    public override string Type { get => base.Type + "/" + "Topography"; }

    [JsonIgnore]
    public SpeckleMesh topographyMesh
    {
      get => this as SpeckleMesh;
      set { this.Vertices = value.Vertices; this.Faces = value.Faces; }
    }

    [JsonIgnore]
    public Dictionary<string, object> parameters
    {
      get => (Properties != null && Properties.ContainsKey("parameters")) ? (Properties["parameters"] as Dictionary<string, object>) : null;
      set => Properties["parameters"] = value;
    }

    public Topography() { }
  }

  [Serializable]
  public partial class AdaptiveComponent : SpeckleMesh, ISpeckleElement
  {

    public override string Type { get => base.Type + "/" + "AdaptiveComponent"; }

    [JsonIgnore]
    public string familyName
    {
      get => (Properties != null && Properties.ContainsKey("familyName")) ? ((string)Properties["familyName"]) : null;
      set => Properties["familyName"] = value;
    }

    [JsonIgnore]
    public string familyType
    {
      get => (Properties != null && Properties.ContainsKey("familyType")) ? ((string)Properties["familyType"]) : null;
      set => Properties["familyType"] = value;
    }

    [JsonIgnore]
    public List<SpecklePoint> points
    {
      get
      {
        if (Properties == null || !Properties.ContainsKey("points"))
          return null;
        try
        {
          return (Properties["points"] as List<object>).Select(x => x as SpecklePoint).ToList();
        }
        catch
        {
          //fail quietly
        }
        return null;
       
      }
      set => Properties["points"] = value;
    }

    [JsonIgnore]
    public Dictionary<string, object> parameters
    {
      get => (Properties != null && Properties.ContainsKey("parameters")) ? (Properties["parameters"] as Dictionary<string, object>) : null;
      set => Properties["parameters"] = value;
    }

    public AdaptiveComponent() { }
  }

  [Serializable]
  public partial class HostedFamilyInstance : SpeckleMesh, ISpeckleElement
  {

    public override string Type { get => base.Type + "/" + "HostedFamilyInstance"; }

    [JsonIgnore]
    public Dictionary<string, object> parameters
    {
      get => (Properties != null && Properties.ContainsKey("parameters")) ? (Properties["parameters"] as Dictionary<string, object>) : null;
      set => Properties["parameters"] = value;
    }

    [JsonIgnore]
    public string hostElementSpeckleId
    {
      get => (Properties != null && Properties.ContainsKey("hostElementSpeckleId")) ? ((string)Properties["hostElementSpeckleId"]) : null;
      set => Properties["hostElementSpeckleId"] = value;
    }

    [JsonIgnore]
    public string familyName
    {
      get => (Properties != null && Properties.ContainsKey("familyName")) ? ((string)Properties["familyName"]) : null;
      set => Properties["familyName"] = value;
    }

    [JsonIgnore]
    public string familyType
    {
      get => (Properties != null && Properties.ContainsKey("familyType")) ? ((string)Properties["familyType"]) : null;
      set => Properties["familyType"] = value;
    }

    [JsonIgnore]
    public List<SpecklePoint> points
    {
      get => (Properties != null && Properties.ContainsKey("points")) ? (Properties["points"] as List<SpecklePoint>) : null;
      set => Properties["points"] = value;
    }

    public HostedFamilyInstance() { }
  }

  [Serializable]
  public partial class FamilyInstance : SpeckleMesh, ISpeckleElement
  {
    public override string Type { get => base.Type + "/" + "FamilyInstance"; }

    [JsonIgnore]
    public SpecklePoint basePoint
    {
      get => (Properties != null && Properties.ContainsKey("basePoint")) ? (Properties["basePoint"] as SpecklePoint) : null;
      set => Properties["basePoint"] = value;
    }

    //[JsonIgnore]
    //public string view
    //{
    //  get => (Properties != null && Properties.ContainsKey("view")) ? ((string)Properties["view"]) : null;
    //  set => Properties["view"] = value;
    //}

    //[JsonIgnore]
    //public Level level
    //{
    //  get => (Properties != null && Properties.ContainsKey("level")) ? (Properties["level"] as Level) : null;
    //  set => Properties["level"] = value;
    //}

    [JsonIgnore]
    public string familyName
    {
      get => (Properties != null && Properties.ContainsKey("familyName")) ? ((string)Properties["familyName"]) : null;
      set => Properties["familyName"] = value;
    }

    [JsonIgnore]
    public string familyType
    {
      get => (Properties != null && Properties.ContainsKey("familyType")) ? ((string)Properties["familyType"]) : null;
      set => Properties["familyType"] = value;
    }

    //rotation around the Z axis in degress
    [JsonIgnore]
    public double? rotation
    {
      get => (Properties != null && Properties.ContainsKey("rotation")) ? ((double)Properties["rotation"]) : 0;
      set => Properties["rotation"] = value;
    }

    [JsonIgnore]
    public Dictionary<string, object> parameters
    {
      get => (Properties != null && Properties.ContainsKey("parameters")) ? (Properties["parameters"] as Dictionary<string, object>) : null;
      set => Properties["parameters"] = value;
    }

    public FamilyInstance() { }
  }

  [Serializable]
  public partial class GenericElement : SpeckleMesh, ISpeckleElement
  {
    public override string Type { get => base.Type + "/" + "GenericElement"; }

    [JsonIgnore]
    public Dictionary<string, object> parameters
    {
      get => (Properties != null && Properties.ContainsKey("parameters")) ? (Properties["parameters"] as Dictionary<string, object>) : null;
      set => Properties["parameters"] = value;
    }

    public GenericElement() { }
  }

  [Serializable]
  public partial class Room : SpeckleMesh, ISpeckleElement
  {
    public override string Type { get => base.Type + "/" + "Room"; }

    [JsonIgnore]
    public SpeckleObject baseCurve
    {
      get => (Properties != null && Properties.ContainsKey("baseCurve")) ? ((SpeckleObject)Properties["baseCurve"]) : null;
      set => Properties["baseCurve"] = value;
    }

    [JsonIgnore]
    public SpecklePoint roomLocation
    {
      get => (Properties != null && Properties.ContainsKey("roomLocation")) ? ((SpecklePoint)Properties["roomLocation"]) : null;
      set => Properties["roomLocation"] = value;
    }

    [JsonIgnore]
    public string roomName
    {
      get => (Properties != null && Properties.ContainsKey("roomName")) ? ((string)Properties["roomName"]) : null;
      set => Properties["roomName"] = value;
    }

    // WTF Moment: room number is a string property in revit. LOLOLOL
    [JsonIgnore]
    public string roomNumber
    {
      get => (Properties != null && Properties.ContainsKey("roomNumber")) ? ((string)Properties["roomNumber"]) : null;
      set => Properties["roomNumber"] = value;
    }

    [JsonIgnore]
    public double roomArea
    {
      get => (Properties != null && Properties.ContainsKey("roomArea")) ? ((double)Properties["roomArea"]) : 0;
      set => Properties["roomArea"] = value;
    }

    [JsonIgnore]
    public double roomVolume
    {
      get => (Properties != null && Properties.ContainsKey("roomVolume")) ? ((double)Properties["roomVolume"]) : 0;
      set => Properties["roomVolume"] = value;
    }


    // Number
    // Name
    // Center Point
    // Area
    // 

    [JsonIgnore]
    public Dictionary<string, object> parameters
    {
      get => (Properties != null && Properties.ContainsKey("parameters")) ? (Properties["parameters"] as Dictionary<string, object>) : null;
      set => Properties["parameters"] = value;
    }

    public Room() { }
  }
}
