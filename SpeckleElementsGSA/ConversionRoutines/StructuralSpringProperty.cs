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
  public class GSASpringProperty : IGSASpeckleContainer
  {
    public int GSAId { get; set; }
    public string GWACommand { get; set; }
    public List<string> SubGWACommand { get; set; } = new List<string>();
    public dynamic Value { get; set; } = new StructuralSpringProperty();

    public void ParseGWACommand(GSAInterfacer GSA)
    {
      if (this.GWACommand == null)
        return;

      var pieces = this.GWACommand.ListSplit("\t");

      var obj = new StructuralSpringProperty();

      var counter = 1; // Skip identifier

      this.GSAId = Convert.ToInt32(pieces[counter++]);
      obj.ApplicationId = GSA.GetSID(this.GetGSAKeyword(), this.GSAId);
      obj.Name = pieces[counter++].Trim(new char[] { '"' });
      counter++; //Skip colour
      string gsaAxis = pieces[counter++];

      if (gsaAxis == "GLOBAL")
        obj.Axis = GSA.Parse0DAxis(0, out string gwaRec);
      else if (gsaAxis == "VERTICAL")
        obj.Axis = GSA.Parse0DAxis(-14, out string gwaRec);
      else
      {
        obj.Axis = GSA.Parse0DAxis(Convert.ToInt32(gsaAxis), out string gwaRec);
        this.SubGWACommand.Add(gwaRec);
      }

      var springPropertyType = pieces[counter++];
      obj.SpringType = springPropertyType.StringToEnum<StructuralSpringPropertyType>();

      var stiffnesses = new double[6];

      switch (obj.SpringType)
      {
        case StructuralSpringPropertyType.General:
          counter--;
          for (var i = 0; i < 6; i++)
            double.TryParse(pieces[counter += 2], out stiffnesses[i]);
          counter--;
          break;
        case StructuralSpringPropertyType.Friction:
          counter--;
          for (var i = 0; i < 3; i++)
            double.TryParse(pieces[counter += 2], out stiffnesses[i]);
          counter--;
          break;
        case StructuralSpringPropertyType.Axial:
        case StructuralSpringPropertyType.Tension:
        case StructuralSpringPropertyType.Compression:
        case StructuralSpringPropertyType.Lockup:
        case StructuralSpringPropertyType.Gap:
          double.TryParse(pieces[counter++], out stiffnesses[0]);
          break;
        case StructuralSpringPropertyType.Torsional:
          // TODO: As of build 48 of GSA, the torsional stiffness is not extracted in GWA records
          return;
        default:
          return;
      };
      
      obj.Stiffness = new StructuralVectorSix(stiffnesses);
      this.Value = obj;
    }

    private string SpringPropertyTypeToGWA(StructuralSpringPropertyType springPropertyType)
    {
      //Even though the values are mostly just the upper case versions of the num values - create an explicit 
      //conversion here in case the enum values ever change
      switch (springPropertyType)
      {
        case StructuralSpringPropertyType.Axial: return "AXIAL";
        case StructuralSpringPropertyType.Torsional: return "TORSIONAL";
        case StructuralSpringPropertyType.Compression: return "COMPRESSION";
        case StructuralSpringPropertyType.Tension: return "TENSION";
        case StructuralSpringPropertyType.Connector: return "CONNECT";
        case StructuralSpringPropertyType.Lockup: return "LOCKUP";
        case StructuralSpringPropertyType.Gap: return "GAP";
        default: return "GENERAL";
      }
    }

    public void SetGWACommand(GSAInterfacer GSA)
    {
      if (this.Value == null)
        return;

      Type destType = typeof(GSASpringProperty);

      StructuralSpringProperty springProp = this.Value as StructuralSpringProperty;

      string keyword = destType.GetGSAKeyword();

      int index = GSA.Indexer.ResolveIndex(destType, springProp);

      string axisRef = "GLOBAL"; //Default value

      if (springProp.Axis != null)
      {
        if (springProp.Axis.Xdir.Value.SequenceEqual(new double[] { 0, 0, 1 }) &&
        springProp.Axis.Ydir.Value.SequenceEqual(new double[] { 1, 0, 0 }) &&
        springProp.Axis.Normal.Value.SequenceEqual(new double[] { 0, 1, 0 }))
        {
          axisRef = "VERTICAL";
        }
        else
        {
          try
          {
            axisRef = GSA.SetAxis(springProp.Axis).ToString();
          }
          catch { axisRef = "GLOBAL"; }
        }
      }

      var ls = new List<string>
      {
        "SET",
        keyword + ":" + GSA.GenerateSID(springProp),
        index.ToString(),
        string.IsNullOrEmpty(springProp.Name) ? "" : springProp.Name,
        "NO_RGB",
        axisRef,
        SpringPropertyTypeToGWA(springProp.SpringType)
      };

      for (var i = 0; i < 6; i++)
      {
        ls.Add("0"); //Curve
        ls.Add(springProp.Stiffness.Value[i].ToString());
      }
      ls.Add("0");  //Damping ratio

      GSA.RunGWACommand(string.Join("\t", ls));
    }
  }

  public static partial class Conversions
  {
    public static bool ToNative(this StructuralSpringProperty prop)
    {
      if (Conversions.GSATargetLayer == GSATargetLayer.Analysis)
        new GSASpringProperty() { Value = prop }.SetGWACommand(GSA);
      else if (Conversions.GSATargetLayer == GSATargetLayer.Design)
        new GSASpringProperty() { Value = prop }.SetGWACommand(GSA);

      return true;
    }

    public static SpeckleObject ToSpeckle(this GSASpringProperty dummyObject)
    {
      Type objType = dummyObject.GetType();

      if (!GSASenderObjects.ContainsKey(objType))
        GSASenderObjects[objType] = new List<object>();

      //Get all relevant GSA entities in this entire model
      var springProperties = new List<GSASpringProperty>();

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
        var springProperty = new GSASpringProperty() { GWACommand = p };
        springProperty.ParseGWACommand(GSA);
        springProperties.Add(springProperty);
      }

      GSASenderObjects[objType].AddRange(springProperties);

      if (springProperties.Count() > 0 || deletedLines.Count() > 0) return new SpeckleObject();

      return new SpeckleNull();
    }
  }
}
