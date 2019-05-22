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
    public static List<Autodesk.Revit.DB.Wall> ToNative( this SpeckleElements.Wall myWall )
    {
      var (docObjs, stateObjs) = GetExistingElementsByApplicationId( myWall.ApplicationId, myWall.Type );

      var wallType = Doc.GetDefaultElementTypeId( ElementTypeGroup.WallType );
      var myWallType = GetElementByName( typeof( Autodesk.Revit.DB.WallType ), myWall.wallType );

      // filter null elements!
      docObjs = docObjs.Where( obj => obj != null ).ToList();
      stateObjs = stateObjs.Where( obj => obj != null ).ToList();

      List<Autodesk.Revit.DB.Wall> ret = new List<Autodesk.Revit.DB.Wall>();
      List<Curve> segments = GetSegmentList( myWall.baseCurve );

      // If there are no existing document objects, create them.
      if( docObjs.Count == 0 )
      {
        foreach( var baseCurve in segments )
        {
          if( myWall.baseLevel == null )
            myWall.baseLevel = new SpeckleElements.Level() { elevation = baseCurve.GetEndPoint( 0 ).Z / Scale, levelName = "Speckle Level " + baseCurve.GetEndPoint( 0 ).Z / Scale };

          var levelId = ((Level) myWall.baseLevel.ToNative()).Id;
          var revitWall = Wall.Create( Doc, baseCurve, myWallType.Id, levelId, myWall.height * Scale, myWall.offset * Scale, false, true );

          if( myWall.topLevel != null )
          {
            var topLevelId = ((Level) myWall.topLevel.ToNative()).Id;
            revitWall.get_Parameter( BuiltInParameter.WALL_HEIGHT_TYPE ).Set(topLevelId);
          }

          SetElementParams( revitWall, myWall.parameters );
          ret.Add( revitWall );
        }
        return ret;
      }
      // If there are as many docobjects as segments, edit them all
      else if( segments.Count == docObjs.Count )
      {
        for( int i = 0; i < segments.Count; i++ )
        {
          LocationCurve locationCurve = (LocationCurve) ((Wall) docObjs[ i ]).Location;
          myWall.baseLevel?.ToNative();
          locationCurve.Curve = segments[ i ];
          SetWallHeightOffset( (Wall) docObjs[ i ], myWall.height, myWall.offset );
          SetElementParams( docObjs[ i ], myWall.parameters );

          ret.Add( docObjs[ i ] as Wall );
        }
        return ret;
      }
      // If there are more new segments than doc objects, edit the existing and create the new
      else if( segments.Count > docObjs.Count )
      {
        //Edit existing walls
        for( int i = 0; i < docObjs.Count; i++ )
        {
          LocationCurve locationCurve = (LocationCurve) ((Wall) docObjs[ i ]).Location;
          myWall.baseLevel?.ToNative();
          locationCurve.Curve = segments[ i ];
          SetWallHeightOffset( (Wall) docObjs[ i ], myWall.height, myWall.offset );
          SetElementParams( docObjs[ i ], myWall.parameters );

          ret.Add( docObjs[ i ] as Wall );
        }

        //Add new walls
        for( int i = docObjs.Count; i < segments.Count; i++ )
        {
          var baseCurve = segments[ i ];
          if( myWall.baseLevel == null )
            myWall.baseLevel = new SpeckleElements.Level() { elevation = baseCurve.GetEndPoint( 0 ).Z, levelName = "Speckle Level " + baseCurve.GetEndPoint( 0 ).Z };

          var levelId = ((Level) myWall.baseLevel.ToNative()).Id;
          var revitWall = Wall.Create( Doc, baseCurve, levelId, false );
          revitWall = SetWallHeightOffset( revitWall, myWall.height, myWall.offset );
          SetElementParams( revitWall, myWall.parameters );
          ret.Add( revitWall );
        }

        return ret;
      }
      // If there are more doc objects than segments, edit the existing ones and delete the rest!
      else if( segments.Count < docObjs.Count )
      {
        // Deletion
        for( int i = segments.Count; i < docObjs.Count; i++ )
        {
          Doc.Delete( docObjs[ i ].Id );
        }
        // Editing
        for( int i = 0; i < segments.Count; i++ )
        {
          LocationCurve locationCurve = (LocationCurve) ((Wall) docObjs[ i ]).Location;
          myWall.baseLevel?.ToNative();
          locationCurve.Curve = segments[ i ];
          SetWallHeightOffset( (Wall) docObjs[ i ], myWall.height, myWall.offset );
          ret.Add( docObjs[ i ] as Wall );
        }
        return ret;
      }
      return ret;
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
      speckleWall.baseCurve = SpeckleCore.Converter.Serialise( ((LocationCurve) myWall.Location).Curve );

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

      speckleWall.ApplicationId = myWall.UniqueId;
      speckleWall.GenerateHash();

      return speckleWall;
    }
  }
}
