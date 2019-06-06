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
  [GSAObject("MEMB.7", new string[] { }, "elements", true, false, new Type[] { typeof(GSA2DElement), typeof(GSA2DLoad), typeof(GSA2DElementResult) }, new Type[] { typeof(GSA2DProperty) })]
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

      foreach (GSA2DElement e in elements)
      {
        int verticesOffset = obj.Vertices.Count() / 3;
        obj.Vertices.AddRange(e.Value.Vertices);
        obj.Faces.Add((e.Value.Faces as List<int>).First());
        obj.Faces.AddRange((e.Value.Faces as List<int>).Skip(1).Select(x => x + verticesOffset));

        obj.Axis.Add(e.Value.Axis);
        obj.Offset.Add(e.Value.Offset);

        obj.ElementStructuralId.Add(e.Value.StructuralId);

        // Result merging
        if (obj.Result == null)
          obj.Result = new Dictionary<string, object>();

        foreach (string loadCase in e.Value.Result.Keys)
        {
          if (!obj.Result.ContainsKey(loadCase))
            obj.Result[loadCase] = new Structural2DElementResult();

          var resultExport = e.Value.Result[loadCase] as Structural2DElementResult;

          if (resultExport != null)
          {
            if ((obj.Result[loadCase] as Structural2DElementResult).Displacement == null)
              (obj.Result[loadCase] as Structural2DElementResult).Displacement = resultExport.Displacement;
            else
              foreach (string key in (obj.Result[loadCase] as Structural2DElementResult).Displacement.Keys)
                (obj.Result[loadCase] as Structural2DElementResult).Displacement[key].AddRange(resultExport.Displacement[key]);

            if ((obj.Result[loadCase] as Structural2DElementResult).Force == null)
              (obj.Result[loadCase] as Structural2DElementResult).Force = resultExport.Force;
            else
              foreach (string key in (obj.Result[loadCase] as Structural2DElementResult).Force.Keys)
                (obj.Result[loadCase] as Structural2DElementResult).Force[key].AddRange(resultExport.Force[key]);

            if ((obj.Result[loadCase] as Structural2DElementResult).Stress == null)
              (obj.Result[loadCase] as Structural2DElementResult).Stress = resultExport.Stress;
            else
              foreach (string layer in (obj.Result[loadCase] as Structural2DElementResult).Stress.Keys)
                foreach (string key in (obj.Result[loadCase] as Structural2DElementResult).Stress[layer].Keys)
                  (obj.Result[loadCase] as Structural2DElementResult).Stress[layer][key].AddRange(resultExport.Stress[layer][key]);
          }
          else
          {
            if ((obj.Result[loadCase] as Structural2DElementResult).Displacement == null)
              (obj.Result[loadCase] as Structural2DElementResult).Displacement = new Dictionary<string, List<double>>()
              {
                  {"x", new List<double>() { 0 } },
                  {"y", new List<double>() { 0 } },
                  {"z", new List<double>() { 0 } },
              };
            else
              foreach (string key in (obj.Result[loadCase] as Structural2DElementResult).Displacement.Keys)
                (obj.Result[loadCase] as Structural2DElementResult).Displacement[key].Add(0);

            if ((obj.Result[loadCase] as Structural2DElementResult).Force == null)
              (obj.Result[loadCase] as Structural2DElementResult).Force = new Dictionary<string, List<double>>()
                  {
                      {"nx", new List<double>() { 0 } },
                      {"ny", new List<double>() { 0 } },
                      {"nxy", new List<double>() { 0 } },
                      {"mx", new List<double>() { 0 } },
                      {"my", new List<double>() { 0 } },
                      {"mxy", new List<double>() { 0 } },
                      {"vx", new List<double>() { 0 } },
                      {"vy", new List<double>() { 0 } },
                  };
            else
              foreach (string key in (obj.Result[loadCase] as Structural2DElementResult).Force.Keys)
                (obj.Result[loadCase] as Structural2DElementResult).Force[key].Add(0);

            if ((obj.Result[loadCase] as Structural2DElementResult).Stress == null)
              (obj.Result[loadCase] as Structural2DElementResult).Stress = new Dictionary<string, Dictionary<string, List<double>>>()
                  {
                    { "bottom", new Dictionary<string, List<double>>() {
                      { "sxx", new List<double>() { 0 } },
                      {"syy", new List<double>() { 0 } },
                      {"tzx", new List<double>() { 0 } },
                      {"tzy", new List<double>() { 0 } },
                      {"txy", new List<double>() { 0 } },
                    } },
                    { "middle", new Dictionary<string, List<double>>() {
                      { "sxx", new List<double>() { 0 } },
                      {"syy", new List<double>() { 0 } },
                      {"tzx", new List<double>() { 0 } },
                      {"tzy", new List<double>() { 0 } },
                      {"txy", new List<double>() { 0 } },
                    } },
                    { "top", new Dictionary<string, List<double>>() {
                      { "sxx", new List<double>() { 0 } },
                      {"syy", new List<double>() { 0 } },
                      {"tzx", new List<double>() { 0 } },
                      {"tzy", new List<double>() { 0 } },
                      {"txy", new List<double>() { 0 } },
                    } },
                  };
            else
              foreach (string layer in (obj.Result[loadCase] as Structural2DElementResult).Stress.Keys)
                foreach (string key in (obj.Result[loadCase] as Structural2DElementResult).Stress[layer].Keys)
                  (obj.Result[loadCase] as Structural2DElementResult).Stress[layer][key].Add(0);
          }
        }

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

    public static SpeckleObject ToSpeckle(this GSA2DElementMesh dummyObject)
    {
      if (!GSASenderObjects.ContainsKey(typeof(GSA2DElementMesh)))
        GSASenderObjects[typeof(GSA2DElementMesh)] = new List<object>();

      List<GSA2DElementMesh> meshes = new List<GSA2DElementMesh>();

      // Perform mesh merging
      var uniqueMembers = new List<string>(GSASenderObjects[typeof(GSA2DElement)].Select(x => (x as GSA2DElement).Member).Distinct());
      foreach (string member in uniqueMembers)
      {
        var elementList = GSASenderObjects[typeof(GSA2DElement)].Where(x => (x as GSA2DElement).Member == member).Cast<GSA2DElement>().ToList();
        GSA2DElementMesh mesh = new GSA2DElementMesh();
        mesh.ParseGWACommand(GSA, elementList);
        meshes.Add(mesh);

        GSASenderObjects[typeof(GSA2DElement)].RemoveAll(x => elementList.Contains(x));
      }

      GSASenderObjects[typeof(GSA2DElementMesh)].AddRange(meshes);

      return new SpeckleNull(); // Return null because ToSpeckle method for GSA2DElement will handle this change
    }
  }
}
