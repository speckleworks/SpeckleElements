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
      if (Conversions.GSAElement2DResults.Count() == 0)
        return new SpeckleNull();

      if (!GSASenderObjects.ContainsKey(typeof(GSA2DElement)))
        return new SpeckleNull();

      List<GSA2DElement> elements = GSASenderObjects[typeof(GSA2DElement)].Cast<GSA2DElement>().ToList();

      foreach (KeyValuePair<string, Tuple<int, int, List<string>>> kvp in Conversions.GSAElement2DResults)
      {
        foreach (string loadCase in GSAResultCases)
        {
          if (!GSA.CaseExist(loadCase))
            continue;

          foreach (GSA2DElement element in elements)
          {
            int id = Convert.ToInt32(element.Value.StructuralId);

            if (element.Value.Result == null)
              element.Value.Result = new Dictionary<string, object>();

            var resultExport = GSA.GetGSAResult(id, kvp.Value.Item1, kvp.Value.Item2, kvp.Value.Item3, loadCase, GSAResultInLocalAxis ? "local" : "global");

            if (resultExport == null)
              continue;

            if (!element.Value.Result.ContainsKey(loadCase))
              element.Value.Result[loadCase] = new Structural2DElementResult()
              {
                Value = new Dictionary<string, object>()
              };

            // Let's split the dictionary into xxx_face and xxx_vertex
            var faceDictionary = resultExport.ToDictionary(
              x => x.Key,
              x => new List<double>() { (x.Value as List<double>).Last() } as object);
            var vertexDictionary = resultExport.ToDictionary(
              x => x.Key,
              x => (x.Value as List<double>).Take((x.Value as List<double>).Count - 1).ToList() as object);

            (element.Value.Result[loadCase] as Structural2DElementResult).Value[kvp.Key + "_face"] = faceDictionary;
            (element.Value.Result[loadCase] as Structural2DElementResult).Value[kvp.Key + "_vertex"] = vertexDictionary;
          }
        }
      }
      
      return new SpeckleObject();
    }
  }
}
