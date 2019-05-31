using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using SpeckleCore;
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
      var baseLine = (Curve) SpeckleCore.Converter.Deserialise( myCol.baseLine, new string[ ] { "dynamo" } );
      var start = baseLine.GetEndPoint( 0 );
      var end = baseLine.GetEndPoint( 1 );

      if( docObj != null )
      {
        var type = Doc.GetElement( docObj.GetTypeId() ) as ElementType;
        if( myCol.columnType != null && (myCol.columnType != type.Name || myCol.columnFamily != type.FamilyName) )
        {
          Doc.Delete( docObj.Id );
          // Will create a new one, exits fully this nested if
        }
        else
        {
          // Edit Endpoints and return
          var existingFamilyInstance = (Autodesk.Revit.DB.FamilyInstance) docObj;
          existingFamilyInstance.get_Parameter( BuiltInParameter.SLANTED_COLUMN_TYPE_PARAM ).Set( (double) SlantedOrVerticalColumnType.CT_EndPoint );
          var existingLocationCurve = existingFamilyInstance.Location as LocationCurve;
          existingLocationCurve.Curve = baseLine;

          //existingFamilyInstance.ChangeTypeId();
          return existingFamilyInstance;
        }
      }

      // below, new creation of a column.
      FamilySymbol sym;
      sym = GetFamilySymbolByFamilyNameAndTypeAndCategory( myCol.columnFamily, myCol.columnType, BuiltInCategory.OST_StructuralColumns );

      if( sym == null )
      {
        sym = GetFamilySymbolByFamilyNameAndTypeAndCategory( myCol.columnFamily, myCol.columnType, BuiltInCategory.OST_Columns );
      }

      if( sym == null )
      {
        MissingFamiliesAndTypes.Add( myCol.columnFamily + " " + myCol.columnType );
        return null;
      }

      if( myCol.baseLevel == null )
        myCol.baseLevel = new SpeckleElements.Level() { elevation = baseLine.GetEndPoint( 0 ).Z / Scale, levelName = "Speckle Level " + baseLine.GetEndPoint( 0 ).Z / Scale };
      var myLevel = myCol.baseLevel.ToNative() as Autodesk.Revit.DB.Level;

      if( !sym.IsActive ) sym.Activate();

      var familyInstance = Doc.Create.NewFamilyInstance( start, sym, myLevel, Autodesk.Revit.DB.Structure.StructuralType.Column );

      familyInstance.get_Parameter( BuiltInParameter.SLANTED_COLUMN_TYPE_PARAM ).Set( (double) SlantedOrVerticalColumnType.CT_EndPoint );

      if( myCol.topLevel != null )
      {
        var myTopLevel = myCol.topLevel.ToNative();
        familyInstance.get_Parameter( BuiltInParameter.FAMILY_TOP_LEVEL_PARAM ).Set( myTopLevel.Id );
      }

      familyInstance.get_Parameter( BuiltInParameter.FAMILY_BASE_LEVEL_OFFSET_PARAM ).Set( myCol.bottomOffset * Scale );
      familyInstance.get_Parameter( BuiltInParameter.FAMILY_TOP_LEVEL_OFFSET_PARAM ).Set( myCol.topOffset * Scale );

      var locationCurve = familyInstance.Location as LocationCurve;
      locationCurve.Curve = baseLine;

      return familyInstance;
    }

    public static List<SpeckleObject> ColumnToSpeckle( Autodesk.Revit.DB.FamilyInstance myFamily )
    {
      var myColumn = new Column();
      var allSolids = GetElementSolids( myFamily, opt: new Options() { DetailLevel = ViewDetailLevel.Fine, ComputeReferences = true } );

      (myColumn.Faces, myColumn.Vertices) = GetFaceVertexArrFromSolids( allSolids );

      myColumn.baseLine = (SpeckleCoreGeometryClasses.SpeckleLine) SpeckleCore.Converter.Serialise( myFamily.GetAnalyticalModel().GetCurve() );

      myColumn.columnFamily = myFamily.Symbol.FamilyName;
      myColumn.columnType = Doc.GetElement( myFamily.GetTypeId() ).Name;

      myColumn.parameters = GetElementParams( myFamily );

      var baseLevel = (Autodesk.Revit.DB.Level) Doc.GetElement( myFamily.get_Parameter( BuiltInParameter.FAMILY_BASE_LEVEL_PARAM ).AsElementId() );
      var topLevel = (Autodesk.Revit.DB.Level) Doc.GetElement( myFamily.get_Parameter( BuiltInParameter.FAMILY_TOP_LEVEL_PARAM ).AsElementId() );

      myColumn.baseLevel = baseLevel?.ToSpeckle();
      myColumn.topLevel = topLevel?.ToSpeckle();

      var bottomAttOffset = myFamily.get_Parameter( BuiltInParameter.FAMILY_BASE_LEVEL_OFFSET_PARAM )?.AsDouble();
      myColumn.bottomOffset = bottomAttOffset != null ? (double) bottomAttOffset / Scale : 0.0;

      var topAttOffset = myFamily.get_Parameter( BuiltInParameter.FAMILY_TOP_LEVEL_OFFSET_PARAM )?.AsDouble();
      myColumn.topOffset = topAttOffset != null ? (double) topAttOffset / Scale : 0.0;

      myColumn.GenerateHash();
      myColumn.ApplicationId = myFamily.UniqueId;

      //var analyticalModel = AnalyticalStickToSpeckle(myFamily);

      return new List<SpeckleObject>() { myColumn };//.Concat(analyticalModel).ToList();
    }
  }
}
