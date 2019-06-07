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
  [GSAObject("MEMB.7", new string[] { }, "elements", true, true, new Type[] { typeof(GSA1DElement), typeof(GSA1DLoad), typeof(GSA1DElementResult)}, new Type[] { typeof(GSA1DProperty) })]
  public class GSA1DElementPolyline : IGSASpeckleContainer
  {
    public string GWACommand { get; set; }
    public List<string> SubGWACommand { get; set; } = new List<string>();
    public dynamic Value { get; set; } = new Structural1DElementPolyline();

    public void ParseGWACommand(GSAInterfacer GSA, List<GSA1DElement> elements)
    {
      if (elements.Count() < 1)
        return;

      Structural1DElementPolyline obj = new Structural1DElementPolyline();
      obj.Value = new List<double>();
      obj.ElementStructuralId = new List<string>();

      obj.ElementType = elements.First().Value.ElementType;
      obj.PropertyRef = elements.First().Value.PropertyRef;
      obj.ZAxis = new List<StructuralVectorThree>();
      obj.EndRelease = new List<StructuralVectorBoolSix>();
      obj.Offset = new List<StructuralVectorThree>();

      // Match up coordinates
      List<Tuple<string, string>> coordinates = new List<Tuple<string, string>>();

      foreach (GSA1DElement e in elements)
        coordinates.Add( new Tuple<string, string>(
          string.Join(",", (e.Value.Value as List<double>).Take(3).Select(x => Math.Round(x, 4).ToString())),
          string.Join(",", (e.Value.Value as List<double>).Skip(3).Take(3).Select(x => Math.Round(x, 4).ToString()))
        ));

      // Find start coordinate
      List<string> flatCoordinates = coordinates.SelectMany(x => new List<string>() { x.Item1, x.Item2 }).ToList();
      List<string> uniqueCoordinates = flatCoordinates.Where(x => flatCoordinates.Count(y => y == x) == 1).ToList();

      string current = uniqueCoordinates[0];
      while(coordinates.Count > 0)
      {
        var matchIndex = 0;
        var reverseCoordinates = false;

        try
        {
          matchIndex = coordinates.FindIndex(x => x.Item1 == current);
          reverseCoordinates = false;
        }
        catch
        {
          matchIndex = coordinates.FindIndex(x => x.Item2 == current);
          reverseCoordinates = true;
        }

        var element = elements[matchIndex];

        obj.ElementStructuralId.Add(element.Value.StructuralId);
        obj.ZAxis.Add(element.Value.ZAxis);

        if (obj.Value.Count == 0)
        {
          if (!reverseCoordinates)
            obj.Value.AddRange((element.Value.Value as List<double>).Take(3));
          else
            obj.Value.AddRange((element.Value.Value as List<double>).Skip(3).Take(3));
        }

        if (!reverseCoordinates)
        { 
          obj.Value.AddRange((element.Value.Value as List<double>).Skip(3).Take(3));
          obj.EndRelease.AddRange(element.Value.EndRelease);
          obj.Offset.AddRange(element.Value.Offset);
        }
        else
        {
          obj.Value.AddRange((element.Value.Value as List<double>).Take(3));
          obj.EndRelease.Add((element.Value.EndRelease as List<StructuralVectorBoolSix>).Last());
          obj.EndRelease.Add((element.Value.EndRelease as List<StructuralVectorBoolSix>).First());
          obj.Offset.Add((element.Value.Offset as List<StructuralVectorThree>).Last());
          obj.Offset.Add((element.Value.Offset as List<StructuralVectorThree>).First());
        }

        // Result merging
        if (obj.Result == null)
          obj.Result = new Dictionary<string, object>();

        foreach (string loadCase in element.Value.Result.Keys)
        {
          if (!obj.Result.ContainsKey(loadCase))
            obj.Result[loadCase] = new Structural1DElementResult();

          var resultExport = element.Value.Result[loadCase] as Structural1DElementResult;

          if (resultExport != null)
          {
            if ((obj.Result[loadCase] as Structural1DElementResult).Displacement == null)
              (obj.Result[loadCase] as Structural1DElementResult).Displacement = new Dictionary<string, object>(resultExport.Displacement);
            else
              foreach (string key in (obj.Result[loadCase] as Structural1DElementResult).Displacement.Keys)
                ((obj.Result[loadCase] as Structural1DElementResult).Displacement[key] as List<double>).AddRange(resultExport.Displacement[key] as List<double>);

            if ((obj.Result[loadCase] as Structural1DElementResult).Force == null)
              (obj.Result[loadCase] as Structural1DElementResult).Force = new Dictionary<string, object>(resultExport.Force);
            else
              foreach (string key in (obj.Result[loadCase] as Structural1DElementResult).Force.Keys)
                ((obj.Result[loadCase] as Structural1DElementResult).Force[key] as List<double>).AddRange(resultExport.Force[key] as List<double>);

            if ((obj.Result[loadCase] as Structural1DElementResult).Stress == null)
              (obj.Result[loadCase] as Structural1DElementResult).Stress = new Dictionary<string, object>(resultExport.Stress);
            else
              foreach (string key in (obj.Result[loadCase] as Structural1DElementResult).Stress.Keys)
                ((obj.Result[loadCase] as Structural1DElementResult).Stress[key] as List<double>).AddRange(resultExport.Stress[key] as List<double>);
          }
          else
          {
            if ((obj.Result[loadCase] as Structural1DElementResult).Displacement == null)
              (obj.Result[loadCase] as Structural1DElementResult).Displacement = new Dictionary<string, object>()
              {
                  {"x", new List<double>() { 0 } },
                  {"y", new List<double>() { 0 } },
                  {"z", new List<double>() { 0 } },
                  {"xx", new List<double>() { 0 } },
                  {"yy", new List<double>() { 0 } },
                  {"zz", new List<double>() { 0 } },
              };
            else
              foreach (string key in (obj.Result[loadCase] as Structural1DElementResult).Displacement.Keys)
                ((obj.Result[loadCase] as Structural1DElementResult).Displacement[key] as List<double>).Add(0);

            if ((obj.Result[loadCase] as Structural1DElementResult).Force == null)
              (obj.Result[loadCase] as Structural1DElementResult).Force = new Dictionary<string, object>()
                {
                  {"fx", new List<double>() { 0 } },
                  {"fy", new List<double>() { 0 } },
                  {"fz", new List<double>() { 0 } },
                  {"mx", new List<double>() { 0 } },
                  {"my", new List<double>() { 0 } },
                  {"mz", new List<double>() { 0 } },
                };
            else
              foreach (string key in (obj.Result[loadCase] as Structural1DElementResult).Force.Keys)
                ((obj.Result[loadCase] as Structural1DElementResult).Force[key] as List<double>).Add(0);

            if ((obj.Result[loadCase] as Structural1DElementResult).Stress == null)
              (obj.Result[loadCase] as Structural1DElementResult).Stress = new Dictionary<string, object>() {
                {"a", new List<double>() { 0 } },
                {"sy", new List<double>() { 0 } },
                {"sz", new List<double>() { 0 } },
                {"by+", new List<double>() { 0 } },
                {"by-", new List<double>() { 0 } },
                {"bz+", new List<double>() { 0 } },
                {"bz-", new List<double>() { 0 } },
                {"comb+", new List<double>() { 0 } },
                {"comb-", new List<double>() { 0 } },
              };
            else
              foreach (string key in (obj.Result[loadCase] as Structural1DElementResult).Stress.Keys)
                ((obj.Result[loadCase] as Structural1DElementResult).Stress[key] as List<double>).Add(0);
          }
        }

        coordinates.RemoveAt(matchIndex);

        this.SubGWACommand.Add(element.GWACommand);
        this.SubGWACommand.AddRange(element.SubGWACommand);
      }
      
      this.Value = obj;
    }

    public void SetGWACommand(GSAInterfacer GSA)
    {
      if (this.Value == null)
        return;

      Structural1DElementPolyline obj = this.Value as Structural1DElementPolyline;

      int group = GSA.Indexer.ResolveIndex(typeof(GSA1DElementPolyline), obj);

      Structural1DElement[] elements = obj.Explode();

      foreach (Structural1DElement element in elements)
      {
        if (Conversions.GSATargetLayer == GSATargetLayer.Analysis)
          new GSA1DElement() { Value = element }.SetGWACommand(GSA, group);
        else if (Conversions.GSATargetLayer == GSATargetLayer.Design)
          new GSA1DMember() { Value = element }.SetGWACommand(GSA, group);
      }
    }
  }

  public static partial class Conversions
  {
    public static bool ToNative(this SpecklePolyline inputObject)
    {
      Structural1DElementPolyline convertedObject = new Structural1DElementPolyline();

      foreach (PropertyInfo p in convertedObject.GetType().GetProperties().Where(p => p.CanWrite))
      {
        PropertyInfo inputProperty = inputObject.GetType().GetProperty(p.Name);
        if (inputProperty != null)
          p.SetValue(convertedObject, inputProperty.GetValue(inputObject));
      }

      return convertedObject.ToNative();
    }

    public static bool ToNative(this Structural1DElementPolyline poly)
    {
      new GSA1DElementPolyline() { Value = poly }.SetGWACommand(GSA);

      return true;
    }

    public static SpeckleObject ToSpeckle(this GSA1DElementPolyline dummyObject)
    {
      if (!GSASenderObjects.ContainsKey(typeof(GSA1DElementPolyline)))
        GSASenderObjects[typeof(GSA1DElementPolyline)] = new List<object>();

      List<GSA1DElementPolyline> polylines = new List<GSA1DElementPolyline>();

      // Perform mesh merging
      var uniqueMembers = new List<string>(GSASenderObjects[typeof(GSA1DElement)].Select(x => (x as GSA1DElement).Member).Distinct());
      foreach (string member in uniqueMembers)
      {
        var elementList = GSASenderObjects[typeof(GSA1DElement)].Where(x => (x as GSA1DElement).Member == member).Cast<GSA1DElement>().ToList();
        GSA1DElementPolyline poly = new GSA1DElementPolyline();
        poly.ParseGWACommand(GSA, elementList);
        polylines.Add(poly);

        GSASenderObjects[typeof(GSA1DElement)].RemoveAll(x => elementList.Contains(x));
      }

      GSASenderObjects[typeof(GSA1DElementPolyline)].AddRange(polylines);

      return new SpeckleNull(); // Return null because ToSpeckle method for GSA1DElement will handle this change
    }
  }
}
