using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SpeckleCore;
using SpeckleCoreGeometryClasses;
using SpeckleElements;

namespace SpeckleElementsGSA
{
  // Keyword set as MEMB to not clash with grouping of members
  [GSAObject("MEMB.7", new string[] { }, "elements", true, false, new Type[] { typeof(GSA2DElement) }, new Type[] { typeof(GSA2DProperty) })]
  public class GSA2DElementMesh : IGSASpeckleContainer
  {
    public string GWACommand { get; set; }
    public List<string> SubGWACommand { get; set; } = new List<string>();
    public dynamic Value { get; set; } = new Structural2DElementMesh();

    public void ParseGWACommand(GSAInterfacer GSA, List<GSA2DElement> elements)
    {
      if (elements.Count() < 1)
        return;

      Structural2DElementMesh obj = new Structural2DElementMesh();
      obj.Vertices = new List<double>();
      obj.Faces = new List<int>();
      obj.ElementStructuralId = new List<string>();

      obj.ElementType = elements.First().Value.ElementType;
      obj.PropertyRef = elements.First().Value.PropertyRef;
      obj.Axis = new List<StructuralAxis>();
      obj.Offset = new List<double>();

      foreach(GSA2DElement e in elements)
      {
        int verticesOffset = obj.Vertices.Count() / 3;
        obj.Vertices.AddRange(e.Value.Vertices);
        obj.Faces.Add((e.Value.Faces as List<int>).First());
        obj.Faces.AddRange((e.Value.Faces as List<int>).Skip(1).Select(x => x + verticesOffset));

        obj.Axis.Add(e.Value.Axis);
        obj.Offset.Add(e.Value.Offset);

        obj.ElementStructuralId.Add(e.Value.StructuralId);

        this.SubGWACommand.Add(e.GWACommand);
        this.SubGWACommand.AddRange(e.SubGWACommand);
      }

      this.Value = obj;
    }

    public void SetGWACommand(GSAInterfacer GSA)
    {
      if (this.Value == null)
        return;

      Structural2DElementMesh obj = this.Value as Structural2DElementMesh;

      int group = GSA.Indexer.ResolveIndex(typeof(GSA2DElementMesh), obj);

      Structural2DElement[] elements = obj.Explode();

      foreach (Structural2DElement element in elements)
      {
        if (Conversions.GSATargetLayer == GSATargetLayer.Analysis)
          new GSA2DElement() { Value = element }.SetGWACommand(GSA, group);
      }
    }
  }
  
  public static partial class Conversions
  {
    public static bool ToNative(this SpeckleMesh inputObject)
    {
      Structural2DElementMesh convertedObject = new Structural2DElementMesh();

      foreach (PropertyInfo p in convertedObject.GetType().GetProperties().Where(p => p.CanWrite))
      {
        PropertyInfo inputProperty = inputObject.GetType().GetProperty(p.Name);
        if (inputProperty != null)
          p.SetValue(convertedObject, inputProperty.GetValue(inputObject));
      }

      return convertedObject.ToNative();
    }

    public static bool ToNative(this Structural2DElementMesh mesh)
    {
      if (Conversions.GSATargetLayer == GSATargetLayer.Analysis)
        new GSA2DElementMesh() { Value = mesh }.SetGWACommand(GSA);
      else if (Conversions.GSATargetLayer == GSATargetLayer.Design)
        new GSA2DMember() { Value = mesh }.SetGWACommand(GSA);

      return true;
    }

    public static SpeckleObject ToSpeckle(this GSA2DElementMesh mesh)
    {
      return new SpeckleNull();
    }
  }
}
