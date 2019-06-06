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
  [GSAObject("", new string[] { }, "elements", true, false, new Type[] { typeof(GSA2DElement) }, new Type[] { })]
  public class GSA2DElementResult : IGSASpeckleContainer
  {
    public string GWACommand { get; set; }
    public List<string> SubGWACommand { get; set; } = new List<string>();
    public dynamic Value { get; set; } = new Structural2DElementResult();
  }

  public static partial class Conversions
  {
    public static SpeckleObject ToSpeckle(this GSA2DElementResult dummyObject)
    {
      if (!GSASendResults)
        return new SpeckleNull();

      if (!GSASenderObjects.ContainsKey(typeof(GSA2DElement)))
        return new SpeckleNull();

      List<GSA2DElement> elements = GSASenderObjects[typeof(GSA2DElement)].Cast<GSA2DElement>().ToList();
      List<GSA2DElementMesh> meshes = GSASenderObjects[typeof(GSA2DElementMesh)].Cast<GSA2DElementMesh>().ToList();

      // Note: A lot faster to extract by type of result

      // Extract displacements
      foreach (string loadCase in GSAResultCases)
      {
        if (!GSA.CaseExist(loadCase))
          continue;

        foreach (GSA2DElement element in elements)
        {
          int id = Convert.ToInt32(element.Value.StructuralId);

          if (element.Value.Result == null)
            element.Value.Result = new Dictionary<string, object>();

          var resultExport = GSA.Get2DElementDisplacements(id, loadCase, GSAResultInLocalAxis ? "local" : "global");

          if (resultExport == null)
            resultExport = new Dictionary<string, object>()
              {
                  {"x", new List<double>() { 0 } },
                  {"y", new List<double>() { 0 } },
                  {"z", new List<double>() { 0 } },
              };

          if (!element.Value.Result.ContainsKey(loadCase))
            element.Value.Result[loadCase] = new Structural2DElementResult();

          (element.Value.Result[loadCase] as Structural2DElementResult).Displacement = resultExport;
        }
      }

      // Extract forces
      foreach (string loadCase in GSAResultCases)
      {
        if (!GSA.CaseExist(loadCase))
          continue;

        foreach (GSA2DElement element in elements)
        {
          int id = Convert.ToInt32(element.Value.StructuralId);

          if (element.Value.Result == null)
            element.Value.Result = new Dictionary<string, object>();

          var resultExport = GSA.Get2DElementForces(id, loadCase, GSAResultInLocalAxis ? "local" : "global");

          if (resultExport == null)
            resultExport = new Dictionary<string, object>()
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

          if (!element.Value.Result.ContainsKey(loadCase))
            element.Value.Result[loadCase] = new Structural2DElementResult();

          (element.Value.Result[loadCase] as Structural2DElementResult).Force = resultExport;
        }
      }

      // Extract stresses
      foreach (string loadCase in GSAResultCases)
      {
        if (!GSA.CaseExist(loadCase))
          continue;

        foreach (GSA2DElement element in elements)
        {
          int id = Convert.ToInt32(element.Value.StructuralId);

          if (element.Value.Result == null)
            element.Value.Result = new Dictionary<string, object>();

          var resultExport = new Dictionary<string, object>() {
            { "bottom", GSA.Get2DElementStresses(id, loadCase, GSAResultInLocalAxis ? "local" : "global", GSA2DElementLayer.Bottom) },
            { "middle", GSA.Get2DElementStresses(id, loadCase, GSAResultInLocalAxis ? "local" : "global", GSA2DElementLayer.Middle) },
            { "top", GSA.Get2DElementStresses(id, loadCase, GSAResultInLocalAxis ? "local" : "global", GSA2DElementLayer.Top) },
          };

          if (!resultExport.Values.Any(x => x != null))
            resultExport = new Dictionary<string, object>()
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

          if (!element.Value.Result.ContainsKey(loadCase))
            element.Value.Result[loadCase] = new Structural2DElementResult();

          (element.Value.Result[loadCase] as Structural2DElementResult).Stress = resultExport;
        }
      }

      return new SpeckleObject();
    }
  }
}
