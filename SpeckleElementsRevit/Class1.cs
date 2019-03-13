using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.UI;
using SpeckleCore;
using SpeckleElements;
using SpeckleRevit;

namespace SpeckleElementsRevit
{
  public class Initialiser : ISpeckleInitializer
  {
    public Initialiser( ) { }
  }

  public static partial class Conversions
  {
    public static UIDocument GetCurrentDocument( )
    {
      return SpeckleRevit.UI.SpeckleUiBindingsRevit.CurrentDoc;
    }

    public static Autodesk.Revit.DB.Grid ToNative(this GridLine myGridLine)
    {
      return null;
    }
  }
}
