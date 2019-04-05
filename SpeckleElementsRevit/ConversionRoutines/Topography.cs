using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using SpeckleElements;

namespace SpeckleElementsRevit
{
  public static partial class Conversions
  {
    public static TopographySurface ToNative( this Topography mySurface )
    {
      var (docObj, stateObj) = GetExistingElementByApplicationId( mySurface.ApplicationId, mySurface.Type );

      var pts = new List<XYZ>();
      for( int i = 0; i < mySurface.Vertices.Count; i += 3 )
      {
        pts.Add( new XYZ( mySurface.Vertices[ i ], mySurface.Vertices[ i + 1 ], mySurface.Vertices[ i + 2 ] ) );
      }

      if( docObj != null )
      {
        var srf = (TopographySurface) docObj;
        srf.DeletePoints( srf.GetPoints() );
        srf.AddPoints( pts );
        return srf;
      }
      return TopographySurface.Create( Doc, pts );
    }

    public static Topography ToSpeckle( this TopographySurface mySurface )
    {
      return null;
    }
  }
}
