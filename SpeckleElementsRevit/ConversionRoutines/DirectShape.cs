using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using SpeckleCoreGeometryClasses;
using SpeckleElementsClasses;

namespace SpeckleElementsRevit
{
  public static partial class Conversions
  {
    public static Autodesk.Revit.DB.DirectShape ToNative( this SpeckleElementsClasses.DirectShape myDs )
    {
      var (docObj, stateObj) = GetExistingElementByApplicationId(myDs.ApplicationId, myDs.Type );

      //can't edit existing DS
      if ( docObj != null )
      {
        Doc.Delete( docObj.Id );
      }

      IList<GeometryObject> mesh = (IList<GeometryObject>)SpeckleCore.Converter.Deserialise(obj: (SpeckleMesh) myDs.directShapeMesh, excludeAssebmlies: new string[] { "SpeckleCoreGeometryDynamo", "SpeckleElementsRevit", "SpeckleStructuralRevit" });

      var cat = BuiltInCategory.OST_GenericModel;
      var bic = BuiltInCategories.GetFromCategory(myDs.category);

      BuiltInCategory.TryParse(bic, out cat);
      var catId = Doc.Settings.Categories.get_Item(cat).Id;

      var ds = Autodesk.Revit.DB.DirectShape.CreateElement(Doc, catId);
      ds.ApplicationId = myDs.ApplicationId;
      ds.ApplicationDataId = Guid.NewGuid().ToString();
      ds.SetShape(mesh);
      ds.Name = myDs.directShapeName;

      SetElementParams(ds, myDs.parameters);

      return ds;
    }

    //public static Topography ToSpeckle( this TopographySurface mySurface )
    //{
    //  var speckleTopo = new Topography();

    //  speckleTopo.Vertices = new List<double>();
    //  speckleTopo.Faces = new List<int>();

    //  var geom = mySurface.get_Geometry( new Options() );
    //  foreach ( var element in geom )
    //  {
    //    if ( element is Mesh )
    //    {
    //      var mesh = ( Mesh ) element;

    //      foreach ( var vert in mesh.Vertices )
    //      {
    //        speckleTopo.Vertices.AddRange( new double[ ] { vert.X / Scale, vert.Y / Scale, vert.Z / Scale } );
    //      }

    //      for ( int i = 0; i < mesh.NumTriangles; i++ )
    //      {
    //        var triangle = mesh.get_Triangle( i );
    //        var A = triangle.get_Index( 0 );
    //        var B = triangle.get_Index( 1 );
    //        var C = triangle.get_Index( 2 );
    //        speckleTopo.Faces.Add( 0 );
    //        speckleTopo.Faces.AddRange( new int[ ] { ( int ) A, ( int ) B, ( int ) C } );
    //      }
    //    }
    //  }

    //  speckleTopo.parameters = GetElementParams( mySurface );
    //  speckleTopo.ApplicationId = mySurface.UniqueId;

    //  speckleTopo.GenerateHash();
    //  return speckleTopo;
    //}
  }
}
