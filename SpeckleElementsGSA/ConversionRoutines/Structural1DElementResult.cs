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
      if (Conversions.GSAElement1DResults.Count() == 0)
        return new SpeckleNull();

      if (!GSASenderObjects.ContainsKey(typeof(GSA1DElement)))
        return new SpeckleNull();

      List<GSA1DElement> elements = GSASenderObjects[typeof(GSA1DElement)].Cast<GSA1DElement>().ToList();

      foreach (KeyValuePair<string, Tuple<int, int, List<string>>> kvp in Conversions.GSAElement1DResults)
      {
        foreach (string loadCase in GSAResultCases)
        {
          if (!GSA.CaseExist(loadCase))
            continue;

          foreach (GSA1DElement element in elements)
          {
            int id = Convert.ToInt32(element.Value.StructuralId);

            if (element.Value.Result == null)
              element.Value.Result = new Dictionary<string, object>();

            var resultExport = GSA.GetGSAResult(id, kvp.Value.Item1, kvp.Value.Item2, kvp.Value.Item3, loadCase, GSAResultInLocalAxis ? "local" : "global", Conversions.GSAResult1DNumPosition);
            
            if (resultExport == null)
              continue;

            if (!element.Value.Result.ContainsKey(loadCase))
              element.Value.Result[loadCase] = new Structural1DElementResult()
              {
                Value = new Dictionary<string, object>()
              };

            (element.Value.Result[loadCase] as Structural1DElementResult).Value[kvp.Key] = resultExport;
          }
        }
      }

      // Linear interpolate the line values
      foreach (GSA1DElement element in elements)
      {
        var dX = (element.Value.Value[3] - element.Value.Value[0]) / (Conversions.GSAResult1DNumPosition + 1);
        var dY = (element.Value.Value[4] - element.Value.Value[1]) / (Conversions.GSAResult1DNumPosition + 1);
        var dZ = (element.Value.Value[5] - element.Value.Value[2]) / (Conversions.GSAResult1DNumPosition + 1);

        var interpolatedVertices = new List<double>();
        interpolatedVertices.AddRange((element.Value.Value as List<double>).Take(3));
        
        for (int i = 1; i <= Conversions.GSAResult1DNumPosition; i++)
        {
          interpolatedVertices.Add(interpolatedVertices[0] + dX * i);
          interpolatedVertices.Add(interpolatedVertices[1] + dY * i);
          interpolatedVertices.Add(interpolatedVertices[2] + dZ * i);
        }

        interpolatedVertices.AddRange((element.Value.Value as List<double>).Skip(3).Take(3));

        element.Value.ResultVertices = interpolatedVertices;
      }

      return new SpeckleObject();
    }
  }
}
