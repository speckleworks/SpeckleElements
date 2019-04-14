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

    public static List<Autodesk.Revit.DB.Wall> ToNative( this SpeckleElements.Wall myWall )
    {
      var (docObjs, stateObjs) = GetExistingElementsByApplicationId( myWall.ApplicationId, myWall.Type );

      // filter null elements!
      docObjs = docObjs.Where( obj => obj != null ).ToList();
      stateObjs = stateObjs.Where( obj => obj != null ).ToList();

      List<Autodesk.Revit.DB.Wall> ret = new List<Autodesk.Revit.DB.Wall>();
      List<Curve> segments = GetSegmentList( myWall.baseCurve );

      // If there are no existing document objects, create them.
      if ( docObjs.Count == 0 )
      {
        foreach ( var baseCurve in segments )
        {
          if ( myWall.level == null )
            myWall.level = new SpeckleElements.Level() { elevation = baseCurve.GetEndPoint( 0 ).Z / Scale, levelName = "Speckle Level " + baseCurve.GetEndPoint( 0 ).Z / Scale };

          var levelId = ( ( Level ) myWall.level.ToNative() ).Id;
          var revitWall = Wall.Create( Doc, baseCurve, levelId, false );
          revitWall = SetWallHeightOffset( revitWall, myWall.height, myWall.offset );
          ret.Add( revitWall );
        }
        return ret;
      }
      // If there are as many docobjects as segments, edit them all
      else if ( segments.Count == docObjs.Count )
      {
        for ( int i = 0; i < segments.Count; i++ )
        {
          LocationCurve locationCurve = ( LocationCurve ) ( ( Wall ) docObjs[ i ] ).Location;
          myWall.level?.ToNative();
          locationCurve.Curve = segments[ i ];
          SetWallHeightOffset( ( Wall ) docObjs[ i ], myWall.height, myWall.offset );
          ret.Add( docObjs[ i ] as Wall );
        }
        return ret;
      }
      // If there are more new segments than doc objects, edit the existing and create the new
      else if ( segments.Count > docObjs.Count )
      {
        //Edit existing walls
        for ( int i = 0; i < docObjs.Count; i++ )
        {
          LocationCurve locationCurve = ( LocationCurve ) ( ( Wall ) docObjs[ i ] ).Location;
          myWall.level?.ToNative();
          locationCurve.Curve = segments[ i ];
          SetWallHeightOffset( ( Wall ) docObjs[ i ], myWall.height, myWall.offset );
          ret.Add( docObjs[ i ] as Wall );
        }

        //Add new walls
        for ( int i = docObjs.Count; i < segments.Count; i++ )
        {
          var baseCurve = segments[ i ];
          if ( myWall.level == null )
            myWall.level = new SpeckleElements.Level() { elevation = baseCurve.GetEndPoint( 0 ).Z, levelName = "Speckle Level " + baseCurve.GetEndPoint( 0 ).Z };

          var levelId = ( ( Level ) myWall.level.ToNative() ).Id;
          var revitWall = Wall.Create( Doc, baseCurve, levelId, false );
          revitWall = SetWallHeightOffset( revitWall, myWall.height * Scale, myWall.offset * Scale );
          ret.Add( revitWall );
        }

        return ret;
      }
      // If there are more doc objects than segments, edit the existing ones and delete the rest!
      else if ( segments.Count < docObjs.Count )
      {
        // Deletion
        for ( int i = segments.Count; i < docObjs.Count; i++ )
        {
          Doc.Delete( docObjs[ i ].Id );
        }
        // Editing
        for ( int i = 0; i < segments.Count; i++ )
        {
          LocationCurve locationCurve = ( LocationCurve ) ( ( Wall ) docObjs[ i ] ).Location;
          myWall.level?.ToNative();
          locationCurve.Curve = segments[ i ];
          SetWallHeightOffset( ( Wall ) docObjs[ i ], myWall.height, myWall.offset );
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
      if ( heightParam != null && !heightParam.IsReadOnly )
        heightParam.Set( height * Scale );

      var offsetParam = revitWall.get_Parameter( BuiltInParameter.WALL_BASE_OFFSET );
      if ( offsetParam != null && !offsetParam.IsReadOnly )
        offsetParam.Set( offset );

      return revitWall;
    }

    // TODO: Wall to Speckle
    public static SpeckleElements.Wall ToSpeckle( this Autodesk.Revit.DB.Wall myWall )
    {
      var speckleWall = new SpeckleElements.Wall();
      speckleWall.baseCurve = SpeckleCore.Converter.Serialise( ( ( LocationCurve ) myWall.Location ).Curve );

      var type = myWall.GetType();

      speckleWall.parameters = GetElementParams( myWall );

      // Get the mesh of the element
      var opts = new Options(); opts.DetailLevel = ViewDetailLevel.Medium;
      var geo = myWall.get_Geometry( opts );

      foreach ( Solid mySolid in geo )
      {
        if ( mySolid == null ) continue;

        var faceArr = new List<int>();
        var vertexArr = new List<double>();

        int prevVertCount = 0;

        foreach ( Face f in mySolid.Faces )
        {
          var m = f.Triangulate();
          var points = m.Vertices;

          foreach ( var point in m.Vertices )
          {
            vertexArr.AddRange( new double[ ] { point.X / Scale, point.Y / Scale, point.Z / Scale } );
          }

          for ( int i = 0; i < m.NumTriangles; i++ )
          {
            var triangle = m.get_Triangle( i );
            var A = triangle.get_Index( 0 );
            var B = triangle.get_Index( 1 );
            var C = triangle.get_Index( 2 );

            faceArr.Add( 0 ); // TRIANGLE flag
            faceArr.Add( ( int ) A + prevVertCount );
            faceArr.Add( ( int ) B + prevVertCount );
            faceArr.Add( ( int ) C + prevVertCount );
          }
          prevVertCount += m.Vertices.Count;
        }

        speckleWall.Faces = faceArr;
        speckleWall.Vertices = vertexArr;
      }

      speckleWall.GenerateHash();

      return speckleWall;
    }
  }
}
