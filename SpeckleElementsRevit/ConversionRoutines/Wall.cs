using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using SpeckleCore;
using SpeckleCoreGeometryClasses;

namespace SpeckleElementsRevit
{
  public static partial class Conversions
  {
    // TODO: A polycurve spawning multiple walls is not yet handled properly with diffing, etc.
    // TODO: Most probably, just get rid of the polyline wall handling stuff. It's rather annyoing and confusing...
    public static Wall ToNative( this SpeckleElements.Wall myWall )
    {
      var (docObj, stateObj) = GetExistingElementByApplicationId( myWall.ApplicationId, myWall.Type );

      var myWallType = GetElementByName( typeof( WallType ), myWall.wallType ) as WallType;

      // cheating a bit. 
      var segments = GetSegmentList( myWall.baseCurve );
      var baseCurve = segments[ 0 ];

      // Create new wall case
      if( docObj == null )
      {

        if( myWall.baseLevel == null )
          myWall.baseLevel = new SpeckleElements.Level() { elevation = baseCurve.GetEndPoint( 0 ).Z / Scale, levelName = "Speckle Level " + baseCurve.GetEndPoint( 0 ).Z / Scale };

        var levelId = ((Level) myWall.baseLevel.ToNative()).Id;
        var revitWall = Wall.Create( Doc, baseCurve, myWallType.Id, levelId, myWall.height * Scale, myWall.offset * Scale, false, true );

        if( myWall.topLevel != null )
        {
          var topLevelId = ((Level) myWall.topLevel.ToNative()).Id;
          revitWall.get_Parameter( BuiltInParameter.WALL_HEIGHT_TYPE ).Set( topLevelId );
        }

        if( myWall.Properties.ContainsKey( "__flipped" ) )
        {
          var flipped = Convert.ToBoolean( myWall.Properties[ "__flipped" ] );
          if( flipped != revitWall.Flipped )
            revitWall.Flip();
        }

        SetElementParams( revitWall, myWall.parameters );
        return revitWall;
      }

      // Edit existing wall case
      var existingRevitWall = (Wall) docObj;

      if( existingRevitWall.WallType.Name != myWallType.Name )
      {
        existingRevitWall.ChangeTypeId( myWallType.Id );
      }

      LocationCurve locationCurve = (LocationCurve) existingRevitWall.Location;
      myWall.baseLevel?.ToNative();
      locationCurve.Curve = baseCurve;

      SetWallHeightOffset( existingRevitWall, myWall.height, myWall.offset );
      existingRevitWall.WallType = myWallType as WallType;


      if( myWall.Properties.ContainsKey( "__flipped" ) )
      {
        var flipped = Convert.ToBoolean( myWall.Properties[ "__flipped" ] );
        if( flipped != existingRevitWall.Flipped )
          existingRevitWall.Flip();
      }

      SetElementParams( existingRevitWall, myWall.parameters );

      return existingRevitWall;
    }

    /// <summary>
    /// Sets params on wall.
    /// </summary>
    /// <param name="revitWall"></param>
    /// <param name="height"></param>
    /// <param name="offset"></param>
    /// <returns></returns>
    private static Autodesk.Revit.DB.Wall SetWallHeightOffset( Autodesk.Revit.DB.Wall revitWall, double height, double offset )
    {
      var heightParam = revitWall.get_Parameter( BuiltInParameter.WALL_USER_HEIGHT_PARAM );
      if( heightParam != null && !heightParam.IsReadOnly )
        heightParam.Set( height * Scale );

      var offsetParam = revitWall.get_Parameter( BuiltInParameter.WALL_BASE_OFFSET );
      if( offsetParam != null && !offsetParam.IsReadOnly )
        offsetParam.Set( offset * Scale );

      return revitWall;
    }

    // TODO: Wall to Speckle
    // TODO: Set levels, heights, etc.
    // Does not go through nicely from revit to revit
    public static SpeckleElements.Wall ToSpeckle( this Autodesk.Revit.DB.Wall myWall )
    {
      var speckleWall = new SpeckleElements.Wall();
      speckleWall.baseCurve = SpeckleCore.Converter.Serialise( ((LocationCurve) myWall.Location).Curve ) as SpeckleObject;

      var heightParam = myWall.get_Parameter( BuiltInParameter.WALL_USER_HEIGHT_PARAM );
      var heightValue = heightParam.AsDouble();
      var height = UnitUtils.ConvertFromInternalUnits( heightValue, heightParam.DisplayUnitType );
      speckleWall.height = heightValue / Scale;

      var offsetParam = myWall.get_Parameter( BuiltInParameter.WALL_BASE_OFFSET );
      var offsetValue = offsetParam.AsDouble();
      var offset = UnitUtils.ConvertFromInternalUnits( offsetValue, offsetParam.DisplayUnitType );
      speckleWall.offset = offsetValue / Scale;

      speckleWall.wallType = myWall.WallType.Name;

      var level = (Level) Doc.GetElement( myWall.get_Parameter( BuiltInParameter.WALL_BASE_CONSTRAINT ).AsElementId() );
      speckleWall.baseLevel = level.ToSpeckle();

      try
      {
        var topLevel = (Level) Doc.GetElement( myWall.get_Parameter( BuiltInParameter.WALL_HEIGHT_TYPE ).AsElementId() );
        speckleWall.topLevel = topLevel.ToSpeckle();
      }
      catch( Exception e ) { }

      speckleWall.parameters = GetElementParams( myWall );

      var grid = myWall.CurtainGrid;

      // TODO: Should move maybe in base class defintion
      speckleWall.Properties[ "__flipped" ] = myWall.Flipped;

      speckleWall.ApplicationId = myWall.UniqueId;
      speckleWall.GenerateHash();

      // meshing for walls in case they are curtain grids
      if( grid != null )
      {
        var mySolids = new List<Solid>();
        foreach( ElementId panelId in grid.GetPanelIds() )
        {
          mySolids.AddRange( GetElementSolids( Doc.GetElement( panelId ) ) );
        }
        foreach( ElementId mullionId in grid.GetMullionIds() )
        {
          mySolids.AddRange( GetElementSolids( Doc.GetElement( mullionId ) ) );
        }
        (speckleWall.Faces, speckleWall.Vertices) = GetFaceVertexArrFromSolids( mySolids );
      }
      else
        (speckleWall.Faces, speckleWall.Vertices) = GetFaceVertexArrayFromElement( myWall, new Options() { DetailLevel = ViewDetailLevel.Fine, ComputeReferences = false } );

      return speckleWall;
    }
  }
}
