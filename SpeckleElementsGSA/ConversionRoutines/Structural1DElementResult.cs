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
  [GSAObject("", new string[] { }, "elements", true, false, new Type[] { typeof(GSA1DElement) }, new Type[] { })]
  public class GSA1DElementResult : IGSASpeckleContainer
  {
    public string GWACommand { get; set; }
    public List<string> SubGWACommand { get; set; } = new List<string>();
    public dynamic Value { get; set; } = new Structural1DElementResult();
  }

  public static partial class Conversions
  {
    public static SpeckleObject ToSpeckle(this GSA1DElementResult dummyObject)
    {
      if (!GSASendResults)
        return new SpeckleNull();

      if (!GSASenderObjects.ContainsKey(typeof(GSA1DElement)))
        return new SpeckleNull();

      List<GSA1DElement> elements = GSASenderObjects[typeof(GSA1DElement)].Cast<GSA1DElement>().ToList();

      // Note: A lot faster to extract by type of result

      // Extract displacements
      foreach (string loadCase in GSAResultCases)
      {
        if (!GSA.CaseExist(loadCase))
          continue;

        foreach (GSA1DElement element in elements)
        {
          int id = Convert.ToInt32(element.Value.StructuralId);

          if (element.Value.Result == null)
            element.Value.Result = new Dictionary<string, object>();

          var resultExport = GSA.Get1DElementDisplacements(id, loadCase, GSAResultInLocalAxis ? "local" : "global");

          if (resultExport == null)
            continue;

          if (!element.Value.Result.ContainsKey(loadCase))
            element.Value.Result[loadCase] = new Structural1DElementResult();

          (element.Value.Result[loadCase] as Structural1DElementResult).Displacement = resultExport;
        }
      }

      // Extract forces
      foreach (string loadCase in GSAResultCases)
      {
        if (!GSA.CaseExist(loadCase))
          continue;

        foreach (GSA1DElement element in elements)
        {
          int id = Convert.ToInt32(element.Value.StructuralId);

          if (element.Value.Result == null)
            element.Value.Result = new Dictionary<string, object>();

          var resultExport = GSA.Get1DElementForces(id, loadCase, GSAResultInLocalAxis ? "local" : "global");

          if (resultExport == null)
            continue;

          if (!element.Value.Result.ContainsKey(loadCase))
            element.Value.Result[loadCase] = new Structural1DElementResult();

          (element.Value.Result[loadCase] as Structural1DElementResult).Force = resultExport;
        }
      }

      // Extract stresses
      foreach (string loadCase in GSAResultCases)
      {
        if (!GSA.CaseExist(loadCase))
          continue;

        foreach (GSA1DElement element in elements)
        {
          int id = Convert.ToInt32(element.Value.StructuralId);

          if (element.Value.Result == null)
            element.Value.Result = new Dictionary<string, object>();

          var resultExport = GSA.Get1DElementStresses(id, loadCase, GSAResultInLocalAxis ? "local" : "global");

          if (resultExport == null)
            continue;

          if (!element.Value.Result.ContainsKey(loadCase))
            element.Value.Result[loadCase] = new Structural1DElementResult();

          (element.Value.Result[loadCase] as Structural1DElementResult).Stress = resultExport;
        }
      }

      return new SpeckleObject();
    }
  }
}
