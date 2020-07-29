using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using SpeckleCore;
using SpeckleCore.Data;
using SpeckleElementsClasses;

namespace SpeckleElementsRevit
{
  public static partial class Conversions
  {
    //TODO
    public static Element ToNative(this Column myCol)
    {
      var (docObj, stateObj) = GetExistingElementByApplicationId(myCol.ApplicationId, myCol.Type);

      var exclusions = new List<string> { "Base Offset", "Top Offset" };

      //setting params on myCol from myCol.parameters, if they are missing
      if (myCol.parameters != null)
      {
        if (myCol.bottomOffset == null && myCol.parameters.ContainsKey("Base Offset"))
          myCol.bottomOffset = myCol.parameters["Base Offset"] as double?;

        if (myCol.topOffset == null && myCol.parameters.ContainsKey("Top Offset"))
          myCol.topOffset = myCol.parameters["Top Offset"] as double?;

      }

      var baseLine = (Autodesk.Revit.DB.Curve)SpeckleCore.Converter.Deserialise(obj: myCol.baseLine, excludeAssebmlies: new string[] { "SpeckleCoreGeometryDynamo" });
      var start = baseLine.GetEndPoint(0);
      var end = baseLine.GetEndPoint(1);

      var isVertical = IsColumnVertical(myCol);

      // get family symbol; it's used throughout
      FamilySymbol familySymbol = TryGetColumnFamilySymbol(myCol.columnFamily, myCol.columnType);

      // Freak out if we don't have a symbol.
      if (familySymbol == null)
      {
        ConversionErrors.Add(new SpeckleConversionError { Message = $"Missing family: {myCol.columnFamily} {myCol.columnType}" });
        throw new RevitFamilyNotFoundException($"No 'Column' family found in the project");
      }


      // Activate the symbol yo! 
      if (!familySymbol.IsActive) familySymbol.Activate();

      if (docObj != null)
      {
        var type = Doc.GetElement(docObj.GetTypeId()) as ElementType;

        // if family changed, tough luck - delete and rewind
        if (myCol.columnFamily != type.FamilyName)
        {
          Doc.Delete(docObj.Id);
        }
        else
        {
          // Edit Endpoints and return
          var existingFamilyInstance = (Autodesk.Revit.DB.FamilyInstance)docObj;

          // Edit curve only if i'm not vertical
          if (existingFamilyInstance.Location is LocationCurve)
          {
            existingFamilyInstance.get_Parameter(BuiltInParameter.SLANTED_COLUMN_TYPE_PARAM).Set((double)SlantedOrVerticalColumnType.CT_EndPoint);

            var existingLocationCurve = existingFamilyInstance.Location as LocationCurve;
            existingLocationCurve.Curve = baseLine;
          }
          else if (existingFamilyInstance.Location is LocationPoint)
          {
            var existingLocationPoint = existingFamilyInstance.Location as LocationPoint;
            existingLocationPoint.Point = start;
          }

          // check if type changed, and try and change it
          if (myCol.columnType != null && (myCol.columnType != type.Name))
          {
            existingFamilyInstance.ChangeTypeId(familySymbol.Id);
          }

          // Final preparations for good measure
          Autodesk.Revit.DB.Level mytopLevel;
          if (myCol.topLevel == null)
            mytopLevel = Doc.GetElement(existingFamilyInstance.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_PARAM).AsElementId()) as Autodesk.Revit.DB.Level;
          else
            mytopLevel = myCol.topLevel.ToNative() as Autodesk.Revit.DB.Level;

          Autodesk.Revit.DB.Level mybaseLevel;
          if (myCol.baseLevel == null)
            mybaseLevel = Doc.GetElement(existingFamilyInstance.get_Parameter(BuiltInParameter.FAMILY_BASE_LEVEL_PARAM).AsElementId()) as Autodesk.Revit.DB.Level;
          else
            mybaseLevel = myCol.baseLevel.ToNative() as Autodesk.Revit.DB.Level;

          SetColParams(myCol, existingFamilyInstance, baseLine, exclusions, mytopLevel, mybaseLevel);

          MatchFlippingAndRotation(existingFamilyInstance, myCol, baseLine);
          SetElementParams(existingFamilyInstance, myCol.parameters, exclusions);
          return existingFamilyInstance;
        }
      }

      // Create base level
      if (myCol.baseLevel == null)
        myCol.baseLevel = new SpeckleElementsClasses.Level() { elevation = baseLine.GetEndPoint(0).Z / Scale, levelName = "Speckle Level " + baseLine.GetEndPoint(0).Z / Scale };
      var baseLevel = myCol.baseLevel.ToNative() as Autodesk.Revit.DB.Level;

      Autodesk.Revit.DB.FamilyInstance familyInstance = null;

      if (myCol.parameters != null && myCol.parameters.ContainsKey("Column Style")) // Comes from revit
      {
        if (Convert.ToInt16(myCol.parameters["Column Style"]) == 2) // SLANTED
        {
          familyInstance = Doc.Create.NewFamilyInstance(baseLine, familySymbol, baseLevel, Autodesk.Revit.DB.Structure.StructuralType.Column);
        }
        else // NOT SLANTED
        {
          familyInstance = Doc.Create.NewFamilyInstance(start, familySymbol, baseLevel, Autodesk.Revit.DB.Structure.StructuralType.Column);
        }
      }
      else // Comes from gh
      {
        if (isVertical)
        {
          familyInstance = Doc.Create.NewFamilyInstance(start, familySymbol, baseLevel, Autodesk.Revit.DB.Structure.StructuralType.Column);
        }
        else
        {
          familyInstance = Doc.Create.NewFamilyInstance(baseLine, familySymbol, baseLevel, Autodesk.Revit.DB.Structure.StructuralType.Column);
        }
      }


      Autodesk.Revit.DB.Level myTopLevel = null;
      // Set the top level
      if (myCol.topLevel != null)
      {
        myTopLevel = myCol.topLevel.ToNative();
        var param = familyInstance.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_PARAM);
        if (param != null)
          param.Set(myTopLevel.Id);
      }

      SetColParams(myCol, familyInstance, baseLine, exclusions, myTopLevel, baseLevel);

      return familyInstance;
    }

    private static void SetColParams(Column myCol, Autodesk.Revit.DB.FamilyInstance familyInstance, Autodesk.Revit.DB.Curve baseLine, List<string> exclusions, Autodesk.Revit.DB.Level myTopLevel, Autodesk.Revit.DB.Level baseLevel)
    {
      if (myCol.bottomOffset == null)
        myCol.bottomOffset = 0;
      if (myCol.topOffset == null)
        myCol.topOffset = 0;

      var topParam = familyInstance.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_OFFSET_PARAM);
      var bottomParam = familyInstance.get_Parameter(BuiltInParameter.FAMILY_BASE_LEVEL_OFFSET_PARAM);

      //checking if BASE offset needs to be set before or after TOP offset
      if (myTopLevel != null && myTopLevel.Elevation + (double)myCol.bottomOffset / Scale <= baseLevel.Elevation)
      {
        if (bottomParam != null)
          bottomParam.Set((double)myCol.bottomOffset * Scale);
        if (topParam != null)
          topParam.Set((double)myCol.topOffset * Scale);
      }
      else
      {
        if (topParam != null)
          topParam.Set((double)myCol.topOffset * Scale);
        if (bottomParam != null)
          bottomParam.Set((double)myCol.bottomOffset * Scale);
      }

      // Final preparations
      MatchFlippingAndRotation(familyInstance, myCol, baseLine);
      SetElementParams(familyInstance, myCol.parameters, exclusions);
    }

    public static FamilySymbol TryGetColumnFamilySymbol(string columnFamily, string columnType)
    {
      FamilySymbol sym;
      sym = GetFamilySymbolByFamilyNameAndTypeAndCategory(columnFamily, columnType, BuiltInCategory.OST_StructuralColumns);

      if (sym == null)
      {
        sym = GetFamilySymbolByFamilyNameAndTypeAndCategory(columnFamily, columnType, BuiltInCategory.OST_Columns);
      }

      return sym;
    }

    private static void MatchFlippingAndRotation(Autodesk.Revit.DB.FamilyInstance myInstance, Column myColumn, Autodesk.Revit.DB.Curve baseLine)
    {

      // TODO: All these try catches can be replaced with if ( Dictionary.ContainsKey(LOLOLO) )
      try
      {
        var handFlip = Convert.ToBoolean(myColumn.Properties["__handFlipped"]);
        if (handFlip != myInstance.HandFlipped)
          myInstance.flipHand();
      }
      catch { }

      try
      {
        var faceFlip = Convert.ToBoolean(myColumn.Properties["__facingFlipped"]);
        if (faceFlip != myInstance.FacingFlipped)
          myInstance.flipFacing();
      }
      catch { }

      try
      {
        // TODO: Check against existing rotation (if any) and deduct that)
        var rotation = Convert.ToDouble(myColumn.Properties["__rotation"]);

        var start = baseLine.GetEndPoint(0);
        var end = baseLine.GetEndPoint(1);
        var myLine = Autodesk.Revit.DB.Line.CreateBound(start, end);

        if (myInstance.Location is LocationPoint)
          ((LocationPoint)myInstance.Location).Rotate(myLine, rotation - ((LocationPoint)myInstance.Location).Rotation);
        //else
        //  ElementTransformUtils.RotateElement( Doc, myInstance.Id, myLine, rotation );
      }
      catch (Exception e) { }
    }

    /// <summary>
    /// Checks whether the column is vertical or not.
    /// </summary>
    /// <param name="myCol"></param>
    /// <returns></returns>
    private static bool IsColumnVertical(Column myCol)
    {
      var lineArr = myCol.baseLine.Value;
      var diffX = Math.Abs(lineArr[0] - lineArr[3]);
      var diffY = Math.Abs(lineArr[1] - lineArr[4]);

      if (diffX < 0.1 && diffY < 0.1)
        return true;

      return false;
    }

    public static SpeckleObject ColumnToSpeckle(Autodesk.Revit.DB.FamilyInstance myFamily)
    {
      var myColumn = new Column();

      var baseLevel = (Autodesk.Revit.DB.Level)Doc.GetElement(myFamily.get_Parameter(BuiltInParameter.FAMILY_BASE_LEVEL_PARAM).AsElementId());
      var topLevel = (Autodesk.Revit.DB.Level)Doc.GetElement(myFamily.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_PARAM).AsElementId());

      myColumn.baseLevel = baseLevel?.ToSpeckle();
      myColumn.topLevel = topLevel?.ToSpeckle();

      try
      {
        myColumn.baseLine = (SpeckleCoreGeometryClasses.SpeckleLine)SpeckleCore.Converter.Serialise(myFamily.GetAnalyticalModel().GetCurve());
      }
      catch
      {
        var basePt = (myFamily.Location as LocationPoint).Point;
        var topPt = new XYZ(basePt.X, basePt.Y, topLevel.Elevation);
        myColumn.baseLine = (SpeckleCoreGeometryClasses.SpeckleLine)SpeckleCore.Converter.Serialise(Autodesk.Revit.DB.Line.CreateBound(basePt, topPt));
      }

      myColumn.columnFamily = myFamily.Symbol.FamilyName;
      myColumn.columnType = Doc.GetElement(myFamily.GetTypeId()).Name;

      myColumn.parameters = GetElementParams(myFamily);
      myColumn.typeParameters = GetElementTypeParams(myFamily);



      // TODO: Maybe move this column properties in the class defintion
      myColumn.Properties["__facingFlipped"] = myFamily.FacingFlipped;
      myColumn.Properties["__handFlipped"] = myFamily.HandFlipped;

      if (myFamily.Location is LocationPoint)
      {
        myColumn.Properties["__rotation"] = ((LocationPoint)myFamily.Location).Rotation;
      }
      else if (myFamily.Location is LocationCurve)
      {
        var myAngle = myFamily.get_Parameter(BuiltInParameter.STRUCTURAL_BEND_DIR_ANGLE).AsDouble(); // Stands for cross-section rotation!
        var rads = UnitUtils.ConvertFromInternalUnits(myAngle, DisplayUnitType.DUT_RADIANS);
        myColumn.Properties["__rotation"] = rads;

        // TODO: Figure this column rotation shit out. 
        // For now... Do nothing??
        //var t = myFamily.GetTotalTransform();
      }


      myColumn.GenerateHash();
      myColumn.ApplicationId = myFamily.UniqueId;
      myColumn.elementId = myFamily.Id.ToString();

      // leaving the mesh out of the hashing process might address the randomatic hash generation we're getting
      // and hence the nuking the usability of local caching and diffing
      var allSolids = GetElementSolids(myFamily, opt: new Options() { DetailLevel = ViewDetailLevel.Fine, ComputeReferences = true });
      (myColumn.Faces, myColumn.Vertices) = GetFaceVertexArrFromSolids(allSolids);

      return myColumn;
    }
  }
}
