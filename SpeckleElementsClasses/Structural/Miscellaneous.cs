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
  public enum StructuralBridgeCurvature
  {
    Straight,
    LeftCurve,
    RightCurve
  }

  public enum StructuralBridgePathType
  {
    Lane,
    Footway,
    Track,
    Vehicle,
    Carriage1Way,
    Carriage2Way
  }

  [Serializable]
  public partial class StructuralAssembly : SpeckleLine, IStructural
  {
    public override string Type { get => base.Type + "/StructuralAssembly"; }

    [JsonIgnore]
    private Dictionary<string, object> StructuralProperties
    {
      get
      {
        if (base.Properties == null)
          base.Properties = new Dictionary<string, object>();

        if (!base.Properties.ContainsKey("structural"))
          base.Properties["structural"] = new Dictionary<string, object>();

        return base.Properties["structural"] as Dictionary<string, object>;

      }
      set
      {
        if (base.Properties == null)
          base.Properties = new Dictionary<string, object>();

        base.Properties["structural"] = value;
      }
    }

    [JsonIgnore]
    public int NumPoints
    {
      get
      {
        if (StructuralProperties.ContainsKey("numPoints") && int.TryParse(StructuralProperties["numPoints"].ToString(), out int numPoints))
        {
          return numPoints;
        }
        return 0;
      }
      set => StructuralProperties["numPoints"] = value;
    }

    /// <summary>Base SpeckleLine.</summary>
    [JsonIgnore]
    public SpeckleLine BaseLine
    {
      get => this as SpeckleLine;
      set
      {
        this.Value = value.Value;
        this.Domain = value.Domain;
      }
    }

    [JsonIgnore]
    public SpecklePoint OrientationPoint
    {
      get => StructuralProperties.ContainsKey("orientationPoint") ? (StructuralProperties["orientationPoint"] as SpecklePoint) : null;
      set => StructuralProperties["orientationPoint"] = value;
    }

    [JsonIgnore]
    public double Width
    {
      get => (StructuralProperties.ContainsKey("width") && double.TryParse(StructuralProperties["width"].ToString(), out double width)) ? width : 0;
      set => StructuralProperties["width"] = value;
    }

    /// <summary>Application ID of StructuralLoadCase.</summary>
    [JsonIgnore]
    public List<string> ElementRefs
    {
      get
      {
        if (StructuralProperties.ContainsKey("elementRefs"))
        {
          try
          {
            try
            {
              return (List<string>)StructuralProperties["elementRefs"];
            }
            catch
            {
              this.ElementRefs = ((List<object>)StructuralProperties["elementRefs"]).Select(x => Convert.ToString(x)).ToList();
              return this.ElementRefs;
            }
          }
          catch
          { return null; }
        }
        else
          return null;
      }
      set => StructuralProperties["elementRefs"] = value;
    }
  }

  [Serializable]
  public partial class StructuralConstructionStage : SpeckleObject, IStructural
  {
    public override string Type { get => "StructuralConstructionStage"; }

    /// <summary>Application ID of members to include in the stage of the construction sequence.</summary>
    [JsonProperty("elementRefs", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public List<string> ElementRefs { get; set; }

    /// <summary>Number of days in the stage</summary>
    [JsonProperty("stageDays", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public int StageDays { get; set; }
  }

  [Serializable]
  public partial class StructuralStagedNodalRestraints : SpeckleObject, IStructural
  {
    public override string Type { get => "StructuralStagedNodalRestraints"; }

    /// <summary>A list of the X, Y, Z, Rx, Ry, and Rz restraints.</summary>
    [JsonProperty("restraint", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public StructuralVectorBoolSix Restraint { get; set; }

    /// <summary>Application IDs of StructuralNodes to apply restrain.</summary>
    [JsonProperty("nodeRefs", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public List<string> NodeRefs { get; set; }

    /// <summary>Application IDs of StructuralConstructionStages to apply restraints on</summary>
    [JsonProperty("constructionStageRefs", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public List<string> ConstructionStageRefs { get; set; }
  }

  [Serializable]
  public partial class StructuralRigidConstraints : SpeckleObject, IStructural
  {
    public override string Type { get => "StructuralRigidConstraints"; }

    /// <summary>A list of the X, Y, Z, Rx, Ry, and Rz constraints.</summary>
    [JsonProperty("constraints", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public StructuralVectorBoolSix Constraint { get; set; }

    /// <summary>Application IDs of StructuralNodes to apply constraints.</summary>
    [JsonProperty("nodeRefs", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public List<string> NodeRefs { get; set; }

    /// <summary>Application IDs of StructuralConstructionStages to apply restraints on</summary>
    [JsonProperty("constructionStageRefs", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public List<string> ConstructionStageRefs { get; set; }
  }

  [Serializable]
  public partial class StructuralBridgeAlignment : SpeckleObject, IStructural
  {
    public override string Type { get => "StructuralBridgeAlignment"; }

    [JsonProperty("elevation", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public double Elevation { get; set; }

    /// <summary>List nodes on the alignment.</summary>
    [JsonIgnore]
    public List<StructuralBridgeAlignmentNode> Nodes
    {
      get
      {
        if (StructuralProperties.ContainsKey("nodes"))
        {
          try
          {
            try
            {
              return (List<StructuralBridgeAlignmentNode>)StructuralProperties["nodes"];
            }
            catch
            {
              this.Nodes = ((List<object>)StructuralProperties["nodes"]).Select(x => x as StructuralBridgeAlignmentNode).ToList();
              return this.Nodes;
            }
          }
          catch
          { return null; }
        }
        else
          return null;
      }
      set => StructuralProperties["nodes"] = value;
    }

    [JsonIgnore]
    private Dictionary<string, object> StructuralProperties
    {
      get
      {
        if (base.Properties == null)
          base.Properties = new Dictionary<string, object>();

        if (!base.Properties.ContainsKey("structural"))
          base.Properties["structural"] = new Dictionary<string, object>();

        return base.Properties["structural"] as Dictionary<string, object>;

      }
      set
      {
        if (base.Properties == null)
          base.Properties = new Dictionary<string, object>();

        base.Properties["structural"] = value;
      }
    }
  }

  [Serializable]
  public partial class StructuralBridgeAlignmentNode : SpeckleObject, IStructural
  {
    public override string Type { get => "StructuralBridgeAlignmentNode"; }

    [JsonProperty("chainage", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public double Chainage { get; set; }

    [JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
    [JsonProperty("curvature", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public StructuralBridgeCurvature Curvature { get; set; }

    [JsonProperty("radius", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public double Radius { get; set; }
  }

  [Serializable]
  public partial class StructuralBridgePath : SpeckleObject, IStructural
  {
    public override string Type { get => "StructuralBridgePath"; }

    [JsonProperty("alignmentRef", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public string AlignmentRef { get; set; }

    [JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
    [JsonProperty("pathType", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public StructuralBridgePathType PathType { get; set; }

    [JsonProperty("leftOffset", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public double LeftOffset { get; set; }

    [JsonProperty("rightOffset", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public double RightOffset { get; set; }

    [JsonProperty("centreOffset", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public double CentreOffset { get; set; }

    [JsonProperty("gauge", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public double Gauge { get; set; }

    [JsonProperty("leftRailFactor", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public double LeftRailFactor { get; set; }
  }

  [Serializable]
  public partial class StructuralBridgeVehicle : SpeckleObject, IStructural
  {
    public override string Type { get => "StructuralBridgeVehicle"; }

    [JsonProperty("width", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public double Width { get; set; }

    [JsonProperty("axles", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public List<StructuralBridgeVehicleAxle> Axles
    {
      get
      {
        if (StructuralProperties.ContainsKey("axles"))
        { 
          try
          {
            try
            {
              return (List<StructuralBridgeVehicleAxle>)StructuralProperties["axles"];
            }
            catch
            {
              this.Axles = ((List<object>)StructuralProperties["axles"]).Select(x => x as StructuralBridgeVehicleAxle).ToList();
              return this.Axles;
            }
          }
          catch
          { return null; }
        }
        else
          return null;
      }
      set => StructuralProperties["axles"] = value;
    }

    [JsonIgnore]
    private Dictionary<string, object> StructuralProperties
    {
      get
      {
        if (base.Properties == null)
          base.Properties = new Dictionary<string, object>();

        if (!base.Properties.ContainsKey("structural"))
          base.Properties["structural"] = new Dictionary<string, object>();

        return base.Properties["structural"] as Dictionary<string, object>;

      }
      set
      {
        if (base.Properties == null)
          base.Properties = new Dictionary<string, object>();

        base.Properties["structural"] = value;
      }
    }
  }

  [Serializable]
  public partial class StructuralBridgeVehicleAxle : SpeckleObject, IStructural
  {
    public override string Type { get => "StructuralBridgeVehicleAxle"; }

    [JsonProperty("position", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public double Position { get; set; }

    [JsonProperty("wheelOffset", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public double WheelOffset { get; set; }

    [JsonProperty("leftWheelLoad", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public double LeftWheelLoad { get; set; }

    [JsonProperty("rightWheelLoad", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public double RightWheelLoad { get; set; }
  }
}
