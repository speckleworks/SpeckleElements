using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpeckleCore;
using SpeckleElements;

namespace SpeckleElementsGSA
{
  [GSAObject("PROP_SPR.3", new string[] { }, "properties", true, true, new Type[] { }, new Type[] { })]
  public class GSALinearSpringProperty : IGSASpeckleContainer
  {
    public int GSAId { get; set; }
    public string GWACommand { get; set; }
    public List<string> SubGWACommand { get; set; } = new List<string>();
    public dynamic Value { get; set; } = new StructuralLinearSpringProperty();

    public void ParseGWACommand(GSAInterfacer GSA)
    {
      if (this.GWACommand == null)
        return;

      var pieces = this.GWACommand.ListSplit("\t");

      const int numStiffnesses = 6; 

      var obj = new StructuralLinearSpringProperty();

      var counter = 1; // Skip identifier

      this.GSAId = Convert.ToInt32(pieces[counter++]);
      obj.ApplicationId = GSA.GetSID(this.GetGSAKeyword(), this.GSAId);
      obj.Name = pieces[counter++].Trim(new char[] { '"' });
      counter++; //Skip colour
      obj.Axis = (StructuralSpringAxis)Enum.Parse(typeof(StructuralSpringAxis), (pieces[counter++] as string), true);
      var springPropertyType = pieces[counter++];
      if (springPropertyType.ToLower() != "general")
      {
        //this type is not currently handled by this class
        return;
      }
      obj.Type = springPropertyType;
      var stiffnesses = new double[numStiffnesses];
      for (var i = 0; i < numStiffnesses; i++)
      {
        double.TryParse(pieces[counter += 2], out stiffnesses[i]);
      }
      obj.Stiffness = new StructuralVectorSix(stiffnesses);
      this.Value = obj;
    }

    public void SetGWACommand(GSAInterfacer GSA)
    {
      if (this.Value == null)
        return;

      Type destType = typeof(GSALinearSpringProperty);

      StructuralLinearSpringProperty lsp = this.Value as StructuralLinearSpringProperty;

      string keyword = destType.GetGSAKeyword();

      int index = GSA.Indexer.ResolveIndex(destType, lsp);

      List<string> ls = new List<string>
        {
          "SET",
          keyword + ":" + GSA.GenerateSID(lsp),
          index.ToString(),
          string.IsNullOrEmpty(lsp.Name) ? "" : lsp.Name,
          "NO_RGB",
          lsp.Axis.ToString(),
          "GENERAL"
      };

      for (var i = 0; i < 6; i++)
      {
        ls.Add("0"); //Curve
        ls.Add(lsp.Stiffness.Value[i].ToString());
      }
      ls.Add("0");  //Damping ratio

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
