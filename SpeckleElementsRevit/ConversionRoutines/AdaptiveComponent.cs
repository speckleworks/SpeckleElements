using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;

namespace SpeckleElementsRevit
{

  public static partial class Conversions
  {
    public static Autodesk.Revit.DB.FamilyInstance ToNative(this SpeckleElementsClasses.AdaptiveComponent myAdaptiveComponent)
    {
      var (docObj, stateObj) = GetExistingElementByApplicationId(myAdaptiveComponent.ApplicationId, myAdaptiveComponent.Type);
      
      // if we don't have a document object
      if(docObj == null )
      {
        var symbol = GetAdaptiveFamilySymbol(myAdaptiveComponent.familyName);
        if(symbol == null)
        {
          // TODO: Throw nice error
        }

        var component = AdaptiveComponentInstanceUtils.CreateAdaptiveComponentInstance(Doc, symbol);
        
        var pointIds = new List<ElementId>();
        pointIds = AdaptiveComponentInstanceUtils.GetInstancePlacementPointElementRefIds(component).ToList();

        // TODO: check if points match lengths

        for(int i = 0; i< pointIds.Count; i++)
        {

        }

      } else // we have a document object already, so check if we can edit it.
      {

      }

      return null;
    }


    public static FamilySymbol GetAdaptiveFamilySymbol(string name)
    {
      var collector = new FilteredElementCollector(Doc).OfCategory(BuiltInCategory.OST_Mass).OfClass(typeof(FamilySymbol)).ToElements().Cast<FamilySymbol>();

      foreach (var fam in collector)
      {
        if (fam.Name == name) return fam;
      }

      return null;
    }
  }
}
