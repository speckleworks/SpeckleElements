using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using SpeckleCore;
using SpeckleElementsClasses;
using SpeckleCoreGeometryClasses;
using SpeckleCore.Data;

namespace SpeckleElementsRevit
{
  public static partial class Conversions
  {
    public static Autodesk.Revit.DB.FamilyInstance ToNative(this SpeckleElementsClasses.FamilyInstance myFamInst)
    {
      var (docObj, stateObj) = GetExistingElementByApplicationId(myFamInst.ApplicationId, myFamInst.Type);


      // get family symbol; it's used throughout
      FamilySymbol familySymbol = GetFamilySymbolByFamilyNameAndType(myFamInst.familyName, myFamInst.familyType);

      // Freak out if we don't have a symbol.
      if (familySymbol == null)
      {
        ConversionErrors.Add(new SpeckleConversionError { Message = $"Missing family: {myFamInst.familyName} {myFamInst.familyType}" });
        throw new RevitFamilyNotFoundException($"No such family found in the project");
      }

      // Activate the symbol yo! 
      if (!familySymbol.IsActive) familySymbol.Activate();

      XYZ xyz = (XYZ)SpeckleCore.Converter.Deserialise(obj: myFamInst.basePoint, excludeAssebmlies: new string[] { "SpeckleCoreGeometryDynamo" });

   
      var myTypeBasedFamInst = Doc.Create.NewFamilyInstance(xyz, familySymbol, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);

      SetElementParams(myTypeBasedFamInst, myFamInst.parameters);
      return myTypeBasedFamInst;
    }



    /// <summary>
    /// Entry point for all revit family conversions. TODO: Check for Beams and Columns and any other "dedicated" speckle elements and convert them as such rather than to the generic "family instance" object.
    /// </summary>
    /// <param name="myElement"></param>
    /// <returns></returns>
    public static object ToSpeckle( this Autodesk.Revit.DB.FamilyInstance myFamily )
    {
      // TODO: check if this family is a column (BuiltInCategory.OST_StructuralColumns)
      // or a beam (BuiltInCategory.OST_StructuralFraming or BuiltInCategory.OST_BeamAnalytical?)

      if ( myFamily.Category.Id.IntegerValue == ( int ) BuiltInCategory.OST_StructuralFraming )
      {
        return BeamToSpeckle( myFamily );
      }

      if ( myFamily.Category.Id.IntegerValue == ( int ) BuiltInCategory.OST_StructuralColumns || myFamily.Category.Id.IntegerValue == ( int ) BuiltInCategory.OST_Columns )
      {
        return ColumnToSpeckle( myFamily );
      }

      var speckleFamily = new SpeckleElementsClasses.FamilyInstance();

      speckleFamily.parameters = GetElementParams( myFamily );

      var allSolids = GetElementSolids( myFamily, opt: new Options() { DetailLevel = ViewDetailLevel.Fine, ComputeReferences = true } );

      var famSubElements = GetFamSubElements( myFamily );
      foreach ( var sb in famSubElements )
      {
        allSolids.AddRange( GetElementSolids( sb ) );
      }

      (speckleFamily.Faces, speckleFamily.Vertices) = GetFaceVertexArrFromSolids( allSolids );

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
        if ( element is Autodesk.Revit.DB.FamilyInstance )
        {
          mySubElements.AddRange( GetFamSubElements( element as Autodesk.Revit.DB.FamilyInstance ) );
        }
      }
      return mySubElements;
    }
  }
}
