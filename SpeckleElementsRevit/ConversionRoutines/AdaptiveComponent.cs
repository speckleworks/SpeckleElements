using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using SpeckleCore.Data;
using SpeckleCoreGeometryClasses;

namespace SpeckleElementsRevit
{

  public static partial class Conversions
  {
    public static Autodesk.Revit.DB.FamilyInstance ToNative(this SpeckleElementsClasses.AdaptiveComponent myAdaptiveComponent)
    {
      var (docObj, stateObj) = GetExistingElementByApplicationId(myAdaptiveComponent.ApplicationId, myAdaptiveComponent.Type);

      var familySymbol = GetFamilySymbolByFamilyNameAndType(myAdaptiveComponent.familyName, myAdaptiveComponent.familyType);

      if (familySymbol == null)
      {
        ConversionErrors.Add(new SpeckleConversionError { Message = $"Missing family: {myAdaptiveComponent.familyName} {myAdaptiveComponent.familyType}" });
        throw new RevitFamilyNotFoundException($"No 'AdaptiveComponent' family found in the project");
      }

      if (!familySymbol.IsActive) familySymbol.Activate();

      if (myAdaptiveComponent.points == null)
      {
        ConversionErrors.Add(new SpeckleConversionError { Message = $"Wrong number of points supplied to adapive family" });
        return null;
      }
      if (docObj != null) // we have a document object already, so check if we can edit it.
      {
        var type = Doc.GetElement(docObj.GetTypeId()) as ElementType;

        // if family changed, tough luck - delete and rewind
        if (myAdaptiveComponent.familyName != type.FamilyName)
        {
          //delete and continue crating it
          Doc.Delete(docObj.Id);
        }
        // edit element
        else
        {
          var existingFamilyInstance = (Autodesk.Revit.DB.FamilyInstance)docObj;

          // check if type changed, and try and change it
          if (myAdaptiveComponent.familyType != null && (myAdaptiveComponent.familyType != type.Name))
          {
            existingFamilyInstance.ChangeTypeId(familySymbol.Id);
          }

          SetAdaptiveComponentPoints(existingFamilyInstance, myAdaptiveComponent.points);
          SetElementParams(existingFamilyInstance, myAdaptiveComponent.parameters);
          return existingFamilyInstance;
        }
      }

      // if we don't have a document object
     else
      {
        var component = AdaptiveComponentInstanceUtils.CreateAdaptiveComponentInstance(Doc, familySymbol);
        SetAdaptiveComponentPoints(component, myAdaptiveComponent.points);
        SetElementParams(component, myAdaptiveComponent.parameters);
        return component;
      }


      return null;
    }

    private static void SetAdaptiveComponentPoints(FamilyInstance component, List<SpecklePoint> points)
    {
      var pointIds = new List<ElementId>();
      pointIds = AdaptiveComponentInstanceUtils.GetInstancePlacementPointElementRefIds(component).ToList();

      if (pointIds.Count != points.Count)
      {
        ConversionErrors.Add(new SpeckleConversionError { Message = $"Wrong number of points supplied to adapive family" });
        return;
      }

      //set base points
      for (int i = 0; i < pointIds.Count; i++)
      {
        var point = Doc.GetElement(pointIds[i]) as ReferencePoint;
        point.Position = (XYZ)SpeckleCore.Converter.Deserialise(obj: points[i], excludeAssebmlies: new string[] { "SpeckleCoreGeometryDynamo" });
      }
    }
  }
}
