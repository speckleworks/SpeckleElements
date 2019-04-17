using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using SpeckleCore;
using SpeckleElements;
using SpeckleCoreGeometryClasses;

namespace SpeckleElementsRevit
{
  public static partial class Conversions
  {
    // TODO: Family instance conversions and such

    /// <summary>
    /// Entry point for all revit family conversions. TODO: Check for Beams and Columns and any other "dedicated" speckle elements and convert them as such rather than to the generic "family instance" object.
    /// </summary>
    /// <param name="myElement"></param>
    /// <returns></returns>
    public static SpeckleObject ToSpeckle( this Autodesk.Revit.DB.FamilyInstance myFamily )
    {
      var speckleFamily = new SpeckleElements.FamilyInstance();

      speckleFamily.parameters = GetElementParams( myFamily );

      var allSolids = GetElementSolids( myFamily, opt: new Options() { DetailLevel = ViewDetailLevel.Fine, ComputeReferences = true } );

      var famSubElements = GetFamSubElements( myFamily );
      foreach(var sb in famSubElements)
      {
        allSolids.AddRange( GetElementSolids( sb ) );
      }

      (speckleFamily.Faces, speckleFamily.Vertices) = GetFaceVertexArrFromSolids( allSolids );

      // TODO: Get all subfamilies and sub-sub-sub families
      // get their frigging solids
      // mesh the fuckers


      //var test = myFamily.GetSubComponentIds();
      //var test2 = myFamily.GetSubelements();
      //var test3 = 1;

      //var test4 = Doc.GetElement( test.ToList()[0] );
      //var test234 = 1;

      //var allsolids2 = GetElementSolids( test4, opt: new Options() { DetailLevel = ViewDetailLevel.Fine, ComputeReferences = true } );
      //(speckleFamily.Faces, speckleFamily.Vertices) = GetFaceVertexArrFromSolids( allsolids2 );

      speckleFamily.GenerateHash();
      speckleFamily.ApplicationId = myFamily.UniqueId;

      return speckleFamily;
    }

    public static List<Element> GetFamSubElements( Autodesk.Revit.DB.FamilyInstance myFamily )
    {
      var mySubElements = new List<Element>();
      foreach ( var id in myFamily.GetSubComponentIds() )
      {
        var element = Doc.GetElement( id );
        mySubElements.Add( element );
        if(element is Autodesk.Revit.DB.FamilyInstance )
        {
          mySubElements.AddRange( GetFamSubElements( element as Autodesk.Revit.DB.FamilyInstance ) );
        }
      }
      return mySubElements;
    }
  }
}
