using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SpeckleCore;
using SpeckleCoreGeometryClasses;

namespace SpeckleElements
{
  public interface IStructural : ISpeckleInitializer { }

  #region Helper objects
  [Serializable]
  public partial class StructuralVectorThree : SpeckleVector, IStructural
  {
    public override string Type { get => base.Type + "/StructuralVectorThree"; }
    
    /// <summary>Base SpeckleVector.</summary>
    [JsonIgnore]
    public SpeckleVector baseVector
    {
      get => this as SpeckleVector;
      set => this.Value = value.Value;
    }
  }

  [Serializable]
  public partial class StructuralVectorBoolThree : SpeckleObject, IStructural
  {
    public override string Type { get => "StructuralVectorBoolThree"; }
    
    /// <summary>An array containing the X, Y, and Z values of the vector.</summary>
    [JsonProperty("value", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public List<bool> Value { get; set; }
  }

  [Serializable]
  public partial class StructuralVectorSix : SpeckleObject, IStructural
  {
    public override string Type { get => "StructuralVectorSix"; }
    
    /// <summary>An array containing the X, Y, Z, XX, YY, and ZZ values of the vector.</summary>
    [JsonProperty("value", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public List<double> Value { get; set; }
  }

  [Serializable]
  public partial class StructuralVectorBoolSix : SpeckleObject, IStructural
  {
    public override string Type { get => "StructuralVectorBoolSix"; }
    
    /// <summary>An array containing the X, Y, Z, XX, YY, and ZZ values of the vector.</summary>
    [JsonProperty("value", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public List<bool> Value { get; set; }
  }

  [Serializable]
  public partial class StructuralAxis : SpecklePlane, IStructural
  {
    public override string Type { get => base.Type + "/StructuralAxis"; }
    
    /// <summary>Base SpecklePlane.</summary>
    [JsonIgnore]
    public SpecklePlane basePlane
    {
      get => this as SpecklePlane;
      set
      {
        this.Origin = value.Origin;
        this.Normal = value.Normal;
        this.Xdir = value.Xdir;
        this.Ydir = value.Ydir;
      }
    }
  }
  #endregion

  #region Loads
  [Serializable]
  public partial class StructuralLoadCase : SpeckleObject, IStructural
  {
    public override string Type { get => "StructuralLoadCase"; }
    
    /// <summary>Type of load the case contains.</summary>
    [JsonConverter(typeof(StringEnumConverter))]
    [JsonProperty("caseType", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public StructuralLoadCaseType CaseType { get; set; }
  }

  [Serializable]
  public partial class StructuralLoadTask : SpeckleObject, IStructural
  {
    public override string Type { get => "StructuralLoadTask"; }
    
    /// <summary>Type of analysis to perform.</summary>
    [JsonConverter(typeof(StringEnumConverter))]
    [JsonProperty("taskType", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public StructuralLoadTaskType TaskType { get; set; }

    /// <summary>Application IDs of StructuralLoadCase to include.</summary>
    [JsonProperty("loadCaseRefs", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public List<string> LoadCaseRefs { get; set; }

    /// <summary>Load factors for each load case.</summary>
    [JsonProperty("loadFactors", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public List<double> LoadFactors { get; set; }
  }

  [Serializable]
  public partial class StructuralLoadTaskBuckling : SpeckleObject, IStructural
  {
    public override string Type { get => "StructuralLoadTaskBuckling"; }
    
    /// <summary>Type of analysis to perform.</summary>
    [JsonConverter(typeof(StringEnumConverter))]
    [JsonProperty("taskType", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public StructuralLoadTaskType TaskType { get => StructuralLoadTaskType.Buckling; }

    /// <summary>Number of modes.</summary>
    [JsonProperty("numModes", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public int NumModes { get; set; }

    /// <summary>Maximum number of iterations.</summary>
    [JsonProperty("maxNumIterations", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public int MaxNumIterations { get; set; }

    /// <summary>Name of the combination case.</summary>
    [JsonProperty("resultCaseRef", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public string ResultCaseRef { get; set; }

    /// <summary>Stage definition for the task</summary>
    [JsonProperty("stageDefinitionRef", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public string StageDefinitionRef { get; set; }
  }

  [Serializable]
  public partial class StructuralLoadCombo : SpeckleObject, IStructural
  {
    public override string Type { get => "StructuralLoadCombo"; }
    
    /// <summary>Type of combination method.</summary>
    [JsonConverter(typeof(StringEnumConverter))]
    [JsonProperty("comboType", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public StructuralLoadComboType ComboType { get; set; }

    /// <summary>Application IDs of StructuralLoadTask to include.</summary>
    [JsonProperty("loadTaskRefs", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public List<string> LoadTaskRefs { get; set; }

    /// <summary>Load factors for each load task.</summary>
    [JsonProperty("loadTaskFactors", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public List<double> LoadTaskFactors { get; set; }

    /// <summary>Application IDs of StructuralLoadCombo to include.</summary>
    [JsonProperty("loadComboRefs", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public List<string> LoadComboRefs { get; set; }

    /// <summary>Load factors for each load combo.</summary>
    [JsonProperty("loadComboFactors", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public List<double> LoadComboFactors { get; set; }
  }

  [Serializable]
  public partial class Structural0DLoad : SpeckleObject, IStructural
  {
    public override string Type { get => "Structural0DLoad"; }
    
    /// <summary>A list of Fx, Fy, Fz, Mx, My, and Mz loads.</summary>
    [JsonProperty("loading", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public StructuralVectorSix Loading { get; set; }

    /// <summary>Application IDs of StructuralNodes to apply load.</summary>
    [JsonProperty("nodeRefs", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public List<string> NodeRefs { get; set; }

    /// <summary>Application ID of StructuralLoadCase.</summary>
    [JsonProperty("loadCaseRef", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public string LoadCaseRef { get; set; }
  }

  [Serializable]
  public partial class Structural1DLoad : SpeckleObject, IStructural
  {
    public override string Type { get => "Structural1DLoad"; }
    
    /// <summary>A list of Fx, Fy, Fz, Mx, My, and Mz loads.</summary>
    [JsonProperty("loading", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public StructuralVectorSix Loading { get; set; }

    /// <summary>Application IDs of Structural1DElements to apply load.</summary>
    [JsonProperty("elementRefs", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public List<string> ElementRefs { get; set; }

    /// <summary>Application ID of StructuralLoadCase.</summary>
    [JsonProperty("loadCaseRef", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public string LoadCaseRef { get; set; }
  }

  [Serializable]
  public partial class Structural2DLoad : SpeckleObject, IStructural
  {
    public override string Type { get => "Structural2DLoad"; }
    
    /// <summary>A list of Fx, Fy, and Fz loads.</summary>
    [JsonProperty("loading", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public StructuralVectorThree Loading { get; set; }

    /// <summary>Application IDs of Structural2DElementMeshes to apply load.</summary>
    [JsonProperty("elementRefs", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public List<string> ElementRefs { get; set; }

    /// <summary>Application ID of StructuralLoadCase.</summary>
    [JsonProperty("loadCaseRef", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public string LoadCaseRef { get; set; }
  }

  [Serializable]
  public partial class Structural1DLoadLine : SpeckleLine, IStructural
  {
    public override string Type { get => base.Type + "/Structural1DLoadLine"; }

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
    
    /// <summary>Base SpeckleLine.</summary>
    [JsonIgnore]
    public SpeckleLine baseLine
    {
      get => this as SpeckleLine;
      set
      {
        this.Value = value.Value;
        this.Domain = value.Domain;
      }
    }

    /// <summary>A list of Fx, Fy, Fz, Mx, My, and Mz loads.</summary>
    [JsonIgnore]
    public StructuralVectorSix Loading
    {
      get => StructuralProperties.ContainsKey("loading") ? (StructuralProperties["loading"] as StructuralVectorSix) : null;
      set => StructuralProperties["loading"] = value;
    }

    /// <summary>Application ID of StructuralLoadCase.</summary>
    [JsonIgnore]
    public string LoadCaseRef
    {
      get => StructuralProperties.ContainsKey("loadCaseRef") ? (StructuralProperties["loadCaseRef"] as string) : null;
      set => StructuralProperties["loadCaseRef"] = value;
    }
  }

  [Serializable]
  public partial class Structural2DLoadPanel : SpecklePolyline, IStructural
  {
    public override string Type { get => base.Type + "/Structural2DLoadPanel"; }

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
    
    /// <summary>Base SpecklePolyline.</summary>
    [JsonIgnore]
    public SpecklePolyline basePolyline
    {
      get => this as SpecklePolyline;
      set
      {
        this.Value = value.Value;
        this.Closed = value.Closed;
        this.Domain = value.Domain;
      }
    }

    /// <summary>A list of Fx, Fy, and Fz loads.</summary>
    [JsonIgnore]
    public StructuralVectorThree Loading
    {
      get => StructuralProperties.ContainsKey("loading") ? (StructuralProperties["loading"] as StructuralVectorThree) : null;
      set => StructuralProperties["loading"] = value;
    }

    /// <summary>Application ID of StructuralLoadCase.</summary>
    [JsonIgnore]
    public string LoadCaseRef
    {
      get => StructuralProperties.ContainsKey("loadCaseRef") ? (StructuralProperties["loadCaseRef"] as string) : null;
      set => StructuralProperties["loadCaseRef"] = value;
    }
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
  public partial class StructuralStageDefinition : SpeckleObject, IStructural
  {
    public override string Type { get => "StructuralStageDefinition"; }
    
    /// <summary>Application ID of members to include in the stage of the construction sequence.</summary>
    [JsonProperty("elementRefs", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public List<string> ElementRefs { get; set; }

    /// <summary>Number of days in the stage</summary>
    [JsonProperty("stageDays", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public int StageDays { get; set; }
  }

  [Serializable]
  public partial class StructuralGravityLoading : SpeckleObject, IStructural
  {
    public override string Type { get => "StructuralGravityLoading"; }
    
    /// <summary>A list of x, y, z factors</summary>
    [JsonProperty("gravityFactors", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public StructuralVectorThree GravityFactors { get; set; }

    /// <summary>Application ID of StructuralLoadCase.</summary>
    [JsonProperty("loadCaseRef", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public string LoadCaseRef { get; set; }
  }

  #endregion

  #region Properties
  [Serializable]
  public partial class StructuralLinearSpringProperty : SpeckleObject, IStructural
  {
    public override string Type { get => base.Type + "/StructuralLinearSpringProperty"; }

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
    
    /// <summary>Axis of the spring.</summary>
    [JsonIgnore]
    public StructuralSpringAxis Axis
    {
      get => StructuralProperties.ContainsKey("axis")
        ? (StructuralSpringAxis)Enum.Parse(typeof(StructuralSpringAxis), (StructuralProperties["axis"] as string), true) 
        : StructuralSpringAxis.Global;
      set => StructuralProperties["axis"] = value.ToString();
    }

    /// <summary>X, Y, Z, XX, YY, ZZ stiffnesses.</summary>
    [JsonIgnore]
    public StructuralVectorSix Stiffness
    {
      get => StructuralProperties.ContainsKey("stiffness") ? (StructuralProperties["stiffness"] as StructuralVectorSix) : null;
      set => StructuralProperties["stiffness"] = value;
    }
  }

  [Serializable]
  public partial class StructuralMaterialConcrete : SpeckleObject, IStructural
  {
    public override string Type { get => "StructuralMaterialConcrete"; }
    
    /// <summary>Young's modulus (E) of material.</summary>
    [JsonProperty("youngsModulus", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public double YoungsModulus { get; set; }

    /// <summary>Shear modulus (G) of material.</summary>
    [JsonProperty("shearModulus", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public double ShearModulus { get; set; }

    /// <summary>Poission's ratio (ν) of material.</summary>
    [JsonProperty("poissonsRatio", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public double PoissonsRatio { get; set; }

    /// <summary>Density (ρ) of material.</summary>
    [JsonProperty("density", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public double Density { get; set; }

    /// <summary>Coefficient of thermal expansion (α) of material.</summary>
    [JsonProperty("coeffThermalExpansion", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public double CoeffThermalExpansion { get; set; }

    /// <summary>Compressive strength.</summary>
    [JsonProperty("compressiveStrength", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public double CompressiveStrength { get; set; }

    /// <summary>Max strain at failure.</summary>
    [JsonProperty("maxStrain", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public double MaxStrain { get; set; }

    /// <summary>Aggragate size.</summary>
    [JsonProperty("aggragateSize", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public double AggragateSize { get; set; }
  }

  [Serializable]
  public partial class StructuralMaterialSteel : SpeckleObject, IStructural
  {
    public override string Type { get => "StructuralMaterialSteel"; }
    
    /// <summary>Young's modulus (E) of material.</summary>
    [JsonProperty("youngsModulus", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public double YoungsModulus { get; set; }

    /// <summary>Shear modulus (G) of material.</summary>
    [JsonProperty("shearModulus", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public double ShearModulus { get; set; }

    /// <summary>Poission's ratio (ν) of material.</summary>
    [JsonProperty("poissonsRatio", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public double PoissonsRatio { get; set; }

    /// <summary>Density (ρ) of material.</summary>
    [JsonProperty("density", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public double Density { get; set; }

    /// <summary>Coefficient of thermal expansion (α) of material.</summary>
    [JsonProperty("coeffThermalExpansion", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public double CoeffThermalExpansion { get; set; }

    /// <summary>Yield strength.</summary>
    [JsonProperty("yieldStrength", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public double YieldStrength { get; set; }

    /// <summary>Ultimate strength.</summary>
    [JsonProperty("ultimateStrength", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public double UltimateStrength { get; set; }

    /// <summary>Max strain at failure.</summary>
    [JsonProperty("maxStrain", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public double MaxStrain { get; set; }
  }

  [Serializable]
  public partial class Structural1DProperty : SpeckleObject, IStructural
  {
    public override string Type { get => "Structural1DProperty"; }
    
    /// <summary>SpecklePolyline or SpeckleCircle of the cross-section.</summary>
    [JsonProperty("profile", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public SpeckleObject Profile { get; set; }

    /// <summary>Cross-section shape.</summary>
    [JsonConverter(typeof(StringEnumConverter))]
    [JsonProperty("shape", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public Structural1DPropertyShape Shape { get; set; }

    /// <summary>Is the section filled or hollow?</summary>
    [JsonProperty("hollow", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public bool Hollow { get; set; }

    /// <summary>Thickness of the section if hollow.</summary>
    [JsonProperty("thickness", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public double Thickness { get; set; }

    /// <summary>Application ID of StructuralMaterial.</summary>
    [JsonProperty("materialRef", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public string MaterialRef { get; set; }
  }

  [Serializable]
  public partial class Structural2DProperty : SpeckleObject, IStructural
  {
    public override string Type { get => "Structural2DProperty"; }
    
    /// <summary>Thickness of the 2D element.</summary>
    [JsonProperty("thickness", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public double Thickness { get; set; }

    /// <summary>Application ID of StructuralMaterial.</summary>
    [JsonProperty("materialRef", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public string MaterialRef { get; set; }

    /// <summary>Reference surface for property.</summary>
    [JsonProperty("referenceSurface", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public Structural2DPropertyReferenceSurface ReferenceSurface { get; set; }
  }
  #endregion

  #region Nodes and Elements
  [Serializable]
  public partial class StructuralNode : SpecklePoint, IStructural
  {
    public override string Type { get => base.Type + "/StructuralNode"; }

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
    
    /// <summary>Base SpecklePoint.</summary>
    [JsonIgnore]
    public SpecklePoint basePoint
    {
      get => this as SpecklePoint;
      set => this.Value = value.Value;
    }

    /// <summary>Local axis of the node.</summary>
    [JsonIgnore]
    public StructuralAxis Axis
    {
      get => StructuralProperties.ContainsKey("axis") ? (StructuralProperties["axis"] as StructuralAxis) : null;
      set => StructuralProperties["axis"] = value;
    }

    /// <summary>A list of the X, Y, Z, Rx, Ry, and Rz restraints.</summary>
    [JsonIgnore]
    public StructuralVectorBoolSix Restraint
    {
      get => StructuralProperties.ContainsKey("restraint") ? (StructuralProperties["restraint"] as StructuralVectorBoolSix) : null;
      set => StructuralProperties["restraint"] = value;
    }

    /// <summary>A list of the X, Y, Z, Rx, Ry, and Rz stiffnesses.</summary>
    [JsonIgnore]
    public StructuralVectorSix Stiffness
    {
      get => StructuralProperties.ContainsKey("stiffness") ? (StructuralProperties["stiffness"] as StructuralVectorSix) : null;
      set => StructuralProperties["stiffness"] = value;
    }

    /// <summary>Mass of the node.</summary>
    [JsonIgnore]
    public double Mass
    {
      get => StructuralProperties.ContainsKey("mass") ? ((double)StructuralProperties["mass"]) : 0;
      set => StructuralProperties["mass"] = value;
    }

    /// <summary>Analysis results.</summary>
    [JsonIgnore]
    public Dictionary<string, object> Result
    {
      get => StructuralProperties.ContainsKey("result") ? (StructuralProperties["result"] as Dictionary<string, object>) : null;
      set => StructuralProperties["result"] = value;
    }
  }

  [Serializable]
  public partial class Structural1DElement : SpeckleLine, IStructural
  {
    public override string Type { get => base.Type + "/Structural1DElement"; }

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
    
    /// <summary>Base SpeckleLine.</summary>
    [JsonIgnore]
    public SpeckleLine baseLine
    {
      get => this as SpeckleLine;
      set => this.Value = value.Value;
    }

    /// <summary>Type of 1D element.</summary>
    [JsonIgnore]
    public Structural1DElementType ElementType
    {
      get => StructuralProperties.ContainsKey("elementType") ? (Structural1DElementType)Enum.Parse(typeof(Structural1DElementType), (StructuralProperties["elementType"] as string), true) : Structural1DElementType.Generic;
      set => StructuralProperties["elementType"] = value.ToString();
    }

    /// <summary>Application ID of Structural1DProperty.</summary>
    [JsonIgnore]
    public string PropertyRef
    {
      get => StructuralProperties.ContainsKey("propertyRef") ? (StructuralProperties["propertyRef"] as string) : null;
      set => StructuralProperties["propertyRef"] = value;
    }

    /// <summary>Local axis of 1D element.</summary>
    [JsonIgnore]
    public StructuralVectorThree ZAxis
    {
      get => StructuralProperties.ContainsKey("zAxis") ? (StructuralProperties["zAxis"] as StructuralVectorThree) : null;
      set => StructuralProperties["zAxis"] = value;
    }

    /// <summary>List of X, Y, Z, Rx, Ry, and Rz releases on each end.</summary>
    [JsonIgnore]
    public List<StructuralVectorBoolSix> EndRelease
    {
      get
      {
        if (StructuralProperties.ContainsKey("endRelease"))
        {
          try
          {
            try
            {
              return (List<StructuralVectorBoolSix>)StructuralProperties["endRelease"];
            }
            catch
            {
              return ((List<object>)StructuralProperties["endRelease"]).Select(x => x as StructuralVectorBoolSix).ToList();
            }
          }
          catch
          { return null; }
        }
        else
          return null;
      }
      set => StructuralProperties["endRelease"] = value;
    }

    /// <summary>List of X, Y, and, Z offsets on each end.</summary>
    [JsonIgnore]
    public List<StructuralVectorThree> Offset
    {
      get
      {
        if (StructuralProperties.ContainsKey("offset"))
        {
          try
          {
            try
            {
              return (List<StructuralVectorThree>)StructuralProperties["offset"];
            }
            catch
            {
              return ((List<object>)StructuralProperties["offset"]).Select(x => x as StructuralVectorThree).ToList();
            }
          }
          catch
          { return null; }
        }
        else
          return null;
      }
      set => StructuralProperties["offset"] = value;
    }

    /// <summary>GSA target mesh size.</summary>
    [JsonIgnore]
    public double MeshSize
    {
      get => StructuralProperties.ContainsKey("gsaMeshSize") ? ((double)StructuralProperties["gsaMeshSize"]) : 0;
      set => StructuralProperties["gsaMeshSize"] = value;
    }

    /// <summary>GSA dummy status.</summary>
    [JsonIgnore]
    public bool Dummy
    {
      get => StructuralProperties.ContainsKey("gsaDummy") ? ((bool)StructuralProperties["gsaDummy"]) : false;
      set => StructuralProperties["gsaDummy"] = value;
    }

    /// <summary>Vertex location of results.</summary>
    [JsonIgnore]
    public List<double> ResultVertices
    {
      get
      {
        if (StructuralProperties.ContainsKey("resultVertices"))
        {
          try
          {
            try
            {
              return (List<double>)StructuralProperties["resultVertices"];
            }
            catch
            {
              this.ResultVertices = ((List<object>)StructuralProperties["resultVertices"]).Select(x => Convert.ToDouble(x)).ToList();
              return this.ResultVertices;
            }
          }
          catch
          { return null; }
        }
        else
          return null;
      }
      set => StructuralProperties["resultVertices"] = value;
    }

    /// <summary>Analysis results.</summary>
    [JsonIgnore]
    public Dictionary<string, object> Result
    {
      get => StructuralProperties.ContainsKey("result") ? (StructuralProperties["result"] as Dictionary<string, object>) : null;
      set => StructuralProperties["result"] = value;
    }
  }

  [Serializable]
  public partial class Structural1DElementPolyline : SpecklePolyline, IStructural
  {
    public override string Type { get => base.Type + "/Structural1DElementPolyline"; }

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
    
    /// <summary>Application ID of elements to reference from other objects.</summary>
    [JsonIgnore]
    public List<string> ElementApplicationId
    {
      get
      {
        if (StructuralProperties.ContainsKey("elementApplicationId"))
        {
          try
          {
            try
            {
              return (List<string>)StructuralProperties["elementApplicationId"];
            }
            catch
            {
              this.ElementApplicationId = ((List<object>)StructuralProperties["elementApplicationId"]).Select(x => Convert.ToString(x)).ToList();
              return this.ElementApplicationId;
            }
          }
          catch
          { return null; }
        }
        else
          return null;
      }
      set => StructuralProperties["elementApplicationId"] = value;
    }

    /// <summary>Base SpecklePolyline.</summary>
    [JsonIgnore]
    public SpecklePolyline basePolyline
    {
      get => this as SpecklePolyline;
      set
      {
        this.Value = value.Value;
        this.Closed = value.Closed;
        this.Domain = value.Domain;
      }
    }

    /// <summary>Type of 1D element.</summary>
    [JsonIgnore]
    public Structural1DElementType ElementType
    {
      get => StructuralProperties.ContainsKey("elementType") ? (Structural1DElementType)Enum.Parse(typeof(Structural1DElementType), (StructuralProperties["elementType"] as string), true) : Structural1DElementType.Generic;
      set => StructuralProperties["elementType"] = value.ToString();
    }

    /// <summary>Application ID of Structural1DProperty.</summary>
    [JsonIgnore]
    public string PropertyRef
    {
      get => StructuralProperties.ContainsKey("propertyRef") ? (StructuralProperties["propertyRef"] as string) : null;
      set => StructuralProperties["propertyRef"] = value;
    }

    /// <summary>Local Z axis of 1D elements.</summary>
    [JsonIgnore]
    public List<StructuralVectorThree> ZAxis
    {
      get
      {
        if (StructuralProperties.ContainsKey("zAxis"))
        {
          try
          {
            try
            {
              return (List<StructuralVectorThree>)StructuralProperties["zAxis"];
            }
            catch
            {
              this.ZAxis = ((List<object>)StructuralProperties["zAxis"]).Select(x => x as StructuralVectorThree).ToList();
              return this.ZAxis;
            }
          }
          catch
          { return null; }
        }
        else
          return null;
      }
      set => StructuralProperties["zAxis"] = value;
    }

    /// <summary>List of X, Y, Z, Rx, Ry, and Rz releases of each node.</summary>
    [JsonIgnore]
    public List<StructuralVectorBoolSix> EndRelease
    {
      get
      {
        if (StructuralProperties.ContainsKey("endRelease"))
        {
          try
          {
            try
            {
              return (List<StructuralVectorBoolSix>)StructuralProperties["endRelease"];
            }
            catch
            {
              this.EndRelease = ((List<object>)StructuralProperties["endRelease"]).Select(x => x as StructuralVectorBoolSix).ToList();
              return this.EndRelease;
            }
          }
          catch
          { return null; }
        }
        else
          return null;
      }
      set => StructuralProperties["endRelease"] = value;
    }

    /// <summary>List of X, Y, Z, Rx, Ry, and Rz offsets of each node.</summary>
    [JsonIgnore]
    public List<StructuralVectorThree> Offset
    {
      get
      {
        if (StructuralProperties.ContainsKey("offset"))
        {
          try
          {
            try
            {
              return (List<StructuralVectorThree>)StructuralProperties["offset"];
            }
            catch
            {
              this.Offset = ((List<object>)StructuralProperties["offset"]).Select(x => x as StructuralVectorThree).ToList();
              return this.Offset;
            }
          }
          catch
          { return null; }
        }
        else
          return null;
      }
      set => StructuralProperties["offset"] = value;
    }

    /// <summary>GSA target mesh size.</summary>
    [JsonIgnore]
    public double MeshSize
    {
      get => StructuralProperties.ContainsKey("gsaMeshSize") ? ((double)StructuralProperties["gsaMeshSize"]) : 0;
      set => StructuralProperties["gsaMeshSize"] = value;
    }

    /// <summary>GSA dummy status.</summary>
    [JsonIgnore]
    public bool Dummy
    {
      get => StructuralProperties.ContainsKey("gsaDummy") ? ((bool)StructuralProperties["gsaDummy"]) : false;
      set => StructuralProperties["gsaDummy"] = value;
    }

    /// <summary>Vertex location of results.</summary>
    [JsonIgnore]
    public List<double> ResultVertices
    {
      get
      {
        if (StructuralProperties.ContainsKey("resultVertices"))
        {
          try
          {
            try
            {
              return (List<double>)StructuralProperties["resultVertices"];
            }
            catch
            {
              this.ResultVertices = ((List<object>)StructuralProperties["resultVertices"]).Select(x => Convert.ToDouble(x)).ToList();
              return this.ResultVertices;
            }
          }
          catch
          { return null; }
        }
        else
          return null;
      }
      set => StructuralProperties["resultVertices"] = value;
    }

    /// <summary>Analysis results.</summary>
    [JsonIgnore]
    public Dictionary<string, object> Result
    {
      get => StructuralProperties.ContainsKey("result") ? (StructuralProperties["result"] as Dictionary<string, object>) : null;
      set => StructuralProperties["result"] = value;
    }
  }

  [Serializable]
  public partial class Structural2DElement : SpeckleMesh, IStructural
  {
    public override string Type { get => base.Type + "/Structural2DElement"; }

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
    
    /// <summary>Base SpeckleMesh.</summary>
    [JsonIgnore]
    public SpeckleMesh baseMesh
    {
      get => this as SpeckleMesh;
      set
      {
        this.Vertices = value.Vertices;
        this.Faces = value.Faces;
        this.Colors = value.Colors;
        this.TextureCoordinates = value.TextureCoordinates;
      }
    }

    /// <summary>Type of 2D element.</summary>
    [JsonIgnore]
    public Structural2DElementType ElementType
    {
      get => StructuralProperties.ContainsKey("elementType") ? (Structural2DElementType)Enum.Parse(typeof(Structural2DElementType), (StructuralProperties["elementType"] as string), true) : Structural2DElementType.Generic;
      set => StructuralProperties["elementType"] = value.ToString();
    }

    /// <summary>Application ID of Structural2DProperty.</summary>
    [JsonIgnore]
    public string PropertyRef
    {
      get => StructuralProperties.ContainsKey("propertyRef") ? (StructuralProperties["propertyRef"] as string) : null;
      set => StructuralProperties["propertyRef"] = value;
    }

    /// <summary>Local axis of 2D element.</summary>
    [JsonIgnore]
    public StructuralAxis Axis
    {
      get => StructuralProperties.ContainsKey("axis") ? (StructuralProperties["axis"] as StructuralAxis) : null;
      set => StructuralProperties["axis"] = value;
    }

    /// <summary>Offset of 2D element.</summary>
    [JsonIgnore]
    public double Offset
    {
      get => StructuralProperties.ContainsKey("offset") ? ((double)StructuralProperties["offset"]) : 0;
      set => StructuralProperties["offset"] = value;
    }

    /// <summary>GSA target mesh size.</summary>
    [JsonIgnore]
    public double MeshSize
    {
      get => StructuralProperties.ContainsKey("gsaMeshSize") ? ((double)StructuralProperties["gsaMeshSize"]) : 0;
      set => StructuralProperties["gsaMeshSize"] = value;
    }

    /// <summary>GSA dummy status.</summary>
    [JsonIgnore]
    public bool Dummy
    {
      get => StructuralProperties.ContainsKey("gsaDummy") ? ((bool)StructuralProperties["gsaDummy"]) : false;
      set => StructuralProperties["gsaDummy"] = value;
    }

    /// <summary>Analysis results.</summary>
    [JsonIgnore]
    public Dictionary<string, object> Result
    {
      get => StructuralProperties.ContainsKey("result") ? (StructuralProperties["result"] as Dictionary<string, object>) : null;
      set => StructuralProperties["result"] = value;
    }
  }

  [Serializable]
  public partial class Structural2DElementMesh : SpeckleMesh, IStructural
  {
    public override string Type { get => base.Type + "/Structural2DElementMesh"; }

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
    
    /// <summary>Application ID of elements to reference from other objects.</summary>
    [JsonIgnore]
    public List<string> ElementApplicationId
    {
      get
      {
        if (StructuralProperties.ContainsKey("elementApplicationId"))
        {
          try
          {
            try
            {
              return (List<string>)StructuralProperties["elementApplicationId"];
            }
            catch
            {
              this.ElementApplicationId = ((List<object>)StructuralProperties["elementApplicationId"]).Select(x => Convert.ToString(x)).ToList();
              return this.ElementApplicationId;
            }
          }
          catch
          { return null; }
        }
        else
          return null;
      }
      set => StructuralProperties["elementApplicationId"] = value;
    }

    /// <summary>Base SpeckleMesh.</summary>
    [JsonIgnore]
    public SpeckleMesh baseMesh
    {
      get => this as SpeckleMesh;
      set
      {
        this.Vertices = value.Vertices;
        this.Faces = value.Faces;
        this.Colors = value.Colors;
        this.TextureCoordinates = value.TextureCoordinates;
      }
    }

    /// <summary>Type of 2D element.</summary>
    [JsonIgnore]
    public Structural2DElementType ElementType
    {
      get => StructuralProperties.ContainsKey("elementType") ? (Structural2DElementType)Enum.Parse(typeof(Structural2DElementType), (StructuralProperties["elementType"] as string), true) : Structural2DElementType.Generic;
      set => StructuralProperties["elementType"] = value.ToString();
    }

    /// <summary>Application ID of Structural2DProperty.</summary>
    [JsonIgnore]
    public string PropertyRef
    {
      get => StructuralProperties.ContainsKey("propertyRef") ? (StructuralProperties["propertyRef"] as string) : null;
      set => StructuralProperties["propertyRef"] = value;
    }

    /// <summary>Local axis of each 2D element.</summary>
    [JsonIgnore]
    public List<StructuralAxis> Axis
    {
      get
      {
        if (StructuralProperties.ContainsKey("axis"))
        {
          try
          {
            try
            {
              return (List<StructuralAxis>)StructuralProperties["axis"];
            }
            catch
            {
              return ((List<object>)StructuralProperties["axis"]).Select(x => x as StructuralAxis).ToList();
            }
          }
          catch
          { return null; }
        }
        else
          return null;
      }
      set => StructuralProperties["axis"] = value;
    }

    /// <summary>Offset of easch 2D element.</summary>
    [JsonIgnore]
    public List<double> Offset
    {
      get
      {
        if (StructuralProperties.ContainsKey("offset"))
        {
          try
          {
            try
            {
              return (List<double>)StructuralProperties["offset"];
            }
            catch
            {
              this.Offset = ((List<object>)StructuralProperties["offset"]).Select(x => Convert.ToDouble(x)).ToList();
              return this.Offset;
            }
          }
          catch
          { return null; }
        }
        else
          return null;
      }
      set => StructuralProperties["offset"] = value;
    }

    /// <summary>GSA target mesh size.</summary>
    [JsonIgnore]
    public double MeshSize
    {
      get => StructuralProperties.ContainsKey("gsaMeshSize") ? ((double)StructuralProperties["gsaMeshSize"]) : 0;
      set => StructuralProperties["gsaMeshSize"] = value;
    }

    /// <summary>GSA dummy status.</summary>
    [JsonIgnore]
    public bool Dummy
    {
      get => StructuralProperties.ContainsKey("gsaDummy") ? ((bool)StructuralProperties["gsaDummy"]) : false;
      set => StructuralProperties["gsaDummy"] = value;
    }

    /// <summary>Analysis results.</summary>
    [JsonIgnore]
    public Dictionary<string, object> Result
    {
      get => StructuralProperties.ContainsKey("result") ? (StructuralProperties["result"] as Dictionary<string, object>) : null;
      set => StructuralProperties["result"] = value;
    }
  }

  [Serializable]
  public partial class Structural2DVoid : SpeckleMesh, IStructural
  {
    public override string Type { get => base.Type + "/Structural2DVoid"; }

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
    
    /// <summary>Base SpeckleMesh.</summary>
    [JsonIgnore]
    public SpeckleMesh baseMesh
    {
      get => this as SpeckleMesh;
      set
      {
        this.Vertices = value.Vertices;
        this.Faces = value.Faces;
        this.Colors = value.Colors;
        this.TextureCoordinates = value.TextureCoordinates;
      }
    }
  }
  #endregion

  #region Results
  [Serializable]
  public partial class StructuralNodeResult : SpeckleObject, IStructural
  {
    public override string Type { get => "StructuralNodeResult"; }
    
    /// <summary>ApplicationID of object referred to.</summary>
    [JsonProperty("targetRef", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public string TargetRef { get; set; }

    /// <summary>Indicates whether the results are in the global or local axis.</summary>
    [JsonProperty("isGlobal", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public bool IsGlobal { get; set; }

    /// <summary>Results.</summary>
    [JsonProperty("value", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    [Newtonsoft.Json.JsonConverter(typeof(SpecklePropertiesConverter))]
    public Dictionary<string, object> Value { get; set; }
  }

  [Serializable]
  public partial class Structural1DElementResult : SpeckleObject, IStructural
  {
    public override string Type { get => "Structural1DElementResult"; }

    /// <summary>ApplicationID of object referred to.</summary>
    [JsonProperty("targetRef", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public string TargetRef { get; set; }

    /// <summary>Indicates whether the results are in the global or local axis.</summary>
    [JsonProperty("isGlobal", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public bool IsGlobal { get; set; }

    /// <summary>Results.</summary>
    [JsonProperty("value", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    [Newtonsoft.Json.JsonConverter(typeof(SpecklePropertiesConverter))]
    public Dictionary<string, object> Value { get; set; }
  }

  [Serializable]
  public partial class Structural2DElementResult : SpeckleObject, IStructural
  {
    public override string Type { get => "Structural2DElementResult"; }

    /// <summary>ApplicationID of object referred to.</summary>
    [JsonProperty("targetRef", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public string TargetRef { get; set; }

    /// <summary>Indicates whether the results are in the global or local axis.</summary>
    [JsonProperty("isGlobal", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public bool IsGlobal { get; set; }

    /// <summary>Results.</summary>
    [JsonProperty("value", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    [Newtonsoft.Json.JsonConverter(typeof(SpecklePropertiesConverter))]
    public Dictionary<string, object> Value { get; set; }
  }

  [Serializable]
  public partial class StructuralMiscResult : SpeckleObject, IStructural
  {
    public override string Type { get => "StructuralMiscResult"; }
    
    /// <summary>Description of result.</summary>
    [JsonProperty("description", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public string Description { get; set; }

    /// <summary>ApplicationID of object referred to.</summary>
    [JsonProperty("targetRef", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public string TargetRef { get; set; }

    /// <summary>Indicates whether the results are in the global or local axis.</summary>
    [JsonProperty("isGlobal", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public bool IsGlobal { get; set; }

    /// <summary>Results.</summary>
    [JsonProperty("value", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    [Newtonsoft.Json.JsonConverter(typeof(SpecklePropertiesConverter))]
    public Dictionary<string, object> Value { get; set; }
  }
  #endregion
}
