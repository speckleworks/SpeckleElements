using SpeckleCore;
using SpeckleElements;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpeckleElementsGSA
{
  [GSAObject("LOAD_GRAVITY.2", new string[] { }, "loads", true, true, new Type[] { typeof(GSALoadCase) }, new Type[] { typeof(GSALoadCase) })]
  public class GSAGravityLoading : IGSASpeckleContainer
  {
    public int Axis; // Store this temporarily to generate other loads

    public string GWACommand { get; set; }
    public List<string> SubGWACommand { get; set; } = new List<string>();
    public dynamic Value { get; set; } = new StructuralGravityLoading();

    public void ParseGWACommand(GSAInterfacer GSA)
    {
      if (this.GWACommand == null)
        return;

      StructuralGravityLoading obj = new StructuralGravityLoading();

      string[] pieces = this.GWACommand.ListSplit(",");

      int counter = 1; // Skip identifier
      obj.Name = pieces[counter++].Trim(new char[] { '"' });

      counter++; // Skip elements - assumed to always be "all" at this point int time

      obj.LoadCaseRef = pieces[counter++];

      var vector = new double[3];
      for (var i = 0; i < 3; i++)
        double.TryParse(pieces[counter++], out vector[i]);

      obj.GravityFactors = new StructuralVectorThree(vector);

      this.Value = obj;
    }

    public void SetGWACommand(GSAInterfacer GSA)
    {
      if (this.Value == null)
        return;

      StructuralGravityLoading load = this.Value as StructuralGravityLoading;

      if (load.GravityFactors == null)
        return;

      string keyword = typeof(GSAGravityLoading).GetGSAKeyword();

      int loadCaseIndex = 0;
      try
      {
        loadCaseIndex = GSA.Indexer.LookupIndex(typeof(GSALoadCase), load.LoadCaseRef).Value;
      }
      catch { loadCaseIndex = GSA.Indexer.ResolveIndex(typeof(GSALoadCase), load.LoadCaseRef); }

      int index = GSA.Indexer.ResolveIndex(typeof(GSAGravityLoading));

      var ls = new List<string>
        {
          "SET",
          keyword + ":" + GSA.GenerateSID(load),
          string.IsNullOrEmpty(load.Name) ? "" : load.Name,
          "all",
          loadCaseIndex.ToString(),
          load.GravityFactors.Value[0].ToString(),
          load.GravityFactors.Value[1].ToString(),
          load.GravityFactors.Value[2].ToString(),
        };

      GSA.RunGWACommand(string.Join("\t", ls));
    }
  }


  public static partial class Conversions
  {
    public static bool ToNative(this StructuralGravityLoading load)
    {
      new GSAGravityLoading() { Value = load }.SetGWACommand(GSA);

      return true;
    }

    public static SpeckleObject ToSpeckle(this GSAGravityLoading dummyObject)
    {
      Type objType = dummyObject.GetType();

      if (!GSASenderObjects.ContainsKey(typeof(GSAGravityLoading)))
        GSASenderObjects[typeof(GSAGravityLoading)] = new List<object>();

      List<GSAGravityLoading> loads = new List<GSAGravityLoading>();

      string keyword = typeof(GSAGravityLoading).GetGSAKeyword();
      string[] subKeywords = typeof(GSAGravityLoading).GetSubGSAKeyword();

      string[] lines = GSA.GetGWARecords("GET_ALL," + keyword);
      List<string> deletedLines = GSA.GetDeletedGWARecords("GET_ALL," + keyword).ToList();
      foreach (string k in subKeywords)
        deletedLines.AddRange(GSA.GetDeletedGWARecords("GET_ALL," + k));

      // Remove deleted lines
      GSASenderObjects[typeof(GSAGravityLoading)].RemoveAll(l => deletedLines.Contains((l as IGSASpeckleContainer).GWACommand));
      foreach (KeyValuePair<Type, List<object>> kvp in GSASenderObjects)
        kvp.Value.RemoveAll(l => (l as IGSASpeckleContainer).SubGWACommand.Any(x => deletedLines.Contains(x)));

      // Filter only new lines
      string[] prevLines = GSASenderObjects[typeof(GSAGravityLoading)].Select(l => (l as IGSASpeckleContainer).GWACommand).ToArray();
      string[] newLines = lines.Where(l => !prevLines.Contains(l)).ToArray();

      foreach (string p in newLines)
      {
        GSAGravityLoading load = new GSAGravityLoading() { GWACommand = p };
        //Pass in ALL the nodes and members - the Parse_ method will search through them
        load.ParseGWACommand(GSA);
        loads.Add(load);
      }

      GSASenderObjects[objType].AddRange(loads);

      if (loads.Count() > 0 || deletedLines.Count() > 0) return new SpeckleObject();

      return new SpeckleNull();
    }
  }
}
