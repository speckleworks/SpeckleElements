using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using SpeckleCore;
using SpeckleElements;

namespace SpeckleElementsRevit
{
  public class Initialiser : ISpeckleInitializer
  {
    public Initialiser( ) { }

    /// <summary>
    /// Revit doc will be injected in here by the revit plugin. 
    /// To create a similar kit, make sure you declare this property in your initialiser class. 
    /// </summary>
    public static UIApplication RevitApp { get; set; }
    public static List<SpeckleStream> LocalRevitState { get; set; }
  }

  public static partial class Conversions
  {
    public static Document GetDoc( )
    {
      return Initialiser.RevitApp.ActiveUIDocument.Document;
    }

    public static Element GetExistingElementById( string _id )
    {
      foreach ( var stream in Initialiser.LocalRevitState )
      {
        var found = stream.Objects.FirstOrDefault( s => s._id == _id );
        if ( found != null )
          return GetDoc().GetElement( found.Properties[ "revitUniqueId" ] as string );
      }
      return null;
    }

    public static Autodesk.Revit.DB.Grid ToNative( this GridLine myGridLine )
    {
      var existing = GetExistingElementById( myGridLine._id );
      if ( existing != null )
      {
        // Enter "Edit mode"
        return null;
      }
      else
      {
        var res = Autodesk.Revit.DB.Grid.Create( GetDoc(), Line.CreateBound( new XYZ( myGridLine.Value[ 0 ], myGridLine.Value[ 1 ], myGridLine.Value[ 2 ] ), new XYZ( myGridLine.Value[ 3 ], myGridLine.Value[ 4 ], myGridLine.Value[ 5 ] ) ) );

        return res;
      }
    }
  }
}
