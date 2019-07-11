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
  [GSAObject("", new string[] { }, "results", true, false, new Type[] { }, new Type[] { })]
  public class GSAMiscResult : IGSASpeckleContainer
  {
    public int GSAId { get; set; }
    public string GWACommand { get; set; }
    public List<string> SubGWACommand { get; set; } = new List<string>();
    public dynamic Value { get; set; } = new StructuralMiscResult();
  }

  public static partial class Conversions
  {
    public static SpeckleObject ToSpeckle(this GSAMiscResult dummyObject)
    {
      GSASenderObjects[typeof(GSAMiscResult)] = new List<object>();

      var loadCaseNames = new Dictionary<string, string>();

      //The "A"s
      var loadtasks = GSA.GetSplitGWARecordsByKeyword((typeof(GSALoadTask)).GetGSAKeyword());
      if (loadtasks != null && loadtasks.Length > 0)
      {
        for (var i = 0; i < loadtasks.Count(); i++)
        {
          if (loadtasks[i].Length > 2)
          {
            loadCaseNames.Add("A" + loadtasks[i][1], loadtasks[i][2]);
          }
        }
          
      }

      //The "C"s
      var combotasks = GSA.GetSplitGWARecordsByKeyword((typeof(GSALoadCombo)).GetGSAKeyword());
      if (combotasks != null && combotasks.Length > 0)
      {
        for (var i = 0; i < combotasks.Count(); i++)
        {
          if (combotasks[i].Length > 2)
          {
            loadCaseNames.Add("C" + combotasks[i][1], combotasks[i][2]);
          }
        }
      }

      if (Conversions.GSAMiscResults.Count() == 0)
        return new SpeckleNull();

      List<GSAMiscResult> results = new List<GSAMiscResult>();

      foreach (KeyValuePair<string, Tuple<string, int, int, List<string>>> kvp in Conversions.GSAMiscResults)
      {
        foreach (string loadCase in GSAResultCases)
        {
          if (!GSA.CaseExist(loadCase))
            continue;

          int id = 0;
          int highestIndex = 0;

          var loadCaseName = (loadCaseNames.ContainsKey(loadCase)) ? loadCaseNames[loadCase] : "";

          if (!string.IsNullOrEmpty(kvp.Value.Item1))
          {
            highestIndex = (int)GSA.RunGWACommand("HIGHEST\t" + kvp.Value.Item1);
            id = 1;
          }

          while (id <= highestIndex)
          {
            if (id == 0 || (int)GSA.RunGWACommand("EXIST\t" + kvp.Value.Item1 + "\t" + id.ToString()) == 1)
            {
              var resultExport = GSA.GetGSAResult(id, kvp.Value.Item2, kvp.Value.Item3, kvp.Value.Item4, loadCase, GSAResultInLocalAxis ? "local" : "global");

              if (resultExport == null)
              {
                id++;
                continue;
              }
              
              StructuralMiscResult newRes = new StructuralMiscResult();
              newRes.Description = kvp.Key;
              if (id != 0)
                newRes.TargetRef = GSA.GetSID(kvp.Value.Item1, id);
              newRes.IsGlobal = !GSAResultInLocalAxis;
              newRes.Value = resultExport;
							newRes.LoadCaseRef = loadCaseName;
							newRes.GenerateHash();
              results.Add(new GSAMiscResult() { Value = newRes });
            }
            id++;
          }
        }
      }

      GSASenderObjects[typeof(GSAMiscResult)].AddRange(results);

      return new SpeckleObject();
    }
  }
}
