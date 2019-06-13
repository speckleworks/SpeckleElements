using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using SpeckleCore;
using SpeckleCoreGeometryClasses;
using SpeckleElements;

namespace SpeckleElementsRevit
{
  public static partial class Conversions
  {
    //TODO
    public static Element ToNative(this Structural1DElement myBeam)
    {
      return null;
    }

    public static List<SpeckleObject> ToSpeckle(this Autodesk.Revit.DB.Structure.AnalyticalModelStick myStick)
    {
      List<SpeckleObject> returnObjects = new List<SpeckleObject>();

      if (!myStick.IsEnabled())
        return new List<SpeckleObject>();

      // Get the family
      var myFamily = (Autodesk.Revit.DB.FamilyInstance)Doc.GetElement(myStick.GetElementId());

      var myElement = new Structural1DElement();

      // TODO:
      if (!myStick.IsSingleCurve())
        return returnObjects;

      var curve = SpeckleCore.Converter.Serialise(myStick.GetCurve());
      if (curve is SpeckleLine)
        myElement.baseLine = curve as SpeckleCoreGeometryClasses.SpeckleLine;
      else if (curve is SpeckleArc)
        // SHOULD TURN TO POLYLINE
        myElement.Value = (curve as SpeckleCoreGeometryClasses.SpeckleArc).StartPoint.Value.Concat((curve as SpeckleCoreGeometryClasses.SpeckleArc).EndPoint.Value).ToList();
      else
        return returnObjects;

      var coordinateSystem = myStick.GetLocalCoordinateSystem();
      if (coordinateSystem != null)
        myElement.ZAxis = new StructuralVectorThree(new double[] { coordinateSystem.BasisZ.X, coordinateSystem.BasisZ.Y, coordinateSystem.BasisZ.Z });

      if (myStick is AnalyticalModelColumn)
      {
        StructuralVectorBoolSix endRelease1 = null, endRelease2 = null;

        switch (myStick.get_Parameter(BuiltInParameter.STRUCTURAL_BOTTOM_RELEASE_TYPE).AsInteger())
        {
          case 0:
            endRelease1 = new StructuralVectorBoolSix(new bool[] { false, false, false, false, false, false });
            break;
          case 1:
            endRelease1 = new StructuralVectorBoolSix(new bool[] { false, false, false, true, true, true });
            break;
          case 2:
            endRelease1 = new StructuralVectorBoolSix(new bool[] { false, false, false, false, true, true });
            break;
          case 3:
            endRelease1 = new StructuralVectorBoolSix(new bool[] {
              myStick.get_Parameter(BuiltInParameter.STRUCTURAL_BOTTOM_RELEASE_FX).AsInteger() == 1,
              myStick.get_Parameter(BuiltInParameter.STRUCTURAL_BOTTOM_RELEASE_FY).AsInteger() == 1,
              myStick.get_Parameter(BuiltInParameter.STRUCTURAL_BOTTOM_RELEASE_FZ).AsInteger() == 1,
              myStick.get_Parameter(BuiltInParameter.STRUCTURAL_BOTTOM_RELEASE_MX).AsInteger() == 1,
              myStick.get_Parameter(BuiltInParameter.STRUCTURAL_BOTTOM_RELEASE_MY).AsInteger() == 1,
              myStick.get_Parameter(BuiltInParameter.STRUCTURAL_BOTTOM_RELEASE_MZ).AsInteger() == 1,
            });
            break;
        }

        switch (myStick.get_Parameter(BuiltInParameter.STRUCTURAL_TOP_RELEASE_TYPE).AsInteger())
        {
          case 0:
            endRelease2 = new StructuralVectorBoolSix(new bool[] { false, false, false, false, false, false });
            break;
          case 1:
            endRelease2 = new StructuralVectorBoolSix(new bool[] { false, false, false, true, true, true });
            break;
          case 2:
            endRelease2 = new StructuralVectorBoolSix(new bool[] { false, false, false, false, true, true });
            break;
          case 3:
            endRelease2 = new StructuralVectorBoolSix(new bool[] {
              myStick.get_Parameter(BuiltInParameter.STRUCTURAL_TOP_RELEASE_FX).AsInteger() == 1,
              myStick.get_Parameter(BuiltInParameter.STRUCTURAL_TOP_RELEASE_FY).AsInteger() == 1,
              myStick.get_Parameter(BuiltInParameter.STRUCTURAL_TOP_RELEASE_FZ).AsInteger() == 1,
              myStick.get_Parameter(BuiltInParameter.STRUCTURAL_TOP_RELEASE_MX).AsInteger() == 1,
              myStick.get_Parameter(BuiltInParameter.STRUCTURAL_TOP_RELEASE_MY).AsInteger() == 1,
              myStick.get_Parameter(BuiltInParameter.STRUCTURAL_TOP_RELEASE_MZ).AsInteger() == 1,
            });
            break;
        }
        
        myElement.EndRelease = new List<StructuralVectorBoolSix>() { endRelease1, endRelease2 };
      }
      else
      {
        StructuralVectorBoolSix endRelease1 = null, endRelease2 = null;

        switch (myStick.get_Parameter(BuiltInParameter.STRUCTURAL_START_RELEASE_TYPE).AsInteger())
        {
          case 0:
            endRelease1 = new StructuralVectorBoolSix(new bool[] { false, false, false, false, false, false });
            break;
          case 1:
            endRelease1 = new StructuralVectorBoolSix(new bool[] { false, false, false, true, true, true });
            break;
          case 2:
            endRelease1 = new StructuralVectorBoolSix(new bool[] { false, false, false, false, true, true });
            break;
          case 3:
            endRelease1 = new StructuralVectorBoolSix(new bool[] {
              myStick.get_Parameter(BuiltInParameter.STRUCTURAL_START_RELEASE_FX).AsInteger() == 1,
              myStick.get_Parameter(BuiltInParameter.STRUCTURAL_START_RELEASE_FY).AsInteger() == 1,
              myStick.get_Parameter(BuiltInParameter.STRUCTURAL_START_RELEASE_FZ).AsInteger() == 1,
              myStick.get_Parameter(BuiltInParameter.STRUCTURAL_START_RELEASE_MX).AsInteger() == 1,
              myStick.get_Parameter(BuiltInParameter.STRUCTURAL_START_RELEASE_MY).AsInteger() == 1,
              myStick.get_Parameter(BuiltInParameter.STRUCTURAL_START_RELEASE_MZ).AsInteger() == 1,
            });
            break;
        }

        switch (myStick.get_Parameter(BuiltInParameter.STRUCTURAL_END_RELEASE_TYPE).AsInteger())
        {
          case 0:
            endRelease2 = new StructuralVectorBoolSix(new bool[] { false, false, false, false, false, false });
            break;
          case 1:
            endRelease2 = new StructuralVectorBoolSix(new bool[] { false, false, false, true, true, true });
            break;
          case 2:
            endRelease2 = new StructuralVectorBoolSix(new bool[] { false, false, false, false, true, true });
            break;
          case 3:
            endRelease2 = new StructuralVectorBoolSix(new bool[] {
              myStick.get_Parameter(BuiltInParameter.STRUCTURAL_END_RELEASE_FX).AsInteger() == 1,
              myStick.get_Parameter(BuiltInParameter.STRUCTURAL_END_RELEASE_FY).AsInteger() == 1,
              myStick.get_Parameter(BuiltInParameter.STRUCTURAL_END_RELEASE_FZ).AsInteger() == 1,
              myStick.get_Parameter(BuiltInParameter.STRUCTURAL_END_RELEASE_MX).AsInteger() == 1,
              myStick.get_Parameter(BuiltInParameter.STRUCTURAL_END_RELEASE_MY).AsInteger() == 1,
              myStick.get_Parameter(BuiltInParameter.STRUCTURAL_END_RELEASE_MZ).AsInteger() == 1,
            });
            break;
        }

        myElement.EndRelease = new List<StructuralVectorBoolSix>() { endRelease1, endRelease2 };
      }

      // Property
      try
      {
        var mySection = new Structural1DProperty();

        mySection.Name = Doc.GetElement(myStick.GetElementId()).Name;
        mySection.StructuralId = mySection.Name;

        switch (myFamily.Symbol.GetStructuralSection().StructuralSectionGeneralShape)
        {
          case Autodesk.Revit.DB.Structure.StructuralSections.StructuralSectionGeneralShape.GeneralI:
            mySection.Shape = Structural1DPropertyShape.I;
            mySection.Hollow = false;
            break;
          case Autodesk.Revit.DB.Structure.StructuralSections.StructuralSectionGeneralShape.GeneralT:
            mySection.Shape = Structural1DPropertyShape.T;
            mySection.Hollow = false;
            break;
          case Autodesk.Revit.DB.Structure.StructuralSections.StructuralSectionGeneralShape.GeneralH:
            mySection.Shape = Structural1DPropertyShape.Rectangular;
            mySection.Hollow = true;
            mySection.Thickness = (double)typeof(Autodesk.Revit.DB.Structure.StructuralSections.StructuralSectionGeneralH).GetProperty("WallNominalThickness").GetValue(myFamily.Symbol.GetStructuralSection()) / Scale;
            break;
          case Autodesk.Revit.DB.Structure.StructuralSections.StructuralSectionGeneralShape.GeneralR:
            mySection.Shape = Structural1DPropertyShape.Circular;
            mySection.Hollow = true;
            mySection.Thickness = (double)typeof(Autodesk.Revit.DB.Structure.StructuralSections.StructuralSectionGeneralR).GetProperty("WallNominalThickness").GetValue(myFamily.Symbol.GetStructuralSection()) / Scale;
            mySection.Profile = new SpeckleCircle(
              new SpecklePlane(new SpecklePoint(0, 0, 0),
                new SpeckleVector(0, 0, 1),
                new SpeckleVector(1, 0, 0),
                new SpeckleVector(0, 1, 0)),
              (double)typeof(Autodesk.Revit.DB.Structure.StructuralSections.StructuralSectionGeneralR).GetProperty("Diameter").GetValue(myFamily.Symbol.GetStructuralSection()) / 2 / Scale);
            break;
          case Autodesk.Revit.DB.Structure.StructuralSections.StructuralSectionGeneralShape.GeneralF:
            mySection.Shape = Structural1DPropertyShape.Rectangular;
            mySection.Hollow = false;
            break;
          case Autodesk.Revit.DB.Structure.StructuralSections.StructuralSectionGeneralShape.GeneralS:
            mySection.Shape = Structural1DPropertyShape.Circular;
            mySection.Profile = new SpeckleCircle(
              new SpecklePlane(new SpecklePoint(0, 0, 0),
                new SpeckleVector(0, 0, 1),
                new SpeckleVector(1, 0, 0),
                new SpeckleVector(0, 1, 0)),
              (double)typeof(Autodesk.Revit.DB.Structure.StructuralSections.StructuralSectionGeneralR).GetProperty("Diameter").GetValue(myFamily.Symbol.GetStructuralSection()) / 2 / Scale);
            mySection.Hollow = false;
            break;
          default:
            mySection.Shape = Structural1DPropertyShape.Generic;
            mySection.Hollow = false;
            break;
        }

        // Generate section profile
        var profile = myFamily.GetSweptProfile().GetSweptProfile();

        if (mySection.Shape != Structural1DPropertyShape.Circular)
        {
          var myProfile = new SpecklePolyline();

          myProfile.Value = new List<double>();

          for (int i = 0; i < profile.Curves.Size; i++)
          {
            var sectionCurves = SpeckleCore.Converter.Serialise(profile.Curves.get_Item(i));

            var sectionCoordinates = new List<double>();
            var nextCoordinates = new List<double>();

            if (sectionCurves is SpeckleLine)
            {
              sectionCoordinates = (sectionCurves as SpeckleLine).Value.Select(x => Math.Round(x, 10)).ToList();

              if (myProfile.Value.Count == 0)
              {
                myProfile.Value = sectionCoordinates;
                continue;
              }

              if (myProfile.Value.Skip(myProfile.Value.Count - 3).SequenceEqual(sectionCoordinates.Take(3)))
                nextCoordinates = sectionCoordinates.Skip(3).ToList();
              else
                break;
            }
            else if (sectionCurves is SpeckleArc)
            {
              if (myProfile.Value.Count == 0)
              {
                myProfile.Value = (sectionCurves as SpeckleArc).StartPoint.Value.Select(x => Math.Round(x, 10))
                  .Concat((sectionCurves as SpeckleArc).MidPoint.Value.Select(x => Math.Round(x, 10)))
                  .Concat((sectionCurves as SpeckleArc).EndPoint.Value.Select(x => Math.Round(x, 10)))
                  .ToList();
                continue;
              }

              if (myProfile.Value.Skip(myProfile.Value.Count - 3).SequenceEqual((sectionCurves as SpeckleArc).StartPoint.Value.Select(x => Math.Round(x, 10))))
                nextCoordinates = (sectionCurves as SpeckleArc).EndPoint.Value.Select(x => Math.Round(x, 10)).ToList();
              else if (myProfile.Value.Skip(myProfile.Value.Count - 3).SequenceEqual((sectionCurves as SpeckleArc).EndPoint.Value.Select(x => Math.Round(x, 10))))
                nextCoordinates = (sectionCurves as SpeckleArc).StartPoint.Value.Select(x => Math.Round(x, 10)).ToList();
              else
                break;
            }

            if (nextCoordinates.SequenceEqual(myProfile.Value.Take(3)))
            {
              myProfile.Closed = true;
              break;
            }
            else
              myProfile.Value.AddRange(nextCoordinates);
          }

          myProfile.GenerateHash();

          mySection.Profile = myProfile;
        }

        // Material
        try
        {
          var matType = myFamily.StructuralMaterialType;

          var matAsset = ((Autodesk.Revit.DB.PropertySetElement)Doc.GetElement(((Autodesk.Revit.DB.Material)Doc.GetElement(myFamily.StructuralMaterialId)).StructuralAssetId)).GetStructuralAsset();

          SpeckleObject myMaterial = null;

          switch (matType)
          {
            case Autodesk.Revit.DB.Structure.StructuralMaterialType.Concrete:
              var concMat = new StructuralMaterialConcrete();
              concMat.StructuralId = Doc.GetElement(myFamily.StructuralMaterialId).Name;
              concMat.Name = concMat.StructuralId;
              concMat.YoungsModulus = matAsset.YoungModulus.X;
              concMat.ShearModulus = matAsset.ShearModulus.X;
              concMat.PoissonsRatio = matAsset.PoissonRatio.X;
              concMat.Density = matAsset.Density;
              concMat.CoeffThermalExpansion = matAsset.ThermalExpansionCoefficient.X;
              concMat.CompressiveStrength = matAsset.ConcreteCompression;
              concMat.MaxStrain = 0;
              concMat.AggragateSize = 0;
              myMaterial = concMat;
              break;
            case Autodesk.Revit.DB.Structure.StructuralMaterialType.Steel:
              var steelMat = new StructuralMaterialSteel();
              steelMat.StructuralId = Doc.GetElement(myFamily.StructuralMaterialId).Name;
              steelMat.Name = steelMat.StructuralId;
              steelMat.YoungsModulus = matAsset.YoungModulus.X;
              steelMat.ShearModulus = matAsset.ShearModulus.X;
              steelMat.PoissonsRatio = matAsset.PoissonRatio.X;
              steelMat.Density = matAsset.Density;
              steelMat.CoeffThermalExpansion = matAsset.ThermalExpansionCoefficient.X;
              steelMat.YieldStrength = matAsset.MinimumYieldStress;
              steelMat.UltimateStrength = matAsset.MinimumTensileStrength;
              steelMat.MaxStrain = 0;
              myMaterial = steelMat;
              break;
            default:
              var defMat = new StructuralMaterialSteel();
              defMat.StructuralId = Doc.GetElement(myFamily.StructuralMaterialId).Name;
              defMat.Name = defMat.StructuralId;
              myMaterial = defMat;
              break;
          }

          myMaterial.GenerateHash();
          mySection.ApplicationId = myStick.UniqueId + "_section";
          mySection.MaterialRef = (myMaterial as IStructural).StructuralId;

          returnObjects.Add(myMaterial);
        }
        catch { }

        mySection.GenerateHash();
        mySection.ApplicationId = myStick.UniqueId + "_material";
        myElement.PropertyRef = mySection.StructuralId;

        returnObjects.Add(mySection);
      }
      catch { }

      myElement.GenerateHash();
      myElement.ApplicationId = myStick.UniqueId;
      returnObjects.Add(myElement);

      return returnObjects;
    }
  }
}
