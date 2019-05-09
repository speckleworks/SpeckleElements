using SpeckleCoreGeometryClasses;
using SpeckleElements;
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
      speckleRoom.roomName = myRoom.Number;

      speckleRoom.roomLocation = (SpecklePoint) SpeckleCore.Converter.Serialise( myRoom.Location );

      (speckleRoom.Faces, speckleRoom.Vertices) = GetFaceVertexArrayFromElement( myRoom );

      var seg = myRoom.GetBoundarySegments( new Autodesk.Revit.DB.SpatialElementBoundaryOptions() );

      //var shell = myRoom.ClosedShell;
      speckleRoom.parameters = GetElementParams( myRoom );

      speckleRoom.ApplicationId = myRoom.UniqueId;
      speckleRoom.GenerateHash();

      return speckleRoom;
      return null;
    }

  }
}
