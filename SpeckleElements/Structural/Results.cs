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
  [Serializable]
  public partial class StructuralNodeResult : SpeckleObject, IStructural
  {
    public override string Type { get => "StructuralNodeResult"; }

    /// <summary>ApplicationID of object referred to.</summary>
    [SNJ.JsonProperty("targetRef", Required = SNJ.Required.Default, NullValueHandling = SNJ.NullValueHandling.Ignore)]
    public string TargetRef { get; set; }

    /// <summary>Indicates whether the results are in the global or local axis.</summary>
    [SNJ.JsonProperty("isGlobal", Required = SNJ.Required.Default, NullValueHandling = SNJ.NullValueHandling.Ignore)]
    public bool IsGlobal { get; set; }

    /// <summary>Results.</summary>
    [SNJ.JsonProperty("value", Required = SNJ.Required.Default, NullValueHandling = SNJ.NullValueHandling.Ignore)]
    [SpeckleNewtonsoft.Newtonsoft.Json.JsonConverter(typeof(SpecklePropertiesConverter))]
    public Dictionary<string, object> Value { get; set; }
  }

  [Serializable]
  public partial class Structural1DElementResult : SpeckleObject, IStructural
  {
    public override string Type { get => "Structural1DElementResult"; }

    /// <summary>ApplicationID of object referred to.</summary>
    [SNJ.JsonProperty("targetRef", Required = SNJ.Required.Default, NullValueHandling = SNJ.NullValueHandling.Ignore)]
    public string TargetRef { get; set; }

    /// <summary>Indicates whether the results are in the global or local axis.</summary>
    [SNJ.JsonProperty("isGlobal", Required = SNJ.Required.Default, NullValueHandling = SNJ.NullValueHandling.Ignore)]
    public bool IsGlobal { get; set; }

    /// <summary>Results.</summary>
    [SNJ.JsonProperty("value", Required = SNJ.Required.Default, NullValueHandling = SNJ.NullValueHandling.Ignore)]
    [SpeckleNewtonsoft.Newtonsoft.Json.JsonConverter(typeof(SpecklePropertiesConverter))]
    public Dictionary<string, object> Value { get; set; }
  }

  [Serializable]
  public partial class Structural2DElementResult : SpeckleObject, IStructural
  {
    public override string Type { get => "Structural2DElementResult"; }

    /// <summary>ApplicationID of object referred to.</summary>
    [SNJ.JsonProperty("targetRef", Required = SNJ.Required.Default, NullValueHandling = SNJ.NullValueHandling.Ignore)]
    public string TargetRef { get; set; }

    /// <summary>Indicates whether the results are in the global or local axis.</summary>
    [SNJ.JsonProperty("isGlobal", Required = SNJ.Required.Default, NullValueHandling = SNJ.NullValueHandling.Ignore)]
    public bool IsGlobal { get; set; }

    /// <summary>Results.</summary>
    [SNJ.JsonProperty("value", Required = SNJ.Required.Default, NullValueHandling = SNJ.NullValueHandling.Ignore)]
    [SpeckleNewtonsoft.Newtonsoft.Json.JsonConverter(typeof(SpecklePropertiesConverter))]
    public Dictionary<string, object> Value { get; set; }
  }

  [Serializable]
  public partial class StructuralMiscResult : SpeckleObject, IStructural
  {
    public override string Type { get => "StructuralMiscResult"; }

    /// <summary>Description of result.</summary>
    [SNJ.JsonProperty("description", Required = SNJ.Required.Default, NullValueHandling = SNJ.NullValueHandling.Ignore)]
    public string Description { get; set; }

    /// <summary>ApplicationID of object referred to.</summary>
    [SNJ.JsonProperty("targetRef", Required = SNJ.Required.Default, NullValueHandling = SNJ.NullValueHandling.Ignore)]
    public string TargetRef { get; set; }

    /// <summary>Indicates whether the results are in the global or local axis.</summary>
    [SNJ.JsonProperty("isGlobal", Required = SNJ.Required.Default, NullValueHandling = SNJ.NullValueHandling.Ignore)]
    public bool IsGlobal { get; set; }

    /// <summary>Results.</summary>
    [SNJ.JsonProperty("value", Required = SNJ.Required.Default, NullValueHandling = SNJ.NullValueHandling.Ignore)]
    [SpeckleNewtonsoft.Newtonsoft.Json.JsonConverter(typeof(SpecklePropertiesConverter))]
    public Dictionary<string, object> Value { get; set; }

    /// <summary>String indicating source of result.</summary>
    [SNJ.JsonProperty("resultSource", Required = SNJ.Required.Default, NullValueHandling = SNJ.NullValueHandling.Ignore)]
    public string ResultSource { get; set; }
  }
}
