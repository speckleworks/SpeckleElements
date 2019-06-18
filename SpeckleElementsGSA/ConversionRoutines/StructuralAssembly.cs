using SpeckleCore;
using SpeckleElements;
using System;
using System.Collections.Generic;
using System.Linq;
using SpeckleCoreGeometryClasses;

namespace SpeckleElementsGSA
{
  [GSAObject("ASSEMBLY.2", new string[] { }, "loads", true, true, new Type[] { typeof(GSANode), typeof(GSA1DElement), typeof(GSA2DElement), typeof(GSA1DMember), typeof(GSA2DMember) }, new Type[] { typeof(GSANode), typeof(GSA1DElement), typeof(GSA2DElement), typeof(GSA1DMember), typeof(GSA2DMember) })]
  public class GSAAssembly : IGSASpeckleContainer
  {
    public int GSAId { get; set; }
    public string GWACommand { get; set; }
    public List<string> SubGWACommand { get; set; } = new List<string>();
    public dynamic Value { get; set; } = new StructuralAssembly();

    public void ParseGWACommand(GSAInterfacer GSA, List<GSANode> nodes, List<GSA1DElement> e1Ds, List<GSA2DElement> e2Ds, List<GSA1DMember> m1Ds, List<GSA2DMember> m2Ds)
    {
      if (this.GWACommand == null)
        return;

      var obj = new StructuralAssembly();

      var pieces = this.GWACommand.ListSplit("\t");

      var counter = 1; // Skip identifier

      this.GSAId = Convert.ToInt32(pieces[counter++]);
      obj.ApplicationId = GSA.GetSID(this.GetGSAKeyword(), this.GSAId);
      obj.Name = pieces[counter++].Trim(new char[] { '"' });

      var elementList = pieces[counter++];

      obj.ElementRefs = new List<string>();

      if (Conversions.GSATargetLayer == GSATargetLayer.Analysis)
      {
        var elementId = elementList.ListSplit(" ").Where(x => !x.StartsWith("G")).Select(x => Convert.ToInt32(x));
        foreach (int id in elementId)
        {
          object elem = e1Ds.Where(e => e.GSAId == id).FirstOrDefault();

          if (elem == null)
            elem = e2Ds.Where(e => e.GSAId == id).FirstOrDefault();

          if (elem == null)
            continue;

          obj.ElementRefs.Add((elem as SpeckleObject).ApplicationId);
          this.SubGWACommand.Add((elem as IGSASpeckleContainer).GWACommand);
        }
      }
      else
      {
        var groupIds = GSA.GetGroupsFromGSAList(elementList).ToList();
        foreach (int id in groupIds)
        {
          var memb1Ds = m1Ds.Where(m => m.Group == id);
          var memb2Ds = m2Ds.Where(m => m.Group == id);
          
          obj.ElementRefs.AddRange(memb1Ds.Select(m => (string)m.Value.ApplicationId));
          obj.ElementRefs.AddRange(memb2Ds.Select(m => (string)m.Value.ApplicationId));
          this.SubGWACommand.AddRange(memb1Ds.Select(m => m.GWACommand));
          this.SubGWACommand.AddRange(memb2Ds.Select(m => m.GWACommand));
        }
      }

      counter++; //TOPO

      obj.Value = new List<double>();
      for (int i = 0; i < 2; i++)
      {
        string key = pieces[counter++];
        GSANode node = nodes.Where(n => n.GSAId == Convert.ToInt32(key)).FirstOrDefault();
        obj.Value.AddRange(node.Value.Value);
        this.SubGWACommand.Add(node.GWACommand);
      }
      var orientationNodeId = Convert.ToInt32(pieces[counter++]);
      GSANode orientationNode = nodes.Where(n => n.GSAId == orientationNodeId).FirstOrDefault();
      this.SubGWACommand.Add(orientationNode.GWACommand);
      obj.OrientationPoint = new SpecklePoint(orientationNode.Value.Value[0], orientationNode.Value.Value[1], orientationNode.Value.Value[2]);

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

      var target = new List<int>();
      var targetString = " ";

      if (assembly.ElementRefs != null && assembly.ElementRefs.Count() > 0)
      { 
        if (Conversions.GSATargetLayer == GSATargetLayer.Analysis)
        {
          var e1DIndices = GSA.Indexer.LookupIndices(typeof(GSA1DElement), assembly.ElementRefs).Where(x => x.HasValue).Select(x => x.Value).ToList();
          var e2DIndices = GSA.Indexer.LookupIndices(typeof(GSA2DElement), assembly.ElementRefs).Where(x => x.HasValue).Select(x => x.Value).ToList();
          target.AddRange(e1DIndices);
          target.AddRange(e2DIndices);
          targetString = string.Join(" ", target);
        }
        else if (Conversions.GSATargetLayer == GSATargetLayer.Design)
        {
          var m1DIndices = GSA.Indexer.LookupIndices(typeof(GSA1DMember), assembly.ElementRefs).Where(x => x.HasValue).Select(x => x.Value).ToList();
          var m2DIndices = GSA.Indexer.LookupIndices(typeof(GSA2DMember), assembly.ElementRefs).Where(x => x.HasValue).Select(x => x.Value).ToList();
          target.AddRange(m1DIndices);
          target.AddRange(m2DIndices);
          targetString = string.Join(" ", target.Select( x => "G" + x));
        }
      }

      List<int> nodeIndices = new List<int>();
      for (int i = 0; i < assembly.Value.Count(); i += 3)
        nodeIndices.Add(GSA.NodeAt(assembly.Value[i], assembly.Value[i + 1], assembly.Value[i + 2], Conversions.GSACoincidentNodeAllowance));

      var numPoints = (assembly.NumPoints == 0) ? 10 : assembly.NumPoints;

      List<string> ls = new List<string>
        {
          "SET",
          keyword + ":" + GSA.GenerateSID(assembly),
          index.ToString(),
          string.IsNullOrEmpty(assembly.Name) ? "" : assembly.Name,
          targetString,
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
      var e1Ds = new List<GSA1DElement>();
      var e2Ds = new List<GSA2DElement>();
      var m1Ds = new List<GSA1DMember>();
      var m2Ds = new List<GSA2DMember>();

      if (Conversions.GSATargetLayer == GSATargetLayer.Analysis)
      {
        e1Ds = GSASenderObjects[typeof(GSA1DElement)].Cast<GSA1DElement>().ToList();
        e2Ds = GSASenderObjects[typeof(GSA2DElement)].Cast<GSA2DElement>().ToList();
      }
      else if (Conversions.GSATargetLayer == GSATargetLayer.Design)
      {
        m1Ds = GSASenderObjects[typeof(GSA1DMember)].Cast<GSA1DMember>().ToList();
        m2Ds = GSASenderObjects[typeof(GSA2DMember)].Cast<GSA2DMember>().ToList();
      }

      string keyword = objType.GetGSAKeyword();
      string[] subKeywords = objType.GetSubGSAKeyword();

      string[] lines = GSA.GetGWARecords("GET_ALL\t" + keyword);
      List<string> deletedLines = GSA.GetDeletedGWARecords("GET_ALL\t" + keyword).ToList();
      foreach (string k in subKeywords)
        deletedLines.AddRange(GSA.GetDeletedGWARecords("GET_ALL\t" + k));

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
        assembly.ParseGWACommand(GSA, nodes, e1Ds, e2Ds, m1Ds, m2Ds);
        assemblies.Add(assembly);
      }

      GSASenderObjects[objType].AddRange(assemblies);

      if (assemblies.Count() > 0 || deletedLines.Count() > 0) return new SpeckleObject();

      return new SpeckleNull();
    }
  }
}
