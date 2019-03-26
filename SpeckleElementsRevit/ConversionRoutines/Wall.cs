using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using SpeckleCoreGeometryClasses;

namespace SpeckleElementsRevit
{
  public static partial class Conversions
  {

    public static Autodesk.Revit.DB.Wall ToNative( this SpeckleElements.Wall myWall )
    {
      var (docObj, stateObj) = GetExistingElementByApplicationId( myWall.ApplicationId, myWall.Type );

      // If the existing doc object is not marked as modified, return the original.
      if ( stateObj != null && docObj != null && myWall._id == stateObj._id && ( bool ) stateObj.Properties[ "userModified" ] == false )
        return ( Autodesk.Revit.DB.Wall ) docObj;

      // Create base curve
      Curve baseCurve = null;
      switch ( myWall.baseCurve )
      {
        case SpeckleLine line:
          baseCurve = ( Line ) SpeckleCore.Converter.Deserialise( line );
          break;
        case SpeckleArc arc:
          baseCurve = ( Arc ) SpeckleCore.Converter.Deserialise( arc );
          break;
      }

      // If no existing document object, create it.
      if ( docObj == null )
      {
        // TODO: Create wall
        if ( myWall.level == null )
        {
          myWall.level = new SpeckleElements.Level() { elevation = baseCurve.GetEndPoint( 0 ).Z, Name = "Speckle Level "+ baseCurve.GetEndPoint( 0 ).Z };
        }
        var levelId = ( ( Level ) myWall.level.ToNative() ).Id;
        var revitWall = Wall.Create( Doc, baseCurve, levelId, false );
        revitWall = SetWallHeightOffset( revitWall, myWall.height, myWall.offset );
        return revitWall;

      }

      // Otherwise, enter edit mode.
      LocationCurve locationCurve = ( LocationCurve ) ( ( Wall ) docObj ).Location;
      myWall.level?.ToNative();
      locationCurve.Curve = baseCurve;

      return SetWallHeightOffset( ( Wall ) docObj, myWall.height, myWall.offset );
    }

    public static Autodesk.Revit.DB.Wall WallFromLine( SpeckleLine line )
    {
      return null;
    }

    private static Autodesk.Revit.DB.Wall SetWallHeightOffset( Autodesk.Revit.DB.Wall revitWall, double height, double offset )
    {
      var heightParam = revitWall.get_Parameter( BuiltInParameter.WALL_USER_HEIGHT_PARAM );
      if ( heightParam != null && !heightParam.IsReadOnly )
        heightParam.Set( height * Scale );

      var offsetParam = revitWall.get_Parameter( BuiltInParameter.WALL_BASE_OFFSET );
      if ( offsetParam != null && !offsetParam.IsReadOnly )
        offsetParam.Set( offset );

      return revitWall;
    }

  }
}
