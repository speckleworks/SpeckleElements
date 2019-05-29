using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using SpeckleElements;

namespace SpeckleElementsRevit
{
  public static partial class Conversions
  {
    //TODO
    public static Element ToNative( this Column myCol )
    {
      var (docObj, stateObj) = GetExistingElementByApplicationId( myCol.ApplicationId, myCol.Type );

      var baseLine = (Curve) SpeckleCore.Converter.Deserialise( obj: myCol.baseLine, excludeAssebmlies: new string[ ] { "SpeckleCoreGeometryDynamo" } );
      var start = baseLine.GetEndPoint( 0 );
      var end = baseLine.GetEndPoint( 1 );

      // get family symbol; it's used throughout
      FamilySymbol familySymbol = TryGetColumnFamilySymbol( myCol.columnFamily, myCol.columnType );
      
      if( docObj != null )
      {
        var type = Doc.GetElement( docObj.GetTypeId() ) as ElementType;

        // if family changed, tough luck - delete and rewind
        if( myCol.columnFamily != type.FamilyName) 
        {
          Doc.Delete( docObj.Id );
        }
        else
        {
          // Edit Endpoints and return
          var existingFamilyInstance = (Autodesk.Revit.DB.FamilyInstance) docObj;
          existingFamilyInstance.get_Parameter( BuiltInParameter.SLANTED_COLUMN_TYPE_PARAM ).Set( (double) SlantedOrVerticalColumnType.CT_EndPoint );
          var existingLocationCurve = existingFamilyInstance.Location as LocationCurve;
          existingLocationCurve.Curve = baseLine;

          // check if type changed, and try and change it
          if( myCol.columnType != null && (myCol.columnType != type.Name))
          {
            existingFamilyInstance.ChangeTypeId( familySymbol.Id );
          }
          
          SetElementParams( existingFamilyInstance, myCol.parameters );
          return existingFamilyInstance;
        }
      }

      // below, new creation of a column.
      // fake out if we don't have a symbol.
      if( familySymbol == null )
        return null;


      if( !familySymbol.IsActive ) familySymbol.Activate();

      // Set base level
      if( myCol.baseLevel == null )
        myCol.baseLevel = new SpeckleElements.Level() { elevation = baseLine.GetEndPoint( 0 ).Z / Scale, levelName = "Speckle Level " + baseLine.GetEndPoint( 0 ).Z / Scale };
      var baseLevel = myCol.baseLevel.ToNative() as Autodesk.Revit.DB.Level;

     
      var familyInstance = Doc.Create.NewFamilyInstance( start, familySymbol, baseLevel, Autodesk.Revit.DB.Structure.StructuralType.Column );

      familyInstance.get_Parameter( BuiltInParameter.SLANTED_COLUMN_TYPE_PARAM ).Set( (double) SlantedOrVerticalColumnType.CT_EndPoint );

      if( myCol.topLevel != null )
      {
        var myTopLevel = myCol.topLevel.ToNative();
        familyInstance.get_Parameter( BuiltInParameter.FAMILY_TOP_LEVEL_PARAM ).Set( myTopLevel.Id );
      }

      var locationCurve = familyInstance.Location as LocationCurve;
      locationCurve.Curve = baseLine;

      SetElementParams( familyInstance, myCol.parameters );

      return familyInstance;
    }

    public static FamilySymbol TryGetColumnFamilySymbol(string columnFamily, string columnType )
    {
      FamilySymbol sym;
      sym = GetFamilySymbolByFamilyNameAndTypeAndCategory(columnFamily, columnType, BuiltInCategory.OST_StructuralColumns );

      if( sym == null )
      {
        sym = GetFamilySymbolByFamilyNameAndTypeAndCategory( columnFamily, columnType, BuiltInCategory.OST_Columns );
      }

      if( sym == null )
      {
        MissingFamiliesAndTypes.Add( columnFamily + " " + columnType );
      }

      return sym;
    }

    public static Column ColumnToSpeckle( Autodesk.Revit.DB.FamilyInstance myFamily )
    {
      var myColumn = new Column();

      myColumn.baseLine = (SpeckleCoreGeometryClasses.SpeckleLine) SpeckleCore.Converter.Serialise( myFamily.GetAnalyticalModel().GetCurve() );

      myColumn.columnFamily = myFamily.Symbol.FamilyName;
      myColumn.columnType = Doc.GetElement( myFamily.GetTypeId() ).Name;

      myColumn.parameters = GetElementParams( myFamily );

      var baseLevel = (Autodesk.Revit.DB.Level) Doc.GetElement( myFamily.get_Parameter( BuiltInParameter.FAMILY_BASE_LEVEL_PARAM ).AsElementId() );
      var topLevel = (Autodesk.Revit.DB.Level) Doc.GetElement( myFamily.get_Parameter( BuiltInParameter.FAMILY_TOP_LEVEL_PARAM ).AsElementId() );

      myColumn.baseLevel = baseLevel?.ToSpeckle();
      myColumn.topLevel = topLevel?.ToSpeckle();

      myColumn.Properties[ "facingFlipped" ] = myFamily.FacingFlipped;
      myColumn.Properties[ "handFlipped" ] = myFamily.HandFlipped;

      myColumn.GenerateHash();
      myColumn.ApplicationId = myFamily.UniqueId;

      // leaving the mesh out of the hashing process might address the randomatic hash generation we're getting
      // and hence the nuking the usability of local caching and diffing
      var allSolids = GetElementSolids( myFamily, opt: new Options() { DetailLevel = ViewDetailLevel.Fine, ComputeReferences = true } );
      (myColumn.Faces, myColumn.Vertices) = GetFaceVertexArrFromSolids( allSolids );

      return myColumn;
    }
  }
}
