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
        else
        {
          // Edit location curve
          var existingFamilyInstance = ( Autodesk.Revit.DB.FamilyInstance ) docObj;
          var existingLocationCurve = existingFamilyInstance.Location as LocationCurve;
          existingLocationCurve.Curve = baseLine;
          return existingFamilyInstance;
        }
      }

      FamilySymbol sym;
      sym = GetFamilySymbolByFamilyNameAndTypeAndCategory( myBeam.beamFamily, myBeam.beamType, BuiltInCategory.OST_StructuralFraming );

      if ( sym == null )
        sym = GetFamilySymbolByFamilyNameAndTypeAndCategory( myBeam.beamFamily, myBeam.beamType, BuiltInCategory.OST_BeamAnalytical );

      if ( sym == null )
        return null;

      if ( myBeam.level == null )
        myBeam.level = new SpeckleElements.Level() { elevation = 0, levelName = "Speckle Level 0" };
      var myLevel = myBeam.level.ToNative() as Autodesk.Revit.DB.Level;

      if ( !sym.IsActive ) sym.Activate();
      var familyInstance = Doc.Create.NewFamilyInstance( baseLine, sym, myLevel, StructuralType.Beam );

      return familyInstance;
    }
  }
}
