using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using SpeckleCore;
using SpeckleCoreGeometryClasses;
using SpeckleElements;

namespace SpeckleElementsRevit
{
  public static partial class Conversions
  {

    public static Opening ToNative( this Shaft myShaft )
    {
      var (docObj, stateObj) = GetExistingElementByApplicationId( myShaft.ApplicationId, myShaft.Type );

      var shaftCurves = new CurveArray();
      var segments = GetSegmentList( myShaft.baseCurve );
      foreach ( var x in segments ) shaftCurves.Append( x );

      if ( docObj != null )
        Doc.Delete( docObj.Id );

      var bottomLevel = myShaft.bottomLevel.ToNative();
      var topLevel = myShaft.topLevel.ToNative();

      var revitShaft = Doc.Create.NewOpening( bottomLevel, topLevel, shaftCurves );
      return revitShaft;
    }

    // NOTE: if we will include more element based openings, we will need to figure out how to split this method, 
    // or create a generic opening class
    // TODO: actually test this
    public static Shaft ToSpeckle( this Opening myShaft )
    {
      var spkShaft = new SpeckleElements.Shaft();      
      var poly = new SpecklePolycurve();
      poly.Segments = new List<SpeckleObject>();

      foreach ( Curve curve in myShaft.BoundaryCurves )
      {
        if ( curve == null ) continue;
        poly.Segments.Add( SpeckleCore.Converter.Serialise( curve ) as SpeckleObject );
      }

      spkShaft.baseCurve = poly;
      spkShaft.ApplicationId = myShaft.UniqueId;
      spkShaft.GenerateHash();
      return null;
    }
  }
}
