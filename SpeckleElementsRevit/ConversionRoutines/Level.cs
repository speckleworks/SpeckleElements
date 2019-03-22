using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using SpeckleCore;
using SpeckleElements;

namespace SpeckleElementsRevit
{
  public static partial class Conversions
  {
    public static Autodesk.Revit.DB.Level ToNative( this SpeckleElements.Level myLevel )
    {
      var (docObj, stateObj) = GetExistingElementByApplicationId( myLevel.ApplicationId );

      // If no doc object, means we need to create it!
      if ( docObj == null )
      {
        // TODO: CREATE LEVEL
      }

      // if the new and old have the same id (hash equivalent) and the doc obj is not marked as being modified, return the doc object
      if ( docObj != null && myLevel._id == stateObj._id && ( bool ) stateObj.Properties[ "userModified" ] == false )
      {
        return ( Autodesk.Revit.DB.Level ) docObj;
      }

      // TODO: EDIT LEVEL
      return null;
    }

    public static SpeckleElements.Level ToSpeckle(this Autodesk.Revit.DB.Level myLevel)
    {
      throw new NotImplementedException();
    }
  }

}
