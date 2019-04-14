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

    public static Autodesk.Revit.DB.Floor ToNative( this SpeckleElements.Floor mySlab )
    {
      var (docObj, stateObj) = GetExistingElementByApplicationId( mySlab.ApplicationId, mySlab.Type );

      var slabCurves = new CurveArray();
      var segments = GetSegmentList( mySlab.baseCurve );
      foreach ( var x in segments ) slabCurves.Append( x );

      if ( mySlab.level == null )
        mySlab.level = new SpeckleElements.Level() { createView = true, elevation = segments[ 0 ].GetEndPoint( 0 ).Z / Scale, levelName = "Speckle Level " + segments[ 0 ].GetEndPoint( 0 ).Z / Scale };


      // TODO: Editing a slab profile is a pain apparently.
      // See https://thebuildingcoder.typepad.com/blog/2008/11/editing-a-floor-profile.html
      if ( docObj != null )
        Doc.Delete( docObj.Id );

      if ( mySlab.floorType == null )
      {
        var myNullTypeFloor = Doc.Create.NewFloor( slabCurves, false );
        return myNullTypeFloor;
      }

      FloorType type = ( FloorType ) GetElementByClassAndName( typeof( Autodesk.Revit.DB.FloorType ), mySlab.floorType );
      var myTypeBasedFloor = Doc.Create.NewFloor( slabCurves, type, ( ( Autodesk.Revit.DB.Level ) mySlab.level.ToNative() ), false );

      return myTypeBasedFloor;
    }

    public static SpeckleElements.Floor ToSpeckle( this Autodesk.Revit.DB.Floor myFloor )
    {
      // TODO: convert floor to slaby slab
      var speckleFloor = new SpeckleElements.Floor();

      speckleFloor.parameters = GetElementParams( myFloor );
      (speckleFloor.Faces, speckleFloor.Vertices) = GetElementMesh( myFloor );

      var geo = myFloor.get_Geometry( new Options() { DetailLevel = ViewDetailLevel.Medium });

      speckleFloor.baseCurve = getFloorOutline( myFloor );

      speckleFloor.ApplicationId = myFloor.UniqueId;
      speckleFloor.GenerateHash();

      return speckleFloor;
    }

    public static SpecklePolycurve getFloorOutline( Autodesk.Revit.DB.Floor myFloor )
    {
      var geometry = myFloor.get_Geometry( new Options() { DetailLevel = ViewDetailLevel.Medium });
      var poly = new SpecklePolycurve();
      poly.Segments = new List<SpeckleObject>();

      foreach (Solid solid in geometry) // let's hope it's only one?
      {
        if ( solid == null ) continue;
        var f = GetLowestFace( solid );
        var crvLoops = f.GetEdgesAsCurveLoops();
        foreach(var crvloop in crvLoops)
        {
          foreach(var curve in crvloop )
          {
            var c = curve as Curve;
            if ( c == null ) continue;
            poly.Segments.Add( SpeckleCore.Converter.Serialise( c ));
          }
        }
      }
      return poly;
    }

    public static Face GetLowestFace(Solid mySolid)
    {
      PlanarFace lowest = null;
      foreach(var face in mySolid.Faces)
      {
        var planarFace = face as PlanarFace;
        if ( planarFace == null ) continue;
        if ( lowest == null ) lowest = planarFace;
        if ( lowest.Origin.Z < planarFace.Origin.Z ) lowest = planarFace;
      }
      return lowest;
    }
  }
}
