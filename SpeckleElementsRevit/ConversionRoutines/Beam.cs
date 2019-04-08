using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using SpeckleElements;

namespace SpeckleElementsRevit
{
  public static partial class Conversions
  {
    // TODO
    public static Element ToNative( this Beam myBeam )
    {
      var (docObj, stateObj) = GetExistingElementByApplicationId( myBeam.ApplicationId, myBeam.Type );

      var baseLine = ( Curve ) SpeckleCore.Converter.Deserialise( myBeam.baseLine );
      var start = baseLine.GetEndPoint( 0 );
      var end = baseLine.GetEndPoint( 1 );

      if ( docObj != null )
      {
        var type = Doc.GetElement( docObj.GetTypeId() ) as ElementType;
        if ( myBeam.beamType != null && ( myBeam.beamType != type.Name || myBeam.beamFamily != type.FamilyName ) )
        {
          Doc.Delete( docObj.Id );
          // Will create a new one, exits fully this nested if
        }
        //  else if ( ( bool ) stateObj.Properties[ "userModified" ] == true )
        //  {
        //    // Edit Endpoints and return
        //    var existingFamilyInstance = ( Autodesk.Revit.DB.FamilyInstance ) docObj;
        //    existingFamilyInstance.get_Parameter( BuiltInParameter.SLANTED_COLUMN_TYPE_PARAM ).Set( ( double ) SlantedOrVerticalColumnType.CT_EndPoint );
        //    var existingLocationCurve = existingFamilyInstance.Location as LocationCurve;
        //    existingLocationCurve.Curve = baseLine;
        //    return existingFamilyInstance;
        //  }
        //  else // nothing changed so get out
        //    return docObj;
      }

      FamilySymbol sym;

      try
      {
        sym = GetFamilySymbolByFamilyNameAndTypeAndCategory( myBeam.beamFamily, myBeam.beamType, BuiltInCategory.OST_StructuralFraming );
      }
      catch
      {
        sym = GetFamilySymbolByFamilyNameAndTypeAndCategory( myBeam.beamFamily, myBeam.beamType, BuiltInCategory.OST_Columns );
      }

      if ( myBeam.level == null )
        myBeam.level = new SpeckleElements.Level() { elevation = baseLine.GetEndPoint( 0 ).Z / Scale, levelName = "Speckle Level " + baseLine.GetEndPoint( 0 ).Z / Scale };
      var myLevel = myBeam.level.ToNative() as Autodesk.Revit.DB.Level;

      if ( !sym.IsActive ) sym.Activate();
      var familyInstance = Doc.Create.NewFamilyInstance(baseLine, sym, )
      //StructuralType.Beam;
      return null;
    }
  }
}
