using System;
using System.Collections.Generic;
using System.Linq;
using SpeckleCore;
using SpeckleElements;

namespace SpeckleElementsGSA
{
  [GSAObject("GEN_REST.2", new string[] { }, "elements", true, true, new Type[] { typeof(GSANode), typeof(GSAConstructionStage) }, new Type[] { typeof(GSANode), typeof(GSAConstructionStage) })]
  public class GSAGeneralisedNodeRestraints : IGSASpeckleContainer
  {
    public int GSAId { get; set; }
    public string GWACommand { get; set; }
    public List<string> SubGWACommand { get; set; } = new List<string>();
    public dynamic Value { get; set; } = new StructuralNodeRestraints();

    public void ParseGWACommand(GSAInterfacer GSA, List<GSANode> nodes, List<GSAConstructionStage> stages)
    {
      if (this.GWACommand == null)
        return;

      StructuralNodeRestraints obj = new StructuralNodeRestraints();

      string[] pieces = this.GWACommand.ListSplit("\t");

      int counter = 1; // Skip identifier

      this.GSAId = Convert.ToInt32(pieces[counter++]);
      obj.ApplicationId = GSA.GetSID(this.GetGSAKeyword(), this.GSAId);
      obj.Name = pieces[counter++];

      //Restraints
      var gwaRestraintBits = pieces[counter++].ListSplit(" ").Select(b => (b == "1") ? true : false).ToArray();
      obj.Restraint = new StructuralVectorBoolSix(gwaRestraintBits);

      var gwaNodeGsaIds = pieces[counter++].ListSplit(" ");
      obj.ElementRefs = nodes.Where(n => gwaNodeGsaIds.Any(gwaNodeId => gwaNodeId == n.GSAId.ToString())).Select(x => (string)x.Value.ApplicationId).ToList();

      var gwaStageDefGsaIds = pieces[counter++].ListSplit(" ");
      obj.StageDefinitionRefs = stages.Where(sd => gwaStageDefGsaIds.Any(gwaSDId => gwaSDId == sd.GSAId.ToString())).Select(x => (string)x.Value.ApplicationId).ToList();

      this.Value = obj;
    }

    public void SetGWACommand(GSAInterfacer GSA)
    {
      if (this.Value == null)
        return;

      StructuralNodeRestraints obj = this.Value as StructuralNodeRestraints;
			var destinationType = typeof(GSAGeneralisedNodeRestraints);

			string keyword = destinationType.GetGSAKeyword();
      var subkeywords = destinationType.GetSubGSAKeyword();

      int index = GSA.Indexer.ResolveIndex(destinationType, obj);

      var nodesStr = "all"; //default value
      if (obj.ElementRefs != null && obj.ElementRefs.Count() >= 1)
      {
        var nodeIndices = GSA.Indexer.LookupIndices(typeof(GSANode), obj.ElementRefs);
        nodeIndices.Sort();
        nodesStr = string.Join(" ", nodeIndices);
      }

      var stageDefStr = "all"; //default value
      if (obj.StageDefinitionRefs != null && obj.StageDefinitionRefs.Count() >= 1)
      {
        var stageDefIndices = GSA.Indexer.LookupIndices(typeof(GSAConstructionStage), obj.StageDefinitionRefs);
        stageDefIndices.Sort();
        stageDefStr = string.Join(" ", stageDefIndices);
      }

      var ls = new List<string>
        {
          "SET_AT",
          index.ToString(),
          keyword + ":" + GSA.GenerateSID(obj),
					string.IsNullOrEmpty(obj.Name) ? " " : obj.Name
				};

			ls.AddRange(obj.Restraint.Value.Select(v => v ? "1" : "0"));
			ls.Add(nodesStr);
			ls.Add(stageDefStr);

      GSA.RunGWACommand(string.Join("\t", ls));
    }
  }

  public static partial class Conversions
  {
    public static bool ToNative(this StructuralNodeRestraints nodeRestraints)
    {
      var gsaGeneralisedNodeRestraints = new GSAGeneralisedNodeRestraints() { Value = nodeRestraints };

      gsaGeneralisedNodeRestraints.SetGWACommand(GSA);

      return true;
    }

    public static SpeckleObject ToSpeckle(this GSAGeneralisedNodeRestraints dummyObject)
    {
      var destinationType = typeof(GSAGeneralisedNodeRestraints);

      if (!GSASenderObjects.ContainsKey(destinationType))
        GSASenderObjects[destinationType] = new List<object>();

      var genNodeRestraints = new List<GSAGeneralisedNodeRestraints>();

      var stageDefs = GSASenderObjects[typeof(GSAConstructionStage)].Cast<GSAConstructionStage>().ToList();
      var nodes = GSASenderObjects[typeof(GSANode)].Cast<GSANode>().ToList();

      string keyword = destinationType.GetGSAKeyword();
      string[] subKeywords = destinationType.GetSubGSAKeyword();

      string[] lines = GSA.GetGWARecords("GET_ALL\t" + keyword);
      List<string> deletedLines = GSA.GetDeletedGWARecords("GET_ALL\t" + keyword).ToList();
      foreach (string k in subKeywords)
        deletedLines.AddRange(GSA.GetDeletedGWARecords("GET_ALL\t" + k));

      // Remove deleted lines
      GSASenderObjects[destinationType].RemoveAll(l => deletedLines.Contains((l as IGSASpeckleContainer).GWACommand));
      foreach (var kvp in GSASenderObjects)
        kvp.Value.RemoveAll(l => (l as IGSASpeckleContainer).SubGWACommand.Any(x => deletedLines.Contains(x)));

      // Filter only new lines
      string[] prevLines = GSASenderObjects[destinationType].Select(l => (l as IGSASpeckleContainer).GWACommand).ToArray();
      string[] newLines = lines.Where(l => !prevLines.Contains(l)).ToArray();

      foreach (string p in newLines)
      {
        var genNodeRestraint = new GSAGeneralisedNodeRestraints() { GWACommand = p };
        genNodeRestraint.ParseGWACommand(GSA, nodes, stageDefs);
        genNodeRestraints.Add(genNodeRestraint);
      }

      GSASenderObjects[destinationType].AddRange(genNodeRestraints);

      if (genNodeRestraints.Count() > 0 || deletedLines.Count() > 0) return new SpeckleObject();

      return new SpeckleNull();
    }
  }
}
