using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using SpeckleCore;
using SpeckleCoreGeometryClasses;
using SpeckleElementsClasses;

namespace SpeckleElementsRevit
{
  public static partial class Conversions
  {

    public static Autodesk.Revit.DB.Floor ToNative( this SpeckleElementsClasses.Floor mySlab )
    {
      var (docObj, stateObj) = GetExistingElementByApplicationId( mySlab.ApplicationId, mySlab.Type );

      //if( stateObj != null && Convert.ToBoolean( stateObj.Properties[ "userModified" ] ) == false && docObj != null )
      //{
      //  return docObj as Autodesk.Revit.DB.Floor;
      //}

      var slabCurves = new CurveArray();
      var segments = GetSegmentList( mySlab.baseCurve );
      foreach( var x in segments ) slabCurves.Append( x );

      if( mySlab.level == null )
        mySlab.level = new SpeckleElementsClasses.Level() { createView = true, elevation = segments[ 0 ].GetEndPoint( 0 ).Z / Scale };

      // NOTE: I have not found a way to edit a slab outline properly, so whenever we bake, we renew the element.
      if( docObj != null )
        Doc.Delete( docObj.Id );

      if( mySlab.floorType == null )
      {
        var myNullTypeFloor = Doc.Create.NewFloor( slabCurves, false );

        SetElementParams( myNullTypeFloor, mySlab.parameters );
        SetElementTypeParams(myNullTypeFloor, mySlab.parameters);
        return myNullTypeFloor;
      }

      FloorType type = (FloorType) GetElementByClassAndName( typeof( Autodesk.Revit.DB.FloorType ), mySlab.floorType );

      var fltype = GetElementByName( typeof( FloorType ), mySlab.floorType );

      var myTypeBasedFloor = Doc.Create.NewFloor( slabCurves, type, ((Autodesk.Revit.DB.Level) mySlab.level.ToNative()), false );

      SetElementParams( myTypeBasedFloor, mySlab.parameters );
      return myTypeBasedFloor;
    }

    public static SpeckleElementsClasses.Floor ToSpeckle( this Autodesk.Revit.DB.Floor myFloor )
    {
      var speckleFloor = new SpeckleElementsClasses.Floor();

      speckleFloor.parameters = GetElementParams( myFloor );
      speckleFloor.typeParameters = GetElementTypeParams(myFloor);

      var geo = myFloor.get_Geometry( new Options() { DetailLevel = ViewDetailLevel.Medium } );

      speckleFloor.floorType = myFloor.FloorType.Name;
      speckleFloor.baseCurve = getFloorOutline( myFloor );

      speckleFloor.ApplicationId = myFloor.UniqueId;
      speckleFloor.elementId = myFloor.Id.ToString();
      speckleFloor.GenerateHash();

      (speckleFloor.Faces, speckleFloor.Vertices) = GetFaceVertexArrayFromElement( myFloor, new Options() { DetailLevel = ViewDetailLevel.Fine, ComputeReferences = false } );

      return speckleFloor;
    }

    public static SpecklePolycurve getFloorOutline( Autodesk.Revit.DB.Floor myFloor )
    {
      var geometry = myFloor.get_Geometry( new Options() { DetailLevel = ViewDetailLevel.Medium } );
      var poly = new SpecklePolycurve();
      poly.Segments = new List<SpeckleObject>();

      foreach( Solid solid in geometry ) // let's hope it's only one?
      {
        if( solid == null ) continue;
        var f = GetLowestFace( solid );
        var crvLoops = f.GetEdgesAsCurveLoops();
        foreach( var crvloop in crvLoops )
        {
          foreach( var curve in crvloop )
          {
            var c = curve as Autodesk.Revit.DB.Curve;

            if ( c == null ) continue;
            poly.Segments.Add( SpeckleCore.Converter.Serialise( c ) as SpeckleObject );
          }
        }
      }
      return poly;
    }

    public static Face GetLowestFace( Solid mySolid )
    {
      PlanarFace lowest = null;
      foreach( var face in mySolid.Faces )
      {
        var planarFace = face as PlanarFace;
        if( planarFace == null ) continue;
        if( lowest == null ) lowest = planarFace;
        if( lowest.Origin.Z < planarFace.Origin.Z ) lowest = planarFace;
      }
      return lowest;
    }
  }
}
