using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpeckleCore;
using SpeckleCoreGeometryClasses;
using Newtonsoft.Json;

namespace SpeckleElements
{
  public partial class StructuralAssembly
  {
    public StructuralAssembly() { }

    public StructuralAssembly(double[] value, string[] elementRefs, SpeckleLine baseLine, SpecklePoint orientationPoint, int numPoints = 0, string applicationId = null, double width = 0, Dictionary<string, object> properties = null)
    {
      this.Properties = properties;
      this.ApplicationId = applicationId;
      this.ElementRefs = elementRefs.ToList();
      this.Value = value.ToList();
      this.OrientationPoint = orientationPoint;
      this.NumPoints = numPoints;
      this.Width = width;
      this.BaseLine = baseLine;
      GenerateHash();
    }

    public override void Scale(double factor)
    {
      for (int i = 0; i < this.Value.Count(); i++)
      {
        this.Value[i] *= factor;
      }
      for (int i = 0; i < this.OrientationPoint.Value.Count(); i++)
      {
        this.OrientationPoint.Value[i] *= factor;
      }

      this.Properties = ScaleProperties(this.Properties, factor);
      this.GenerateHash();
    }
  }

  public partial class StructuralConstructionStage
  {
    public StructuralConstructionStage() { }

    public StructuralConstructionStage(string[] elementRefs, int stageDays, string applicationId = null, Dictionary<string, object> properties = null)
    {
      this.ElementRefs = elementRefs.ToList();
      this.StageDays = stageDays;
      this.ApplicationId = applicationId;
      this.Properties = properties;

      this.GenerateHash();
    }

    public override void Scale(double factor)
    {
      this.Properties = ScaleProperties(this.Properties, factor);
      this.GenerateHash();
    }
  }

  public partial class StructuralStagedNodalRestraints
  {
    public StructuralStagedNodalRestraints() { }

    public StructuralStagedNodalRestraints(StructuralVectorBoolSix restraint, string[] nodeRefs, string[] constructionStageRefs, string applicationId = null, Dictionary<string, object> properties = null)
    {
      this.Restraint = restraint;
      this.NodeRefs = nodeRefs.ToList();
      this.ConstructionStageRefs = constructionStageRefs.ToList();
      this.ApplicationId = applicationId;
      this.Properties = properties;

      GenerateHash();
    }

    public override void Scale(double factor)
    {
      this.Properties = ScaleProperties(this.Properties, factor);
      this.GenerateHash();
    }
  }
}
