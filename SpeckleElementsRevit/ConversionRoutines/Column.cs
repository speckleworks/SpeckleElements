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
      var baseLine = (Curve) SpeckleCore.Converter.Deserialise( myCol.baseLine );
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
        else if ( ( bool ) stateObj.Properties[ "userModified" ] == true )
        {
          // Edit Endpoints and return
          var existingFamilyInstance = ( Autodesk.Revit.DB.FamilyInstance ) docObj;
          existingFamilyInstance.get_Parameter( BuiltInParameter.SLANTED_COLUMN_TYPE_PARAM ).Set( ( double ) SlantedOrVerticalColumnType.CT_EndPoint );
          var existingLocationCurve = existingFamilyInstance.Location as LocationCurve;
          existingLocationCurve.Curve = baseLine;
          return existingFamilyInstance;
        }
        else // nothing changed so get out
          return docObj;
      }

      // below, new creation of a beam.
      FamilySymbol sym;
      try
      {
        sym = GetFamilySymbolByFamilyNameAndTypeAndCategory( myCol.columnFamily, myCol.columnType, BuiltInCategory.OST_StructuralColumns );
      }
      catch
      {
        sym = GetFamilySymbolByFamilyNameAndTypeAndCategory( myCol.columnFamily, myCol.columnType, BuiltInCategory.OST_Columns );
      }

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

  }
}
