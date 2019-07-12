extern alias SpeckleNewtonsoft;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpeckleCore;
using SpeckleCoreGeometryClasses;
using SNJ = SpeckleNewtonsoft.Newtonsoft.Json;

namespace SpeckleElements
{
  public enum StructuralLoadCaseType
  {
    Generic,
    Dead,
    Soil,
    Live,
    Rain,
    Snow,
    Wind,
    Earthquake,
    Thermal
  }

  public enum StructuralLoadTaskType
  {
    LinearStatic,
    NonlinearStatic,
    Modal,
    Buckling
  }

  public enum StructuralLoadComboType
  {
    Envelope,
    LinearAdd
  }

  [Serializable]
  public partial class StructuralLoadCase : SpeckleObject, IStructural
  {
    public override string Type { get => "StructuralLoadCase"; }

    /// <summary>Type of load the case contains.</summary>
    [SNJ.JsonConverter(typeof(SNJ.Converters.StringEnumConverter))]
    [SNJ.JsonProperty("caseType", Required = SNJ.Required.Default, NullValueHandling = SNJ.NullValueHandling.Ignore)]
    public StructuralLoadCaseType CaseType { get; set; }
  }

  [Serializable]
  public partial class StructuralLoadTask : SpeckleObject, IStructural
  {
    public override string Type { get => "StructuralLoadTask"; }

    /// <summary>Type of analysis to perform.</summary>
    [SNJ.JsonConverter(typeof(SNJ.Converters.StringEnumConverter))]
    [SNJ.JsonProperty("taskType", Required = SNJ.Required.Default, NullValueHandling = SNJ.NullValueHandling.Ignore)]
    public StructuralLoadTaskType TaskType { get; set; }

    /// <summary>Application IDs of StructuralLoadCase to include.</summary>
    [SNJ.JsonProperty("loadCaseRefs", Required = SNJ.Required.Default, NullValueHandling = SNJ.NullValueHandling.Ignore)]
    public List<string> LoadCaseRefs { get; set; }

    /// <summary>Load factors for each load case.</summary>
    [SNJ.JsonProperty("loadFactors", Required = SNJ.Required.Default, NullValueHandling = SNJ.NullValueHandling.Ignore)]
    public List<double> LoadFactors { get; set; }
  }

  [Serializable]
  public partial class StructuralLoadTaskBuckling : SpeckleObject, IStructural
  {
    public override string Type { get => "StructuralLoadTaskBuckling"; }

    /// <summary>Type of analysis to perform.</summary>
    [SNJ.JsonConverter(typeof(SNJ.Converters.StringEnumConverter))]
    [SNJ.JsonProperty("taskType", Required = SNJ.Required.Default, NullValueHandling = SNJ.NullValueHandling.Ignore)]
    public StructuralLoadTaskType TaskType { get => StructuralLoadTaskType.Buckling; }

    /// <summary>Number of modes.</summary>
    [SNJ.JsonProperty("numModes", Required = SNJ.Required.Default, NullValueHandling = SNJ.NullValueHandling.Ignore)]
    public int NumModes { get; set; }

    /// <summary>Maximum number of iterations.</summary>
    [SNJ.JsonProperty("maxNumIterations", Required = SNJ.Required.Default, NullValueHandling = SNJ.NullValueHandling.Ignore)]
    public int MaxNumIterations { get; set; }

    /// <summary>Name of the combination case.</summary>
    [SNJ.JsonProperty("resultCaseRef", Required = SNJ.Required.Default, NullValueHandling = SNJ.NullValueHandling.Ignore)]
    public string ResultCaseRef { get; set; }

    /// <summary>Stage definition for the task</summary>
    [SNJ.JsonProperty("stageDefinitionRef", Required = SNJ.Required.Default, NullValueHandling = SNJ.NullValueHandling.Ignore)]
    public string StageDefinitionRef { get; set; }
  }

  [Serializable]
  public partial class StructuralLoadCombo : SpeckleObject, IStructural
  {
    public override string Type { get => "StructuralLoadCombo"; }

    /// <summary>Type of combination method.</summary>
    [SNJ.JsonConverter(typeof(SNJ.Converters.StringEnumConverter))]
    [SNJ.JsonProperty("comboType", Required = SNJ.Required.Default, NullValueHandling = SNJ.NullValueHandling.Ignore)]
    public StructuralLoadComboType ComboType { get; set; }

    /// <summary>Application IDs of StructuralLoadTask to include.</summary>
    [SNJ.JsonProperty("loadTaskRefs", Required = SNJ.Required.Default, NullValueHandling = SNJ.NullValueHandling.Ignore)]
    public List<string> LoadTaskRefs { get; set; }

    /// <summary>Load factors for each load task.</summary>
    [SNJ.JsonProperty("loadTaskFactors", Required = SNJ.Required.Default, NullValueHandling = SNJ.NullValueHandling.Ignore)]
    public List<double> LoadTaskFactors { get; set; }

    /// <summary>Application IDs of StructuralLoadCombo to include.</summary>
    [SNJ.JsonProperty("loadComboRefs", Required = SNJ.Required.Default, NullValueHandling = SNJ.NullValueHandling.Ignore)]
    public List<string> LoadComboRefs { get; set; }

    /// <summary>Load factors for each load combo.</summary>
    [SNJ.JsonProperty("loadComboFactors", Required = SNJ.Required.Default, NullValueHandling = SNJ.NullValueHandling.Ignore)]
    public List<double> LoadComboFactors { get; set; }
  }

  [Serializable]
  public partial class StructuralGravityLoading : SpeckleObject, IStructural
  {
    public override string Type { get => "StructuralGravityLoading"; }

    /// <summary>A list of x, y, z factors</summary>
    [SNJ.JsonProperty("gravityFactors", Required = SNJ.Required.Default, NullValueHandling = SNJ.NullValueHandling.Ignore)]
    public StructuralVectorThree GravityFactors { get; set; }

    /// <summary>Application ID of StructuralLoadCase.</summary>
    [SNJ.JsonProperty("loadCaseRef", Required = SNJ.Required.Default, NullValueHandling = SNJ.NullValueHandling.Ignore)]
    public string LoadCaseRef { get; set; }
  }

  [Serializable]
  public partial class Structural0DLoad : SpeckleObject, IStructural
  {
    public override string Type { get => "Structural0DLoad"; }

    /// <summary>A list of Fx, Fy, Fz, Mx, My, and Mz loads.</summary>
    [SNJ.JsonProperty("loading", Required = SNJ.Required.Default, NullValueHandling = SNJ.NullValueHandling.Ignore)]
    public StructuralVectorSix Loading { get; set; }

    /// <summary>Application IDs of StructuralNodes to apply load.</summary>
    [SNJ.JsonProperty("nodeRefs", Required = SNJ.Required.Default, NullValueHandling = SNJ.NullValueHandling.Ignore)]
    public List<string> NodeRefs { get; set; }

    /// <summary>Application ID of StructuralLoadCase.</summary>
    [SNJ.JsonProperty("loadCaseRef", Required = SNJ.Required.Default, NullValueHandling = SNJ.NullValueHandling.Ignore)]
    public string LoadCaseRef { get; set; }
  }

  [Serializable]
  public partial class Structural1DLoad : SpeckleObject, IStructural
  {
    public override string Type { get => "Structural1DLoad"; }

    /// <summary>A list of Fx, Fy, Fz, Mx, My, and Mz loads.</summary>
    [SNJ.JsonProperty("loading", Required = SNJ.Required.Default, NullValueHandling = SNJ.NullValueHandling.Ignore)]
    public StructuralVectorSix Loading { get; set; }

    /// <summary>Application IDs of Structural1DElements to apply load.</summary>
    [SNJ.JsonProperty("elementRefs", Required = SNJ.Required.Default, NullValueHandling = SNJ.NullValueHandling.Ignore)]
    public List<string> ElementRefs { get; set; }

    /// <summary>Application ID of StructuralLoadCase.</summary>
    [SNJ.JsonProperty("loadCaseRef", Required = SNJ.Required.Default, NullValueHandling = SNJ.NullValueHandling.Ignore)]
    public string LoadCaseRef { get; set; }
  }

  [Serializable]
  public partial class Structural1DLoadLine : SpeckleLine, IStructural
  {
    public override string Type { get => base.Type + "/Structural1DLoadLine"; }

    [SNJ.JsonIgnore]
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
    [SNJ.JsonIgnore]
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
    [SNJ.JsonIgnore]
    public StructuralVectorSix Loading
    {
      get => StructuralProperties.ContainsKey("loading") ? (StructuralProperties["loading"] as StructuralVectorSix) : null;
      set => StructuralProperties["loading"] = value;
    }

    /// <summary>Application ID of StructuralLoadCase.</summary>
    [SNJ.JsonIgnore]
    public string LoadCaseRef
    {
      get => StructuralProperties.ContainsKey("loadCaseRef") ? (StructuralProperties["loadCaseRef"] as string) : null;
      set => StructuralProperties["loadCaseRef"] = value;
    }
  }

  [Serializable]
  public partial class Structural2DLoad : SpeckleObject, IStructural
  {
    public override string Type { get => "Structural2DLoad"; }

    /// <summary>A list of Fx, Fy, and Fz loads.</summary>
    [SNJ.JsonProperty("loading", Required = SNJ.Required.Default, NullValueHandling = SNJ.NullValueHandling.Ignore)]
    public StructuralVectorThree Loading { get; set; }

    /// <summary>Application IDs of Structural2DElementMeshes to apply load.</summary>
    [SNJ.JsonProperty("elementRefs", Required = SNJ.Required.Default, NullValueHandling = SNJ.NullValueHandling.Ignore)]
    public List<string> ElementRefs { get; set; }

    /// <summary>Application ID of StructuralLoadCase.</summary>
    [SNJ.JsonProperty("loadCaseRef", Required = SNJ.Required.Default, NullValueHandling = SNJ.NullValueHandling.Ignore)]
    public string LoadCaseRef { get; set; }
  }

  [Serializable]
  public partial class Structural2DLoadPanel : SpecklePolyline, IStructural
  {
    public override string Type { get => base.Type + "/Structural2DLoadPanel"; }

    [SNJ.JsonIgnore]
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
    [SNJ.JsonIgnore]
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
    [SNJ.JsonIgnore]
    public StructuralVectorThree Loading
    {
      get => StructuralProperties.ContainsKey("loading") ? (StructuralProperties["loading"] as StructuralVectorThree) : null;
      set => StructuralProperties["loading"] = value;
    }

    /// <summary>Application ID of StructuralLoadCase.</summary>
    [SNJ.JsonIgnore]
    public string LoadCaseRef
    {
      get => StructuralProperties.ContainsKey("loadCaseRef") ? (StructuralProperties["loadCaseRef"] as string) : null;
      set => StructuralProperties["loadCaseRef"] = value;
    }
  }

  [Serializable]
  public partial class Structural2DThermalLoad : SpeckleObject, IStructural
  {
    public override string Type { get => "Structural2DThermalLoad"; }

    /// <summary>Temperature at the top surface of the element.</summary>
    [SNJ.JsonProperty("topTemperature", Required = SNJ.Required.Default, NullValueHandling = SNJ.NullValueHandling.Ignore)]
    public double TopTemperature;

    /// <summary>Temperature at the bottom surface of the element.</summary>
    [SNJ.JsonProperty("bottomTemperature", Required = SNJ.Required.Default, NullValueHandling = SNJ.NullValueHandling.Ignore)]
    public double BottomTemperature;

    /// <summary>Application ID of StructuralLoadCase.</summary>
    [SNJ.JsonProperty("loadCaseRef", Required = SNJ.Required.Default, NullValueHandling = SNJ.NullValueHandling.Ignore)]
    public string LoadCaseRef { get; set; }

    /// <summary>Application IDs of Structural2DElements to apply load.</summary>
    [SNJ.JsonProperty("elementRefs", Required = SNJ.Required.Default, NullValueHandling = SNJ.NullValueHandling.Ignore)]
    public List<string> ElementRefs { get; set; }
  }

}
