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

    public static UIApplication RevitApp { get; set; }
  }

  public static partial class Conversions
  {
    public static Document GetDoc( )
    {
      return Initialiser.RevitApp.ActiveUIDocument.Document;
    }

    public static Autodesk.Revit.DB.Grid ToNative( this GridLine myGridLine )
    {
      var res = Autodesk.Revit.DB.Grid.Create( GetDoc(), Line.CreateBound( new XYZ( myGridLine.Value[ 0 ], myGridLine.Value[ 1 ], myGridLine.Value[ 2 ] ), new XYZ( myGridLine.Value[ 3 ], myGridLine.Value[ 4 ], myGridLine.Value[ 5 ] ) ) );

      return res;
    }
  }
}
