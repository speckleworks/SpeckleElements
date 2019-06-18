using SpeckleCore;
using SpeckleElements;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpeckleElementsGSA
{
  [GSAObject("ASSEMBLY.2", new string[] { }, "loads", true, true, new Type[] { typeof(GSANode), typeof(GSA2DMember) }, new Type[] { typeof(GSANode), typeof(GSA2DMember) })]
  public class GSAAssembly : IGSASpeckleContainer
  {
    public string GWACommand { get; set; }
    public List<string> SubGWACommand { get; set; } = new List<string>();
    public dynamic Value { get; set; } = new StructuralAssembly();

    public void ParseGWACommand(GSAInterfacer GSA, List<GSANode> nodes, List<GSA2DMember> members)
    {
      if (this.GWACommand == null)
        return;

      var obj = new StructuralAssembly();

      var pieces = this.GWACommand.ListSplit(",");

      var counter = 1; // Skip identifier
      obj.StructuralId = pieces[counter++];
      obj.Name = pieces[counter++].Trim(new char[] { '"' });

      var groupIds = GSA.GetGroupsFromGSAList(pieces[counter++]).ToList();

      if (members != null && members.Count() > 0)
      {
        var memberRefs = new List<string>();
        for (var i = 0; i < groupIds.Count(); i++)
        {
          var member = members.Where(n => n.Group == groupIds[i]).FirstOrDefault();
          memberRefs.Add(member.Value.StructuralId);
        }
        obj.MemberRefs = memberRefs;
      }
      counter++; //TOPO
      var node1 = pieces[counter++];
      var node2 = pieces[counter++];

      this.Value = obj;
    }

    public void SetGWACommand(GSAInterfacer GSA)
    {
      if (this.Value == null)
        return;

      Type destType = typeof(GSAAssembly);

      StructuralAssembly assembly = this.Value as StructuralAssembly;

      string keyword = destType.GetGSAKeyword();

      int index = GSA.Indexer.ResolveIndex(destType, assembly);
      var memberIndices = GSA.Indexer.LookupIndices(typeof(GSA2DMember), assembly.MemberRefs);

      List<int> nodeIndices = new List<int>();
      for (int i = 0; i < assembly.Value.Count(); i += 3)
      {
        nodeIndices.Add(GSA.NodeAt(assembly.Value[i], assembly.Value[i + 1], assembly.Value[i + 2], Conversions.GSACoincidentNodeAllowance));
      }

      var numPoints = (assembly.NumPoints == 0) ? 10 : assembly.NumPoints;

      //The width parameter is intentionally not being used here as the meaning doesn't map to the y coordinate parameter of the ASSEMBLY keyword
      //It is therefore to be ignored here for GSA purposes.

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
    public static bool ToNative(this StructuralAssembly assembly)
    {
      new GSAAssembly() { Value = assembly }.SetGWACommand(GSA);

      return true;
    }

    public static SpeckleObject ToSpeckle(this GSAAssembly dummyObject)
    {
      Type objType = dummyObject.GetType();

      if (!GSASenderObjects.ContainsKey(objType))
        GSASenderObjects[objType] = new List<object>();

      //Get all relevant GSA entities in this entire model
      var assemblies = new List<GSAAssembly>();
      var nodes = GSASenderObjects[typeof(GSANode)].Cast<GSANode>().ToList();
      var members = new List<GSA2DMember>();
      if (GSASenderObjects.ContainsKey(typeof(GSA2DMember)))
      {
        members = GSASenderObjects[typeof(GSA2DMember)].Cast<GSA2DMember>().ToList();
      }

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
        GSAAssembly assembly = new GSAAssembly() { GWACommand = p };
        //Pass in ALL the nodes and members - the Parse_ method will search through them
        assembly.ParseGWACommand(GSA, nodes, members);
        assemblies.Add(assembly);
      }

      GSASenderObjects[objType].AddRange(assemblies);

      if (assemblies.Count() > 0 || deletedLines.Count() > 0) return new SpeckleObject();

      return new SpeckleNull();
    }
  }
}
