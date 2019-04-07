using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using SpeckleElements;

namespace SpeckleElementsRevit
{ 
  public static partial class Conversions
  {
    //TODO
    public static Element ToNative(this Column myCol )
    {
      var (docObj, stateObj) = GetExistingElementByApplicationId( myCol.ApplicationId, myCol.Type );

      var baseLine = GetSegmentList( myCol.baseLine )[ 0 ];
      //var famElement = GetElementByClassAndName(typeof(FamilySymbol),)

      return null;
    }

  }
}
