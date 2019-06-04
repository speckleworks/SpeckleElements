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
    public interface IStructural : ISpeckleInitializer
    {
        string StructuralId { get; set; }
    }

    #region Helper objects
    [Serializable]
    public partial class StructuralVectorThree : SpeckleVector, IStructural
    {
        public override string Type { get => base.Type + "/StructuralVectorThree"; }

        /// <summary>Structural ID to reference from other objects.</summary>
        [SNJ.JsonIgnore]
        public string StructuralId { get; set; }

        /// <summary>Base SpeckleVector.</summary>
        [SNJ.JsonIgnore]
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

        /// <summary>Structural ID to reference from other objects.</summary>
        [SNJ.JsonIgnore]
        public string StructuralId { get; set; }

        /// <summary>An array containing the X, Y, and Z values of the vector.</summary>
        [SNJ.JsonProperty("value", Required = SNJ.Required.Default, NullValueHandling = SNJ.NullValueHandling.Ignore)]
        public List<bool> Value { get; set; }
    }

    [Serializable]
    public partial class StructuralVectorSix : SpeckleObject, IStructural
    {
        public override string Type { get => "StructuralVectorSix"; }

        /// <summary>Structural ID to reference from other objects.</summary>
        [SNJ.JsonIgnore]
        public string StructuralId { get; set; }

        /// <summary>An array containing the X, Y, Z, XX, YY, and ZZ values of the vector.</summary>
        [SNJ.JsonProperty("value", Required = SNJ.Required.Default, NullValueHandling = SNJ.NullValueHandling.Ignore)]
        public List<double> Value { get; set; }
    }

    [Serializable]
    public partial class StructuralVectorBoolSix : SpeckleObject, IStructural
    {
        public override string Type { get => "StructuralVectorBoolSix"; }

        /// <summary>Structural ID to reference from other objects.</summary>
        [SNJ.JsonIgnore]
        public string StructuralId { get; set; }

        /// <summary>An array containing the X, Y, Z, XX, YY, and ZZ values of the vector.</summary>
        [SNJ.JsonProperty("value", Required = SNJ.Required.Default, NullValueHandling = SNJ.NullValueHandling.Ignore)]
        public List<bool> Value { get; set; }
    }

    [Serializable]
    public partial class StructuralAxis : SpecklePlane, IStructural
    {
        public override string Type { get => base.Type + "/StructuralAxis"; }

        /// <summary>Structural ID to reference from other objects.</summary>
        [SNJ.JsonIgnore]
        public string StructuralId { get; set; }

        /// <summary>Base SpecklePlane.</summary>
        [SNJ.JsonIgnore]
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

        /// <summary>Structural ID to reference from other objects.</summary>
        [SNJ.JsonProperty("structuralId", Required = SNJ.Required.Default, NullValueHandling = SNJ.NullValueHandling.Ignore)]
        public string StructuralId { get; set; }

        /// <summary>Type of load the case contains.</summary>
        [SNJ.JsonConverter(typeof(SNJ.Converters.StringEnumConverter))]
        [SNJ.JsonProperty("caseType", Required = SNJ.Required.Default, NullValueHandling = SNJ.NullValueHandling.Ignore)]
        public StructuralLoadCaseType CaseType { get; set; }
    }

    [Serializable]
    public partial class StructuralLoadTask : SpeckleObject, IStructural
    {
        public override string Type { get => "StructuralLoadTask"; }

        /// <summary>Structural ID to reference from other objects.</summary>
        [SNJ.JsonProperty("structuralId", Required = SNJ.Required.Default, NullValueHandling = SNJ.NullValueHandling.Ignore)]
        public string StructuralId { get; set; }

        /// <summary>Type of analysis to perform.</summary>
        [SNJ.JsonConverter(typeof(SNJ.Converters.StringEnumConverter))]
        [SNJ.JsonProperty("taskType", Required = SNJ.Required.Default, NullValueHandling = SNJ.NullValueHandling.Ignore)]
        public StructuralLoadTaskType TaskType { get; set; }

        /// <summary>Structural IDs of StructuralLoadCase to include.</summary>
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

        /// <summary>Structural ID to reference from other objects.</summary>
        [SNJ.JsonProperty("structuralId", Required = SNJ.Required.Default, NullValueHandling = SNJ.NullValueHandling.Ignore)]
        public string StructuralId { get; set; }

        /// <summary>Number of modes.</summary>
        [SNJ.JsonProperty("NumModes", Required = SNJ.Required.Default, NullValueHandling = SNJ.NullValueHandling.Ignore)]
        public int NumModes { get; set; }

        /// <summary>Maximum number of iterations.</summary>
        [SNJ.JsonProperty("MaxNumIterations", Required = SNJ.Required.Default, NullValueHandling = SNJ.NullValueHandling.Ignore)]
        public int MaxNumIterations { get; set; }

        /// <summary>Name of the combination case.</summary>
        [SNJ.JsonProperty("ResultCaseRef", Required = SNJ.Required.Default, NullValueHandling = SNJ.NullValueHandling.Ignore)]
        public string ResultCaseRef { get; set; }

        /// <summary>Stage definition for the task</summary>
        [SNJ.JsonProperty("stageDefinitionRef", Required = SNJ.Required.Default, NullValueHandling = SNJ.NullValueHandling.Ignore)]
        public string StageDefinitionRef { get; set; }
    }

    [Serializable]
    public partial class StructuralLoadCombo : SpeckleObject, IStructural
    {
        public override string Type { get => "StructuralLoadCombo"; }

        /// <summary>Structural ID to reference from other objects.</summary>
        [SNJ.JsonProperty("structuralId", Required = SNJ.Required.Default, NullValueHandling = SNJ.NullValueHandling.Ignore)]
        public string StructuralId { get; set; }

        /// <summary>Type of combination method.</summary>
        [SNJ.JsonConverter(typeof(SNJ.Converters.StringEnumConverter))]
        [SNJ.JsonProperty("comboType", Required = SNJ.Required.Default, NullValueHandling = SNJ.NullValueHandling.Ignore)]
        public StructuralLoadComboType ComboType { get; set; }

        /// <summary>Structural IDs of StructuralLoadTask to include.</summary>
        [SNJ.JsonProperty("loadTaskRefs", Required = SNJ.Required.Default, NullValueHandling = SNJ.NullValueHandling.Ignore)]
        public List<string> LoadTaskRefs { get; set; }

        /// <summary>Load factors for each load task.</summary>
        [SNJ.JsonProperty("loadTaskFactors", Required = SNJ.Required.Default, NullValueHandling = SNJ.NullValueHandling.Ignore)]
        public List<double> LoadTaskFactors { get; set; }

        /// <summary>Structural IDs of StructuralLoadCombo to include.</summary>
        [SNJ.JsonProperty("loadComboRefs", Required = SNJ.Required.Default, NullValueHandling = SNJ.NullValueHandling.Ignore)]
        public List<string> LoadComboRefs { get; set; }

        /// <summary>Load factors for each load combo.</summary>
        [SNJ.JsonProperty("loadComboFactors", Required = SNJ.Required.Default, NullValueHandling = SNJ.NullValueHandling.Ignore)]
        public List<double> LoadComboFactors { get; set; }
    }

    [Serializable]
    public partial class Structural0DLoad : SpeckleObject, IStructural
    {
        public override string Type { get => "Structural0DLoad"; }

        /// <summary>Structural ID to reference from other objects.</summary>
        [SNJ.JsonProperty("structuralId", Required = SNJ.Required.Default, NullValueHandling = SNJ.NullValueHandling.Ignore)]
        public string StructuralId { get; set; }

        /// <summary>A list of Fx, Fy, Fz, Mx, My, and Mz loads.</summary>
        [SNJ.JsonProperty("loading", Required = SNJ.Required.Default, NullValueHandling = SNJ.NullValueHandling.Ignore)]
        public StructuralVectorSix Loading { get; set; }

        /// <summary>Structural IDs of StructuralNodes to apply load.</summary>
        [SNJ.JsonProperty("nodeRefs", Required = SNJ.Required.Default, NullValueHandling = SNJ.NullValueHandling.Ignore)]
        public List<string> NodeRefs { get; set; }

        /// <summary>Structural ID of StructuralLoadCase.</summary>
        [SNJ.JsonProperty("loadCaseRef", Required = SNJ.Required.Default, NullValueHandling = SNJ.NullValueHandling.Ignore)]
        public string LoadCaseRef { get; set; }
    }

    [Serializable]
    public partial class Structural1DLoad : SpeckleObject, IStructural
    {
        public override string Type { get => "Structural1DLoad"; }

        /// <summary>Structural ID to reference from other objects.</summary>
        [SNJ.JsonProperty("structuralId", Required = SNJ.Required.Default, NullValueHandling = SNJ.NullValueHandling.Ignore)]
        public string StructuralId { get; set; }

        /// <summary>A list of Fx, Fy, Fz, Mx, My, and Mz loads.</summary>
        [SNJ.JsonProperty("loading", Required = SNJ.Required.Default, NullValueHandling = SNJ.NullValueHandling.Ignore)]
        public StructuralVectorSix Loading { get; set; }

        /// <summary>Structural IDs of Structural1DElements to apply load.</summary>
        [SNJ.JsonProperty("elementRefs", Required = SNJ.Required.Default, NullValueHandling = SNJ.NullValueHandling.Ignore)]
        public List<string> ElementRefs { get; set; }

        /// <summary>Structural ID of StructuralLoadCase.</summary>
        [SNJ.JsonProperty("loadCaseRef", Required = SNJ.Required.Default, NullValueHandling = SNJ.NullValueHandling.Ignore)]
        public string LoadCaseRef { get; set; }
    }

    [Serializable]
    public partial class Structural2DLoad : SpeckleObject, IStructural
    {
        public override string Type { get => "Structural2DLoad"; }

        /// <summary>Structural ID to reference from other objects.</summary>
        [SNJ.JsonProperty("structuralId", Required = SNJ.Required.Default, NullValueHandling = SNJ.NullValueHandling.Ignore)]
        public string StructuralId { get; set; }

        /// <summary>A list of Fx, Fy, and Fz loads.</summary>
        [SNJ.JsonProperty("loading", Required = SNJ.Required.Default, NullValueHandling = SNJ.NullValueHandling.Ignore)]
        public StructuralVectorThree Loading { get; set; }

        /// <summary>Structural IDs of Structural2DElementMeshes to apply load.</summary>
        [SNJ.JsonProperty("elementRefs", Required = SNJ.Required.Default, NullValueHandling = SNJ.NullValueHandling.Ignore)]
        public List<string> ElementRefs { get; set; }

        /// <summary>Structural ID of StructuralLoadCase.</summary>
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

        /// <summary>Structural ID to reference from other objects.</summary>
        [SNJ.JsonIgnore]
        public string StructuralId
        {
            get => StructuralProperties.ContainsKey("structuralId") ? (StructuralProperties["structuralId"] as string) : null;
            set => StructuralProperties["structuralId"] = value;
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

        /// <summary>Structural ID of StructuralLoadCase.</summary>
        [SNJ.JsonIgnore]
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

        /// <summary>Structural ID to reference from other objects.</summary>
        [SNJ.JsonIgnore]
        public string StructuralId
        {
            get => StructuralProperties.ContainsKey("structuralId") ? (StructuralProperties["structuralId"] as string) : null;
            set => StructuralProperties["structuralId"] = value;
        }

        /// <summary>Base SpecklePolyline.</summary>
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

        /// <summary>Structural ID of StructuralLoadCase.</summary>
        [SNJ.JsonIgnore]
        public List<string> MemberRefs
        {
            get
            {
                var list = new List<String>();
                if (StructuralProperties.ContainsKey("memberRefs"))
                {
                    var memberRefs = (List<object>)StructuralProperties["memberRefs"];
                    foreach (var memberRef in memberRefs)
                    {
                        list.Add((string)memberRef);
                    }
                }
                return list;
            }
            set => StructuralProperties["memberRefs"] = value.ToArray();
        }
    }

    [Serializable]
    public partial class StructuralStageDefinition : SpeckleObject, IStructural
    {
        public override string Type { get => "StructuralStageDefinition"; }

        /// <summary>Structural ID to reference from other objects.</summary>
        [SNJ.JsonProperty("structuralId", Required = SNJ.Required.Default, NullValueHandling = SNJ.NullValueHandling.Ignore)]
        public string StructuralId { get; set; }

        /// <summary>Structural ID of members to include in the stage of the construction sequence.</summary>
        [SNJ.JsonProperty("memberRefs", Required = SNJ.Required.Default, NullValueHandling = SNJ.NullValueHandling.Ignore)]
        public List<string> MemberRefs { get; set; }

        /// <summary>Number of days in the stage</summary>
        [SNJ.JsonProperty("stageDays", Required = SNJ.Required.Default, NullValueHandling = SNJ.NullValueHandling.Ignore)]
        public int StageDays { get; set; }
    }
    #endregion

    #region Properties
    [Serializable]
    public partial class StructuralMaterialConcrete : SpeckleObject, IStructural
    {
        public override string Type { get => "StructuralMaterialConcrete"; }

        /// <summary>Structural ID to reference from other objects.</summary>
        [SNJ.JsonProperty("structuralId", Required = SNJ.Required.Default, NullValueHandling = SNJ.NullValueHandling.Ignore)]
        public string StructuralId { get; set; }

        /// <summary>Young's modulus (E) of material.</summary>
        [SNJ.JsonProperty("youngsModulus", Required = SNJ.Required.Default, NullValueHandling = SNJ.NullValueHandling.Ignore)]
        public double YoungsModulus { get; set; }

        /// <summary>Shear modulus (G) of material.</summary>
        [SNJ.JsonProperty("shearModulus", Required = SNJ.Required.Default, NullValueHandling = SNJ.NullValueHandling.Ignore)]
        public double ShearModulus { get; set; }

        /// <summary>Poission's ratio (ν) of material.</summary>
        [SNJ.JsonProperty("poissonsRatio", Required = SNJ.Required.Default, NullValueHandling = SNJ.NullValueHandling.Ignore)]
        public double PoissonsRatio { get; set; }

        /// <summary>Density (ρ) of material.</summary>
        [SNJ.JsonProperty("density", Required = SNJ.Required.Default, NullValueHandling = SNJ.NullValueHandling.Ignore)]
        public double Density { get; set; }

        /// <summary>Coefficient of thermal expansion (α) of material.</summary>
        [SNJ.JsonProperty("coeffThermalExpansion", Required = SNJ.Required.Default, NullValueHandling = SNJ.NullValueHandling.Ignore)]
        public double CoeffThermalExpansion { get; set; }

        /// <summary>Compressive strength.</summary>
        [SNJ.JsonProperty("compressiveStrength", Required = SNJ.Required.Default, NullValueHandling = SNJ.NullValueHandling.Ignore)]
        public double CompressiveStrength { get; set; }

        /// <summary>Max strain at failure.</summary>
        [SNJ.JsonProperty("maxStrain", Required = SNJ.Required.Default, NullValueHandling = SNJ.NullValueHandling.Ignore)]
        public double MaxStrain { get; set; }

        /// <summary>Aggragate size.</summary>
        [SNJ.JsonProperty("aggragateSize", Required = SNJ.Required.Default, NullValueHandling = SNJ.NullValueHandling.Ignore)]
        public double AggragateSize { get; set; }
    }

    [Serializable]
    public partial class StructuralMaterialSteel : SpeckleObject, IStructural
    {
        public override string Type { get => "StructuralMaterialSteel"; }

        /// <summary>Structural ID to reference from other objects.</summary>
        [SNJ.JsonProperty("structuralId", Required = SNJ.Required.Default, NullValueHandling = SNJ.NullValueHandling.Ignore)]
        public string StructuralId { get; set; }

        /// <summary>Young's modulus (E) of material.</summary>
        [SNJ.JsonProperty("youngsModulus", Required = SNJ.Required.Default, NullValueHandling = SNJ.NullValueHandling.Ignore)]
        public double YoungsModulus { get; set; }

        /// <summary>Shear modulus (G) of material.</summary>
        [SNJ.JsonProperty("shearModulus", Required = SNJ.Required.Default, NullValueHandling = SNJ.NullValueHandling.Ignore)]
        public double ShearModulus { get; set; }

        /// <summary>Poission's ratio (ν) of material.</summary>
        [SNJ.JsonProperty("poissonsRatio", Required = SNJ.Required.Default, NullValueHandling = SNJ.NullValueHandling.Ignore)]
        public double PoissonsRatio { get; set; }

        /// <summary>Density (ρ) of material.</summary>
        [SNJ.JsonProperty("density", Required = SNJ.Required.Default, NullValueHandling = SNJ.NullValueHandling.Ignore)]
        public double Density { get; set; }

        /// <summary>Coefficient of thermal expansion (α) of material.</summary>
        [SNJ.JsonProperty("coeffThermalExpansion", Required = SNJ.Required.Default, NullValueHandling = SNJ.NullValueHandling.Ignore)]
        public double CoeffThermalExpansion { get; set; }

        /// <summary>Yield strength.</summary>
        [SNJ.JsonProperty("yieldStrength", Required = SNJ.Required.Default, NullValueHandling = SNJ.NullValueHandling.Ignore)]
        public double YieldStrength { get; set; }

        /// <summary>Ultimate strength.</summary>
        [SNJ.JsonProperty("ultimateStrength", Required = SNJ.Required.Default, NullValueHandling = SNJ.NullValueHandling.Ignore)]
        public double UltimateStrength { get; set; }

        /// <summary>Max strain at failure.</summary>
        [SNJ.JsonProperty("maxStrain", Required = SNJ.Required.Default, NullValueHandling = SNJ.NullValueHandling.Ignore)]
        public double MaxStrain { get; set; }
    }

    [Serializable]
    public partial class Structural1DProperty : SpeckleObject, IStructural
    {
        public override string Type { get => "Structural1DProperty"; }

        /// <summary>Structural ID to reference from other objects.</summary>
        [SNJ.JsonProperty("structuralId", Required = SNJ.Required.Default, NullValueHandling = SNJ.NullValueHandling.Ignore)]
        public string StructuralId { get; set; }

        /// <summary>SpecklePolyline or SpeckleCircle of the cross-section.</summary>
        [SNJ.JsonProperty("profile", Required = SNJ.Required.Default, NullValueHandling = SNJ.NullValueHandling.Ignore)]
        public SpeckleObject Profile { get; set; }

        /// <summary>Cross-section shape.</summary>
        [SNJ.JsonConverter(typeof(SNJ.Converters.StringEnumConverter))]
        [SNJ.JsonProperty("shape", Required = SNJ.Required.Default, NullValueHandling = SNJ.NullValueHandling.Ignore)]
        public Structural1DPropertyShape Shape { get; set; }

        /// <summary>Is the section filled or hollow?</summary>
        [SNJ.JsonProperty("hollow", Required = SNJ.Required.Default, NullValueHandling = SNJ.NullValueHandling.Ignore)]
        public bool Hollow { get; set; }

        /// <summary>Thickness of the section if hollow.</summary>
        [SNJ.JsonProperty("thickness", Required = SNJ.Required.Default, NullValueHandling = SNJ.NullValueHandling.Ignore)]
        public double Thickness { get; set; }

        /// <summary>Structural ID of StructuralMaterial.</summary>
        [SNJ.JsonProperty("materialRef", Required = SNJ.Required.Default, NullValueHandling = SNJ.NullValueHandling.Ignore)]
        public string MaterialRef { get; set; }
    }

    [Serializable]
    public partial class Structural2DProperty : SpeckleObject, IStructural
    {
        public override string Type { get => "Structural2DProperty"; }

        /// <summary>Structural ID to reference from other objects.</summary>
        [SNJ.JsonProperty("structuralId", Required = SNJ.Required.Default, NullValueHandling = SNJ.NullValueHandling.Ignore)]
        public string StructuralId { get; set; }

        /// <summary>Thickness of the 2D element.</summary>
        [SNJ.JsonProperty("thickness", Required = SNJ.Required.Default, NullValueHandling = SNJ.NullValueHandling.Ignore)]
        public double Thickness { get; set; }

        /// <summary>Structural ID of StructuralMaterial.</summary>
        [SNJ.JsonProperty("materialRef", Required = SNJ.Required.Default, NullValueHandling = SNJ.NullValueHandling.Ignore)]
        public string MaterialRef { get; set; }

        /// <summary>Reference surface for property.</summary>
        [SNJ.JsonProperty("referenceSurface", Required = SNJ.Required.Default, NullValueHandling = SNJ.NullValueHandling.Ignore)]
        public Structural2DPropertyReferenceSurface ReferenceSurface { get; set; }
    }
    #endregion

    #region Nodes and Elements
    [Serializable]
    public partial class StructuralNode : SpecklePoint, IStructural
    {
        public override string Type { get => base.Type + "/StructuralNode"; }

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

        /// <summary>Structural ID to reference from other objects.</summary>
        [SNJ.JsonIgnore]
        public string StructuralId
        {
            get => StructuralProperties.ContainsKey("structuralId") ? (StructuralProperties["structuralId"] as string) : null;
            set => StructuralProperties["structuralId"] = value;
        }

        /// <summary>Base SpecklePoint.</summary>
        [SNJ.JsonIgnore]
        public SpecklePoint basePoint
        {
            get => this as SpecklePoint;
            set => this.Value = value.Value;
        }

        /// <summary>Local axis of the node.</summary>
        [SNJ.JsonIgnore]
        public StructuralAxis Axis
        {
            get => StructuralProperties.ContainsKey("axis") ? (StructuralProperties["axis"] as StructuralAxis) : null;
            set => StructuralProperties["axis"] = value;
        }

        /// <summary>A list of the X, Y, Z, Rx, Ry, and Rz restraints.</summary>
        [SNJ.JsonIgnore]
        public StructuralVectorBoolSix Restraint
        {
            get => StructuralProperties.ContainsKey("restraint") ? (StructuralProperties["restraint"] as StructuralVectorBoolSix) : null;
            set => StructuralProperties["restraint"] = value;
        }

        /// <summary>A list of the X, Y, Z, Rx, Ry, and Rz stiffnesses.</summary>
        [SNJ.JsonIgnore]
        public StructuralVectorSix Stiffness
        {
            get => StructuralProperties.ContainsKey("stiffness") ? (StructuralProperties["stiffness"] as StructuralVectorSix) : null;
            set => StructuralProperties["stiffness"] = value;
        }

        /// <summary>Mass of the node.</summary>
        [SNJ.JsonIgnore]
        public double Mass
        {
            get => StructuralProperties.ContainsKey("mass") ? ((double)StructuralProperties["mass"]) : 0;
            set => StructuralProperties["mass"] = value;
        }

        /// <summary>Analysis results.</summary>
        [SNJ.JsonIgnore]
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

        /// <summary>Structural ID to reference from other objects.</summary>
        [SNJ.JsonIgnore]
        public string StructuralId
        {
            get => StructuralProperties.ContainsKey("structuralId") ? (StructuralProperties["structuralId"] as string) : null;
            set => StructuralProperties["structuralId"] = value;
        }

        /// <summary>Base SpeckleLine.</summary>
        [SNJ.JsonIgnore]
        public SpeckleLine baseLine
        {
            get => this as SpeckleLine;
            set => this.Value = value.Value;
        }

        /// <summary>Type of 1D element.</summary>
        [SNJ.JsonIgnore]
        public Structural1DElementType ElementType
        {
            get => StructuralProperties.ContainsKey("elementType") ? (Structural1DElementType)Enum.Parse(typeof(Structural1DElementType), (StructuralProperties["elementType"] as string), true) : Structural1DElementType.Generic;
            set => StructuralProperties["elementType"] = value.ToString();
        }

        /// <summary>Structural ID of Structural1DProperty.</summary>
        [SNJ.JsonIgnore]
        public string PropertyRef
        {
            get => StructuralProperties.ContainsKey("propertyRef") ? (StructuralProperties["propertyRef"] as string) : null;
            set => StructuralProperties["propertyRef"] = value;
        }

        /// <summary>Local axis of 1D element.</summary>
        [SNJ.JsonIgnore]
        public StructuralVectorThree ZAxis
        {
            get => StructuralProperties.ContainsKey("zAxis") ? (StructuralProperties["zAxis"] as StructuralVectorThree) : null;
            set => StructuralProperties["zAxis"] = value;
        }

        /// <summary>List of X, Y, Z, Rx, Ry, and Rz releases on each end.</summary>
        [SNJ.JsonIgnore]
        public List<StructuralVectorBoolSix> EndRelease
        {
            get => StructuralProperties.ContainsKey("endRelease") ? (StructuralProperties["endRelease"] as List<StructuralVectorBoolSix>) : null;
            set => StructuralProperties["endRelease"] = value;
        }

        /// <summary>List of X, Y, and, Z offsets on each end.</summary>
        [SNJ.JsonIgnore]
        public List<StructuralVectorThree> Offset
        {
            get => StructuralProperties.ContainsKey("offset") ? (StructuralProperties["offset"] as List<StructuralVectorThree>) : null;
            set => StructuralProperties["offset"] = value;
        }

        /// <summary>GSA target mesh size.</summary>
        [SNJ.JsonIgnore]
        public double MeshSize
        {
            get => StructuralProperties.ContainsKey("gsaMeshSize") ? ((double)StructuralProperties["gsaMeshSize"]) : 0;
            set => StructuralProperties["gsaMeshSize"] = value;
        }

        /// <summary>GSA dummy status.</summary>
        [SNJ.JsonIgnore]
        public bool Dummy
        {
            get => StructuralProperties.ContainsKey("gsaDummy") ? ((bool)StructuralProperties["gsaDummy"]) : false;
            set => StructuralProperties["gsaDummy"] = value;
        }
    }

    [Serializable]
    public partial class Structural1DElementPolyline : SpecklePolyline, IStructural
    {
        public override string Type { get => base.Type + "/Structural1DElementPolyline"; }

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

        /// <summary>Structural ID to reference from other objects.</summary>
        [SNJ.JsonIgnore]
        public string StructuralId
        {
            get => StructuralProperties.ContainsKey("structuralId") ? (StructuralProperties["structuralId"] as string) : null;
            set => StructuralProperties["structuralId"] = value;
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

        /// <summary>Type of 1D element.</summary>
        [SNJ.JsonIgnore]
        public Structural1DElementType ElementType
        {
            get => StructuralProperties.ContainsKey("elementType") ? (Structural1DElementType)Enum.Parse(typeof(Structural1DElementType), (StructuralProperties["elementType"] as string), true) : Structural1DElementType.Generic;
            set => StructuralProperties["elementType"] = value.ToString();
        }

        /// <summary>Structural ID of Structural1DProperty.</summary>
        [SNJ.JsonIgnore]
        public string PropertyRef
        {
            get => StructuralProperties.ContainsKey("propertyRef") ? (StructuralProperties["propertyRef"] as string) : null;
            set => StructuralProperties["propertyRef"] = value;
        }

        /// <summary>Local Z axis of 1D elements.</summary>
        [SNJ.JsonIgnore]
        public List<StructuralVectorThree> ZAxis
        {
            get => StructuralProperties.ContainsKey("zAxis") ? (StructuralProperties["zAxis"] as List<StructuralVectorThree>) : null;
            set => StructuralProperties["zAxis"] = value;
        }

        /// <summary>List of X, Y, Z, Rx, Ry, and Rz releases of each node.</summary>
        [SNJ.JsonIgnore]
        public List<StructuralVectorBoolSix> EndRelease
        {
            get => StructuralProperties.ContainsKey("endRelease") ? (StructuralProperties["endRelease"] as List<StructuralVectorBoolSix>) : null;
            set => StructuralProperties["endRelease"] = value;
        }

        /// <summary>List of X, Y, Z, Rx, Ry, and Rz offsets of each node.</summary>
        [SNJ.JsonIgnore]
        public List<StructuralVectorThree> Offset
        {
            get => StructuralProperties.ContainsKey("offset") ? (StructuralProperties["offset"] as List<StructuralVectorThree>) : null;
            set => StructuralProperties["offset"] = value;
        }

        /// <summary>GSA target mesh size.</summary>
        [SNJ.JsonIgnore]
        public double MeshSize
        {
            get => StructuralProperties.ContainsKey("gsaMeshSize") ? ((double)StructuralProperties["gsaMeshSize"]) : 0;
            set => StructuralProperties["gsaMeshSize"] = value;
        }

        /// <summary>GSA dummy status.</summary>
        [SNJ.JsonIgnore]
        public bool Dummy
        {
            get => StructuralProperties.ContainsKey("gsaDummy") ? ((bool)StructuralProperties["gsaDummy"]) : false;
            set => StructuralProperties["gsaDummy"] = value;
        }
    }

    [Serializable]
    public partial class Structural2DElementMesh : SpeckleMesh, IStructural
    {
        public override string Type { get => base.Type + "/Structural2DElementMesh"; }

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

        /// <summary>Structural ID to reference from other objects.</summary>
        [SNJ.JsonIgnore]
        public string StructuralId
        {
            get => StructuralProperties.ContainsKey("structuralId") ? (StructuralProperties["structuralId"] as string) : null;
            set => StructuralProperties["structuralId"] = value;
        }

        /// <summary>Base SpeckleMesh.</summary>
        [SNJ.JsonIgnore]
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
        [SNJ.JsonIgnore]
        public Structural2DElementType ElementType
        {
            get => StructuralProperties.ContainsKey("elementType") ? (Structural2DElementType)Enum.Parse(typeof(Structural2DElementType), (StructuralProperties["elementType"] as string), true) : Structural2DElementType.Generic;
            set => StructuralProperties["elementType"] = value.ToString();
        }

        /// <summary>Structural ID of Structural2DProperty.</summary>
        [SNJ.JsonIgnore]
        public string PropertyRef
        {
            get => StructuralProperties.ContainsKey("propertyRef") ? (StructuralProperties["propertyRef"] as string) : null;
            set => StructuralProperties["propertyRef"] = value;
        }

        /// <summary>Local axis of 2D element.</summary>
        [SNJ.JsonIgnore]
        public StructuralAxis Axis
        {
            get => StructuralProperties.ContainsKey("axis") ? (StructuralProperties["axis"] as StructuralAxis) : null;
            set => StructuralProperties["axis"] = value;
        }

        /// <summary>Offset of 2D element.</summary>
        [SNJ.JsonIgnore]
        public double Offset
        {
            get => StructuralProperties.ContainsKey("offset") ? ((double)StructuralProperties["offset"]) : 0;
            set => StructuralProperties["offset"] = value;
        }

        /// <summary>GSA target mesh size.</summary>
        [SNJ.JsonIgnore]
        public double MeshSize
        {
            get => StructuralProperties.ContainsKey("gsaMeshSize") ? ((double)StructuralProperties["gsaMeshSize"]) : 0;
            set => StructuralProperties["gsaMeshSize"] = value;
        }

        /// <summary>GSA dummy status.</summary>
        [SNJ.JsonIgnore]
        public bool Dummy
        {
            get => StructuralProperties.ContainsKey("gsaDummy") ? ((bool)StructuralProperties["gsaDummy"]) : false;
            set => StructuralProperties["gsaDummy"] = value;
        }

        /// <summary>Analysis results.</summary>
        [SNJ.JsonIgnore]
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

        /// <summary>Structural ID to reference from other objects.</summary>
        [SNJ.JsonIgnore]
        public string StructuralId
        {
            get => StructuralProperties.ContainsKey("structuralId") ? (StructuralProperties["structuralId"] as string) : null;
            set => StructuralProperties["structuralId"] = value;
        }

        /// <summary>Base SpeckleMesh.</summary>
        [SNJ.JsonIgnore]
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

        /// <summary>Structural ID to reference from other objects.</summary>
        [SNJ.JsonProperty("structuralId", Required = SNJ.Required.Default, NullValueHandling = SNJ.NullValueHandling.Ignore)]
        public string StructuralId { get; set; }

        /// <summary>Indicates whether the results are in the global or local axis.</summary>
        [SNJ.JsonProperty("isGlobal", Required = SNJ.Required.Default, NullValueHandling = SNJ.NullValueHandling.Ignore)]
        public bool IsGlobal { get; set; }

        /// <summary>Node reaction.</summary>
        [SNJ.JsonProperty("reaction", Required = SNJ.Required.Default, NullValueHandling = SNJ.NullValueHandling.Ignore)]
        public Dictionary<string, double> Reaction { get; set; }

        /// <summary>Node displacement.</summary>
        [SNJ.JsonProperty("displacement", Required = SNJ.Required.Default, NullValueHandling = SNJ.NullValueHandling.Ignore)]
        public Dictionary<string, double> Displacement { get; set; }

        /// <summary>Node velocity.</summary>
        [SNJ.JsonProperty("velocity", Required = SNJ.Required.Default, NullValueHandling = SNJ.NullValueHandling.Ignore)]
        public Dictionary<string, double> Velocity { get; set; }

        /// <summary>Node acceleration.</summary>
        [SNJ.JsonProperty("acceleration", Required = SNJ.Required.Default, NullValueHandling = SNJ.NullValueHandling.Ignore)]
        public Dictionary<string, double> Acceleration { get; set; }
    }

    [Serializable]
    public partial class Structural2DElementResult : SpeckleObject, IStructural
    {
        public override string Type { get => "Structural2DElementResult"; }

        /// <summary>Structural ID to reference from other objects.</summary>
        [SNJ.JsonProperty("structuralId", Required = SNJ.Required.Default, NullValueHandling = SNJ.NullValueHandling.Ignore)]
        public string StructuralId { get; set; }

        /// <summary>Indicates whether the results are in the global or local axis.</summary>
        [SNJ.JsonProperty("isGlobal", Required = SNJ.Required.Default, NullValueHandling = SNJ.NullValueHandling.Ignore)]
        public bool IsGlobal { get; set; }

        /// <summary>Element displacement.</summary>
        [SNJ.JsonProperty("displacement", Required = SNJ.Required.Default, NullValueHandling = SNJ.NullValueHandling.Ignore)]
        public Dictionary<string, double> Displacement { get; set; }

        /// <summary>Element force.</summary>
        [SNJ.JsonProperty("force", Required = SNJ.Required.Default, NullValueHandling = SNJ.NullValueHandling.Ignore)]
        public Dictionary<string, double> Force { get; set; }

        /// <summary>Element stress.</summary>
        [SNJ.JsonProperty("stress", Required = SNJ.Required.Default, NullValueHandling = SNJ.NullValueHandling.Ignore)]
        public Dictionary<string, object> Stress { get; set; }
    }
    #endregion
}
