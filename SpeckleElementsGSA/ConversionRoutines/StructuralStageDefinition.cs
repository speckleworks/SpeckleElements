using System;
using System.Collections.Generic;
using System.Linq;
using SpeckleCore;
using SpeckleElements;

namespace SpeckleElementsGSA
{
  [GSAObject("ANAL_STAGE.3", new string[] { "LIST.1" }, "elements", true, true, new Type[] { typeof(GSA1DElement), typeof(GSA2DElement), typeof(GSA1DMember), typeof(GSA2DMember) }, new Type[] { typeof(GSA1DMember), typeof(GSA2DMember) })]
  public class GSAStageDefinition : IGSASpeckleContainer
  {
    public int GSAId { get; set; }
    public string GWACommand { get; set; }
    public List<string> SubGWACommand { get; set; } = new List<string>();
    public dynamic Value { get; set; } = new StructuralStageDefinition();

    public void ParseGWACommand(GSAInterfacer GSA, List<GSA1DElement> e1Ds, List<GSA2DElement> e2Ds, List<GSA1DMember> m1Ds, List<GSA2DMember> m2Ds)
    {
      if (this.GWACommand == null)
        return;

      StructuralStageDefinition obj = new StructuralStageDefinition();

      string[] pieces = this.GWACommand.ListSplit("\t");

      int counter = 1; // Skip identifier

      this.GSAId = Convert.ToInt32(pieces[counter++]);
      obj.ApplicationId = GSA.GetSID(this.GetGSAKeyword(), this.GSAId);
      obj.Name = pieces[counter++];

      counter++; //Skip colour

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

      counter++; //Skip creep coefficient
      var intString = pieces[counter++];
      try
      {
        var converted = int.TryParse(intString, out int stageDays);
        if (converted)
        {
          obj.StageDays = stageDays;
        }
      }
      catch { }

      this.Value = obj;
    }

    public void SetGWACommand(GSAInterfacer GSA)
    {
      if (this.Value == null)
        return;

      StructuralStageDefinition stageDef = this.Value as StructuralStageDefinition;

      string keyword = typeof(GSAStageDefinition).GetGSAKeyword();
      var subkeywords = typeof(GSAStageDefinition).GetSubGSAKeyword();

      int index = GSA.Indexer.ResolveIndex(typeof(GSAStageDefinition), stageDef);

      var target = new List<int>();
      var targetString = " ";

      if (stageDef.ElementRefs != null && stageDef.ElementRefs.Count() > 0)
      {
        if (Conversions.GSATargetLayer == GSATargetLayer.Analysis)
        {
          var e1DIndices = GSA.Indexer.LookupIndices(typeof(GSA1DElement), stageDef.ElementRefs).Where(x => x.HasValue).Select(x => x.Value).ToList();
          var e2DIndices = GSA.Indexer.LookupIndices(typeof(GSA2DElement), stageDef.ElementRefs).Where(x => x.HasValue).Select(x => x.Value).ToList();
          target.AddRange(e1DIndices);
          target.AddRange(e2DIndices);
          targetString = string.Join(" ", target);
        }
        else if (Conversions.GSATargetLayer == GSATargetLayer.Design)
        {
          var m1DIndices = GSA.Indexer.LookupIndices(typeof(GSA1DMember), stageDef.ElementRefs).Where(x => x.HasValue).Select(x => x.Value).ToList();
          var m2DIndices = GSA.Indexer.LookupIndices(typeof(GSA2DMember), stageDef.ElementRefs).Where(x => x.HasValue).Select(x => x.Value).ToList();
          target.AddRange(m1DIndices);
          target.AddRange(m2DIndices);
          targetString = string.Join(" ", target.Select(x => "G" + x));
        }
      }

      var stageName = string.IsNullOrEmpty(stageDef.Name) ? " " : stageDef.Name;

      List<string> ls = new List<string>
        {
          // Set ANAL_STAGE
          "SET",
          keyword + ":" + GSA.GenerateSID(stageDef),
          index.ToString(),
          stageName, // Name
          "NO_RGB", // Colour
          targetString, //Elements by group name
          "0", //Creep factor
          stageDef.StageDays.ToString() // Stage
        };

      GSA.RunGWACommand(string.Join("\t", ls));
    }
  }

  public static partial class Conversions
  {
    public static bool ToNative(this StructuralStageDefinition stageDefinition)
    {
      var gsaStageDefinition = new GSAStageDefinition() { Value = stageDefinition };

      gsaStageDefinition.SetGWACommand(GSA);

      return true;
    }

    public static SpeckleObject ToSpeckle(this GSAStageDefinition dummyObject)
    {
      if (!GSASenderObjects.ContainsKey(typeof(GSAStageDefinition)))
        GSASenderObjects[typeof(GSAStageDefinition)] = new List<object>();

      List<GSAStageDefinition> stageDefs = new List<GSAStageDefinition>();
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

      string keyword = typeof(GSAStageDefinition).GetGSAKeyword();
      string[] subKeywords = typeof(GSAStageDefinition).GetSubGSAKeyword();

      string[] lines = GSA.GetGWARecords("GET_ALL\t" + keyword);
      List<string> deletedLines = GSA.GetDeletedGWARecords("GET_ALL\t" + keyword).ToList();
      foreach (string k in subKeywords)
        deletedLines.AddRange(GSA.GetDeletedGWARecords("GET_ALL\t" + k));

      // Remove deleted lines
      GSASenderObjects[typeof(GSAStageDefinition)].RemoveAll(l => deletedLines.Contains((l as IGSASpeckleContainer).GWACommand));
      foreach (KeyValuePair<Type, List<object>> kvp in GSASenderObjects)
        kvp.Value.RemoveAll(l => (l as IGSASpeckleContainer).SubGWACommand.Any(x => deletedLines.Contains(x)));

      // Filter only new lines
      string[] prevLines = GSASenderObjects[typeof(GSAStageDefinition)].Select(l => (l as IGSASpeckleContainer).GWACommand).ToArray();
      string[] newLines = lines.Where(l => !prevLines.Contains(l)).ToArray();

      foreach (string p in newLines)
      {
        GSAStageDefinition combo = new GSAStageDefinition() { GWACommand = p };
        combo.ParseGWACommand(GSA, e1Ds, e2Ds, m1Ds, m2Ds);
        stageDefs.Add(combo);
      }

      GSASenderObjects[typeof(GSAStageDefinition)].AddRange(stageDefs);

      if (stageDefs.Count() > 0 || deletedLines.Count() > 0) return new SpeckleObject();

      return new SpeckleNull();
    }
  }
}
