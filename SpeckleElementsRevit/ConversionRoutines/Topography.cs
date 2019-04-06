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
        pts.Add( new XYZ( mySurface.Vertices[ i ]*Scale, mySurface.Vertices[ i + 1 ]*Scale, mySurface.Vertices[ i + 2 ]*Scale ) );
      }

      if( docObj != null )
      {
        Doc.Delete( docObj.Id );

        // TODO: Can't start a transaction here as we have started a global transaction for the creation of all objects. 
        // TODO: Let each individual ToNative method handle its own transactions. It's a big change, so will leave for later.

        //var srf = (TopographySurface) docObj;

        //using( TopographyEditScope e = new TopographyEditScope( Doc, "Speckle Topo Edit" ) )
        //{
        //  e.Start(srf.Id);
        //  srf.DeletePoints( srf.GetPoints() );
        //  srf.AddPoints( pts );
        //  e.Commit( null );
        //}
        //return srf;
      }

      return TopographySurface.Create( Doc, pts );
    }

    public static Topography ToSpeckle( this TopographySurface mySurface )
    {
      return null;
    }
  }
}
