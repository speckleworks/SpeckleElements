using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpeckleCore;
using SpeckleElements;

namespace SpeckleElementsRevit
{
  public class Initialiser : ISpeckleInitializer
  {
    public Initialiser( ) { }
  }

  public static partial class Conversions
  {
    public static GetCurrentDocument( )
    {
      return Autodesk.Revit.UI.UIApplication
    }

    public static Autodesk.Revit.DB.Grid ToNative(this GridLine myGridLine)
    {
      return null;
    }
  }
}
