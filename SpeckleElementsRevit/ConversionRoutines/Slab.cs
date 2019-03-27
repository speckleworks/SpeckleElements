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

    public static Floor ToNative( this Slab mySlab )
    {
      var (docObj, stateObj) = GetExistingElementByApplicationId( mySlab.ApplicationId, mySlab.Type );

      var slabCurves = new CurveArray();
      var segments = GetSegmentList( mySlab.baseCurve );
      foreach ( var x in segments ) slabCurves.Append( x );

      if ( docObj == null )
      {
        return Doc.Create.NewFloor( slabCurves, false );
      }

      // TODO: Editing a slab profile is a pain apparently. Will defer to wiser minds.
      Doc.Delete( docObj.Id );
      return Doc.Create.NewFloor( slabCurves, false );
    }

    public static Slab ToSpeckle( this Floor myFloor )
    {
      return null;
    }

  }
}
