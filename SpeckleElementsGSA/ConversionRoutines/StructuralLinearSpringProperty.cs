using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpeckleCore;
using SpeckleElements;

namespace SpeckleElementsGSA
{
  [GSAObject("ASSEMBLY.2", new string[] { }, "loads", true, true, new Type[] { }, new Type[] { })]
  public class GSALinearSpringProperty : IGSASpeckleContainer
  {
    public string GWACommand { get; set; }
    public List<string> SubGWACommand { get; set; } = new List<string>();
    public dynamic Value { get; set; } = new StructuralLinearSpringProperty();

    public void ParseGWACommand(GSAInterfacer GSA)
    {
      if (this.GWACommand == null)
        return;

      var obj = new StructuralAssembly();

      var pieces = this.GWACommand.ListSplit(",");

      var counter = 1; // Skip identifier
      obj.StructuralId = pieces[counter++];
      obj.Name = pieces[counter++].Trim(new char[] { '"' });

      this.Value = obj;
    }

    public void SetGWACommand(GSAInterfacer GSA)
    {
      if (this.Value == null)
        return;

      Type destType = typeof(GSALinearSpringProperty);

      StructuralAssembly assembly = this.Value as StructuralAssembly;

      string keyword = destType.GetGSAKeyword();

      int index = GSA.Indexer.ResolveIndex(destType, assembly);
      var memberIndices = GSA.Indexer.LookupIndices(typeof(GSA2DMember), assembly.MemberRefs);

      List<int> nodeIndices = new List<int>();
      for (int i = 0; i < assembly.Value.Count(); i += 3)
      {
        nodeIndices.Add(GSA.NodeAt(assembly.Value[i], assembly.Value[i + 1], assembly.Value[i + 2], Conversions.GSACoincidentNodeAllowance));
      }

      var numPoints = (assembly.NumPoints == 0) ? GSAInterfacer.DefaultAssemblyPoints : assembly.NumPoints;

      List<string> ls = new List<string>
        {
          "SET",
          keyword + ":" + GSA.GenerateSID(assembly),
          index.ToString(),
          string.IsNullOrEmpty(assembly.Name) ? "" : assembly.Name,
          string.Join(" ", memberIndices.Select(i => "G" + i)),
          "TOPO",
          nodeIndices[0].ToString(),
          nodeIndices[1].ToString(),
          GSA.NodeAt(assembly.OrientationPoint.Value[0], assembly.OrientationPoint.Value[1], assembly.OrientationPoint.Value[2], Conversions.GSACoincidentNodeAllowance).ToString(),
          "", //Empty list for int_topo as it assumed that the line is never curved
          "LAGRANGE",
          "0", //Curve order - reserved for future use according to the documentation
          "POINTS",
          numPoints.ToString() //Number of points
        };

      GSA.RunGWACommand(string.Join("\t", ls));
    }
  }

  public static partial class Conversions
  {
    public static bool ToNative(this StructuralLinearSpringProperty assembly)
    {
      if (Conversions.GSATargetLayer == GSATargetLayer.Analysis)
        new GSALinearSpringProperty() { Value = assembly }.SetGWACommand(GSA);
      else if (Conversions.GSATargetLayer == GSATargetLayer.Design)
        new GSALinearSpringProperty() { Value = assembly }.SetGWACommand(GSA);

      return true;
    }

    public static SpeckleObject ToSpeckle(this GSALinearSpringProperty dummyObject)
    {
      Type objType = dummyObject.GetType();

      if (!GSASenderObjects.ContainsKey(objType))
        GSASenderObjects[objType] = new List<object>();

      //Get all relevant GSA entities in this entire model
      var springProperties = new List<GSALinearSpringProperty>();

      string keyword = objType.GetGSAKeyword();
      string[] subKeywords = objType.GetSubGSAKeyword();

      string[] lines = GSA.GetGWARecords("GET_ALL," + keyword);
      List<string> deletedLines = GSA.GetDeletedGWARecords("GET_ALL," + keyword).ToList();
      foreach (string k in subKeywords)
        deletedLines.AddRange(GSA.GetDeletedGWARecords("GET_ALL," + k));

      // Remove deleted lines
      GSASenderObjects[objType].RemoveAll(l => deletedLines.Contains((l as IGSASpeckleContainer).GWACommand));
      foreach (KeyValuePair<Type, List<object>> kvp in GSASenderObjects)
        kvp.Value.RemoveAll(l => (l as IGSASpeckleContainer).SubGWACommand.Any(x => deletedLines.Contains(x)));

      // Filter only new lines
      string[] prevLines = GSASenderObjects[objType].Select(l => (l as IGSASpeckleContainer).GWACommand).ToArray();
      string[] newLines = lines.Where(l => !prevLines.Contains(l)).ToArray();

      foreach (string p in newLines)
      {
        var springProperty = new GSALinearSpringProperty() { GWACommand = p };
        //Pass in ALL the nodes and members - the Parse_ method will search through them
        springProperty.ParseGWACommand(GSA);
        springProperties.Add(springProperty);
      }

      GSASenderObjects[objType].AddRange(springProperties);

      if (springProperties.Count() > 0 || deletedLines.Count() > 0) return new SpeckleObject();

      return new SpeckleNull();
    }
  }
}
