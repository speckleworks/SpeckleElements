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

      //var baseLine = GetSegmentList( myCol.baseLine )[ 0 ];
      var baseLine = ( Curve ) SpeckleCore.Converter.Deserialise( myCol.baseLine );
      var start = baseLine.GetEndPoint( 0 );
      var end = baseLine.GetEndPoint( 1 );

      if ( docObj != null )
      {
        var type = Doc.GetElement( docObj.GetTypeId() ) as ElementType;
        if ( myCol.columnType != null && ( myCol.columnType != type.Name || myCol.columnFamily != type.FamilyName ) )
        {
          Doc.Delete( docObj.Id );
          // Will create a new one, exits fully this nested if
        }
        else
        {
          // Edit Endpoints and return
          var existingFamilyInstance = ( Autodesk.Revit.DB.FamilyInstance ) docObj;
          existingFamilyInstance.get_Parameter( BuiltInParameter.SLANTED_COLUMN_TYPE_PARAM ).Set( ( double ) SlantedOrVerticalColumnType.CT_EndPoint );
          var existingLocationCurve = existingFamilyInstance.Location as LocationCurve;
          existingLocationCurve.Curve = baseLine;
          return existingFamilyInstance;
        }
      }

      // below, new creation of a beam.
      FamilySymbol sym;
      sym = GetFamilySymbolByFamilyNameAndTypeAndCategory( myCol.columnFamily, myCol.columnType, BuiltInCategory.OST_StructuralColumns );

      if ( sym == null )
        sym = GetFamilySymbolByFamilyNameAndTypeAndCategory( myCol.columnFamily, myCol.columnType, BuiltInCategory.OST_Columns );

      if ( sym == null )
        return null;

      if ( myCol.level == null )
        myCol.level = new SpeckleElements.Level() { elevation = baseLine.GetEndPoint( 0 ).Z / Scale, levelName = "Speckle Level " + baseLine.GetEndPoint( 0 ).Z / Scale };
      var myLevel = myCol.level.ToNative() as Autodesk.Revit.DB.Level;

      if ( !sym.IsActive ) sym.Activate();

      var familyInstance = Doc.Create.NewFamilyInstance( start, sym, myLevel, Autodesk.Revit.DB.Structure.StructuralType.Column );
      familyInstance.get_Parameter( BuiltInParameter.SLANTED_COLUMN_TYPE_PARAM ).Set( ( double ) SlantedOrVerticalColumnType.CT_EndPoint );
      var locationCurve = familyInstance.Location as LocationCurve;
      locationCurve.Curve = baseLine;

      return familyInstance;
    }

    public static Column ColumnToSpeckle( Autodesk.Revit.DB.FamilyInstance myFamily )
    {
      var myColumn = new Column();
      var allSolids = GetElementSolids( myFamily, opt: new Options() { DetailLevel = ViewDetailLevel.Fine, ComputeReferences = true } );

      (myColumn.Faces, myColumn.Vertices) = GetFaceVertexArrFromSolids( allSolids );
      
      myColumn.baseLine = (SpeckleCoreGeometryClasses.SpeckleLine) SpeckleCore.Converter.Serialise(  myFamily.GetAnalyticalModel().GetCurve() );

      myColumn.columnFamily = myFamily.Symbol.FamilyName;
      myColumn.columnType = Doc.GetElement( myFamily.GetTypeId()).Name;

      myColumn.parameters = GetElementParams( myFamily );

      myColumn.GenerateHash();
      myColumn.ApplicationId = myFamily.UniqueId;

      return myColumn;
    }
  }
}
