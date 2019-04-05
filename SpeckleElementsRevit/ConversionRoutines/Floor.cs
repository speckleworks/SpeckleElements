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

    public static Autodesk.Revit.DB.Floor ToNative( this SpeckleElements.Floor mySlab )
    {
      var (docObj, stateObj) = GetExistingElementByApplicationId( mySlab.ApplicationId, mySlab.Type );

      var slabCurves = new CurveArray();
      var segments = GetSegmentList( mySlab.baseCurve );
      foreach( var x in segments ) slabCurves.Append( x );

      if( mySlab.level == null )
        mySlab.level = new SpeckleElements.Level() { elevation = segments[ 0 ].GetEndPoint( 0 ).Z / Scale, levelName = "Speckle Level " + segments[ 0 ].GetEndPoint( 0 ).Z / Scale };


      // TODO: Editing a slab profile is a pain apparently.
      // See https://thebuildingcoder.typepad.com/blog/2008/11/editing-a-floor-profile.html
      if( docObj != null )
        Doc.Delete( docObj.Id );

      if( mySlab.floorType == null )
        return Doc.Create.NewFloor( slabCurves, false );
      

      FloorType type = (FloorType) GetElementByClassAndName( typeof( Autodesk.Revit.DB.FloorType ), mySlab.floorType );
      return Doc.Create.NewFloor( slabCurves, type, ((Autodesk.Revit.DB.Level) mySlab.level.ToNative()), false );
    }

    public static SpeckleElements.Floor ToSpeckle( this Autodesk.Revit.DB.Floor myFloor )
    {
      // TODO: convert floor to slaby slab
      return null;
    }

  }
}
