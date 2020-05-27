using Autodesk.Revit.DB;
using SpeckleCore;
using SpeckleCoreGeometryClasses;
using SpeckleElementsClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeckleElementsRevit
{
  public static partial class Conversions
  {

    public static Room ToSpeckle(this Autodesk.Revit.DB.Architecture.Room myRoom )
    {
      var speckleRoom = new Room();

      speckleRoom.roomName = myRoom.get_Parameter(Autodesk.Revit.DB.BuiltInParameter.ROOM_NAME).AsString();
      speckleRoom.roomNumber = myRoom.Number;


      var locPt = ((Autodesk.Revit.DB.LocationPoint) myRoom.Location).Point;
      speckleRoom.roomLocation = new SpecklePoint( locPt.X / Scale, locPt.Y / Scale, locPt.Z / Scale );

      (speckleRoom.Faces, speckleRoom.Vertices) = GetFaceVertexArrayFromElement( myRoom );

      // TODO: Get and set the boundary curve
      var seg = myRoom.GetBoundarySegments( new Autodesk.Revit.DB.SpatialElementBoundaryOptions() );

      var myPolyCurve = new SpecklePolycurve() { Segments = new List<SpeckleCore.SpeckleObject>() };
      foreach(BoundarySegment segment in seg[0])
      {
        var crv = segment.GetCurve();
        var converted = SpeckleCore.Converter.Serialise( crv );
        myPolyCurve.Segments.Add( converted as SpeckleObject );
      }
      speckleRoom.baseCurve = myPolyCurve;
      speckleRoom.parameters = GetElementParams( myRoom );
      if ( myRoom.IsValidType(myRoom.GetTypeId()) )
        speckleRoom.typeParameters = GetElementTypeParams( myRoom );

      speckleRoom.ApplicationId = myRoom.UniqueId;
      speckleRoom.elementId = myRoom.Id.ToString();
      speckleRoom.GenerateHash();

      return speckleRoom;
    }

  }
}
