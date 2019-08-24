using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using SpeckleCore;
using SpeckleElementsClasses;

namespace SpeckleElementsRevit
{
  public static partial class Conversions
  {
    //TODO
    public static Element ToNative( this Column myCol )
    {
      var (docObj, stateObj) = GetExistingElementByApplicationId( myCol.ApplicationId, myCol.Type );

      var exclusions = new List<string> { "Base Offset", "Top Offset" };

      var baseLine = ( Curve ) SpeckleCore.Converter.Deserialise( obj: myCol.baseLine, excludeAssebmlies: new string[ ] { "SpeckleCoreGeometryDynamo" } );
      var start = baseLine.GetEndPoint( 0 );
      var end = baseLine.GetEndPoint( 1 );

      var isVertical = IsColumnVertical( myCol );

      // get family symbol; it's used throughout
      FamilySymbol familySymbol = TryGetColumnFamilySymbol( myCol.columnFamily, myCol.columnType );

      // Freak out if we don't have a symbol.
      if ( familySymbol == null )
        return null;

      // Activate the symbol yo! 
      if ( !familySymbol.IsActive ) familySymbol.Activate();

      if ( docObj != null )
      {
        var type = Doc.GetElement( docObj.GetTypeId() ) as ElementType;

        // if family changed, tough luck - delete and rewind
        if ( myCol.columnFamily != type.FamilyName )
        {
          Doc.Delete( docObj.Id );
        }
        else
        {
          // Edit Endpoints and return
          var existingFamilyInstance = ( Autodesk.Revit.DB.FamilyInstance ) docObj;

          // Edit curve only if i'm not vertical
          if ( existingFamilyInstance.Location is LocationCurve )
          {
            existingFamilyInstance.get_Parameter( BuiltInParameter.SLANTED_COLUMN_TYPE_PARAM ).Set( ( double ) SlantedOrVerticalColumnType.CT_EndPoint );

            var existingLocationCurve = existingFamilyInstance.Location as LocationCurve;
            existingLocationCurve.Curve = baseLine;
          }
          else if ( existingFamilyInstance.Location is LocationPoint )
          {
            var existingLocationPoint = existingFamilyInstance.Location as LocationPoint;
            existingLocationPoint.Point = start;
          }

          // check if type changed, and try and change it
          if ( myCol.columnType != null && ( myCol.columnType != type.Name ) )
          {
            existingFamilyInstance.ChangeTypeId( familySymbol.Id );
          }

          // Final preparations for good measure
          MatchFlippingAndRotation( existingFamilyInstance, myCol, baseLine );
          SetElementParams( existingFamilyInstance, myCol.parameters, exclusions );
          return existingFamilyInstance;
        }
      }

      // Create base level
      if ( myCol.baseLevel == null )
        myCol.baseLevel = new SpeckleElementsClasses.Level() { elevation = baseLine.GetEndPoint( 0 ).Z / Scale, levelName = "Speckle Level " + baseLine.GetEndPoint( 0 ).Z / Scale };
      var baseLevel = myCol.baseLevel.ToNative() as Autodesk.Revit.DB.Level;

      Autodesk.Revit.DB.FamilyInstance familyInstance = null;

      if ( myCol.parameters.ContainsKey( "Column Style" ) ) // Comes from revit
      {
        if ( Convert.ToInt16( myCol.parameters[ "Column Style" ] ) == 2 ) // SLANTED
        {
          familyInstance = Doc.Create.NewFamilyInstance( baseLine, familySymbol, baseLevel, Autodesk.Revit.DB.Structure.StructuralType.Column );
        }
        else // NOT SLANTED
        {
          familyInstance = Doc.Create.NewFamilyInstance( start, familySymbol, baseLevel, Autodesk.Revit.DB.Structure.StructuralType.Column );
        }
      }
      else // Comes from gh
      {
        if ( isVertical )
        {
          familyInstance = Doc.Create.NewFamilyInstance( start, familySymbol, baseLevel, Autodesk.Revit.DB.Structure.StructuralType.Column );
        }
        else
        {
          familyInstance = Doc.Create.NewFamilyInstance( baseLine, familySymbol, baseLevel, Autodesk.Revit.DB.Structure.StructuralType.Column );
        }
      }


      Autodesk.Revit.DB.Level myTopLevel = null;
      // Set the top level
      if ( myCol.topLevel != null )
      {
        myTopLevel = myCol.topLevel.ToNative();
        familyInstance.get_Parameter( BuiltInParameter.FAMILY_TOP_LEVEL_PARAM ).Set( myTopLevel.Id );
      }

      //checking if BASE offset needs to be set before or after TOP offset
      if ( myTopLevel != null && myTopLevel.Elevation + ( double ) myCol.parameters[ "Base Offset" ] / Scale <= baseLevel.Elevation )
      {
        familyInstance.get_Parameter( BuiltInParameter.FAMILY_BASE_LEVEL_OFFSET_PARAM ).Set( ( double ) myCol.parameters[ "Base Offset" ] * Scale );
        familyInstance.get_Parameter( BuiltInParameter.FAMILY_TOP_LEVEL_OFFSET_PARAM ).Set( ( double ) myCol.parameters[ "Top Offset" ] * Scale );
      }
      else
      {
        familyInstance.get_Parameter( BuiltInParameter.FAMILY_TOP_LEVEL_OFFSET_PARAM ).Set( ( double ) myCol.parameters[ "Top Offset" ] * Scale );
        familyInstance.get_Parameter( BuiltInParameter.FAMILY_BASE_LEVEL_OFFSET_PARAM ).Set( ( double ) myCol.parameters[ "Base Offset" ] * Scale );
      }

      // Final preparations
      MatchFlippingAndRotation( familyInstance, myCol, baseLine );
      SetElementParams( familyInstance, myCol.parameters, exclusions );

      return familyInstance;
    }

    public static FamilySymbol TryGetColumnFamilySymbol( string columnFamily, string columnType )
    {
      FamilySymbol sym;
      sym = GetFamilySymbolByFamilyNameAndTypeAndCategory( columnFamily, columnType, BuiltInCategory.OST_StructuralColumns );

      if ( sym == null )
      {
        sym = GetFamilySymbolByFamilyNameAndTypeAndCategory( columnFamily, columnType, BuiltInCategory.OST_Columns );
      }

      if ( sym == null )
      {
        MissingFamiliesAndTypes.Add( columnFamily + " " + columnType );
      } else
      {
        MissingFamiliesAndTypes.Remove( columnFamily + " " + columnType ); // nasty
      }

      return sym;
    }

    private static void MatchFlippingAndRotation( Autodesk.Revit.DB.FamilyInstance myInstance, Column myColumn, Curve baseLine )
    {

      // TODO: All these try catches can be replaced with if ( Dictionary.ContainsKey(LOLOLO) )
      try
      {
        var handFlip = Convert.ToBoolean( myColumn.Properties[ "__handFlipped" ] );
        if ( handFlip != myInstance.HandFlipped )
          myInstance.flipHand();
      }
      catch { }

      try
      {
        var faceFlip = Convert.ToBoolean( myColumn.Properties[ "__facingFlipped" ] );
        if ( faceFlip != myInstance.FacingFlipped )
          myInstance.flipFacing();
      }
      catch { }

      try
      {
        // TODO: Check against existing rotation (if any) and deduct that)
        var rotation = Convert.ToDouble( myColumn.Properties[ "__rotation" ] );

        var start = baseLine.GetEndPoint( 0 );
        var end = baseLine.GetEndPoint( 1 );
        var myLine = Line.CreateBound( start, end );

        if ( myInstance.Location is LocationPoint )
          ( ( LocationPoint ) myInstance.Location ).Rotate( myLine, rotation - ( ( LocationPoint ) myInstance.Location ).Rotation );
        //else
        //  ElementTransformUtils.RotateElement( Doc, myInstance.Id, myLine, rotation );
      }
      catch ( Exception e ) { }
    }

    /// <summary>
    /// Checks whether the column is vertical or not.
    /// </summary>
    /// <param name="myCol"></param>
    /// <returns></returns>
    private static bool IsColumnVertical( Column myCol )
    {
      var lineArr = myCol.baseLine.Value;
      var diffX = Math.Abs( lineArr[ 0 ] - lineArr[ 3 ] );
      var diffY = Math.Abs( lineArr[ 1 ] - lineArr[ 4 ] );

      if ( diffX < 0.1 && diffY < 0.1 )
        return true;

      return false;
    }

    public static SpeckleObject ColumnToSpeckle( Autodesk.Revit.DB.FamilyInstance myFamily )
    {
      var myColumn = new Column();

      var baseLevel = ( Autodesk.Revit.DB.Level ) Doc.GetElement( myFamily.get_Parameter( BuiltInParameter.FAMILY_BASE_LEVEL_PARAM ).AsElementId() );
      var topLevel = ( Autodesk.Revit.DB.Level ) Doc.GetElement( myFamily.get_Parameter( BuiltInParameter.FAMILY_TOP_LEVEL_PARAM ).AsElementId() );

      myColumn.baseLevel = baseLevel?.ToSpeckle();
      myColumn.topLevel = topLevel?.ToSpeckle();

      try
      {
        myColumn.baseLine = ( SpeckleCoreGeometryClasses.SpeckleLine ) SpeckleCore.Converter.Serialise( myFamily.GetAnalyticalModel().GetCurve() );
      } catch
      {
        var basePt = (myFamily.Location as LocationPoint).Point;
        var topPt = new XYZ( basePt.X, basePt.Y, topLevel.Elevation );
        myColumn.baseLine = ( SpeckleCoreGeometryClasses.SpeckleLine ) SpeckleCore.Converter.Serialise( Line.CreateBound( basePt, topPt ) );
      }

      myColumn.columnFamily = myFamily.Symbol.FamilyName;
      myColumn.columnType = Doc.GetElement( myFamily.GetTypeId() ).Name;

      myColumn.parameters = GetElementParams( myFamily );



      // TODO: Maybe move this column properties in the class defintion
      myColumn.Properties[ "__facingFlipped" ] = myFamily.FacingFlipped;
      myColumn.Properties[ "__handFlipped" ] = myFamily.HandFlipped;

      if ( myFamily.Location is LocationPoint )
      {
        myColumn.Properties[ "__rotation" ] = ( ( LocationPoint ) myFamily.Location ).Rotation;
      }
      else if ( myFamily.Location is LocationCurve )
      {
        // TODO: Figure this column rotation shit out. 
        // For now... Do nothing??
        //var t = myFamily.GetTotalTransform();
      }


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
