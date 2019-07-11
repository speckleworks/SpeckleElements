using System;
using System.Collections.Generic;
using System.Linq;
using SpeckleCore;
using SpeckleElements;

namespace SpeckleElementsGSA
{
  [GSAObject("LOAD_2D_THERMAL.2", new string[] { }, "loads", true, true, new Type[] { typeof(GSA2DElement), typeof(GSA2DMember), typeof(GSALoadCase) }, new Type[] { typeof(GSA2DElement), typeof(GSA2DMember), typeof(GSALoadCase) })]
  public class GSA2DElementLoadingThermal : IGSASpeckleContainer
  {
    public int GSAId { get; set; }
    public string GWACommand { get; set; }
    public List<string> SubGWACommand { get; set; } = new List<string>();
    public dynamic Value { get; set; } = new Structural2DElementLoadingThermal();

    public void ParseGWACommand(GSAInterfacer GSA, List<GSA2DElement> e2Ds, List<GSA2DMember> m2Ds)
    {
      if (this.GWACommand == null)
        return;

      Structural2DElementLoadingThermal obj = new Structural2DElementLoadingThermal();

      string[] pieces = this.GWACommand.ListSplit("\t");

      int counter = 1; // Skip identifier

      this.GSAId = Convert.ToInt32(pieces[counter++]);
      obj.ApplicationId = GSA.GetSID(this.GetGSAKeyword(), this.GSAId);
      obj.Name = pieces[counter++];
      obj.LoadCaseRef = pieces[counter++];

      var elementList = pieces[counter++];

      obj.ElementRefs = new List<string>();

      if (Conversions.GSATargetLayer == GSATargetLayer.Analysis)
      {
        var elementId = elementList.ListSplit(" ").Where(x => !x.StartsWith("G")).Select(x => Convert.ToInt32(x));
        foreach (int id in elementId)
        {
          object elem = e2Ds.Where(e => e.GSAId == id).FirstOrDefault();

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
          var memb2Ds = m2Ds.Where(m => m.Group == id);

          obj.ElementRefs.AddRange(memb2Ds.Select(m => (string)m.Value.ApplicationId));
          this.SubGWACommand.AddRange(memb2Ds.Select(m => m.GWACommand));
        }
      }

      obj.LoadingType = GwaValueToThermalType(pieces[counter++]);
      if (obj.LoadingType == StructuralThermalLoadingType.Uniform)
      {
        if (double.TryParse(pieces[counter++], out double temperature))
        {
          obj.UniformTemperature = temperature;
        }
      }
      else 
      {
        if (double.TryParse(pieces[counter++], out var top) && double.TryParse(pieces[counter++], out var bottom))
        {
          obj.Positions.Add(new StructuralTemperatureInterval(top, bottom));
        }

        if (obj.LoadingType == StructuralThermalLoadingType.General)
        {
          for (var i = 0; i < 3; i++)
          {
            if (double.TryParse(pieces[counter++], out top) && double.TryParse(pieces[counter++], out bottom))
            {
              obj.Positions.Add(new StructuralTemperatureInterval(top, bottom));
            }
          }
        }
      }

      this.Value = obj;
    }

    private string ThermalTypeToGwaValue(StructuralThermalLoadingType loadingType)
    {
      switch(loadingType)
      {
        case StructuralThermalLoadingType.General: return "GEN";
        case StructuralThermalLoadingType.Gradient: return "DZ";
        case StructuralThermalLoadingType.Uniform: return "CONS";
      }
      return "";
    }

    private StructuralThermalLoadingType GwaValueToThermalType(string loadingType)
    {
      switch (loadingType)
      {
        case "GEN": return StructuralThermalLoadingType.General;
        case "DZ": return StructuralThermalLoadingType.Gradient;
        default: return StructuralThermalLoadingType.Uniform;
      }
    }

    public void SetGWACommand(GSAInterfacer GSA)
    {
      if (this.Value == null)
        return;

      Structural2DElementLoadingThermal loading = this.Value as Structural2DElementLoadingThermal;

      string keyword = typeof(GSA2DElementLoadingThermal).GetGSAKeyword();
      var subkeywords = typeof(GSA2DElementLoadingThermal).GetSubGSAKeyword();

      int index = GSA.Indexer.ResolveIndex(typeof(GSA2DElementLoadingThermal), loading);

      var target = new List<int>();
      var targetString = " ";

      if (loading.ElementRefs != null && loading.ElementRefs.Count() > 0)
      {
        if (Conversions.GSATargetLayer == GSATargetLayer.Analysis)
        {
          var e2DIndices = GSA.Indexer.LookupIndices(typeof(GSA2DElement), loading.ElementRefs).Where(x => x.HasValue).Select(x => x.Value).ToList();
          target.AddRange(e2DIndices);
          targetString = string.Join(" ", target);
        }
        else if (Conversions.GSATargetLayer == GSATargetLayer.Design)
        {
          var m2DIndices = GSA.Indexer.LookupIndices(typeof(GSA2DMember), loading.ElementRefs).Where(x => x.HasValue).Select(x => x.Value).ToList();
          target.AddRange(m2DIndices);
          targetString = string.Join(" ", target.Select(x => "G" + x));
        }
      }

      int? loadCaseRef = GSA.Indexer.LookupIndex(typeof(GSALoadCase), loading.LoadCaseRef);

      var loadingName = string.IsNullOrEmpty(loading.Name) ? " " : loading.Name;

      List<string> ls = new List<string>
        {
          "SET_AT",
          index.ToString(),
          keyword + ":" + GSA.GenerateSID(loading),
          loadingName, // Name
          targetString, //Elements
					(loadCaseRef.HasValue) ? loadCaseRef.Value.ToString() : "1",
          ThermalTypeToGwaValue(loading.LoadingType)
        };

      if (loading.LoadingType == StructuralThermalLoadingType.Uniform)
      {
        ls.Add(loading.UniformTemperature.ToString());
      }
      else
      {
        for (var i = 0; i < loading.Positions.Count(); i++)
        {
          ls.Add(loading.Positions[i].Top.ToString());
          ls.Add(loading.Positions[i].Bottom.ToString());
        }
      }

      GSA.RunGWACommand(string.Join("\t", ls));
    }
  }

  public static partial class Conversions
  {
    public static bool ToNative(this Structural2DElementLoadingThermal stageDefinition)
    {
      var GSA2DElementLoadingThermal = new GSA2DElementLoadingThermal() { Value = stageDefinition };

      GSA2DElementLoadingThermal.SetGWACommand(GSA);

      return true;
    }

    public static SpeckleObject ToSpeckle(this GSA2DElementLoadingThermal dummyObject)
    {
      if (!GSASenderObjects.ContainsKey(typeof(GSA2DElementLoadingThermal)))
        GSASenderObjects[typeof(GSA2DElementLoadingThermal)] = new List<object>();

      List<GSA2DElementLoadingThermal> stageDefs = new List<GSA2DElementLoadingThermal>();
      var e2Ds = new List<GSA2DElement>();
      var m2Ds = new List<GSA2DMember>();

      if (Conversions.GSATargetLayer == GSATargetLayer.Analysis)
      {
        e2Ds = GSASenderObjects[typeof(GSA2DElement)].Cast<GSA2DElement>().ToList();
      }
      else if (Conversions.GSATargetLayer == GSATargetLayer.Design)
      {
        m2Ds = GSASenderObjects[typeof(GSA2DMember)].Cast<GSA2DMember>().ToList();
      }

      string keyword = typeof(GSA2DElementLoadingThermal).GetGSAKeyword();
      string[] subKeywords = typeof(GSA2DElementLoadingThermal).GetSubGSAKeyword();

      string[] lines = GSA.GetGWARecords("GET_ALL\t" + keyword);
      List<string> deletedLines = GSA.GetDeletedGWARecords("GET_ALL\t" + keyword).ToList();
      foreach (string k in subKeywords)
        deletedLines.AddRange(GSA.GetDeletedGWARecords("GET_ALL\t" + k));

      // Remove deleted lines
      GSASenderObjects[typeof(GSA2DElementLoadingThermal)].RemoveAll(l => deletedLines.Contains((l as IGSASpeckleContainer).GWACommand));
      foreach (KeyValuePair<Type, List<object>> kvp in GSASenderObjects)
        kvp.Value.RemoveAll(l => (l as IGSASpeckleContainer).SubGWACommand.Any(x => deletedLines.Contains(x)));

      // Filter only new lines
      string[] prevLines = GSASenderObjects[typeof(GSA2DElementLoadingThermal)].Select(l => (l as IGSASpeckleContainer).GWACommand).ToArray();
      string[] newLines = lines.Where(l => !prevLines.Contains(l)).ToArray();

      foreach (string p in newLines)
      {
        GSA2DElementLoadingThermal combo = new GSA2DElementLoadingThermal() { GWACommand = p };
        combo.ParseGWACommand(GSA, e2Ds, m2Ds);
        stageDefs.Add(combo);
      }

      GSASenderObjects[typeof(GSA2DElementLoadingThermal)].AddRange(stageDefs);

      if (stageDefs.Count() > 0 || deletedLines.Count() > 0) return new SpeckleObject();

      return new SpeckleNull();
    }
  }
}
