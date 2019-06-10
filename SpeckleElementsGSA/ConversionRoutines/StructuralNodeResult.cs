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
  [GSAObject("", new string[] { }, "elements", true, false, new Type[] { typeof(GSANode) }, new Type[] { })]
  public class GSANodeResult : IGSASpeckleContainer
  {
    public string GWACommand { get; set; }
    public List<string> SubGWACommand { get; set; } = new List<string>();
    public dynamic Value { get; set; } = new StructuralNodeResult();
  }


  public static partial class Conversions
  {
    public static SpeckleObject ToSpeckle(this GSANodeResult dummyObject)
    {
      if (Conversions.GSANodalResults.Count() == 0)
        return new SpeckleNull();

      if (!GSASenderObjects.ContainsKey(typeof(GSANode)))
        return new SpeckleNull();

      List<GSANode> nodes = GSASenderObjects[typeof(GSANode)].Cast<GSANode>().ToList();

      foreach(KeyValuePair<string, Tuple<int, int, List<string>>> kvp in Conversions.GSANodalResults)
      {
        foreach (string loadCase in GSAResultCases)
        {
          if (!GSA.CaseExist(loadCase))
            continue;

          foreach (GSANode node in nodes)
          {
            int id = Convert.ToInt32(node.Value.StructuralId);

            if (node.Value.Result == null)
              node.Value.Result = new Dictionary<string, object>();

            var resultExport = GSA.GetGSAResult(id, kvp.Value.Item1, kvp.Value.Item2, kvp.Value.Item3, loadCase, GSAResultInLocalAxis ? "local" : "global");

            if (resultExport == null)
              continue;

            if (!node.Value.Result.ContainsKey(loadCase))
              node.Value.Result[loadCase] = new StructuralNodeResult()
              {
                Value = new Dictionary<string, object>()
              };
            (node.Value.Result[loadCase] as StructuralNodeResult).Value[kvp.Key] = resultExport;

            node.ForceSend = true;
          }
        }
      }
    
      return new SpeckleObject();
    }
  }
}
