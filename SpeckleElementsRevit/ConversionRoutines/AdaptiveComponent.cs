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
    public static Autodesk.Revit.DB.FamilyInstance ToNative(this SpeckleElementsClasses.HostedFamilyInstance myHostedFamilyInstance)
    {
      var (docObj, stateObj) = GetExistingElementByApplicationId(myHostedFamilyInstance.ApplicationId, myHostedFamilyInstance.Type);
      //var collector = new FilteredElementCollector(Doc).OfCategory(BuiltInCategory.OST_Mass ... WIP
      return null;
    }
  }
}
